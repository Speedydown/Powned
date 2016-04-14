using BaseLogic.DataHandler;
using BaseLogic.HtmlUtil;
using Newtonsoft.Json;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PownedLogic.DataHandlers
{
    public class LoginInfoDataHandler : DataHandler
    {
        private const string LoginURL = "http://registratie.geenstijl.nl/registratie/?view=login";
        private const string LoginPostUrl = "http://registratie.geenstijl.nl/registratie/gs_engine.php?action=login";
        private const string CookieSync = "http://www.steylloos.nl/cookiesync.php?site=POW2&return=";
        private const string CookieSet = "http://new.powned.tv/cookieset.php?cookie=U3BlZWR5ZG93bg==&tk_commenter={0}&return=";

        public static readonly LoginInfoDataHandler instance = new LoginInfoDataHandler();

        public bool IsLoggedIn { get; private set; }
        public string CommenterName { get; private set; }
        public string TK_Commenter { get; private set; }
        public string LoggedInUserName { get; private set; }
        public string LoggedInPassWord { get; private set; }

        private LoginInfoDataHandler()
            : base()
        {
            CreateItemTable<LoginInfo>();
        }

        public LoginInfo GetLoginInfo()
        {
            LoginInfo LoginInfo = GetItems<LoginInfo>().FirstOrDefault();

            if (LoginInfo == null)
            {
                LoginInfo = new LoginInfo() { UserName = string.Empty, Password = string.Empty };

                InsertItem(LoginInfo);

                System.Diagnostics.Debug.WriteLine("[LoginInfo]Adding new logininfo to LocalDB");
            }

            return LoginInfo;
        }

        public void UpdateLoginInfo(LoginInfo loginInfo)
        {
            ClearTable<LoginInfo>();

            InsertItem(loginInfo);
            System.Diagnostics.Debug.WriteLine("[LoginInfo]Updating new logininfo to LocalDB");
        }

        private Dictionary<string, string> GetLoginFormFields(string Source, string Email, string Password)
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

        public async Task Login(LoginInfo loginInfo)
        {
            await Task.Delay(1000);

            if (IsLoggedIn && LoggedInUserName == loginInfo.UserName && LoggedInPassWord == loginInfo.Password)
            {
                return;
            }
            else if (IsLoggedIn && (LoggedInUserName != loginInfo.UserName || LoggedInPassWord != loginInfo.Password))
            {
                loginInfo.CookieCollection = null;
                UpdateLoginInfo(loginInfo);
            }

            LoggedInUserName = loginInfo.UserName;
            LoggedInPassWord = loginInfo.Password;
            IsLoggedIn = false;
            TK_Commenter = null;
            CommenterName = null;

            if (string.IsNullOrEmpty(loginInfo.CookieCollection))
            {
                string PageSource = await HTTPGetUtil.GetDataAsStringFromURL(LoginURL, Encoding.GetEncoding("iso-8859-1"));

                Dictionary<string, string> ValueDictionary = GetLoginFormFields(PageSource, loginInfo.UserName, loginInfo.Password);

                HttpResponseMessage response = await HTTPGetUtil.PostDataToURL(LoginPostUrl, ValueDictionary);

                if (!IsCookieCollectionValid())
                {
                    return;
                }

                if (!(response.StatusCode == HttpStatusCode.OK && !(await response.Content.ReadAsStringAsync()).Contains("mislukt")))
                {
                    return;
                }
                else
                {
                    loginInfo.CookieCollection = JsonConvert.SerializeObject(HTTPGetUtil.Cookiejar.GetCookies(new Uri("http://www.steylloos.nl")).OfType<Cookie>().ToList());
                    UpdateLoginInfo(loginInfo);
                }
            }
            else
            {
                List<Cookie> CookieCollection = JsonConvert.DeserializeObject<List<Cookie>>(loginInfo.CookieCollection);

                if (!IsCookieCollectionValid(CookieCollection))
                {
                    loginInfo.CookieCollection = null;
                    UpdateLoginInfo(loginInfo);

                    await Login(loginInfo);
                }
            }

            IsLoggedIn = true;

            Task.WaitAll(new Task[] 
                { 
                    Task.Run(() => HTTPGetUtil.GetDataAsStringFromURL(CookieSync, Encoding.GetEncoding("iso-8859-1"))),
                    Task.Run(() => HTTPGetUtil.GetDataAsStringFromURL(string.Format(CookieSet, TK_Commenter), Encoding.GetEncoding("iso-8859-1")))
                });
        }

        public bool IsCookieCollectionValid(List<Cookie> CookieCollection = null)
        {
            if (CookieCollection == null)
            {
                CookieCollection = HTTPGetUtil.Cookiejar.GetCookies(new Uri("http://www.steylloos.nl")).OfType<Cookie>().ToList();
            }
            else
            {
                foreach (Cookie c in CookieCollection)
                {
                    HTTPGetUtil.Cookiejar.Add(new Uri("http://www.steylloos.nl"), c);
                }
            }

            foreach (var c in CookieCollection)
            {
                if (c.Expires > DateTime.Now)
                {
                    if (c.Name == "tk_commenter")
                    {
                        TK_Commenter = c.Value;
                    }
                    else if (c.Name == "commenter_name")
                    {
                        CommenterName = c.Value;
                    }
                }
                else
                {
                    return false;
                }
            }

            return CommenterName != string.Empty && TK_Commenter != string.Empty;
        }

        public async Task PlaceComment(string Comment, string DataEntryID)
        {
            if (IsLoggedIn)
            {
                Dictionary<string, string> ValueDictionary = new Dictionary<string, string>();
                ValueDictionary.Add("static", "1");
                ValueDictionary.Add("entry_id", DataEntryID);
                ValueDictionary.Add("text", Comment);
                ValueDictionary.Add("post", "Verstuur");

                HttpResponseMessage response = await HTTPGetUtil.PostDataToURL("http://app.steylloos.nl/mt-comments.fcgi", ValueDictionary);

                string content = await response.Content.ReadAsStringAsync();

                //TODO Check op niet ingelogd foutmelding
            }
        }
    }
}
