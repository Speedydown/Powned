﻿using System;
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
        internal static async Task<NewsItem> GetNewsItemFromSource(string Source)
        {
            string SourceBackupForTwitterImage = Source;
            string Date = string.Empty;
            string Title = string.Empty;
            string Hashtag = string.Empty;
            string ArticleImage = string.Empty;
            string Summary = string.Empty;
            List<string> ArticleContent = new List<string>();
            string AuthorDate = string.Empty;

            Task NewsItemTask = Task.Run(() =>
                {

                    Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<div class=\"acarhead\">", Source, true));

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
                            string Content = HTMLParserUtil.GetContentAndSubstringInput("<p>", "</p>", RawArticleContent, out RawArticleContent, "", true);
                            Content = HTMLParserUtil.CleanHTMLTagsFromString(Content);

                            ArticleContent.Add(Content);
                        }
                        catch
                        {
                            break;
                        }
                    }

                    string image = string.Empty;

                    //if (SourceBackupForTwitterImage.Contains("pic.twitter.com"))
                    //{
                    //    SourceBackupForTwitterImage = SourceBackupForTwitterImage.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("pic.twitter.com", SourceBackupForTwitterImage, false));
                    //    image = "http://pic.twitter.com" + HTMLParserUtil.GetContentAndSubstringInput("pic.twitter.com", "</a>", SourceBackupForTwitterImage, out SourceBackupForTwitterImage);
                    //}

                    AuthorDate = HTMLParserUtil.GetContentAndSubstringInput("<span class=\"author-date\">", "</span>", Source, out Source, "", true);

                });


            ObservableCollection<string> Images = new ObservableCollection<string>();

            Task<List<Comment>> CommentsTask = Task.Run(() => GetCommentsFromSource(SourceBackupForTwitterImage));
            Task InstagramTask = Task.Run(() => GetInstaGramImagesFromSource(SourceBackupForTwitterImage, Images));
            Task TwitterImagesTask = Task.Run(() => GetImagesFromSource(SourceBackupForTwitterImage, Images));

            await NewsItemTask;

            return new NewsItem(Title, Summary, ArticleContent, Date, AuthorDate, ArticleImage, await CommentsTask, Images);
        }

        private static async Task<List<Comment>> GetCommentsFromSource(string Source)
        {
            List<Comment> Comments = new List<Comment>();

            if (ApplicationData.Current.LocalSettings.Values["Reacties weergeven"] != null && Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values["Reacties weergeven"]))
            {
                try
                {
                    string CommentHTML = HTMLParserUtil.GetContentAndSubstringInput("<div id=\"comments\">", "<div id=\"reageerhier\">", Source, out Source, "", true);

                    while (true)
                    {
                        try
                        {
                            CommentHTML = CommentHTML.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<div class=\"comment\"", CommentHTML, true));
                            string Content = string.Empty;

                            string ThisCommentHTML = CommentHTML.Substring(0, HTMLParserUtil.GetPositionOfStringInHTMLSource("<p class=\"footer\">", CommentHTML, false));

                            while (true)
                            {
                                try
                                {
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
                }
                catch
                {

                }
            }

            return Comments;
        }

        private static async Task GetImagesFromSource(string Source, ObservableCollection<string> Images)
        {
            try
            {
                Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<blockquote class=\"twitter-tweet", Source, false));
            }
            catch
            {
                return;
            }

            while (true)
            {
                try
                {
                    //GetTwitterURL's
                    string TwitterHTMLL = HTMLParserUtil.GetContentAndSubstringInput("<blockquote class=\"twitter-tweet", "</blockquote>", Source, out Source);
                    TwitterHTMLL = TwitterHTMLL.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("pic.twitter.com", TwitterHTMLL, false));
                    TwitterHTMLL = TwitterHTMLL.Substring(0, HTMLParserUtil.GetPositionOfStringInHTMLSource("</a>", TwitterHTMLL, false));

                    string ImageURL = await GetPicture("http://" + TwitterHTMLL);

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
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
            try
            {
                Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<blockquote class=\"instagram-media\"", Source, false));
            }
            catch
            {
                return;
            }

            while (true)
            {
                try
                {
                    //GetTwitterURL's
                    string InstagramHTML = HTMLParserUtil.GetContentAndSubstringInput("<blockquote class=\"instagram-media\"", "</blockquote>", Source, out Source);
                    InstagramHTML = InstagramHTML.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<a href=\"", InstagramHTML, false));
                    string InstagramURL = HTMLParserUtil.GetContentAndSubstringInput("<a href=\"", "\" style=\"", InstagramHTML, out InstagramHTML);

                    string ImageURL = await GetPictureInstagram(InstagramURL);

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
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
