using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DangerousUI.Resources;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Nokia.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.Info;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;

namespace DangerousUI
{
    public partial class MainPage : PhoneApplicationPage
    {
        enum State
        {
            Wait,
            Apply,
            Schedule
        };

        WriteableBitmap renderBitmap = null;
        WriteableBitmap tempBitmap = null;
        FilterEffect filterEffects = null;
        WriteableBitmapRenderer renderer = null;
        ContrastFilter contrastFilter = null;
        State state = State.Wait;
        
        readonly int renderScale = 1;

        //smart thing to do
        Queue<Action> _toDoActions = new Queue<Action>();

        public MainPage()
        {
            InitializeComponent();
            contrastFilter = new ContrastFilter(0.0);

            var filters = new IFilter[] 
                {
                    contrastFilter
                };

            filterEffects = new FilterEffect
            {
                Filters = filters
            };

            renderBitmap = new WriteableBitmap((int)(ResultImage.Width * ScreenToPixelFactor / renderScale), (int)(ResultImage.Height * ScreenToPixelFactor / renderScale));
            tempBitmap = new WriteableBitmap((int)(ResultImage.Width * ScreenToPixelFactor / renderScale), (int)(ResultImage.Height * ScreenToPixelFactor / renderScale));
            ResultImage.Source = renderBitmap;
            renderer = new WriteableBitmapRenderer(filterEffects, tempBitmap, OutputOption.Stretch);
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
            using(Stream resultStream = e.ChosenPhoto)
            {
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(resultStream);
                OriginalImage.Source = bmp;

                resultStream.Position = 0;
                IImageProvider imageSource = new StreamImageSource(resultStream);

                filterEffects.Source = imageSource;

                await renderer.RenderAsync();
                tempBitmap.Pixels.CopyTo(renderBitmap.Pixels, 0);
                renderBitmap.Invalidate();
            }            
        }

        private async void ValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _toDoActions.Enqueue(() => 
                                {
                                    contrastFilter.Level = e.NewValue; 
                                });
            Apply();
        }
        void Apply()
        {
            switch (state)
            {
                case State.Wait: // State machine transition: Wait -> Apply
                    state = State.Apply;
                    Render(); // Apply the filter
                    break;
                case State.Apply: // State machine transition: Apply -> Schedule
                    state = State.Schedule;
                    break;
                default:
                    break;
            }
        }

        protected async void Render()
        {
            try
            {
                while (_toDoActions.Count > 0)
                {
                    Action action = _toDoActions.Dequeue();
                    action();
                }
                await renderer.RenderAsync();
                tempBitmap.Pixels.CopyTo(renderBitmap.Pixels, 0);
                renderBitmap.Invalidate(); // redraw
            }
            catch (Exception)
            {
            }
            finally
            {
                switch (state)
                {
                    case State.Apply: // State machine transition : Apply -> Wait
                        state = State.Wait;
                        break;
                    case State.Schedule: // State machine transition: Schedule -> Apply
                        state = State.Apply;
                        Render(); // Apply the filter
                        break;
                    default:
                        break;
                }
            }
        }


        private double _ScreenToPixelFactor = 0;
        // http://developer.nokia.com/Community/Wiki/Optimizing_Imaging_SDK_use_for_rapidly_changing_filter_parameters
        private double ScreenToPixelFactor
        {
            get
            {
                if (_ScreenToPixelFactor == 0)
                {
                    try
                    {
                        _ScreenToPixelFactor = ((System.Windows.Size)DeviceExtendedProperties.GetValue("PhysicalScreenResolution")).Width / 480;
                    }
                    catch (Exception)
                    {
                        _ScreenToPixelFactor = System.Windows.Application.Current.Host.Content.ScaleFactor / 100.0;
                    }
                }
                return _ScreenToPixelFactor;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            JpegRenderer jpegRenderer = new JpegRenderer(filterEffects);

            IBuffer jpegOutput = await jpegRenderer.RenderAsync();

            using (MediaLibrary library = new MediaLibrary())
            {
                string fileName = string.Format("slika_{0:G}", DateTime.Now);
                var picture = library.SavePicture(fileName, jpegOutput.AsStream());
            }
        }
    }
}