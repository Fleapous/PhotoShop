using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoShop
{
    public interface IFilter
    {
        WriteableBitmap Apply(WriteableBitmap In);
        int[,]? Kernel { get; set; }
    }
}
