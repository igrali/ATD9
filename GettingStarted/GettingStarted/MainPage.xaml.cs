using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GettingStarted.Resources;
using Microsoft.Phone.Tasks;

using Nokia.Graphics.Imaging;
using System.Windows.Media.Imaging;

namespace GettingStarted
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var photoChooser = new PhotoChooserTask();
            photoChooser.Completed += photoChooser_Completed;
            photoChooser.ShowCamera = true;
            photoChooser.Show();
        }

        async void photoChooser_Completed(object sender, PhotoResult e)
        {
            var resultStream = e.ChosenPhoto;

            //setting original image source to chosen image
            BitmapImage bmp = new BitmapImage();
            bmp.SetSource(resultStream);
            OriginalImage.Source = bmp;

            resultStream.Position = 0;
            IImageProvider imageSource = new StreamImageSource(resultStream);

            FilterEffect filterEffects = new FilterEffect
            {
                Filters = new IFilter[] 
                {
                    new WarpFilter(WarpEffect.Alien, 0.5),
                    new RotationFilter(35)
                    //new CropFilter(new Windows.Foundation.Rect(400,400,500,500))
                },
                Source = imageSource
            };

            //WriteableBitmap writeableBitmapResult = new WriteableBitmap(bmp.PixelWidth, bmp.PixelHeight);
            WriteableBitmap writeableBitmapSmallResult = new WriteableBitmap((int)ResultImage.Width, (int)ResultImage.Height);

            WriteableBitmapRenderer renderer =
                new WriteableBitmapRenderer(filterEffects, writeableBitmapSmallResult, OutputOption.Stretch);

            writeableBitmapSmallResult = await renderer.RenderAsync();

            ResultImage.Source = writeableBitmapSmallResult;

            resultStream.Dispose();
        }
    }
}