using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PhotoShop
{
    internal class Dithering : IFilter

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

            (double tresholdR, double tresholdG, double tresholdB) = GetTresholds(pixels, pxHeight, pxWidth, stride, bytesPerPixel);

            for (int y = 0; y < pxHeight; y++)
            {
                for (int x = 0; x < pxWidth; x++)
                {
                    int index = y * stride + x * bytesPerPixel;
                    if (x >= XOffset && y >= YOffset)
                    {
                        double doublePixelR = Convert.ToDouble(pixels[index]);
                        double doublePixelG = Convert.ToDouble(pixels[index + 1]);
                        double doublePixelB = Convert.ToDouble(pixels[index + 2]);

                        if (doublePixelR > tresholdR)
                            pixels[index] = 255;
                        else
                            pixels[index] = 0;

                        if (doublePixelG > tresholdG)
                            pixels[index + 1] = 255;
                        else
                            pixels[index + 1] = 0;

                        if (doublePixelB > tresholdB)
                            pixels[index + 2] = 255;
                        else
                            pixels[index + 2] = 0;
                    }
                }
            }

            In.WritePixels(new Int32Rect(0, 0, pxWidth, pxHeight), pixels, stride, 0);
            return In;
        }

        private (double tresholdR, double tresholdG, double tresholdB) GetTresholds(Byte[] pixels, int pxHeight, int pxWidth, int stride, int bytesPerPixel)
        {
            double MaxR = 0;
            double MaxG = 0;
            double MaxB = 0;

            double MinR = double.MaxValue;
            double MinG = double.MaxValue;
            double MinB = double.MaxValue;

            for (int y = 0; y < pxHeight; y++)
            {
                for (int x = 0; x < pxWidth; x++)
                {
                    int index = y * stride + x * bytesPerPixel;

                    double doublePixelR = Convert.ToDouble(pixels[index]);
                    double doublePixelG = Convert.ToDouble(pixels[index + 1]);
                    double doublePixelB = Convert.ToDouble(pixels[index + 2]);

                    if (doublePixelR > MaxR)
                        MaxR = doublePixelR;
                    else if (doublePixelR < MinR)
                        MinR = doublePixelR;

                    if (doublePixelG > MaxG)
                        MaxG = doublePixelG;
                    else if (doublePixelG < MinG)
                        MinG = doublePixelG;

                    if (doublePixelB > MaxB)
                        MaxB = doublePixelB;
                    else if (doublePixelB < MinB)
                        MinB = doublePixelB;


                }
            }

            return ((MaxR + MinR) / 2, (MaxG + MinG) / 2, (MaxB + MinB) / 2);
        }
    }
}
