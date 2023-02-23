using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge.Video;
using AForge.Video.DirectShow;
using Xceed.Wpf.Toolkit;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using CamStream.Helpers;
using CamStream.ViewModels;
using CamStream.Views;
using AForge.Imaging.Filters;
using System.Collections.ObjectModel;
using CamStream.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using Brush = System.Windows.Media.Brush;
using System.Configuration;
using MessageBox = System.Windows.MessageBox;

namespace CamStream
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		/// <summary>
		/// Move Variables Scales
		/// </summary>

		#region Public properties

		double widthScale;
		double heigthScale;
		Shape currentShape = null;
		public double WidthScale { get => widthScale; set { widthScale = value; } }

		public double HeigthScale { get => heigthScale; set { heigthScale = value; } }

		public Shape CurrentShape { get => currentShape; set { currentShape = value; } }
		ImageToolsView imageToolsView = null;
		#endregion

		#region Private properties

		private BrightnessCorrection brightnessFilter;
		private ContrastCorrection contrastFilter;
		private HueModifier hueFilter;
		private SaturationCorrection saturationFilter;
		private Sharpen sharpenFilter;
		private GammaCorrection gammaFilter;
		private Mirror mirrorFilter;
		private Point clickPosition;
		private TranslateTransform originTT;
		private Point lastPositionGrid;
		private IVideoSource streamvideo;
		private static readonly HttpClient client = new HttpClient();
		protected bool isDragging;
		#endregion

		#region ViewModelBase
		public event PropertyChangedEventHandler PropertyChanged;
		public event PropertyChangedEventHandler PropertyChangedAsync;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}

		protected async void OnPropertyChangedAsync(object s, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SelectedResolution))
			{
				await SetResolution();
			}
		}
		#endregion

		#region MainWindowViewModel

		public int brightness;
		public int contrast;
		public int hueValue;
		public float saturation;
		public double gamma;
		public int threshold;
		public bool isGrayscaled;
		public bool isSaturated;
		public bool isHue;
		public bool mirrowOnXAxis;
		public bool mirrowOnYAxis;
		public bool enableImgSettings;
		public ObservableCollection<ResolutionModel> resolutions;
		public ResolutionModel selectedResolution;
		public double windowWidth;
		public double windowHeight;
		public Brush verticalColor;
		public Brush horizontalColor;
		public Brush currentColor;

		public int Brightness
		{
			get { return brightness; }
			set { brightness = value; OnPropertyChanged("Brightness"); }
		}

		public int Contrast
		{
			get { return contrast; }
			set { contrast = value; OnPropertyChanged("Contrast"); }
		}

		public int HueValue
		{
			get { return hueValue; }
			set { hueValue = value; OnPropertyChanged("HueValue"); }
		}

		public float Saturation
		{
			get { return saturation; }
			set { saturation = value; OnPropertyChanged("Saturation"); }
		}

		public double Gamma
		{
			get { return gamma; }
			set { gamma = value; OnPropertyChanged("Gamma"); }
		}

		public int Threshold
		{
			get { return threshold; }
			set { threshold = value; OnPropertyChanged("Threshold"); }
		}

		public bool IsGrayscaled
		{
			get { return isGrayscaled; }
			set { isGrayscaled = value; OnPropertyChanged("IsGrayscaled"); }
		}

		public bool IsSaturated
		{
			get { return isSaturated; }
			set { isSaturated = value; OnPropertyChanged("IsSaturated"); }
		}

		public bool IsHue
		{
			get { return isHue; }
			set { isHue = value; OnPropertyChanged("IsHue"); }
		}

		public bool MirrowOnXAxis
		{
			get { return mirrowOnXAxis; }
			set { mirrowOnXAxis = value; OnPropertyChanged("MirrowOnXAxis"); }
		}

		public bool MirrowOnYAxis
		{
			get { return mirrowOnYAxis; }
			set { mirrowOnYAxis = value; OnPropertyChanged("MirrowOnYAxis"); }
		}

		public bool EnableImgSettings
		{
			get { return enableImgSettings; }
			set { enableImgSettings = value; OnPropertyChanged("EnableImgSettings"); }
		}
		
		public Brush VerticalColor
		{
			get { return verticalColor; }
			set { verticalColor = value; OnPropertyChanged("VerticalColor"); }
		}
		
		public Brush HorizontalColor
		{
			get { return horizontalColor; }
			set { horizontalColor = value; OnPropertyChanged("HorizontalColor"); }
		}

		public Brush CurrentColor
		{
			get { return currentColor; }
			set { currentColor = value; OnPropertyChanged("CurrentColor"); }
		}

		public ObservableCollection<ResolutionModel> Resolutions
		{
			get { return resolutions; }
			set { resolutions = value; OnPropertyChanged("EnableImgSettings"); }
		}

		//public ResolutionModel SelectedResolution
		//{
		//    get { return selectedResolution; }
		//    set { selectedResolution = value; OnPropertyChanged("SelectedResolution"); }
		//}

		public ResolutionModel SelectedResolution
		{
			get { return selectedResolution; }
			set
			{
				selectedResolution = value;
				PropertyChangedAsync?.Invoke(this,
					new PropertyChangedEventArgs(nameof(SelectedResolution)));
			}
		}

		public double WindowWidth
		{
			get { return windowWidth; }
			set { windowWidth = value; OnPropertyChanged("WindowWidth"); }
		}
		
		public double WindowHeight
		{
			get { return windowHeight; }
			set { windowHeight = value; OnPropertyChanged("WindowHeight"); }
		}
		#endregion

		//MJPEGStream streamvideo;
		//TcpClient socket = new TcpClient();
		//socket.Connect("172.18.112.1", 9999);
		public MainWindow()
		{
			InitializeComponent();
			//DataContext = new MainWindowViewModel();
			DataContext = this;

			WidthScale = 5;
			HeigthScale = 5;
			Threshold = 157;
			Brightness = 0;
			Contrast = 0;
			HueValue = 0;
			Saturation = 0;
			Gamma = 100;
			IsSaturated = false;
			IsGrayscaled = false;
			IsHue = false;
			VerticalColor = new SolidColorBrush(Colors.Red);
			HorizontalColor = new SolidColorBrush(Colors.Green);
			CurrentColor = new SolidColorBrush(Colors.Gray);
			EnableImgSettings = false;
			PropertyChangedAsync += OnPropertyChangedAsync;
			//ResolutionsViewModel r = new ResolutionsViewModel();
			//Resolutions = r.GetResolutions();
			Resolutions = GetResolutions();
			if (Resolutions != null)
			{
				SelectedResolution = Resolutions[0];
			}
			
			//InitLocalCam();
		}

		public ObservableCollection<ResolutionModel> GetResolutions()
		{
			try
			{
				string url = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["GetResolutions"]) ? ConfigurationManager.AppSettings["GetResolutions"] : string.Empty;
				WebClient client = new WebClient();
				string response = client.DownloadString(url);
				return JsonConvert.DeserializeObject<ObservableCollection<ResolutionModel>>(response);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}

		private void InitLocalCam()
		{
			//try
			//{
			//	//infoCameraLbl.Content = "";
			//	//Enumerate all video input devices
			//	videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			//	if (videoDevices.Count == 0)
			//	{
			//		//infoCameraLbl.Content = "No local capture devices";
			//	}
			//	foreach (FilterInfo device in videoDevices)
			//	{
			//		int i = 1;
			//		selectionCamCbx.Items.Add(device.Name);
			//		//infoCameraLbl.Content = ("camera initialization completed..." + "\n");
			//		i++;
			//	}
			//	selectionCamCbx.SelectedIndex = 0;
			//}
			//catch (ApplicationException)
			//{
			//	//infoCameraLbl.Content = "No local capture devices";
			//	videoDevices = null;
			//}
		}

		private void startBtn_Click(object sender, RoutedEventArgs e)
		{
			GetFromStream();
			startBtn.Visibility = Visibility.Hidden;
			stopBtn.Visibility = Visibility.Visible;
		}

		private void GetFromStream()
		{
			var url = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["WpfStream"]) ? ConfigurationManager.AppSettings["WpfStream"] : string.Empty;
			streamvideo = new MJPEGStream(url);
			streamvideo.NewFrame += VideoNewFrame;
			streamvideo.Start();
		}

		private void AforgeVideoCapture()
		{
			/*selected = selectionCamCbx.SelectedIndex;

            if (islemdurumu == 0)
            {
                if (kamerabaslat > 0) return;
                try
                {
                    videoSource = new VideoCaptureDevice(videoDevices[selected].MonikerString);
                    videoSource.NewFrame += new NewFrameEventHandler(VideoNewFrame);
                    videoSource.Start(); kamerabaslat = 1; //CAMERA STARTRED
                }
                catch
                {
                    MessageBox.Show("RESTART THE PROGRAM");

                    if (!(videoSource == null))
                        if (videoSource.IsRunning)
                        {
                            videoSource.SignalToStop();
                            videoSource = null;
                        }
                }
            }*/
		}

		private void VideoNewFrame(object sender, NewFrameEventArgs eventarg)
		{
			Bitmap img = (Bitmap)eventarg.Frame.Clone();
			BitmapHelper bitmapHelper = new BitmapHelper();
			BitmapImage bi;
			try
			{
				if (EnableImgSettings)
				{
					brightnessFilter = new BrightnessCorrection(Brightness);
					brightnessFilter.ApplyInPlace(img);
					contrastFilter = new ContrastCorrection(Contrast);
					contrastFilter.ApplyInPlace(img);

					gammaFilter = new GammaCorrection(Gamma / 100);
					gammaFilter.ApplyInPlace(img);
					//sharpenFilter = new Sharpen();
					//sharpenFilter.ApplyInPlace(img);

					if (IsSaturated)
					{
						saturationFilter = new SaturationCorrection(Saturation / 100);
						saturationFilter.ApplyInPlace(img);
					}

					if (IsHue)
					{
						hueFilter = new HueModifier(HueValue);
						hueFilter.ApplyInPlace(img);
					}

					if (MirrowOnXAxis || MirrowOnYAxis)
					{
						mirrorFilter = new Mirror(MirrowOnXAxis, MirrowOnYAxis);
						mirrorFilter.ApplyInPlace(img);
					}

					if (IsGrayscaled)
					{
						using (var grayscaledBitmap = Grayscale.CommonAlgorithms.BT709.Apply(img))
						{
							using (var thresholdedBitmap = new Threshold(Threshold).Apply(grayscaledBitmap))
							{
								bi = bitmapHelper.ToBitmapImage(thresholdedBitmap);
							}
						}
					}
					else
					{
						bi = bitmapHelper.ToBitmapImage(img);
					}
				}
				else
				{
					bi = bitmapHelper.ToBitmapImage(img);
				}

				bi.Freeze(); // avoid cross thread operations and prevents leaks
				Dispatcher.BeginInvoke(new ThreadStart(delegate { imgVideoLeft.Source = bi; }));
				//Dispatcher.BeginInvoke(new Action(() =>
				//  {
				//      imgVideoLeft.Source = bitmapHelper.ToBitmapImage(grayscaledBitmap);
				//  }
				//));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (streamvideo != null)
			{
				streamvideo.Stop();
			}

			if (imageToolsView != null)
			{
				imageToolsView.Close();

			}
		}

		private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var draggableControl = sender as Shape;
			originTT = draggableControl.RenderTransform as TranslateTransform ?? new TranslateTransform();
			isDragging = true;
			clickPosition = e.GetPosition(this);
			draggableControl.CaptureMouse();
		}

		private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			isDragging = false;
			var draggable = sender as Shape;
			draggable.ReleaseMouseCapture();
		}

		private void verticalRec_MouseMove(object sender, MouseEventArgs e)
		{
			var draggableControl = sender as Shape;
			var maxWidthMove = gridViewVideo.ActualWidth;

			if (isDragging && draggableControl != null)
			{
				Point currentPosition = e.GetPosition(this);
				var transform = draggableControl.RenderTransform as TranslateTransform ?? new TranslateTransform();
				transform.X = originTT.X + (currentPosition.X - clickPosition.X);

				//Side bounds
				if (transform.X < maxWidthMove - draggableControl.ActualWidth && transform.X > 0)
				{
					draggableControl.RenderTransform = new TranslateTransform(transform.X, transform.Y);
				}
				else if (transform.X < 0)
				{
					draggableControl.RenderTransform = new TranslateTransform(0, transform.Y);
				}
				if (transform.X >= maxWidthMove - draggableControl.ActualWidth)
				{
					draggableControl.RenderTransform = new TranslateTransform(maxWidthMove - draggableControl.ActualWidth, transform.Y);
				}
			}
		}

		private void horizontalRec_MouseMove(object sender, MouseEventArgs e)
		{
			var draggableControl = sender as Shape;
			var maxHeightMove = gridViewVideo.ActualHeight;

			if (isDragging && draggableControl != null)
			{
				Point currentPosition = e.GetPosition(this);
				var transform = draggableControl.RenderTransform as TranslateTransform ?? new TranslateTransform();
				transform.Y = originTT.Y + (currentPosition.Y - clickPosition.Y);

				//Side bounds
				if (transform.Y < maxHeightMove - draggableControl.ActualHeight && transform.Y > 0)
				{
					draggableControl.RenderTransform = new TranslateTransform(transform.X, transform.Y);
				}
				else if (transform.Y < 0)
				{
					draggableControl.RenderTransform = new TranslateTransform(transform.X, 0);
				}
				if (transform.Y >= maxHeightMove - draggableControl.ActualHeight)
				{
					draggableControl.RenderTransform = new TranslateTransform(transform.X, maxHeightMove - draggableControl.ActualHeight);
				}
			}
		}

		/// <summary>
		/// Load the center position for the lines in accord to the grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			lastPositionGrid.X = gridViewVideo.ActualWidth;
			lastPositionGrid.Y = gridViewVideo.ActualHeight;

			SetLineToCenter();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			verticalRec.Height = gridViewVideo.ActualHeight;
			horizontalRec.Width = gridViewVideo.ActualWidth;

			var positionVert = verticalRec.RenderTransform as TranslateTransform ?? null;
			if (positionVert != null)
			{
				verticalRec.RenderTransform = new TranslateTransform(gridViewVideo.ActualWidth / lastPositionGrid.X * positionVert.X, 0);
			}

			var positionHor = horizontalRec.RenderTransform as TranslateTransform ?? null;
			if (positionHor != null)
			{
				horizontalRec.RenderTransform = new TranslateTransform(0, gridViewVideo.ActualHeight / lastPositionGrid.Y * positionHor.Y);
			}

			lastPositionGrid.X = gridViewVideo.ActualWidth;
			lastPositionGrid.Y = gridViewVideo.ActualHeight;
		}

		private void resetLineBtn_Click(object sender, RoutedEventArgs e)
		{
			SetLineToCenter();
		}

		private void SetLineToCenter()
		{
			double xAxis = gridViewVideo.ActualWidth / 2;
			double yAxis = gridViewVideo.ActualHeight / 2;

			verticalRec.RenderTransform = new TranslateTransform(xAxis - (verticalRec.Width / 2), 0);
			verticalRec.Height = gridViewVideo.ActualHeight;

			horizontalRec.RenderTransform = new TranslateTransform(0, yAxis - (horizontalRec.Height / 2));
			horizontalRec.Width = gridViewVideo.ActualWidth;
		}

		private void showLines_Checked(object sender, RoutedEventArgs e)
		{
			if (verticalRec == null || horizontalRec == null) return;

			verticalRec.Visibility = Visibility.Visible;
			horizontalRec.Visibility = Visibility.Visible;
		}

		private void showLines_Unchecked(object sender, RoutedEventArgs e)
		{
			if (verticalRec == null || horizontalRec == null) return;

			verticalRec.Visibility = Visibility.Hidden;
			horizontalRec.Visibility = Visibility.Hidden;
		}

		private void fixLines_Checked(object sender, RoutedEventArgs e)
		{
			CheckControl((MenuItem)e.OriginalSource);
		}

		private void widthLineChange_Checked(object sender, RoutedEventArgs e)
		{
			CheckControl((MenuItem)e.OriginalSource);
		}

		private void postLineChange_Checked(object sender, RoutedEventArgs e)
		{
			CheckControl((MenuItem)e.OriginalSource);
		}

		private void colorVChange_Checked(object sender, RoutedEventArgs e)
		{
			CheckControl((MenuItem)e.OriginalSource);
		}

		private void colorHChange_Checked(object sender, RoutedEventArgs e)
		{
			CheckControl((MenuItem)e.OriginalSource);
		}

		private void transparentLineChange_Checked(object sender, RoutedEventArgs e)
		{
			CheckControl((MenuItem)e.OriginalSource);
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (widthLineChange.IsChecked)
			{
				WidthLineChange(e.Key);
			}
			else if (transparentLineChange.IsChecked)
			{
				TransparentLineChange(e.Key);
			}
			else if (postLineChange.IsChecked)
			{
				PositionLineChange(e.Key);
			}
		}

		private void PositionLineChange(Key key)
		{
			var maxWidthMove = gridViewVideo.ActualWidth;
			var maxHeightMove = gridViewVideo.ActualHeight;
			var horizontalTransform = horizontalRec.RenderTransform as TranslateTransform ?? new TranslateTransform();
			var verticalTransform = verticalRec.RenderTransform as TranslateTransform ?? new TranslateTransform();

			switch (key)
			{
				case Key.Up:
					horizontalTransform.Y = horizontalTransform.Y - 3;
					if (horizontalTransform.Y < maxHeightMove - horizontalRec.ActualHeight && horizontalTransform.Y > 0)
						horizontalRec.RenderTransform = new TranslateTransform(horizontalTransform.X, horizontalTransform.Y);
					else
						horizontalRec.RenderTransform = new TranslateTransform(horizontalTransform.X, 0);
					break;
				case Key.Down:
					horizontalTransform.Y = horizontalTransform.Y + 3;
					if (horizontalTransform.Y >= maxHeightMove - horizontalRec.ActualHeight)
						horizontalRec.RenderTransform = new TranslateTransform(horizontalTransform.X, maxHeightMove - horizontalRec.ActualHeight);
					break;
				case Key.Right:
					verticalTransform.X = verticalTransform.X + 3;
					if (verticalTransform.X < maxWidthMove - verticalRec.ActualWidth && verticalTransform.X > 0)
						verticalRec.RenderTransform = new TranslateTransform(verticalTransform.X, verticalTransform.Y);
					else
						verticalRec.RenderTransform = new TranslateTransform(maxWidthMove - verticalRec.ActualWidth, verticalTransform.Y);
					break;
				case Key.Left:
					if (verticalTransform.X - 3 >= 0)
						verticalRec.RenderTransform = new TranslateTransform(verticalTransform.X - 3, verticalTransform.Y);
					else
						verticalRec.RenderTransform = new TranslateTransform(0, verticalTransform.Y);
					break;
				default:
					break;
			}
		}

		private void TransparentLineChange(Key key)
		{
			switch (key)
			{
				case Key.Up:
					horizontalRec.Opacity = horizontalRec.Opacity < 1 ? horizontalRec.Opacity + 0.025 : 1;
					break;
				case Key.Down:
					horizontalRec.Opacity = horizontalRec.Opacity - 0.025 > 0 ? horizontalRec.Opacity - 0.025 : 0;
					break;
				case Key.Right:
					verticalRec.Opacity = verticalRec.Opacity < 1 ? verticalRec.Opacity + 0.025 : 1;
					break;
				case Key.Left:
					verticalRec.Opacity = verticalRec.Opacity - 0.025 > 0 ? verticalRec.Opacity - 0.025 : 0;
					break;
				default:
					break;
			}
		}

		private void WidthLineChange(Key key)
		{
			switch (key)
			{
				case Key.Up:
					horizontalRec.Height = horizontalRec.ActualHeight + HeigthScale;
					var newPosUY = (horizontalRec.RenderTransform as TranslateTransform).Y - (HeigthScale / 2);
					horizontalRec.RenderTransform = new TranslateTransform(0, newPosUY);
					break;
				case Key.Down:
					horizontalRec.Height = horizontalRec.ActualHeight - HeigthScale > 0 ? horizontalRec.ActualHeight - heigthScale : 1;
					var newPosDY = (horizontalRec.RenderTransform as TranslateTransform).Y + (HeigthScale / 2);
					horizontalRec.RenderTransform = new TranslateTransform(0, newPosDY);
					break;
				case Key.Right:
					verticalRec.Width = verticalRec.ActualWidth + WidthScale;
					var newPosRX = (verticalRec.RenderTransform as TranslateTransform).X - (WidthScale / 2);
					verticalRec.RenderTransform = new TranslateTransform(newPosRX, 0);
					break;
				case Key.Left:
					verticalRec.Width = verticalRec.ActualWidth - WidthScale > 0 ? verticalRec.ActualWidth - WidthScale : 1;
					var newPosLX = (verticalRec.RenderTransform as TranslateTransform).X + (widthScale / 2);
					verticalRec.RenderTransform = new TranslateTransform(newPosLX, 0);
					break;
				default:
					break;
			}
		}

		private void CheckControl(MenuItem obj)
		{
			foreach (var item in lineItems.Items)
			{
				if (item.GetType().Name == "MenuItem")
				{
					if (((MenuItem)item) != obj && ((MenuItem)item).Name != "showLines")
					{
						((MenuItem)item).IsChecked = false;
					}
				}
			}
		}

		private void verticalRec_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			CurrentShape = sender as Shape;
			CurrentColor = VerticalColor;
		}

		private void horizontalRec_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			CurrentShape = sender as Shape;
			CurrentColor = HorizontalColor;
		}

		private void canvasColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			if (((ColorPicker)sender).SelectedColor != null)
			{
				CurrentShape.Fill = new SolidColorBrush((Color)((ColorPicker)sender).SelectedColor);
			}
		}

		private void imgToolsBtn_Click(object sender, RoutedEventArgs e)
		{
			EnableImgSettings = true;
			imageToolsView = new ImageToolsView(this);
			imageToolsView.Show();
		}

		private async Task SetResolution()
		{
			try
			{
				var json = JsonConvert.SerializeObject(SelectedResolution);
				var data = new StringContent(json, Encoding.UTF8, "application/json");
				var url = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["ChangeResolution"]) ? ConfigurationManager.AppSettings["ChangeResolution"] : string.Empty;
				var response = await client.PostAsync(url, data);
				var result = await response.Content.ReadAsStringAsync();

				WindowWidth = SelectedResolution.width;
				WindowHeight = SelectedResolution.height + 48;

				Console.WriteLine(result);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void centerLineChange_Checked(object sender, RoutedEventArgs e)
		{
			SetLineToCenter();
		}

		private void stopBtn_Click(object sender, RoutedEventArgs e)
		{
			startBtn.Visibility = Visibility.Visible;
			stopBtn.Visibility = Visibility.Hidden;
			streamvideo.Stop();
		}
	}
}
