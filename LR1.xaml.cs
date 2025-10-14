using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Timers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemAI_LR
{
    public sealed partial class LR1 : UserControl
    {
        string filePath;
        VideoCapture capture;
        Image<Bgr, byte> currentFrame;
        Timer timer;
        CascadeClassifier cascade;
		private readonly object captureLock = new();
        public LR1()
        {
            timer = new Timer() { Interval = 30 };
            timer.Elapsed += Timer_Elapsed;
            InitializeComponent();
        }

        

        private async void SwitchCamera_Checked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is ToggleButton toggle)
            {
                Waiting.Visibility = Visibility.Visible;
                toggle.IsEnabled = false;
                await Task.Run(() =>
                {
                    capture = new VideoCapture(0);
                    capture.Set(CapProp.Fps, 30);
                    capture.Set(CapProp.HwAcceleration, (double)VideoAccelerationType.D3D11);
                });
                toggle.IsEnabled = true;
                Waiting.Visibility = Visibility.Collapsed;
                timer.Start();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            lock (captureLock)
            {
                cascade = new("Assets/haarcascade_frontalface_default.xml");
                currentFrame = capture.QueryFrame().ToImage<Bgr, byte>();
                if (currentFrame != null)
                {
                    Image<Gray, byte> grayFrame = currentFrame.Convert<Gray, byte>();
                    Rectangle[] faces = cascade.DetectMultiScale(grayFrame, 1.1, 10, System.Drawing.Size.Empty);
                    foreach (var face in faces)
                    {
                        currentFrame.Draw(face, new Bgr(System.Drawing.Color.Red), 2);
					}
					DispatcherQueue.TryEnqueue(() =>
                    {
                        sourceImg.Source = ToBitmapSource(currentFrame);
                    });
                }
            }
        }

        private ImageSource ToBitmapSource(Image<Bgr, byte> image)
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

		private void SwitchCamera_Unchecked(object sender, RoutedEventArgs e)
		{
            timer.Stop();
            
            lock (captureLock)
            {
                capture?.Dispose();
                capture = null;
                sourceImg.Source = null;
            }
           
		}
	}
}
