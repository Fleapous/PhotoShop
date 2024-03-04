using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace PhotoShop
{
    public delegate (byte, byte, byte) ConvLogicDelegate(double r, double g, double b);
    public class ConvolutionalFilter : IFilter
    {
        public int[,]? Kernel { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public double Sum { get; set; }

        private readonly ConvLogicDelegate transformation;

        public ConvolutionalFilter(int[,] kernel, ConvLogicDelegate transformation, double sum = 1, int xOffset = 0, int yOffset = 0)
        {
            this.Kernel = kernel;
            this.transformation = transformation;
            this.XOffset = xOffset;
            this.YOffset = yOffset;
            this.Sum = sum;
        }

        public static int[,] MakeBlurKernel(int height, int width, out double sum)
        {
            int[,] kernel = new int[height, width];
            sum = 0.0;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    kernel[i, j] = 1;
                    sum++;
                }
            }
            return kernel;
        }
        public static int[,] MakeGaussKernel(int radius, double sigma, out double sumO)
        {
            double[,] kernel = new double[2 * radius + 1, 2 * radius + 1];
            double sum = 0.0;
            sumO = 0.0;

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    double exponentNumerator = -(x * x + y * y);
                    double exponentDenominator = 2 * sigma * sigma;

                    double eExpression = Math.Pow(Math.E, exponentNumerator / exponentDenominator);
                    double kernelValue = eExpression / (2 * Math.PI * sigma * sigma) * 1000;

                    kernel[x + radius, y + radius] = kernelValue;
                    sum += kernelValue;
                }
            }

            int[,] integerKernel = new int[2 * radius + 1, 2 * radius + 1];
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    kernel[x + radius, y + radius] *= 1000;
                    kernel[x + radius, y + radius] /= sum;
                    kernel[x + radius, y + radius] = Math.Round(kernel[x + radius, y + radius]);
                    integerKernel[x + radius, y + radius] = (int)kernel[x + radius, y + radius];
                    sumO += integerKernel[x + radius, y + radius];
                }
            }

            return integerKernel;
        }
        public static int[,] MakeSharpnessKernel(int height, int width)
        {
            int[,] kernel = new int[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    kernel[i, j] = -1;
                }
            }
            kernel[height / 2, width / 2] = (width * height);

            return kernel;
        }
        public static int[,] MakeEdgeDetectionKernel(int height, int width)
        {
            int[,] kernel = new int[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    kernel[i, j] = -1;
                }
            }
            kernel[height / 2, width / 2] = (width * height) - 1;

            return kernel;
        }
        public static int[,] MakeEmbossKernel(int height, int width)
        {
            int[,] kernel = new int[height, width];

            for (int j = 0; j < width; j++)
            {
                kernel[0, j] = -1;
                kernel[height - 1, j] = 1;
            }

            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    kernel[i, j] = 0;
                }
            }

            kernel[height / 2, width / 2] = 1;

            return kernel;
        }

        public WriteableBitmap Apply(WriteableBitmap bitmapIn)
        {
            int width = (int)bitmapIn.Width;
            int height = (int)bitmapIn.Height;
            int kernelWidth = Kernel.GetLength(1);
            int kernelHeight = Kernel.GetLength(0);

            WriteableBitmap paddedBitmap = AddPadding(bitmapIn, kernelWidth / 2);

            WriteableBitmap result = new WriteableBitmap(width, height, bitmapIn.DpiX, bitmapIn.DpiY, bitmapIn.Format, bitmapIn.Palette);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //check for offset
                    if (x < XOffset || y < YOffset)
                    {
                        (byte r, byte g, byte b) = GetPixel(bitmapIn, x, y);
                        SetPixel(result, x, y, r, g, b);
                    }
                    else
                    {
                        double red = 0, green = 0, blue = 0;
                        for (int j = 0; j < kernelHeight; j++)
                        {
                            for (int i = 0; i < kernelWidth; i++)
                            {
                                (byte r, byte g, byte b) = GetPixel(paddedBitmap, x + i, y + j);
                                red += (int)r * Kernel[j, i];
                                green += (int)g * Kernel[j, i];
                                blue += (int)b * Kernel[j, i];
                            }
                        }
                        (byte newR, byte newG, byte newB) = transformation(red, green, blue);
                        SetPixel(result, x, y, newR, newG, newB);
                    }

                }
            }
            return result;
        }


        private (byte, byte, byte) GetPixel(WriteableBitmap bitmap, int x, int y)
        {
            //int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            int bytesPerPixel = 4;
            int stride = bitmap.PixelWidth * bytesPerPixel;
            byte[] pixel = new byte[bytesPerPixel];
            bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, stride, 0);
            return (pixel[2], pixel[1], pixel[0]);
        }

        private void SetPixel(WriteableBitmap bitmap, int x, int y, byte r, byte g, byte b)
        {
            //int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            int bytesPerPixel = 4;
            int stride = bitmap.PixelWidth * bytesPerPixel;
            byte[] pixel = { b, g, r, 255};
            bitmap.WritePixels(new Int32Rect(x, y, 1, 1), pixel, stride, 0);
        }

        private WriteableBitmap AddPadding(WriteableBitmap bitmap, int paddingSize)
        {
            int width = (int)bitmap.Width;
            int height = (int)bitmap.Height;
            int paddedWidth = width + 2 * paddingSize;
            int paddedHeight = height + 2 * paddingSize;

            WriteableBitmap paddedBitmap = new WriteableBitmap(paddedWidth, paddedHeight, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette);

            // Copy original image to padded image
            Int32Rect sourceRect = new Int32Rect(0, 0, width, height);
            Int32Rect destRect = new Int32Rect(paddingSize, paddingSize, width, height);
            paddedBitmap.WritePixels(destRect, bitmap.BackBuffer, bitmap.BackBufferStride * height, bitmap.BackBufferStride);

            //Top & Bottom
            for(int x = 0; x < width; ++x)
            {
                (byte rT, byte gT, byte bT) = GetPixel(bitmap, x, 0);
                (byte rB, byte gB, byte bB) = GetPixel(bitmap, x, height - 1);
                for (int i = 0; i < paddingSize; i++)
                {
                    SetPixel(paddedBitmap, x, i, rT, gT, bT);//setting Top
                    SetPixel(paddedBitmap, x, paddedHeight - i - 1, rB, gB, bB); //setting Bot
                }

            }

            //Left & Right
            for(int y = 0; y < paddedHeight; y++)
            {
                (byte rL, byte gL, byte bL) = GetPixel(paddedBitmap, 0, y);
                (byte rR, byte gR, byte bR) = GetPixel(paddedBitmap, paddedWidth - (paddingSize * 2) - 1, y);
                for (int i = 0; i < paddingSize; i++)
                {
                    SetPixel(paddedBitmap, i, y, rL, gL, bL);//setting Left
                    SetPixel(paddedBitmap, paddedWidth - paddingSize - 1 + i, y, rR, gR, bR); //setting Right
                }
            }
            return paddedBitmap;
        }
    }
}
