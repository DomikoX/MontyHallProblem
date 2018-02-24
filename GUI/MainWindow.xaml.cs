using Core;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }
        public LineSeries LineSeriesFirstChoice { get; set; }
        public LineSeries LineSeriesChangedChoice { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            LineSeriesFirstChoice = new LineSeries()
            {
                Title = "First choice winning rate",
                Fill = Brushes.Transparent,
                PointGeometrySize = 5,
            };
            LineSeriesChangedChoice = new LineSeries()
            {
                Title = "Changed choice winning rate",
                Fill = Brushes.Transparent,
                PointGeometrySize = 5,
            };

            SeriesCollection = new SeriesCollection()
            {

                LineSeriesChangedChoice,
                LineSeriesFirstChoice,

            };
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Int32.TryParse(DoorCount.Text, out var doors) || (doors < 3 || doors > 10))
            {
                MessageBox.Show("Please insert correct number of doors <3,10>", "Wrong input");
                return;
            }
            if (!Int64.TryParse(Replications.Text, out var replications) || replications < 100)
            {
                MessageBox.Show("Please insert correct number of replications (> 100)", "Wrong input");
                return;
            }

            RunButton.IsEnabled = false;
            var mc = new MonteCarlo(doors, replications);
            var percentage = 0;

            LineSeriesFirstChoice.Values = new ChartValues<ObservablePoint>();
            LineSeriesChangedChoice.Values = new ChartValues<ObservablePoint>();
            mc.ChangedChoiceResult += (i, p) =>
           {
               Application.Current.Dispatcher.InvokeAsync(() =>
               {
                   LineSeriesChangedChoice.Values.Add(new ObservablePoint(i, (double)p));
                   Ccl.Content = $"Changed choice winning rate: {p: 0.00}%";
                   Progress.Content = $"{++percentage} % Done";
               });

           };
            mc.FirstChoiceResult += (i, p) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    LineSeriesFirstChoice.Values.Add(new ObservablePoint(i, (double)p));
                    Fcl.Content = $"First choice winning rate: {p: 0.00}%";
                });

            };

            await mc.RunExperiment();
            RunButton.IsEnabled = true;
            await Application.Current.Dispatcher.InvokeAsync(() =>
             {
                 Progress.Content = $"100 % Done";
             });

        }

    }
}
