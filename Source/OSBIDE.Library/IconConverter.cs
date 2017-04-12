using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library
{
    public class IconConverter : System.Windows.Forms.AxHost
    {
        private IconConverter()
            : base(string.Empty)
        {
        }

        public static stdole.IPictureDisp GetIPictureDispFromImage(System.Drawing.Image image)
        {

            return (stdole.IPictureDisp)GetIPictureDispFromPicture(image);
        }
    } 
}
