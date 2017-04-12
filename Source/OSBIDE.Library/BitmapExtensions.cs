using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OSBIDE.Library
{
    public static class BitmapExtensions
    {
        public static ImageSource ToImageSource(this Bitmap bitmap)
        {
            var hbitmap = bitmap.GetHbitmap();
            try
            {
                var imageSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));

                return imageSource;
            }
            finally
            {
                NativeMethods.DeleteObject(hbitmap);
            }
        }
    }

    public static class NativeMethods
    {
        [DllImport("gdi32")]
        public static extern int DeleteObject(IntPtr hObject);
    }
}
