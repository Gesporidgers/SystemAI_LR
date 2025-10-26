using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Microsoft.UI.Windowing;
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemAI_LR
{
	public sealed partial class LR3 : UserControl
	{
		string FilePath;
		Tesseract _ocr;
		Bgr color = new Bgr(System.Drawing.Color.Red);
		public LR3()
		{
			InitializeComponent();
		}

		private async void OpenImage(object sender, RoutedEventArgs e) //Открыть файл
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
				FilePath = file.Path;
				srcImg.Source = new BitmapImage(new(file.Path));
			}
		}

		private async void ProccessImage(object sender, RoutedEventArgs e) //Выполнить распознавание
		{
			string lang = RusBox.IsChecked.Value ? "rus" : "eng";
			if (_ocr != null)
			{
				_ocr.Dispose();
				_ocr = null;
			}
			try
			{
				string tesPath = Emgu.CV.OCR.Tesseract.DefaultTesseractDirectory;
				_ocr = new Tesseract(tesPath, lang, OcrEngineMode.TesseractOnly);
			}
			catch (Exception ex)
			{
				ContentDialog contentDialog = new ContentDialog();
				contentDialog.Content = ex.Message;
				contentDialog.PrimaryButtonText = "OK";
				contentDialog.XamlRoot = (sender as Button).XamlRoot;
				await contentDialog.ShowAsync();
				return;
			}
			Image<Bgr, byte> image = new Image<Bgr, byte>(FilePath);
			Image<Gray, byte> imgGray = image.Convert<Gray, byte>();
			_ocr.SetImage(imgGray);
			int status = int.MinValue;
			await Task.Run(() =>
			{
				status = _ocr.Recognize();
			});

			if (status != 0)
			{
				ErrBar.IsOpen = true;
				return;
			}
			Tesseract.Word[] characters = _ocr.GetWords();
			if (characters.Length == 0)
			{
				imgGray = imgGray.ThresholdBinary(new(65), new(255));
				_ocr.SetImage(imgGray);
				characters = _ocr.GetWords();
				if (characters.Length == 0)
				{
					imgGray = imgGray.ThresholdBinary(new(190), new(255));
					_ocr.SetImage(imgGray);
					characters = _ocr.GetWords();
				}
			}
			foreach (var item in characters)
			{
				image.Draw(item.Region, color);
			}
			TextPanel.Visibility = Visibility.Visible;
			ocredText.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, _ocr.GetUTF8Text());
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Directory.CreateDirectory(Emgu.CV.OCR.Tesseract.DefaultTesseractDirectory);
			string tesPath = Emgu.CV.OCR.Tesseract.DefaultTesseractDirectory;
			bool rusExist = System.IO.File.Exists(Path.Combine(tesPath, "rus"));
			bool engExsist = System.IO.File.Exists(Path.Combine(tesPath, "eng"));
			if ( !rusExist || !engExsist )
			{
				using (HttpClient httpClient = new())
				{
					string src1 = Emgu.CV.OCR.Tesseract.GetLangFileUrl("rus");
					var resp = await httpClient.GetAsync(src1);
					Stream stream = await resp.Content.ReadAsStreamAsync();
					FileStream fileStream = new(Path.Combine(tesPath, "rus.traineddata"),FileMode.Create);
					stream.CopyTo(fileStream);
					stream.Close(); fileStream.Close();
					stream.Dispose(); fileStream.Dispose();

					src1 = Emgu.CV.OCR.Tesseract.GetLangFileUrl("eng");
					resp = await httpClient.GetAsync(src1);
					stream = await resp.Content.ReadAsStreamAsync();
					fileStream = new(Path.Combine(tesPath, "eng.traineddata"), FileMode.Create);
					stream.CopyTo(fileStream);
					stream.Close(); fileStream.Close();
					stream.Dispose(); fileStream.Dispose();
				}
			}
		}
	}
}
