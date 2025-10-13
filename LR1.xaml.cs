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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        Image<Gray, byte> detectedFace = null;
        Timer timer;
        public LR1()
        {
            
            InitializeComponent();
        }

        

        private void SwitchCamera_Checked(object sender, RoutedEventArgs e)
        {

            timer = new Timer() { Interval = 30 };
            timer.Elapsed += Timer_Elapsed;
            if (e.OriginalSource is ToggleButton toggle && toggle.IsChecked.Value)
            {
                capture = new VideoCapture(0);

                capture.Set(CapProp.Fps, 30);
                timer.Start();
            }
            else
            {
                timer.Stop();
                capture?.Dispose();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            currentFrame = capture.QueryFrame().ToImage<Bgr, byte>().Resize(320, 240, Inter.Cubic);
            if (currentFrame != null)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    currentFrame.ToBitmap();
                    sourceImg.Source = ToBitmapSource(currentFrame);
                });
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
    }
}
