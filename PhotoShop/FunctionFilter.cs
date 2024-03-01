using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace PhotoShop
{
    //class for function filters
    public delegate byte FilterLogic(byte logic);
    public class FunctionFilter : IFilter
    {
        private readonly FilterLogic filterLogic;

        public FunctionFilter(FilterLogic filterLogic)
        {
            this.filterLogic = filterLogic;
        }
        public WriteableBitmap Apply(WriteableBitmap In)
        {

            int pxWidth = (int)In.Width;
            int pxHeight = (int)In.Height;

            int bytesPerPixel = (In.Format.BitsPerPixel + 7) / 8;
            int stride = (int)(pxWidth * bytesPerPixel);

            Byte[] pixels = new Byte[stride * pxHeight];
            In.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += bytesPerPixel)
            {
                pixels[i] = filterLogic(pixels[i]);
                pixels[i + 1] = filterLogic(pixels[i + 1]);
                pixels[i + 2] = filterLogic(pixels[i + 2]);
            }
            In.WritePixels(new Int32Rect(0, 0, pxWidth, pxHeight), pixels, stride, 0);

            return In;

        }
    }
}
