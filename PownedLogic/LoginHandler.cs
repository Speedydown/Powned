using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;

namespace PownedLogic
{
    public static class LoginHandler
    {
        internal static Dictionary<string, string> GetLoginFormFields(string Source, string Email, string Password)
        {
            Dictionary<string, string> ValueDictionary = new Dictionary<string, string>();

            Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<form name", Source, true));

            while (true)
            {
                try
                {
                    string FieldName = HTMLParserUtil.GetContentAndSubstringInput("name=\"", "\" value", Source, out Source, string.Empty, false);
                    string FieldValue = HTMLParserUtil.GetContentAndSubstringInput("value=\"", "\" />", Source, out Source, string.Empty, false);

                    if (FieldName == "email")
                    {
                        FieldValue = Email;
                    }

                    ValueDictionary.Add(FieldName, FieldValue);
                }
                catch
                {
                    break;
                }
            }

            ValueDictionary.Add("password", Password);
            ValueDictionary.Add("submit", "login");

            return ValueDictionary;
        }
    }
}
