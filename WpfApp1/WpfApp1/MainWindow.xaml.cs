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
using OxyPlot.Wpf;
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
        private readonly double Ra  = 0.9;
        private readonly double Rb  = -0.3;
        private readonly double M1  = -2.0;
        private readonly double M2  = -1.0;
        private readonly double LengthBeam  = 5.0;
        private readonly double A  = 1.0;
        private readonly double Step  = (5.0 / 60.0);
        private readonly double E = 12000000.0;
        private readonly double Sigma = 16000;

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

            SetScopesDataGrid();
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

        private ObservableCollection<DataPoint> FindOptimizingParameter()
        {
            ObservableCollection<DataPoint> force = GetPointsDiagram(Force1, Force2, Force3);
            ObservableCollection<DataPoint> moment = GetPointsDiagram(Moment1, Moment2, Moment3);
            ObservableCollection<DataPoint> aoptimal = new ObservableCollection<DataPoint>();
            double sqrt3 = Math.Sqrt(3);

            double y = 0.0, z = 0.0, max;
            for(int i=0; i<force.Count; i++)
            {
                y = Math.Abs(6.0 * moment[i].Y / Sigma);
                z = Math.Abs(3 * force[i].Y * sqrt3 / (2 * Sigma));

                max = Math.Max(Math.Pow(y, (1.0 / 3.0)), Math.Sqrt(z));

                aoptimal.Add(new DataPoint(force[i].X, max));
            }

            return aoptimal;
        }

        private ObservableCollection<DataPoint> GetOptimizingParameter(ObservableCollection<DataPoint> input, double percent)
        {
            ObservableCollection<DataPoint> dataPoints = new ObservableCollection<DataPoint>();
            double amax = input.Max(p => p.Y);

            foreach(var el in input)
            {
                if(el.Y<amax*percent)
                {
                    dataPoints.Add(new DataPoint(el.X, amax * percent));
                }
                else
                {
                    dataPoints.Add(el);
                }
            }

            return dataPoints;
        }

        private double FindScope(IEnumerable<DataPoint> points, double step)
        {
            return points.Sum(p => p.Y * step);
        }

        private Tuple<double,double,double,double,double,double> FindScopes()
        {
            var optimal = FindOptimizingParameter();
            var optimalTwo = GetOptimizingParameter(optimal, 0.2);
            var optimalFour = GetOptimizingParameter(optimal, 0.4);
            var optimalSix = GetOptimizingParameter(optimal, 0.6);
            var optimalEight = GetOptimizingParameter(optimal, 0.8);
            var optimalTen = GetOptimizingParameter(optimal, 1.0);

            double v1 = FindScope(optimal, Step);
            double v2 = FindScope(optimalTwo, Step);
            double v3 = FindScope(optimalFour, Step);
            double v4 = FindScope(optimalSix, Step);
            double v5 = FindScope(optimalEight, Step);
            double v6 = FindScope(optimalTen, Step);

            return new Tuple<double, double, double, double, double, double>(v1, v2, v3, v4, v5, v6);
        }

        private void SetScopesDataGrid()
        {
            var scopes = FindScopes();


            var scope1 = new { Percent = 0.0, Scope = scopes.Item1 };
            var scope2 = new { Percent = 20.0, Scope = scopes.Item2 };
            var scope3 = new { Percent = 40.0, Scope = scopes.Item3 };
            var scope4 = new { Percent = 60.0, Scope = scopes.Item4 };
            var scope5 = new { Percent = 80.0, Scope = scopes.Item5 };
            var scope6 = new { Percent = 100.0, Scope = scopes.Item6 };

            var list = new[] { scope1, scope2, scope3, scope4, scope5, scope6 };

            ScopesDataGrid.ItemsSource = list;
        }

        private void MomentButton_Click(object sender, RoutedEventArgs e)
        { 
            PointsDiagram.Clear();
            diagramPlot.Title = "Эпюра моментов";
            diagramPlot.Axes.Clear();
            CopyPoints(GetPointsDiagram(Moment1, Moment2, Moment3), PointsDiagram);
        }

        private void ForceButton_Click(object sender, RoutedEventArgs e)
        {
            PointsDiagram.Clear();
            diagramPlot.Title = "Эпюра перерезывающих сил";
            diagramPlot.Axes.Clear();
            diagramPlot.Axes.Add(new LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, Minimum = 0, Maximum = 5 });
            diagramPlot.Axes.Add(new LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, Minimum = 0, Maximum = 15 });
            CopyPoints(GetPointsDiagram(Force1, Force2, Force3), PointsDiagram);
        }

        private void OptimalButton_Click(object sender, RoutedEventArgs e)
        {
            PointsDiagram.Clear();
            var optimal = FindOptimizingParameter();
            diagramPlot.Title = "Оптимальная конструкция";
            diagramPlot.Axes.Clear();
            CopyPoints(optimal, PointsDiagram);
        }

        private void OptimalTwoPercentButton_Click(object sender, RoutedEventArgs e)
        {
            PointsDiagram.Clear();
            var optimal = FindOptimizingParameter();

            var optimalTwoPercent = GetOptimizingParameter(optimal, 0.2);

            diagramPlot.Title = "20 Процентов";
            diagramPlot.Axes.Clear();
            CopyPoints(optimalTwoPercent, PointsDiagram);
        }

        private void OptimalFourPercentButton_Click(object sender, RoutedEventArgs e)
        {
            PointsDiagram.Clear();
            var optimal = FindOptimizingParameter();

            var optimalFourPercent = GetOptimizingParameter(optimal, 0.4);

            diagramPlot.Title = "40 Процентов";
            diagramPlot.Axes.Clear();
            CopyPoints(optimalFourPercent, PointsDiagram);
        }

        private void OptimalSixPercentButton_Click(object sender, RoutedEventArgs e)
        {
            PointsDiagram.Clear();
            var optimal = FindOptimizingParameter();

            var optimalSixPercent = GetOptimizingParameter(optimal, 0.6);

            diagramPlot.Title = "60 Процентов";
            diagramPlot.Axes.Clear();
            CopyPoints(optimalSixPercent, PointsDiagram);
        }

        private void OptimalEightPercentButton_Click(object sender, RoutedEventArgs e)
        {
            PointsDiagram.Clear();
            var optimal = FindOptimizingParameter();

            var optimalEigthPercent = GetOptimizingParameter(optimal, 0.8);

            diagramPlot.Title = "80 Процентов";
            diagramPlot.Axes.Clear();
            CopyPoints(optimalEigthPercent, PointsDiagram);
        }

        private void OptimalTenPercentButton_Click(object sender, RoutedEventArgs e)
        {
            PointsDiagram.Clear();
            var optimal = FindOptimizingParameter();

            var optimalTenPercent = GetOptimizingParameter(optimal, 1);

            diagramPlot.Title = "100 Процентов";
            diagramPlot.Axes.Clear();
            CopyPoints(optimalTenPercent, PointsDiagram);
        }
    }
}
