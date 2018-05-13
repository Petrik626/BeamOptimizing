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
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using Mathematics.Objects;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Func<double,double> Moment1;
        private Func<double, double> Moment2;
        private Func<double, double> Moment3;
        private Func<double, double> Force1;
        private Func<double, double> Force2;
        private Func<double, double> Force3;
        private double Ra  = 0.9;
        private double Rb  = -0.3;
        private double M1  = -2.0;
        private double M2  = -1.0;
        private double LengthBeam  = 5.0;
        private double A  = 1.0;
        private double Step  = (5.0 / 60.0);

        private ObservableCollection<DataPoint> PointsDiagram { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            PointsDiagram = new ObservableCollection<DataPoint>()
            {
                new DataPoint(0,0),
                new DataPoint(1,0),
                new DataPoint(2,0),
                new DataPoint(3,0)
            };

            diagramPlot.Series[0].ItemsSource = PointsDiagram;
            diagramPlot.InvalidatePlot(true);

            Force1 = (x) => 9;
            Force2 = (x) => -2 * x + 13;
            Force3 = (x) => -2 * x + 10;
            Moment1 = (x) => 9 * x - 20;
            Moment2 = (x) => -x * x + 13 * x - 34;
            Moment3 = (x) => -x * x + 10 * x - 25;
        }

        private ObservableCollection<DataPoint> GetPointsDiagram(Func<double, double> f1, Func<double, double> f2, Func<double, double> f3)
        {
            ObservableCollection<DataPoint> Points = new ObservableCollection<DataPoint>();
            for (double x = 0; x <= LengthBeam; x += Step)
            {
                if (x >= 0.0 && x <= 2 * A)
                {
                    Points.Add(new DataPoint(x, f1.Invoke(x)));
                }
                else if (x > 2 * A && x <= 3 * A)
                {
                    Points.Add(new DataPoint(x, f2.Invoke(x)));
                }
                else if (x > 3 * A && x <= 5 * A)
                {
                    Points.Add(new DataPoint(x, f3.Invoke(x)));
                }
            }

            return Points;
        }

        private void CopyPoints(ObservableCollection<DataPoint> input, ObservableCollection<DataPoint> output)
        {
            foreach(var el in input)
            {
                output.Add(el);
            }
        }

        private void MomentButton_Click(object sender, RoutedEventArgs e)
        {
            PointsDiagram.Clear();
            CopyPoints(GetPointsDiagram(Moment1, Moment2, Moment3), PointsDiagram);
        }

        private void ForceButton_Click(object sender, RoutedEventArgs e)
        {
            PointsDiagram.Clear();
            CopyPoints(GetPointsDiagram(Force1, Force2, Force3), PointsDiagram);
        }
    }
}
