using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PhotoShop
{
    public partial class MainWindow : Window
    {
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
                firstWindowImage.Source = tmpBitmap;
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
            if (index < filtersToApply.Count && originalBitmap != null)
            {
                filtersToApply.RemoveAt(index);
                filterStacks.RemoveAt(index);
                secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
            }
        }

        private void GenerateButtonClick(object sender, RoutedEventArgs e)
        {
            if (originalBitmap != null)
                secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }

        private void GenerateNewKernel_Click(object sender, RoutedEventArgs e)
        {
            //kernel parameters 
            string[] parts = KernelDims.Text.Split(',');



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
                    double sum = 1;
                    double sigma;
                    if (!double.TryParse(sigmaValue.Text, out sigma))
                        MessageBox.Show("Invalid input for sigma. Please enter a valid number.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                    int xOffset = 0;
                    int yOffset = 0;
                    if (!int.TryParse(XOffsetTextBox.Text, out xOffset) || !int.TryParse(YOffsetTextBox.Text, out yOffset))
                        MessageBox.Show("Invalid input for offset value. Please enter a valid number.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                    newKernel = ConstructKernelFromType(filterType, xValue, yValue, out sum, sigma);

                    //Get offset value
                    int Offset;
                    if (!int.TryParse(OffsetField.Text, out Offset))
                        MessageBox.Show("Invalid input for Offset value. Please enter a valid number.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                    int devisor;
                    if (!int.TryParse(DevisorField.Text, out devisor))
                        MessageBox.Show("Invalid input for devisor value. Please enter a valid number.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                    //assighn new settigns to the edited filter
                    filtersToApply[currentFilterIndex] = new ConvolutionalFilter(newKernel, devisor, xOffset, yOffset, Offset);

                    //re construct kernel grid from new kernel settings 
                    ConstructKernelGrid(filtersToApply[currentFilterIndex]);

                }
            }
        }

        private void SaveFilterClick(object sender, RoutedEventArgs e)
        {
            SerializeFilter(filtersToApply[currentFilterIndex], filterStacks[currentFilterIndex].Name);
        }

        private void OpenFIlterClick(object sender, RoutedEventArgs e)
        {
            DeSerializeFilterFromFile();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "Image Files (*.png;*.bmp;*.jpg)|*.png;*.bmp;*.jpg";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                JpegBitmapEncoder jpg = new JpegBitmapEncoder();
                jpg.Frames.Add(BitmapFrame.Create((BitmapSource)secondWindowImage.Source));
                using (Stream stm = File.Create(saveFileDialog.FileName))
                {
                    jpg.Save(stm);
                }
            }
            else
            {
                throw new InvalidOperationException("File saving canceled by the user.");
            }
        }


    }
}
