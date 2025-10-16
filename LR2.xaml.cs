using ABI.Windows.Foundation;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.UI;

using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemAI_LR
{
	public sealed partial class LR2 : UserControl
	{

		private string sceneFile, objectFile;
		private bool flag1, flag2;

		private Image<Bgr, Byte> imgSceneColor = null;
		private Image<Bgr, Byte> imgToFindColor = null;
		private Image<Bgr, Byte> imgCopyOfImageToFindWithBorder = null;
		private bool blnImageSceneLoaded = false;
		private bool blnImageToFindLoaded = false;
		private Image<Bgr, Byte> imgResult = null;
		private Bgr bgrKeyPointsColor = new Bgr(Color.Blue);
		private Bgr bgrMatchingLinesColor = new Bgr(Color.LightPink);
		private Bgr bgrFoundImageColor = new Bgr(Color.Red);
		private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

		private async void ProcessFrame_Click(object sender, RoutedEventArgs e)
		{
			if (!blnImageToFindLoaded || !blnImageSceneLoaded)
			{
				ContentDialog contentDialog = new ContentDialog();
				contentDialog.XamlRoot = (sender as Button).XamlRoot;
				contentDialog.Title = "Ошибка";
				contentDialog.Content = "Необходимо загрузить изображения";
				contentDialog.PrimaryButtonText = "OK";
				await contentDialog.ShowAsync();
				return;
			}


			Image<Gray, Byte> imgSceneGray = null;
			Image<Gray, Byte> imgToFindGray = null;

			VectorOfKeyPoint vkpSceneKeyPoints, vkpToFindKeyPoints;
			int intKNumNearestNeighbors = 2;
			double dblUniquenessThreshold = 0.8;

			int intNumNonZeroElements;

			double dblScaleIncrement = 1.5;
			int intRotationBins = 20;

			double dblRansacReprojectionThreshold = 2.0;

			Rectangle rectImageToFind;
			PointF[] ptfPointsF;
			System.Drawing.Point[] ptPoints;

			imgSceneGray = imgSceneColor.Convert<Gray, Byte>();
			imgToFindGray = imgToFindColor.Convert<Gray, Byte>();
			var sift = new SIFT(); // или new SIFT(0, 3, 0.04, 10, 1.6)

			VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();
			VectorOfKeyPoint sceneKeyPoints = new VectorOfKeyPoint();

			Mat modelDescriptors = new Mat();
			Mat sceneDescriptors = new Mat();

			sift.DetectAndCompute(imgToFindGray, null, modelKeyPoints, modelDescriptors, false);
			sift.DetectAndCompute(imgSceneGray, null, sceneKeyPoints, sceneDescriptors, false);

			Console.WriteLine($"Ключевых точек на модели: {modelKeyPoints.Size}");
			Console.WriteLine($"Ключевых точек на сцене: {sceneKeyPoints.Size}");

			// --- 3. Сопоставление дескрипторов (Brute Force Matcher)
			BFMatcher matcher = new BFMatcher(DistanceType.L2, crossCheck: false);
			using var matches = new VectorOfVectorOfDMatch();
			matcher.KnnMatch(modelDescriptors, sceneDescriptors, matches, k: 2);

			var goodMatches = new System.Collections.Generic.List<Emgu.CV.Structure.MDMatch>();
			for (int i = 0; i < matches.Size; i++)
			{
				if (matches[i].Size < 2) continue;
				var m = matches[i][0];
				var n = matches[i][1];
				if (m.Distance < 0.75 * n.Distance)
					goodMatches.Add(m);
			}
			

			// --- 5. Гомография
			if (goodMatches.Count >= 4)
			{
				PointF[] modelPoints = goodMatches.ToArray().Select(m => modelKeyPoints[m.QueryIdx].Point).ToArray();
				PointF[] scenePoints = goodMatches.ToArray().Select(m => sceneKeyPoints[m.TrainIdx].Point).ToArray();

				Mat homography = CvInvoke.FindHomography(modelPoints, scenePoints, RobustEstimationAlgorithm.Ransac, 5.0);

				if (!homography.IsEmpty)
				{
					// --- 6. Рисуем рамку вокруг найденного объекта
					Rectangle rect = new Rectangle(System.Drawing.Point.Empty, imgToFindGray.Size);
					PointF[] modelCorners =
					{
					new PointF(rect.Left, rect.Top),
					new PointF(rect.Right, rect.Top),
					new PointF(rect.Right, rect.Bottom),
					new PointF(rect.Left, rect.Bottom)
				};	
					imgResult = imgSceneColor.Clone();
					foreach (var m in goodMatches)
					{
						System.Drawing.Point p1 = System.Drawing.Point.Round(modelKeyPoints[m.QueryIdx].Point);
						System.Drawing.Point p2 = System.Drawing.Point.Round(sceneKeyPoints[m.TrainIdx].Point);
						
						CvInvoke.Circle(imgSceneColor,p2,4, new MCvScalar(255,0,0),2);
						CvInvoke.Circle(imgToFindColor,p1,4, new MCvScalar(255,0,0),2);
					}
					PointF[] sceneCorners = CvInvoke.PerspectiveTransform(modelCorners, homography);
					System.Drawing.Point[] points = (from p in sceneCorners select System.Drawing.Point.Round(p)).ToArray();

					

					// --- 7. Рисуем совпадения
					imgResult = imgSceneColor.ConcateHorizontal(imgToFindColor);
					foreach (var m in goodMatches)
					{
						System.Drawing.Point p1 = System.Drawing.Point.Round(modelKeyPoints[m.QueryIdx].Point);
						System.Drawing.Point p2 = System.Drawing.Point.Round(sceneKeyPoints[m.TrainIdx].Point);

						imgResult.Draw(new LineSegment2D(new System.Drawing.Point(p1.X + imgSceneColor.Width, p1.Y), p2), new Bgr(Color.Blue), 1);
					}
					imgResult.DrawPolyline(points, true, bgrFoundImageColor, 2);
					ResultImageS.Source = ClassUtility.ToBitmapSource(imgResult);

				}
			}
		}

		private async void Button_ClickAsync(object sender, RoutedEventArgs e)
		{
			if (sender is Button button)
			{
				//disable the button to avoid double-clicking
				button.IsEnabled = false;

				var picker = new FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);

				picker.CommitButtonText = "Pick File";

				picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

				picker.ViewMode = PickerViewMode.List;

				picker.FileTypeFilter.Add(".jpg");
				picker.FileTypeFilter.Add(".png");
				picker.FileTypeFilter.Add(".webp");

				// Show the picker dialog window
				var file = await picker.PickSingleFileAsync();

				button.IsEnabled = true;
				if (file == null)
					return;
				switch (button.Name)
				{
					case "OpenScene":
						{
							sceneFile = file.Path;
							imgSceneColor = new Image<Bgr, byte>(sceneFile);
							iconImgStatus1.Visibility = Visibility.Visible;
							blnImageSceneLoaded = true;
							break;
						}
					case "OpenObject":
						{
							objectFile = file.Path;
							iconImgStatus2.Visibility = Visibility.Visible;
							imgToFindColor = new Image<Bgr, byte>(objectFile);
							blnImageToFindLoaded = true;
							break;
						}
				}

			}

		}


		public LR2()
		{
			InitializeComponent();

		}
	}
}
	