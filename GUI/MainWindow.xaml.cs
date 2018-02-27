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
        private MontyHallMC MontyHall { get; set; }
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
            PauseButton.IsEnabled = true;
            this.MontyHall = new MontyHallMC(new MontyHallInputData() { DoorsCount = doors }, replications);
            var percentage = 0;

            LineSeriesFirstChoice.Values = new ChartValues<ObservablePoint>();
            LineSeriesChangedChoice.Values = new ChartValues<ObservablePoint>();

            MontyHall.OnNewPartialResult += (i, data) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    LineSeriesChangedChoice.Values.Add(new ObservablePoint(i, data.ChangedChoiseWinningPropability));
                    Ccl.Content = $"Changed choice winning rate: {data.ChangedChoiseWinningPropability: 0.00}%";
                    LineSeriesFirstChoice.Values.Add(new ObservablePoint(i, data.FirstChoiseWinningPropability));
                    Fcl.Content = $"First choice winning rate: {data.FirstChoiseWinningPropability: 0.00}%";
                    Progress.Content = $"{++percentage} % Done";
                });
            };
            
            await MontyHall.RunExperiment();
            RunButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            ContinueButton.IsEnabled = false;
            await Application.Current.Dispatcher.InvokeAsync(() =>
             {
                 Progress.Content = $"100 % Done";
             });

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {            
            MontyHall.Pause();
            PauseButton.IsEnabled = false;
            ContinueButton.IsEnabled = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MontyHall.Continue();
            ContinueButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
        }
    }
}
