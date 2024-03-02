using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PhotoShop
{
    public delegate (byte, byte, byte) ConvLogicDelegate(double r, double g, double b);
    public class ConvolutionalFilter : IFilter
    {
        private readonly int[,] kernel;
        private readonly ConvLogicDelegate transformation;

        public ConvolutionalFilter(int[,] kernel, ConvLogicDelegate transformation)
        {
            this.kernel = kernel;
            this.transformation = transformation;
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
       
        public WriteableBitmap Apply(WriteableBitmap bitmapIn)
        {
            int width = (int)bitmapIn.Width;
            int height = (int)bitmapIn.Height;
            WriteableBitmap paddedBitmap = AddPadding(bitmapIn, 1);
            // Apply convolution operation
            WriteableBitmap result = new WriteableBitmap(width, height, bitmapIn.DpiX, bitmapIn.DpiY, bitmapIn.Format, bitmapIn.Palette);
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double red = 0, green = 0, blue = 0;
                    for (int j = -1; j <= 1; j++)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            (byte r, byte g, byte b) = GetPixel(paddedBitmap, x + i, y + j);
                            red += (int)r * kernel[j + 1, i + 1];
                            green += (int)g * kernel[j + 1, i + 1];
                            blue += (int)b * kernel[j + 1, i + 1];
                        }
                    }
                    (byte newR, byte newG, byte newB) = transformation(red, green, blue);
                    SetPixel(result, x, y, newR, newG, newB);
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
            return (pixel[2], pixel[1], pixel[0]); // RGB order
        }

        private void SetPixel(WriteableBitmap bitmap, int x, int y, byte r, byte g, byte b)
        {
            //int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            int bytesPerPixel = 4;
            int stride = bitmap.PixelWidth * bytesPerPixel;
            byte[] pixel = { b, g, r, 255}; // RGB order
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

            var widthT = paddedBitmap.PixelWidth;
            var heightT = paddedBitmap.PixelHeight;
            var strideT = widthT * ((paddedBitmap.Format.BitsPerPixel + 7) / 8);

            var bitmapData = new byte[heightT, strideT]; // 2D array for storing pixel data

            // Temporary array to hold pixel data for a single row
            var rowPixels = new byte[strideT];

            // Copy pixel data row by row
            for (int y = 0; y < heightT; y++)
            {
                // Copy one row of pixel data at a time
                paddedBitmap.CopyPixels(new Int32Rect(0, y, widthT, 1), rowPixels, strideT, 0);

                // Insert the row into the 2D array
                for (int x = 0; x < strideT; x++)
                {
                    bitmapData[y, x] = rowPixels[x];
                }
            }

            return paddedBitmap;
        }
    }
}
