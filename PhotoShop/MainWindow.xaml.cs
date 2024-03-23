using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
            XOffsetTextBox.Text = filtersToApply[currentFilterIndex].XOffset.ToString();
            YOffsetTextBox.Text = filtersToApply[currentFilterIndex].YOffset.ToString();


            if (filtersToApply[currentFilterIndex].Kernel == null)
            {
                //hide conv ettings
                ConvolutionalSettingsPanel.Visibility = Visibility.Collapsed;
                FunctionalSettingsPanel.Visibility = Visibility.Visible;

                KernelGrid.Children.Clear();
                KernelGrid.RowDefinitions.Clear();
                KernelGrid.ColumnDefinitions.Clear();
                
            }
            else
            {
                //hide functunal settings
                ConvolutionalSettingsPanel.Visibility = Visibility.Visible;
                FunctionalSettingsPanel.Visibility = Visibility.Collapsed;

                //populate x y values
                int xValue = filtersToApply[currentFilterIndex].Kernel.GetLength(0);
                int yValue = filtersToApply[currentFilterIndex].Kernel.GetLength(1);
                string kernelDims = $"{xValue}, {yValue}";

                KernelDims.Text = kernelDims;

                //load the kernel
                ConstructKernelGrid(filtersToApply[currentFilterIndex]);
                if(filtersToApply[currentFilterIndex] is ConvolutionalFilter filter)
                {
                    DevisorField.Text = filter.Sum.ToString();
                    OffsetField.Text = filter.Offset.ToString();
                }
                
            }
                
        }

        private void FilterSetting_LostFocus(object sender, RoutedEventArgs e)
        {
            double value;
            if (!double.TryParse(FilterSetting.Text, out value))
                MessageBox.Show("Invalid input for filter function value. Please enter a valid number.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            int xOffset = 0;
            int yOffset = 0;
            if(!int.TryParse(XOffsetTextBox.Text, out xOffset) || !int.TryParse(YOffsetTextBox.Text, out yOffset))
                MessageBox.Show("Invalid input for offset value. Please enter a valid number.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            filtersToApply[currentFilterIndex] = ConstructFunctionFilterFromType(filterStacks[currentFilterIndex].Name, value, xOffset, yOffset);
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
