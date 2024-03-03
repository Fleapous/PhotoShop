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
    public delegate byte FilterLogicDelegate(byte logic);
    public class FunctionFilter : IFilter
    {
        public int[,]? Kernel { get; set; }
        private readonly FilterLogicDelegate filterLogic;
        public int XOffset {get; set;}
        public int YOffset { get; set;}

        public FunctionFilter(FilterLogicDelegate filterLogic, int xOffset = 0, int yOffset = 0)
        {
            Kernel = null;
            this.filterLogic = filterLogic;
            XOffset = xOffset;
            YOffset = yOffset;  
        }
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
                    if (x >= XOffset && y >= YOffset)
                    {
                        pixels[index] = filterLogic(pixels[index]);
                        pixels[index + 1] = filterLogic(pixels[index + 1]);
                        pixels[index + 2] = filterLogic(pixels[index + 2]);
                    }
                }
            }

            In.WritePixels(new Int32Rect(0, 0, pxWidth, pxHeight), pixels, stride, 0);
            return In;
        }
    }
}
