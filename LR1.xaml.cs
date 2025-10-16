using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Drawing;
using System.Threading.Tasks;
using System.Timers;

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
		CascadeClassifier cascade1;
		CascadeClassifier cascade2;
		private readonly object captureLock = new();
		public LR1()
		{
			cascade1 = new("Assets/haarcascade_frontalface_default.xml");
			cascade2 = new("Assets/haarcascade_eye.xml");
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
					//capture.Set(CapProp.HwAcceleration, (double)VideoAccelerationType.D3D11);
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

				currentFrame = capture.QueryFrame().ToImage<Bgr, byte>();
				if (currentFrame != null)
				{
					Image<Gray, byte> grayFrame = currentFrame.Convert<Gray, byte>();
					Rectangle[] faces = cascade1.DetectMultiScale(grayFrame, 1.1, 10, System.Drawing.Size.Empty);
					Rectangle[] eyesv = cascade2.DetectMultiScale(grayFrame, 1.1, 10, System.Drawing.Size.Empty);
					foreach (var eye in eyesv)
					{
						currentFrame.Draw(eye, new Bgr(System.Drawing.Color.Green), 2);
					}
					foreach (var face in faces)
					{
						currentFrame.Draw(face, new Bgr(System.Drawing.Color.Red), 2);
					}
					DispatcherQueue.TryEnqueue(() =>
					{
						sourceImg.Source = ClassUtility.ToBitmapSource(currentFrame);
					});

				}
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
