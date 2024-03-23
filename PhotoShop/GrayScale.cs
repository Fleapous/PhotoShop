using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PhotoShop
{
    internal class GrayScale : IFilter
    {
        public int[,]? Kernel { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }

        public WriteableBitmap Apply(WriteableBitmap In)
        {
            int pxWidth = (int)In.Width;
            int pxHeight = (int)In.Height;

            int bytesPerPixel = (In.Format.BitsPerPixel + 7) / 8;
            int stride = (int)(pxWidth * bytesPerPixel);

            Byte[] pixels = new Byte[stride * pxHeight];
            In.CopyPixels(pixels, stride, 0);

            for (int y = 0; y < pxHeight; y++)
            {
                for (int x = 0; x < pxWidth; x++)
                {
                    int index = y * stride + x * bytesPerPixel;
                    byte gray = (byte)((pixels[index + 2] * 0.3) + (pixels[index + 1] * 0.59) + (pixels[index] * 0.11));
                    pixels[index] = gray;
                    pixels[index + 1] = gray;
                    pixels[index + 2] = gray;
                }
            }

            In.WritePixels(new Int32Rect(0, 0, pxWidth, pxHeight), pixels, stride, 0);
            return In;
        }
    }
}
