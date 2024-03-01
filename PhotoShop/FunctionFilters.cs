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
    //Utility class for managing Filter stack
    internal class FunctionFilters
    {
        public static WriteableBitmap ApplyFilters(List<IFilter> filters, WriteableBitmap originalImg)
        {
            WriteableBitmap proccessedImg = new WriteableBitmap(originalImg);
            foreach (var filter in filters)
            {
                proccessedImg = filter.Apply(proccessedImg);
            }
            return proccessedImg;
        }
    }
}
