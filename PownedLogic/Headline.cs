using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace PownedLogic
{
    public sealed class Headline
    {
        public string URL { get; private set; }
        public string ImageURL { get; private set; }
        public string Title { get; private set; }
        public string HashTag { get; private set; }

        public int Bounds { get; set; }
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

        public Headline(string URL, string ImageURL, string Title, string HashTag)
        {
            try
            {
                this.Bounds = (int)(Window.Current.Bounds.Width);
            }
            catch
            {
                this.Bounds = 0;
            }

            this.URL = URL;
            this.ImageURL = ImageURL;
            this.Title = Title;
            this.HashTag = HashTag;
        }
    }
}
