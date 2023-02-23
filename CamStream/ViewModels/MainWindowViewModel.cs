using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CamStream.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
        double widthScale;
        double heigthScale;
        Shape currentShape = null;
        public int threshold;
        public double WidthScale { get => widthScale; set { widthScale = value; OnPropertyChanged("WidthScale"); } }

        public double HeigthScale { get => heigthScale; set { heigthScale = value; OnPropertyChanged("HeigthScale"); } }
        public Shape CurrentShape { get => currentShape; set { currentShape = value; OnPropertyChanged("CurrentShape"); } }

        public bool isGrayscaled;

        public bool IsGrayscaled
        {
            get { return isGrayscaled; }
            set { isGrayscaled = value; OnPropertyChanged("IsGrayscaled"); }
        }

        public int Threshold
        {
            get { return threshold; }
            set { threshold = value; OnPropertyChanged("Threshold"); }
        }

        public MainWindowViewModel()
		{

		}
    }
}
