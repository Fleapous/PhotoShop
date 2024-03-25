using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

        private int[,] ConstructKernelFromType(string type, int x, int y, out double sum, double sigma = 0)
        {
            int[,] kernel = new int[x,y];
            sum = 1;
            switch (type)
            {
                case "Blur Filter":
                    sum = 0.0;
                    kernel = ConvolutionalFilter.MakeBlurKernel(x, y, out sum);
                    break;
                case "Gaussian Blur Filter":
                    sum = 0.0;
                    kernel = ConvolutionalFilter.MakeGaussKernel(x/2, sigma, out sum);
                    break;
                case "Sharpness Filter":
                    kernel = ConvolutionalFilter.MakeSharpnessKernel(x, y);
                    break;
                case "Edge detect Filter":
                    kernel = ConvolutionalFilter.MakeEdgeDetectionKernel(x, y);
                    break;
                case "Emboss Filter":
                    kernel = ConvolutionalFilter.MakeEmbossKernel(x, y);
                    break;
                default:
                    kernel = ConvolutionalFilter.MakeBlurKernel(x, y, out sum);
                    break;
            }
            return kernel;
        }

        private FunctionFilter ConstructFunctionFilterFromType(string type, double value, int xOffset, int yOffset)
        {
            switch (type)
            {
                case "Inversion Filter":
                    return new FunctionFilter(pixel => (Byte)(255 - pixel), xOffset, yOffset);
                case "Brightness Filter":
                    return new FunctionFilter(pixel => (byte)Math.Clamp(pixel + value, 0, 255), xOffset, yOffset);
                case "Contrast Filter":
                    return new FunctionFilter(pixel => (byte)Math.Clamp(pixel * value, 0, 255), xOffset, yOffset);
                case "Gamma Filter":
                    return new FunctionFilter(pixel => (byte)Math.Clamp(Math.Pow(pixel / 255.0, value) * 255.0, 0, 255), xOffset, yOffset);
                default:
                    return new FunctionFilter(pixel => (Byte)(255 - pixel), xOffset, yOffset);
            }
        }

        public void SerializeFilter(IFilter filter, string name, double value = 0)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                object serializableData;

                if (filter is ConvolutionalFilter convolutionalFilter)
                {
 

                    serializableData = new
                    {
                        type = "ConvolutionalFilter",
                        name = name,
                        convolutionalFilter.XOffset,
                        convolutionalFilter.YOffset,
                        sum = convolutionalFilter.Sum,
                        Kernel = SerializeKernel(convolutionalFilter.Kernel),
                        offset = convolutionalFilter.Offset,
                    };
                }
                else if (filter is FunctionFilter functionFilter)
                {
                    serializableData = new
                    {
                        type = "FunctionFilter",
                        name = name,
                        functionFilter.XOffset,
                        functionFilter.YOffset,
                        value = value
                    };
                }
                else if (filter is OctTreeFilter octTreeFilter) 
                {
                    serializableData = new
                    {
                        type = "Octree",
                        name = name
                    };
                }
                else if (filter is Dithering ditheringFilter)
                {
                    serializableData = new
                    { 
                        type = "Dithering",
                        name = name
                    };

                }
                else if(filter is GrayScale grayScale)
                {
                    serializableData = new
                    {
                        type = "GrayScale",
                        name = name
                    };
                }
                else
                {
                    throw new NotImplementedException();
                }

                string jsonString = JsonSerializer.Serialize(serializableData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(saveFileDialog.FileName, jsonString);
            }
        }

        public void DeSerializeFilterFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() ==  true)
            {
                string jsonString = File.ReadAllText(openFileDialog.FileName);
                DeserializeFilter(jsonString);
            }
            else
            {
                throw new InvalidOperationException("File selection canceled by the user.");
            }
        }

        public void DeserializeFilter(string jsonString)
        {
            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                JsonElement root = document.RootElement;

                string filterType = root.GetProperty("type").GetString();

                switch (filterType)
                {
                    case "ConvolutionalFilter":
                        double sum = root.GetProperty("sum").GetDouble();

                        int[,] kernel = DeSerializeKernel(root.GetProperty("Kernel"));
                        ConvolutionalFilter convFilter = new ConvolutionalFilter(
                            kernel,
                            sum,
                            root.GetProperty("XOffset").GetInt32(), 
                            root.GetProperty("YOffset").GetInt32(),
                            root.GetProperty("offset").GetInt32());

                        filtersToApply.Add(convFilter);
                        filterStacks.Add(new Stack(root.GetProperty("name").GetString()));
                        break;

                    case "FunctionFilter":
                        double value = root.GetProperty("value").GetDouble();
                        string functionFilterType = root.GetProperty("name").GetString();
                        int xOffset = root.GetProperty("XOffset").GetInt32();
                        int yOffset = root.GetProperty("YOffset").GetInt32();

                        FunctionFilter funcFilter = ConstructFunctionFilterFromType(functionFilterType, value, xOffset, yOffset);
                        filtersToApply.Add(funcFilter);
                        filterStacks.Add(new Stack(root.GetProperty("name").GetString()));
                        break;

                    case "Octree":
                        OctTreeFilter filter = new OctTreeFilter();
                        filtersToApply.Add(filter);
                        filterStacks.Add(new Stack(root.GetProperty("name").GetString()));
                        break;
                    case "Dithering":
                        Dithering dithering = new Dithering();
                        filtersToApply.Add(dithering);
                        filterStacks.Add(new Stack(root.GetProperty("name").GetString()));
                        break;
                    case "GrayScale":
                        GrayScale grayScale = new GrayScale();
                        filtersToApply.Add(grayScale);
                        filterStacks.Add(new Stack(root.GetProperty("name").GetString()));
                        break;
                    default:
                        throw new ArgumentException("Unknown filter type");
                }
            }
        }

        public int[][] SerializeKernel(int[,] array)
        {
            int row = array.GetLength(0);
            int cols = array.GetLength(1);

            int[][] serializedArray = new int[row][];

            for (int i = 0; i < row; i++)
            {
                serializedArray[i] = new int[cols];
                for (int j = 0; j < cols; j++)
                {
                    serializedArray[i][j] = array[i, j];
                }
            }

            return serializedArray;
        }
        private int[,] DeSerializeKernel(JsonElement kernelElement)
        {
            JsonElement kernelArray = kernelElement;

            int rowCount = kernelArray.GetArrayLength();
            int columnCount = kernelArray[0].GetArrayLength();

            int[,] kernel = new int[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    kernel[i, j] = kernelArray[i][j].GetInt32();
                }
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
