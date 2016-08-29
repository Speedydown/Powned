using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace PownedLogic.DataHandlers
{
    public class WindowSizeManager
    {
        public static readonly WindowSizeManager instance = new WindowSizeManager();

        private int _ItemWidth;
        public int ItemWidth
        {
            get
            {
                if (_ItemWidth == 0)
                {
                    ItemWidth = CalculateScreenHeight();
                }

                return _ItemWidth;
            }
            set { _ItemWidth = value; }
        }

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

        private WindowSizeManager()
        {
            if (Window.Current != null)
            {
                Window.Current.SizeChanged += Current_SizeChanged;
            }
        }

        public event WindowSizeChangedEventHandler SizeManagerUpdate;

        private int CalculateScreenHeight()
        {
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

        private int CalculateScreenWidthUwp()
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

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            _ItemWidth = 0;
            SizeManagerUpdate?.Invoke(this, e);
        }
    }
}
