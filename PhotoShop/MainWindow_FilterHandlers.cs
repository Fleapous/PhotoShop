﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PhotoShop
{
    public partial class MainWindow : Window
    {
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

            filtersToApply.Add(new ConvolutionalFilter(blurKernel, blurTransformation, 0, 0));

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
            gaussBlurKernel = ConvolutionalFilter.MakeGaussKernel(1, 1.5, out sum);

            ConvLogicDelegate gaussBlurTransformation = (r, g, b) => {
                double newR = Math.Clamp((int)r / sum, 0, 255);
                double newG = Math.Clamp((int)g / sum, 0, 255);
                double newB = Math.Clamp((int)b / sum, 0, 255);
                return ((byte)newR, (byte)newG, (byte)newB);
            };

            filtersToApply.Add(new ConvolutionalFilter(gaussBlurKernel, gaussBlurTransformation, 0, 0));

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

            filtersToApply.Add(new ConvolutionalFilter(SharpnessKernel, SharpnessTranformation, 0, 0));

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

            filtersToApply.Add(new ConvolutionalFilter(EdgeDetectKernel, EdgeDetectTranformation, 0, 0));

            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
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
            filtersToApply.Add(new FunctionFilter(pixel => (Byte)(255 - pixel), 0, 0));

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
            filtersToApply.Add(new FunctionFilter(pixel => (byte)Math.Clamp(pixel + 30, 0, 255), 0, 0));


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
            filtersToApply.Add(new FunctionFilter(pixel => (byte)Math.Clamp(pixel * 3, 0, 255), 0, 0));


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
            filtersToApply.Add(new FunctionFilter(pixel => (byte)Math.Clamp(Math.Pow(pixel / 255.0, 2) * 255.0, 0, 255), 0, 0));


            secondWindowImage.Source = FunctionFilters.ApplyFilters(filtersToApply, originalBitmap);
        }
    }
}