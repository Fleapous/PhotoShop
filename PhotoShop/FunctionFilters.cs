using System;
using System.Collections.Generic;
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
        public static WriteableBitmap Inversion(WriteableBitmap BitmapIn)
        {
            int width = BitmapIn.PixelWidth;
            int height = BitmapIn.PixelHeight;
            int stride = width * ((BitmapIn.Format.BitsPerPixel + 7) / 8);

            byte[] pixels = new byte[height * stride];
            BitmapIn.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte invertedR = (byte)(255 - pixels[i + 2]);
                byte invertedG = (byte)(255 - pixels[i + 1]);
                byte invertedB = (byte)(255 - pixels[i]);     

                pixels[i + 2] = invertedR;
                pixels[i + 1] = invertedG;
                pixels[i] = invertedB;
            }

            WriteableBitmap result = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            return result;
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
