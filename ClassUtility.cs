using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

namespace SystemAI_LR
{
	internal static class ClassUtility
	{
		public static ImageSource ToBitmapSource(Image<Bgr, byte> image)
		{
			using (var ms = new MemoryStream())
			{
				image.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
				ms.Seek(0, SeekOrigin.Begin);
				var bitmapImage = new BitmapImage();
				bitmapImage.SetSource(ms.AsRandomAccessStream());
				return bitmapImage;
			}
		}
	}
}
