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
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
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
	public sealed partial class LR3 : UserControl
	{
		string FilePath;
		Tesseract _ocr;
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
			}
		}

		private async Task ProccessImage(object sender, RoutedEventArgs e) //Выполнить распознавание
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
			}
			Image<Bgr, byte> image = new Image<Bgr, byte>(FilePath);
			Image<Gray, byte> imgGray = image.Convert<Gray, byte>();
			_ocr.SetImage(imgGray);
			int status = int.MinValue;
			await Task.Run(() =>
			{
				status = _ocr.Recognize();
			});
			
			if ( status != 0)
			{
				ErrBar.IsOpen = true;
				return;
			}
		}
	}
}
