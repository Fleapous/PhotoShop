using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace PhotoShop
{
    internal class FunctionFilters
    {
        private delegate byte FilterLogic(byte channel);

        private static byte InversionLogic(byte pixel)
        {
            return (byte)(255 - pixel);
        }
        public static WriteableBitmap Inversion(WriteableBitmap bitmapIn)
        {
            return ApplyLogic(bitmapIn, InversionLogic);
        }

        private static byte BrightnessLogic(byte pixel)
        {
            return (byte)Math.Clamp(pixel + 30, 0, 255);
        }
        public static WriteableBitmap Brightness(WriteableBitmap bitmapIn)
        {
            return ApplyLogic(bitmapIn, BrightnessLogic);
        }

        private static byte ContrastLogic(byte pixel)
        {
            return (byte)Math.Clamp(pixel * 3, 0, 255);
        }
        public static WriteableBitmap Contrast(WriteableBitmap bitmapIn)
        {
            return ApplyLogic(bitmapIn, ContrastLogic);
        }

        private static byte GammaLogic(byte pixel)
        {
            double gamma = 1.5;
            double normalizedpx = pixel / 255.0;
            double gammaCorrectedPx = Math.Pow(normalizedpx, gamma) * 255.0;
            return (byte)Math.Clamp(gammaCorrectedPx, 0, 255);
        }
        public static WriteableBitmap Gamma(WriteableBitmap bitmapIn)
        {
            return ApplyLogic(bitmapIn, GammaLogic);
        }

        private static WriteableBitmap ApplyLogic(WriteableBitmap BitmapIn, FilterLogic filterLogic)
        {
            int pxWidth = (int)BitmapIn.Width;
            int pxHeight = (int)BitmapIn.Height;

            int bytesPerPixel = (BitmapIn.Format.BitsPerPixel + 7) / 8;
            int stride = (int)(pxWidth * bytesPerPixel);

            Byte[] pixels = new Byte[stride * pxHeight];
            BitmapIn.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += bytesPerPixel)
            {
                pixels[i] = filterLogic(pixels[i]);
                pixels[i + 1] = filterLogic(pixels[i + 1]);
                pixels[i + 2] = filterLogic(pixels[i + 2]);
            }
            BitmapIn.WritePixels(new Int32Rect(0, 0, pxWidth, pxHeight), pixels, stride, 0);

            return BitmapIn;
        }

        public static WriteableBitmap ApplyFilters(List<Func<WriteableBitmap, WriteableBitmap>> filters, WriteableBitmap originalImg)
        {
            WriteableBitmap proccessedImg = new WriteableBitmap(originalImg);
            foreach (var filter in filters)
            {
                proccessedImg = filter(proccessedImg);
            }
            return proccessedImg;
        }
    }
}
