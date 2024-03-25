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
        public int k { get; set; }

        public Dithering(int k)
        {
            this.k = k;
        }

        public WriteableBitmap Apply(WriteableBitmap In)
        {
            int pxWidth = (int)In.Width;
            int pxHeight = (int)In.Height;

            int bytesPerPixel = (In.Format.BitsPerPixel + 7) / 8;
            int stride = (int)(pxWidth * bytesPerPixel);

            Byte[] pixels = new Byte[stride * pxHeight];
            In.CopyPixels(pixels, stride, 0);

            int range = 255 / (k - 1);
            pixels = CalculateDitheringColored(range, pixels, k, pxHeight, pxWidth, stride, bytesPerPixel);

            In.WritePixels(new Int32Rect(0, 0, pxWidth, pxHeight), pixels, stride, 0);
            return In;
        }

        private Byte[] CalculateDitheringColored(int range, Byte[] pixels, int k, int pxHeight, int pxWidth, int stride, int bytesPerPixel)
        {
            int[][] containersPerChannel = new int[3][];

            for (int i = 0; i < 3; i++)
            {
                containersPerChannel[i] = new int[k - 1];

                for (int j = 0; j < k - 1; j++)
                {
                    int sum = 0;
                    int count = 0;
                    for (int y = 0; y < pxHeight; y++)
                    {
                        for (int x = 0; x < pxWidth; x++)
                        {
                            int index = y * stride + x * bytesPerPixel + i; 
                            int colorVal = pixels[index];

                            if (colorVal >= j * range && colorVal < (j + 1) * range)
                            {
                                sum += colorVal;
                                count++;
                            }

                            if (colorVal == 255 && j == k - 2)
                            {
                                sum += colorVal;
                                count++;
                            }
                        }
                    }
                    containersPerChannel[i][j] = count == 0 ? 0 : sum / count;
                }
            }

            for (int y = 0; y < pxHeight; y++)
            {
                for (int x = 0; x < pxWidth; x++)
                {
                    int index = y * stride + x * bytesPerPixel;

                    int[] newColors = new int[3];
                    for (int channel = 0; channel < 3; channel++)
                    {
                        int colorVal = pixels[index + channel]; 

                        int rangeIndex = Math.Min(colorVal / range, k - 2);

                        int newColorVal;
                        if (colorVal <= containersPerChannel[channel][rangeIndex])
                        {
                            newColorVal = rangeIndex * range;
                        }
                        else
                        {
                            newColorVal = (rangeIndex + 1) * range;
                        }

                        newColors[channel] = newColorVal;
                    }

                    for (int channel = 0; channel < 3; channel++)
                    {
                        pixels[index + channel] = (byte)newColors[channel]; 
                    }
                }
            }

            return pixels;
        }

    }
}
