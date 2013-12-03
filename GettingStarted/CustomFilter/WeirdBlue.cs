using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace CustomFilter
{
    public class WeirdBlue : CustomEffectBase
    {
        public WeirdBlue(IImageProvider source)
            : base(source)
        {
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            sourcePixelRegion.ForEachRow((index, width, pos) =>
            {
                for (int x = 0; x < width; x++, index++)
                {
                    Color c = ToColor(sourcePixelRegion.ImagePixels[index]);
                    byte Green = (byte)Math.Min(255, (c.R * .317 + c.G * .912 + c.B * .215));
                    byte Red = (byte)Math.Min(255, (c.R * .911 + c.G * .345 + c.B * .123));
                    byte Blue = (byte)Math.Min(255, (c.R * .712 + c.G * .567 + c.B * .444));
                    //Debug.WriteLine(c.A);
                    c.R = Red;
                    c.G = Green;
                    c.B = Blue;
                    //c.A = 0;
                    targetPixelRegion.ImagePixels[index] = FromColor(c);
                }
            });
        }
    }
}
