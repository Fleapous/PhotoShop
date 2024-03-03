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
        private int currentFilterIndex { get; set; }

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

        private void FilterStack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterStack.SelectedItem == null)
                return;

            FilterNameLabel.Content = ((Stack)FilterStack.SelectedItem).Name;

            //get the filter from list 
            var item = filterStacks.FirstOrDefault(f => f.UniqueIdentifier == ((Stack)FilterStack.SelectedItem).UniqueIdentifier);
            if (item == null)
                return;
            //find the index 
            currentFilterIndex = filterStacks.IndexOf(item);
            


            if (filtersToApply[currentFilterIndex].Kernel == null)
            {
                KernelGrid.Children.Clear();
                KernelGrid.RowDefinitions.Clear();
                KernelGrid.ColumnDefinitions.Clear();
            }
            else
            {
                //populate x y values
                int xValue = filtersToApply[currentFilterIndex].Kernel.GetLength(0);
                int yValue = filtersToApply[currentFilterIndex].Kernel.GetLength(1);
                string kernelDims = $"{xValue}, {yValue}";

                KernelDims.Text = kernelDims;

                //load the kernel
                ConstructKernelGrid(filtersToApply[currentFilterIndex]);
            }
                
        }

        private void GenerateButtonClick(object sender, RoutedEventArgs e)
        {
            if(originalBitmap != null) 
                secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void KernelDims_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string selectedString = textBox.Text;
            string[] parts = selectedString.Split(',');

            if (parts.Length != 2)
            {
                MessageBox.Show("Incorrect format. Please enter two integers separated by a comma.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (!int.TryParse(parts[0].Trim(), out int xValue) || !int.TryParse(parts[1].Trim(), out int yValue))
                {
                    MessageBox.Show("Incorrect format. Please enter two integers separated by a comma.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    // Check if both values are odd and within the range [1, 9]
                    if (!IsOdd(xValue) || !IsOdd(yValue) || !IsWithinRange(xValue) || !IsWithinRange(yValue))
                    {
                        MessageBox.Show("Values must be odd integers within the range [1, 9].", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    //create the new kerenel with new values
                    string filterType = filterStacks[currentFilterIndex].Name;
                    int[,] newKernel = new int[xValue, yValue];
                    newKernel = ConstructKernelFromType(filterType, xValue, yValue);
                    filtersToApply[currentFilterIndex].Kernel = newKernel;
                    ConstructKernelGrid(filtersToApply[currentFilterIndex]);

                }
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
