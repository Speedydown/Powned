using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;
using Windows.Storage;

namespace PownedLogic
{
    public static class NewsItemParser
    {
        public static NewsItem GetNewsItemFromSource(string Source)
        {
            string SourceBackupForTwitterImage = Source;
            Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<div class=\"acarhead\">", Source, true));

            string Date = HTMLParserUtil.GetContentAndSubstringInput("<p class=\"articledate\">", "/p><br />", Source, out Source, "", true);
            string Title = HTMLParserUtil.GetContentAndSubstringInput("<h1>", "</h1><br />", Source, out Source, "", true);
            string Hashtag = HTMLParserUtil.GetContentAndSubstringInput("<p class=\"hashtag\">", "</p>", Source, out Source, "", true);
            string ArticleImage = HTMLParserUtil.GetContentAndSubstringInput("<img src=\"", "\" alt=", Source, out Source, "", true);

            Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<div class=\"artikel-intro\">", Source, true));

            string Summary = HTMLParserUtil.GetContentAndSubstringInput("<p>", "</p>", Source, out Source, "", true);

            string RawArticleContent = HTMLParserUtil.GetContentAndSubstringInput("<div class=\"artikel-main\">", "<div id=\"artikel-footer\">", Source, out Source, "", true);

            List<string> ArticleContent = new List<string>();

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

            if (SourceBackupForTwitterImage.Contains("pic.twitter.com"))
            {
                SourceBackupForTwitterImage = SourceBackupForTwitterImage.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("pic.twitter.com", SourceBackupForTwitterImage, false));
                image = "http://pic.twitter.com" + HTMLParserUtil.GetContentAndSubstringInput("pic.twitter.com", "</a>", SourceBackupForTwitterImage, out SourceBackupForTwitterImage);
            }

            string AuthorDate = HTMLParserUtil.GetContentAndSubstringInput("<span class=\"author-date\">", "</span>", Source, out Source, "", true);

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

            return new NewsItem(Title, Summary, ArticleContent, Date, AuthorDate, ArticleImage, Comments, image);
        }
    }
}
