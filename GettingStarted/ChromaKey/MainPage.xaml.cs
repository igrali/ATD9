using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ChromaKey.Resources;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Nokia.Graphics.Imaging;
using Windows.UI;
using System.IO;

namespace ChromaKey
{
    public partial class MainPage : PhoneApplicationPage
    {
        WriteableBitmap renderBitmap = null;
        WriteableBitmap originalBitmap = null;
        WriteableBitmap backgroundBitmap = null;

        FilterEffect filterEffects = null;
        WriteableBitmapRenderer renderer = null;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            var filters = new List<IFilter> 
                {
                    new ChromaKeyFilter(new Color()
                    {
                        R=144,
                        G=153,
                        B=90
                    },
                    0.12,
                    0.5,
                    false)
                };

            filterEffects = new FilterEffect
            {
                Filters = filters
            };

            renderBitmap = new WriteableBitmap((int)(ResultImage.Width), (int)(ResultImage.Height));
            originalBitmap = new WriteableBitmap((int)(OriginalImage.Width), (int)(OriginalImage.Height));
            backgroundBitmap = new WriteableBitmap((int)(BackgroundImage.Width), (int)(BackgroundImage.Height));

            ResultImage.Source = renderBitmap;
            renderer = new WriteableBitmapRenderer(filterEffects, renderBitmap, OutputOption.Stretch);
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
            using (Stream resultStream = e.ChosenPhoto)
            {
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(resultStream);
                OriginalImage.Source = bmp;

                resultStream.Position = 0;
                IImageProvider imageSource = new StreamImageSource(resultStream);

                filterEffects.Source = imageSource;

                await renderer.RenderAsync();
                renderBitmap.Invalidate();
            }      
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var photoChooser = new PhotoChooserTask();
            photoChooser.Completed += photoChooser_Completed_background;
            photoChooser.ShowCamera = true;
            photoChooser.Show();
        }

        async void photoChooser_Completed_background(object sender, PhotoResult e)
        {
            using (Stream resultStream = e.ChosenPhoto)
            {
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(resultStream);
                BackgroundImage.Source = bmp;

                resultStream.Position = 0;
                IImageProvider imageSource = new StreamImageSource(resultStream);

                var fe = new FilterEffect(imageSource);
                var filters = new List<IFilter>();
                filters.Add(new BlendFilter(filterEffects, BlendFunction.Normal));
                fe.Filters = filters;

                var renderer2 = new WriteableBitmapRenderer(fe, renderBitmap, OutputOption.Stretch);

                await renderer2.RenderAsync();
                renderBitmap.Invalidate();
            } 
        }

    }
}