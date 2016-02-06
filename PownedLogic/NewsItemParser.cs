using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace PownedLogic
{
    public static class NewsItemParser
    {
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        internal static async Task<NewsItem> GetNewsItemFromSource(string Source)
        {
            bool GetMediaContent = localSettings.Values["Media weergeven"] != null && Convert.ToBoolean(localSettings.Values["Media weergeven"]);

            string SourceBackupForTwitterImage = Source;
            string Date = string.Empty;
            string Title = string.Empty;
            string Hashtag = string.Empty;
            string ArticleImage = string.Empty;
            string Summary = string.Empty;
            List<string> ArticleContent = new List<string>();
            string AuthorDate = string.Empty;
            string YoutubeURL = null;
            string entry_id = string.Empty;

            Task NewsItemTask = Task.Run(() =>
                {
                    entry_id = HTMLParserUtil.GetContentAndSubstringInput("entry_id=", "%26static", Source, out Source);


                    Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<div class=\"acarhead\">", Source, true));

                    YoutubeURL = GetYouTubeURL(Source);
                    Date = HTMLParserUtil.GetContentAndSubstringInput("<p class=\"articledate\">", "/p><br />", Source, out Source, "", true);
                    Title = HTMLParserUtil.GetContentAndSubstringInput("<h1>", "</h1><br />", Source, out Source, "", true);
                    Hashtag = HTMLParserUtil.GetContentAndSubstringInput("<p class=\"hashtag\">", "</p>", Source, out Source, "", true);
                    ArticleImage = HTMLParserUtil.GetContentAndSubstringInput("<img src=\"", "\" alt=", Source, out Source, "", true);
                    Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<div class=\"artikel-intro\">", Source, true));

                    Summary = HTMLParserUtil.GetContentAndSubstringInput("<p>", "</p>", Source, out Source, "", true);

                    string RawArticleContent = HTMLParserUtil.GetContentAndSubstringInput("<div class=\"artikel-main\">", "<div id=\"artikel-footer\">", Source, out Source, "", true);

                    while (true)
                    {
                        try
                        {
                            if (!RawArticleContent.Contains("<p>"))
                            {
                                break;
                            }

                            string Content = HTMLParserUtil.GetContentAndSubstringInput("<p>", "</p>", RawArticleContent, out RawArticleContent, "", true);
                            Content = HTMLParserUtil.CleanHTMLTagsFromString(Content);

                            if (Content != string.Empty)
                            {
                                ArticleContent.Add(Content);
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }

                    string image = string.Empty;

                    AuthorDate = HTMLParserUtil.GetContentAndSubstringInput("<span class=\"author-date\">", "</span>", Source, out Source, "", true);

                });


            ObservableCollection<string> Images = new ObservableCollection<string>();

            Task<List<Comment>> CommentsTask = Task.Run(() => GetCommentsFromSource(SourceBackupForTwitterImage));

            if (GetMediaContent)
            {
                Task BaseImageTask = Task.Run(() => GetBaseImagesFromSource(SourceBackupForTwitterImage, Images));
                Task InstagramTask = Task.Run(() => GetInstaGramImagesFromSource(SourceBackupForTwitterImage, Images));
                Task TwitterImagesTask = Task.Run(() => GetImagesFromSource(SourceBackupForTwitterImage, Images));
            }
            else
            {
                YoutubeURL = null;
            }

            await NewsItemTask;

            return new NewsItem(Title, Summary, ArticleContent, Date, AuthorDate, ArticleImage, await CommentsTask, Images, YoutubeURL, entry_id);
        }

        private static string GetYouTubeURL(string Source)
        {
            if (!Source.Contains("https://www.youtube.com/embed/"))
            {
                return null;
            }

            try
            {
                Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("https://www.youtube.com/embed/", Source, false));
                string YoutubeID = HTMLParserUtil.GetContentAndSubstringInput("https://www.youtube.com/embed/", "\"", Source, out Source, "", true);

                return "https://www.youtube.com/embed/" + YoutubeID;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<List<Comment>> GetCommentsFromSource(string Source)
        {
            List<Comment> Comments = new List<Comment>();

            if (ApplicationData.Current.LocalSettings.Values["Reacties weergeven"] != null && Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values["Reacties weergeven"]))
            {
                try
                {
                    string CommentHTML = HTMLParserUtil.GetContentAndSubstringInput("<div id=\"comments\">", "<div id=\"reageerhier\">", Source, out Source, "", true);


                    int HalfOFCommentsIndex = CommentHTML.IndexOf("<div class=\"comment\"", CommentHTML.Length / 2);

                    if (HalfOFCommentsIndex == -1)
                    {
                        return Comments;
                    }

                    Task<List<Comment>> FirstHalf = Task.Run(() => CommentsParser(CommentHTML.Substring(0, HalfOFCommentsIndex)));
                    Task<List<Comment>> SecondHalf = Task.Run(() => CommentsParser(CommentHTML.Substring(HalfOFCommentsIndex)));

                    Comments.AddRange(await FirstHalf);
                    Comments.AddRange(await SecondHalf);
                }
                catch
                {

                }
            }

            return Comments;
        }

        private static void GetBaseImagesFromSource(string Source, ObservableCollection<string> Images)
        {
            Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<div class=\"artikel-main\">", Source, false));
            Source = HTMLParserUtil.GetContentAndSubstringInput("<div class=\"artikel-main\">", "</div>", Source, out Source);

            while (true)
            {
                if (!Source.Contains("<img "))
                {
                    return;
                }

                try
                {
                    Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<img ", Source, true));
                    Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("src=\"", Source, false));
                    string Image = HTMLParserUtil.GetContentAndSubstringInput("src=\"", "\" ", Source, out Source);

                    Images.Add(Image);
                }
                catch
                {
                    break;
                }
            }
        }

        private async static Task<List<Comment>> CommentsParser(string CommentHTML)
        {
            List<Comment> Comments = new List<Comment>();

            while (true)
            {
                try
                {
                    if (!CommentHTML.Contains("<div class=\"comment\""))
                    {
                        break;
                    }

                    CommentHTML = CommentHTML.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<div class=\"comment\"", CommentHTML, true));
                    string Content = string.Empty;

                    string ThisCommentHTML = CommentHTML.Substring(0, HTMLParserUtil.GetPositionOfStringInHTMLSource("<p class=\"footer\">", CommentHTML, false));

                    while (true)
                    {
                        try
                        {
                            if (!ThisCommentHTML.Contains("<p>"))
                            {
                                break;
                            }

                            Content += HTMLParserUtil.GetContentAndSubstringInput("<p>", "</p>", ThisCommentHTML, out ThisCommentHTML, "", true) + "\n";
                        }
                        catch
                        {
                            break;
                        }
                    }

                    Content = Content.Substring(0, Content.Length - 1);
                    Content = HTMLParserUtil.CleanHTMLTagsFromString(Content);

                    string AuthorDateTime = HTMLParserUtil.CleanHTMLTagsFromString(HTMLParserUtil.GetContentAndSubstringInput("<p class=\"footer\">", "<span title", CommentHTML, out CommentHTML, "", true));

                    Comments.Add(new Comment(HTMLParserUtil.CleanHTTPTagsFromInput(Content), AuthorDateTime));
                }
                catch
                {
                    break;
                }
            }

            return Comments;
        }

        private static async Task GetImagesFromSource(string Source, ObservableCollection<string> Images)
        {
            while (true)
            {
                if (!Source.Contains("<blockquote class=\"twitter-tweet"))
                {
                    return;
                }

                try
                {
                    //GetTwitterURL's
                    Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<blockquote class=\"twitter", Source, false));
                    string TwitterHTMLL = HTMLParserUtil.GetContentAndSubstringInput("-tweet", "</blockquote>", Source, out Source);
                    TwitterHTMLL = TwitterHTMLL.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("pic.twitter.com", TwitterHTMLL, false));
                    TwitterHTMLL = TwitterHTMLL.Substring(0, HTMLParserUtil.GetPositionOfStringInHTMLSource("</a>", TwitterHTMLL, false));

                    string ImageURL = await GetPicture("http://" + TwitterHTMLL);

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                      {
                          Images.Add(ImageURL);
                      });
                }
                catch
                {
                    break;
                }
            }
        }

        private static async Task GetInstaGramImagesFromSource(string Source, ObservableCollection<string> Images)
        {
            while (true)
            {
                try
                {
                    if (!Source.Contains("<blockquote class=\"instagram-media\""))
                    {
                        return;
                    }

                    Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<blockquote class=\"instagram-", Source, false));

                    //GetTwitterURL's
                    string InstagramHTML = HTMLParserUtil.GetContentAndSubstringInput("media\"", "</blockquote>", Source, out Source);
                    InstagramHTML = InstagramHTML.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<a href=\"", InstagramHTML, false));
                    string InstagramURL = HTMLParserUtil.GetContentAndSubstringInput("<a href=\"", "\" style=\"", InstagramHTML, out InstagramHTML);

                    string ImageURL = await GetPictureInstagram(InstagramURL);

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                       {
                           try
                           {
                               Images.Add(ImageURL);
                           }
                           catch
                           {

                           }
                       });
                }
                catch (NullReferenceException)
                {

                }
                catch
                {
                    break;
                }
            }
        }

        private static async Task<string> GetPicture(string twitterUri)
        {
            string html = await HTTPGetUtil.GetDataAsStringFromURL(twitterUri);
            string Image = HTMLParserUtil.GetContentAndSubstringInput("<meta  property=\"og:image\" content=\"", ":large\">", html, out html, "\" />");

            return Image.StartsWith("http") ? Image : null;
        }

        private static async Task<string> GetPictureInstagram(string InstagramURI)
        {
            string html = await HTTPGetUtil.GetDataAsStringFromURL(InstagramURI);
            html = html.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<meta property=\"og:image\" content=\"", html, false));
            string Image = HTMLParserUtil.GetContentAndSubstringInput("<meta property=\"og:image\" content=\"", "\" />", html, out html);

            return Image.StartsWith("http") ? Image : null;
        }
    }
}
