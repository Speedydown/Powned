using BaseLogic.DataHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using SQLiteForWindows;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace PownedLogic
{
    public sealed class Headline : DataObject
    {
        public string URL { get; set; }
        public string ImageURL { get; set; }
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
        public int Width
        {
            get
            {
                if (Window.Current != null)
                {
                    return (Bounds / 2) - 7;
                }
                else
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
            this.ImageURL = ImageURL;
            this.Title = Title;
            this.HashTag = HashTag;
        }
    }
}
