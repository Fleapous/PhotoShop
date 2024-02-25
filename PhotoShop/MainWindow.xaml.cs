using System;
using System.Collections.Generic;
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
    public partial class MainWindow : Window
    {
        private WriteableBitmap? originalBitmap { get; set; }
        private List<Func<WriteableBitmap, WriteableBitmap>> filtersToApply = new List<Func<WriteableBitmap, WriteableBitmap>>();
        public MainWindow()
        {
            InitializeComponent();
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
                InversionToggle.IsChecked = false;
                return;
            }

            if(InversionToggle.IsChecked == true)
            {
                filtersToApply.Add(FunctionFilters.Inversion);
            }
            else if(InversionToggle.IsChecked == false)
            {
                filtersToApply.Remove(FunctionFilters.Inversion);
            }
                
            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void BrightnessButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BrightnessToggle.IsChecked = false;
                return;
            }

            if (BrightnessToggle.IsChecked == true)
            {
                filtersToApply.Add(FunctionFilters.Brightness);
            }
            else if (BrightnessToggle.IsChecked == false)
            {
                filtersToApply.Remove(FunctionFilters.Brightness);
            }

            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void ContrastButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Contrasttoggle.IsChecked = false;
                return;
            }

            if (Contrasttoggle.IsChecked == true)
            {
                filtersToApply.Add(FunctionFilters.Contrast);
            }
            else if (Contrasttoggle.IsChecked == false)
            {
                filtersToApply.Remove(FunctionFilters.Contrast);
            }

            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void GammaButtonClick(object sender, RoutedEventArgs e)
        {
            //checking if bitmap is opened
            if (originalBitmap == null)
            {
                MessageBox.Show("Please select an image before applying filters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                GammaToggle.IsChecked = false;
                return;
            }

            if (GammaToggle.IsChecked == true)
            {
                filtersToApply.Add(FunctionFilters.Gamma);
            }
            else if (GammaToggle.IsChecked == false)
            {
                filtersToApply.Remove(FunctionFilters.Ga);
            }

            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }
    }
}
