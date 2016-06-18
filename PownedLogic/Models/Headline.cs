using BaseLogic.DataHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SQLite.Net.Attributes;
using System.Net;
using BaseLogic.HtmlUtil;
using BaseLogic.Xaml_Controls.Interfaces;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace PownedLogic.Model
{
    public sealed class Headline : DataObject, INewsLink
    {
        public string URL { get; set; }
        public string ImageURL { get; set; }
        public Uri ImageUrl2 { get
            {
                return new Uri(ImageURL);
            }
        }

        public string Title { get; set; }
        public string HashTag { get; set; }
        public bool New { get; set; }
        public bool Seen { get; set; }
        public DateTime TimeStamp { get; set; }

        [Ignore]
        public int Bounds
        {
            get
            {
                 try
                 {
                     return (int)(Window.Current.Bounds.Width);
                 }
                 catch
                 {
                     return 0;
                 }
            }
        }

        [Ignore]
        public int WidthUwp
        {
            get
            {
                try
                {
                    if (Bounds == 0)
                    {
                        return 0;
                    }

                    if (Window.Current != null)
                    {
                        int Width = 0;

                        int NumberOfItems = (int)Window.Current.Bounds.Width / 160;

                        Width = ((Bounds) / NumberOfItems) - 10;

                        return Width;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }
        }

        [Ignore]
        public int Width
        {
            get
            {
                try
                {
                    Window.Current.SizeChanged -= Current_SizeChanged;

                    if (Bounds == 0)
                    {
                        return 0;
                    }

                    if (Window.Current != null)
                    {
                        Window.Current.SizeChanged += Current_SizeChanged;

                        if (Window.Current.Bounds.Width > Window.Current.Bounds.Height)
                        {
                            //Landscape
                            return ((Bounds - 120) / 3) - 10;
                        }
                        else
                        {
                            //Portrait
                            return (Bounds / 2) - 7;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }
        }

        public Headline()
        {
            
        }

        public Headline(string URL, string ImageURL, string Title, string HashTag)
        {
            this.URL = URL;
            this.ImageURL = Constants.Hostname + ImageURL;
            this.Title = HTMLParserUtil.CleanHTMLTagsFromString(WebUtility.HtmlDecode(Title));
            this.HashTag = WebUtility.HtmlDecode(HashTag).Replace("&amp;", "&");
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            NotifyPropertyChanged("Width");
        }

        public string Content
        {
            get { return string.Empty; }
        }

        public string CommentCount
        {
            get { return string.Empty; }
        }

        public string Time
        {
            get { return string.Empty; }
        }

        
    }
}
