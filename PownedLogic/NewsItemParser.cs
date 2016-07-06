using BaseLogic.HtmlUtil;
using HtmlAgilityPack;
using Newtonsoft.Json;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.LoadHtml(Source);

            if (htmlDoc.DocumentNode != null)
            {
                var ArticleNode = htmlDoc.DocumentNode.Descendants("article").Where(d => d.Attributes.Count(a => a.Value.Contains("page-item")) > 0).FirstOrDefault();

                string Hashtag = ArticleNode.Attributes.FirstOrDefault(a => a.Name == "data-category").Value;
                string Title = ArticleNode.Attributes.FirstOrDefault(a => a.Name == "data-title").Value;

                var ArticleBody = ArticleNode.Descendants("div").Where(d => d.Attributes.Count(a => a.Value.Contains("page-item-body")) > 0).FirstOrDefault();
                string Author = ArticleNode.Descendants("span").FirstOrDefault(s => s.Attributes.Count(a => a.Value.Contains("page-item__author")) > 0).InnerText;
                string TimeStamp = ArticleNode.Descendants("time").FirstOrDefault(s => s.Attributes.Count(a => a.Value.Contains("page-item__content__date")) > 0).InnerText;

                string Summary = ArticleNode.Descendants("div").FirstOrDefault(s => s.Attributes.Count(a => a.Value.Contains("page-item__content__lead lead")) >
                 0).InnerText;

                List<string> Content = new List<string>();

                foreach (HtmlNode n in ArticleNode.Descendants("p"))
                {
                    if (n.InnerText != Summary && !string.IsNullOrWhiteSpace(n.InnerText))
                    {
                        Content.Add(n.InnerText);
                    }
                }

                ObservableCollection<string> Images = new ObservableCollection<string>();

                //Images
                //Task T = Task.Run(() => GetImagesFromTwitter(ArticleNode, Images));

                return new NewsItem(Title, Summary, Content, TimeStamp, Author, string.Empty, await GetComments(ArticleNode), Images, GetYouTubeURL(ArticleNode), string.Empty);
            }

            return null;
        }

        private static string GetYouTubeURL(HtmlNode ArticleNode)
        {
            if (ApplicationData.Current.LocalSettings.Values["Media weergeven"] != null && Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values["Media weergeven"]))
            {
                var YoutubeNode = ArticleNode.Descendants("iframe").Where(n => n.Attributes.Count(a => a.Value.Contains("youtube")) > 0).FirstOrDefault();

                if (YoutubeNode == null)
                {
                    return null;
                }

                return YoutubeNode.Attributes.SingleOrDefault(a => a.Name == "src").Value;
            }

            return null;
        }

        private static async Task<List<Comment>> GetComments(HtmlNode ArticleNode)
        {
            if (ApplicationData.Current.LocalSettings.Values["Reacties weergeven"] != null && Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values["Reacties weergeven"]))
            {
                string ArticleID = ArticleNode.Attributes.SingleOrDefault(a => a.Name.Contains("data-id")).Value;
                string CommentsJson = await HTTPGetUtil.GetDataAsStringFromURL("https://services.powned.tv/v1/articles/" + ArticleID + "/comments");

                return JsonConvert.DeserializeObject<List<Comment>>(CommentsJson);
            }

            return new List<Comment>();
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

        private static async Task GetImagesFromTwitter(HtmlNode ArticleNode, ObservableCollection<string> Images)
        {
            var TwitterNodes = ArticleNode.Descendants("iframe").Where(n => n.Attributes.Count(a => a.Value.Contains("twitter-tweet")) > 0);

            foreach (HtmlNode TwitterNode in TwitterNodes)
            {
                var TwitterUrlNodes = TwitterNode.Descendants("a").Where(n => n.Attributes.Count(a => a.Value.Contains("MediaCard-borderoverlay")) > 0);

                foreach (HtmlNode TwitterUrlNode in TwitterUrlNodes)
                {
                    Images.Add(await GetPicture(TwitterUrlNode.Attributes.SingleOrDefault(a => a.Name == "href").Value));
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
