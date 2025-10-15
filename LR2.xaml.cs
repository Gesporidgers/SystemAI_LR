using Emgu.CV;
using Emgu.CV.Structure;
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
							iconImgStatus1.Visibility = Visibility.Visible;
							break;
						}
					case "OpenObject":
						{
							objectFile = file.Path;
							iconImgStatus2.Visibility = Visibility.Visible;
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
