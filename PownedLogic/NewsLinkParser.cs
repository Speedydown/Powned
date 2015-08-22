using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PownedLogic
{
    static class NewsLinkParser
    {
        public static IList<NewsLink> GetNewsLinksFromSource(string Source)
        {
            List<NewsLink> NewsLinks = new List<NewsLink>();

            using (XmlReader reader = XmlReader.Create(new StringReader(Source)))
            {
                reader.ReadToFollowing("item");

                while (true)
                {
                    try
                    {
                        reader.ReadToFollowing("title");
                        string Title = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("description");
                        string description = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("link");
                        string link = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("category");
                        string category = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("jrs");
                        string Date = reader.ReadElementContentAsString();

                        NewsLinks.Add(new NewsLink(Title, description, link, category, Date, string.Empty));
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            return NewsLinks;
        }
    }
}
