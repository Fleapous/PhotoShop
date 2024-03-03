using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PhotoShop
{
    public partial class MainWindow : Window
    {
        private void ConstructKernelGrid(IFilter item)
        {

            //clear the grid 
            KernelGrid.Children.Clear();
            KernelGrid.RowDefinitions.Clear();
            KernelGrid.ColumnDefinitions.Clear();

            int rows = item.Kernel.GetLength(0);
            int cols = item.Kernel.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                KernelGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < cols; j++)
            {
                KernelGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int rowIndex = i;
                    int colIndex = j;

                    TextBox textBox = new TextBox();
                    textBox.Text = item.Kernel[i, j].ToString();

                    //adding action for text cahnge
                    textBox.TextChanged += (s, args) =>
                    {
                        int value;
                        if (int.TryParse(textBox.Text, out value))
                        {
                            item.Kernel[rowIndex, colIndex] = value;
                        }
                    };
                    Grid.SetRow(textBox, i);
                    Grid.SetColumn(textBox, j);
                    KernelGrid.Children.Add(textBox);
                }
            }
        }

        private int[,] ConstructKernelFromType(string type, int x, int y, double sigma = 0)
        {
            int[,] kernel = new int[x,y];
            double sum = 0.0;
            switch (type)
            {
                case "Blur Filter":
                    kernel = ConvolutionalFilter.MakeBlurKernel(x, y, out sum);
                    break;
                case "Gaussian Blur Filter":
                    kernel = ConvolutionalFilter.MakeGaussKernel(x, sigma, out sum);
                    break;
                case "Sharpness Filter":
                    kernel = ConvolutionalFilter.MakeSharpnessKernel(x, y);
                    break;
                case "Edge detect Filter":
                    kernel = ConvolutionalFilter.MakeEdgeDetectionKernel(x, y);
                    break;
                default:
                    kernel = ConvolutionalFilter.MakeBlurKernel(x, y, out sum);
                    break;
            }
            return kernel;
        }


        private bool IsOdd(int number)
        {
            return number % 2 != 0;
        }
        private bool IsWithinRange(int number)
        {
            return number >= 1 && number <= 9;
        }
    }
}
