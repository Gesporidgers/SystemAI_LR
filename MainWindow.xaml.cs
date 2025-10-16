using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemAI_LR
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
			Window window = this;
			window.ExtendsContentIntoTitleBar = true;
			window.SetTitleBar(titleBar);
		}
	}
}
