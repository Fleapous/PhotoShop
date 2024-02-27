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
        private List<Func<WriteableBitmap, WriteableBitmap>> filtersToApply = new List<Func<WriteableBitmap, WriteableBitmap>>();
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
            filtersToApply.Add(FunctionFilters.Inversion);
                
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
            filtersToApply.Add(FunctionFilters.Brightness);
            

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
            filtersToApply.Add(FunctionFilters.Contrast);
            

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
            filtersToApply.Add(FunctionFilters.Gamma);
            
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
