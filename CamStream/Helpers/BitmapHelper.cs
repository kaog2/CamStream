using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CamStream.Helpers
{
	public class BitmapHelper
	{
        //DeleteObject release the GDI handle. Without this, there is an "out of Memory"
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public ImageSource ToImageSource(Bitmap bmp)
        {
            try
            {
                IntPtr hBitmap = bmp.GetHbitmap();

                ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                RenderOptions.SetBitmapScalingMode(wpfBitmap, BitmapScalingMode.NearestNeighbor);
                Thread.Sleep(10);

                DeleteObject(hBitmap);

                return wpfBitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public BitmapImage ToBitmapImage(Bitmap btm)
        {
            try
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                MemoryStream ms = new MemoryStream();
                btm.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                bi.StreamSource = ms;
                bi.EndInit();
                return bi;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BitmapImage();
            }
        }
    }
}
