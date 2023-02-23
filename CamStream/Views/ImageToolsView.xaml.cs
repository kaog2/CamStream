using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CamStream.Views
{
	/// <summary>
	/// Interaction logic for ImageToolsView.xaml
	/// </summary>
	public partial class ImageToolsView : Window
	{
		private MainWindow mainWindow = null;
		public ImageToolsView(MainWindow form)
		{
			InitializeComponent();
			DataContext = form.DataContext;
			mainWindow = form;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			mainWindow.EnableImgSettings = false;
			e.Cancel = true;
			Hide();
		}
	}
}
