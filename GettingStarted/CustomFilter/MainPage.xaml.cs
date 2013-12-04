using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CustomFilter.Resources;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Nokia.Graphics.Imaging;

namespace CustomFilter
{
    public partial class MainPage : PhoneApplicationPage
    {

        FilterEffect effects;
        List<IFilter> filters;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            filters = new List<IFilter> 
                {
                    new ContrastFilter(0.9),
                    new LomoFilter(),
                    new FlipFilter(FlipMode.Horizontal)
                };

            effects = new FilterEffect()
            {
                Filters = filters
            };
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

        IImageProvider imageSource;

        async void photoChooser_Completed(object sender, PhotoResult e)
        {
            var resultStream = e.ChosenPhoto;

            //setting original image source to chosen image
            BitmapImage bmp = new BitmapImage();
            bmp.SetSource(resultStream);
            OriginalImage.Source = bmp;

            resultStream.Position = 0;
            imageSource = new StreamImageSource(resultStream);
            
            using (WeirdBlue wBlue = new WeirdBlue(imageSource))
            {
                effects.Source = wBlue;
                WriteableBitmap writeableBitmapSmallResult = new WriteableBitmap((int)ResultImage.Width, (int)ResultImage.Height);
                WriteableBitmapRenderer renderer =
                new WriteableBitmapRenderer(effects, writeableBitmapSmallResult, OutputOption.Stretch);

                writeableBitmapSmallResult = await renderer.RenderAsync();

                ResultImage.Source = writeableBitmapSmallResult;
            }

            resultStream.Dispose();
        }

        private async void Button_Click2(object sender, RoutedEventArgs e)
        {
            if (filters.Count>0)
            {
                filters.RemoveAt(filters.Count - 1);

                using (WeirdBlue wBlue = new WeirdBlue(imageSource))
                {
                    effects.Filters = filters;
                    effects.Source = wBlue;
                    WriteableBitmap writeableBitmapSmallResult = new WriteableBitmap((int)ResultImage.Width, (int)ResultImage.Height);
                    WriteableBitmapRenderer renderer =
                    new WriteableBitmapRenderer(effects, writeableBitmapSmallResult, OutputOption.Stretch);

                    writeableBitmapSmallResult = await renderer.RenderAsync();

                    ResultImage.Source = writeableBitmapSmallResult;
                }
            }
        }
        private void ResultImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }
    }
}