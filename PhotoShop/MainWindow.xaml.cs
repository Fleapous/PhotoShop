using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoShop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private WriteableBitmap? originalBitmap { get; set; }
        private List<IFilter> filtersToApply = new List<IFilter>();
        public ObservableCollection<Stack> filterStacks { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            filterStacks = new ObservableCollection<Stack>();
            DataContext = this;
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Image Files (*.png;*.bmp;*.jpg)|*.png;*.bmp;*.jpg";
            if (dialog.ShowDialog() == true) //img sellected
            {
                string imgPath = dialog.FileName;
                //rendering the img
                var tmpBitmap = new BitmapImage(new Uri(imgPath));
                originalBitmap = new WriteableBitmap(tmpBitmap);
                firstWindowImage.Source = originalBitmap;
            }
        }

        private void InversionButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            filterStacks.Add(new Stack("Inversion Filter"));
            filtersToApply.Add(new FunctionFilter(pixel => (Byte)(255 - pixel)));
                
            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void BrightnessButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            filterStacks.Add(new Stack("Brightness Filter"));
            filtersToApply.Add(new FunctionFilter(pixel => (byte)Math.Clamp(pixel + 30, 0, 255)));
            

            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void ContrastButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            filterStacks.Add(new Stack("Contrast Filter"));
            filtersToApply.Add(new FunctionFilter(pixel => (byte)Math.Clamp(pixel * 3, 0, 255)));
            

            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void GammaButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            filterStacks.Add(new Stack("Gamma Filter"));
            filtersToApply.Add(new FunctionFilter(pixel => (byte)Math.Clamp(Math.Pow(pixel / 255.0, 2) * 255.0, 0, 255)));


            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var id = (Guid)button.Tag;
            var item = filterStacks.FirstOrDefault(f => f.UniqueIdentifier == id);
            if (item == null)
                return;
            int index = filterStacks.IndexOf(item);
            if(index < filtersToApply.Count && originalBitmap != null)
            {
                filtersToApply.RemoveAt(index);
                filterStacks.RemoveAt(index);
                secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
            }
        }

        private void BlurButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            filterStacks.Add(new Stack("Blur Filter"));
            int[,] blurKernel = {
                { 1, 1, 1 },
                { 1, 1, 1 },
                { 1, 1, 1 }
            };

            ConvLogicDelegate blurTransformation = (r, g, b) => {
                double newR = Math.Clamp((int)r / 9, 0, 255);
                double newG = Math.Clamp((int)g / 9, 0, 255);
                double newB = Math.Clamp((int)b / 9, 0, 255);
                return ((byte)newR, (byte)newG, (byte)newB);
            };

            filtersToApply.Add(new ConvolutionalFilter(blurKernel, blurTransformation));

            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void GaussianBlurButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            filterStacks.Add(new Stack("Gaussian Blur Filter"));
            int[,] gaussBlurKernel = new int[3, 3];
            double sum;
            gaussBlurKernel = ConvolutionalFilter.MakeGaussKernel(1,1.5, out sum);

            ConvLogicDelegate gaussBlurTransformation = (r, g, b) => {
                double newR = Math.Clamp((int)r / sum, 0, 255);
                double newG = Math.Clamp((int)g / sum, 0, 255);
                double newB = Math.Clamp((int)b / sum, 0, 255);
                return ((byte)newR, (byte)newG, (byte)newB);
            };

            filtersToApply.Add(new ConvolutionalFilter(gaussBlurKernel, gaussBlurTransformation));

            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void SharpnessButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            filterStacks.Add(new Stack("Sharpness Filter"));
            int[,] SharpnessKernel = new int[3, 3];
            SharpnessKernel = ConvolutionalFilter.MakeSharpnessKernel(3, 3);

            ConvLogicDelegate SharpnessTranformation = (r, g, b) => {
                double newR = Math.Clamp((int)r, 0, 255);
                double newG = Math.Clamp((int)g, 0, 255);
                double newB = Math.Clamp((int)b, 0, 255);
                return ((byte)newR, (byte)newG, (byte)newB);
            };

            filtersToApply.Add(new ConvolutionalFilter(SharpnessKernel, SharpnessTranformation));

            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void EdgeDetectButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            filterStacks.Add(new Stack("Edge detect Filter"));
            int[,] EdgeDetectKernel = new int[3, 3];
            EdgeDetectKernel = ConvolutionalFilter.MakeEdgeDetectionKernel(3, 3);

            ConvLogicDelegate EdgeDetectTranformation = (r, g, b) => {
                double newR = Math.Clamp((int)r, 0, 255);
                double newG = Math.Clamp((int)g, 0, 255);
                double newB = Math.Clamp((int)b, 0, 255);
                return ((byte)newR, (byte)newG, (byte)newB);
            };

            filtersToApply.Add(new ConvolutionalFilter(EdgeDetectKernel, EdgeDetectTranformation));

            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }
    }

    public class Stack
    {
        public string Name { get; set; }
        public Guid UniqueIdentifier { get; set; }

        public Stack(string name)
        {
            Name = name;
            UniqueIdentifier = Guid.NewGuid();
        }
    }
}
