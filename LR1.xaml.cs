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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
		public LR1()
		{
			InitializeComponent();
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button)
			{
				//disable the button to avoid double-clicking
				button.IsEnabled = false;

				var picker = new FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);

				picker.CommitButtonText = "Выберите изображение";

				picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

				picker.ViewMode = PickerViewMode.Thumbnail;

				picker.FileTypeFilter.Add(".jpg");
				picker.FileTypeFilter.Add(".png");
				picker.FileTypeFilter.Add(".bmp");
				
				// Show the picker dialog window
				var file = await picker.PickSingleFileAsync();
				sourceImg.Source = new BitmapImage(new Uri(file.Path));
				filePath = file.Path;
				button.IsEnabled = true;

			}

		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			Image<Bgr, byte> image = new Image<Bgr, byte>(filePath);
			Image<Gray, byte> grascale = image.Convert<Gray, byte>();
		}
	}
}
