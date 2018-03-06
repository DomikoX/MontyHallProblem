using Core;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private MontyHallMC MontyHall { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            SeriesCollection = new SeriesCollection();
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            MontyHall?.Cancel();

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

            long cutOff = (long)(replications * Slider.Value / 100);

            this.MontyHall = new MontyHallMC(new MontyHallInputData() { DoorsCount = doors }, replications);
            var percentage = 0.0;

            SeriesCollection.Clear();
            if (Fcl.IsChecked.Value)
            {
                SeriesCollection.Add(new LineSeries()
                {
                    Title = "First choice winning rate",
                    Stroke = Brushes.Red,

                    Fill = Brushes.Transparent,
                    PointGeometrySize = 0,
                    Values = new ChartValues<ObservablePoint>(),
                });
            }
            if (Ccl.IsChecked.Value)
            {
                SeriesCollection.Add(new LineSeries()
                {
                    Title = "Changed choice winning rate",
                    Stroke = Brushes.Blue,
                    Fill = Brushes.Transparent,
                    PointGeometrySize = 0,
                    Values = new ChartValues<ObservablePoint>(),
                });
            }
            Fcl.Content = $"First choice winning rate: 0%";
            Ccl.Content = $"Changed choice winning rate: 0%";

            MontyHall.OnNewPartialResult += (i, data) =>
            {

                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    percentage = ((double)i / replications * 100);

                    Progress.Content = $"{percentage:0} % Done";
                    ProgressBar.Value = percentage;
                    if (i > cutOff)
                    {

                        if (Fcl.IsChecked.Value)
                        {
                            SeriesCollection[0]?.Values?.Add(new ObservablePoint(i, data.FirstChoiseWinningPropability));
                            Fcl.Content = $"First choice winning rate: {data.FirstChoiseWinningPropability: 0.00}%";
                        }
                        if (Ccl.IsChecked.Value)
                        {
                            SeriesCollection[SeriesCollection.Count - 1]?.Values?.Add(new ObservablePoint(i, data.ChangedChoiseWinningPropability));
                            Ccl.Content = $"Changed choice winning rate: {data.ChangedChoiseWinningPropability: 0.00}%";
                        }
                    }
                });
            };


            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ContinueButton.IsEnabled = false;
                PauseButton.IsEnabled = true;
                RunButton.IsEnabled = false;
            });
            await MontyHall.RunExperiment();

            RunButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            ContinueButton.IsEnabled = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MontyHall.Pause();
            PauseButton.IsEnabled = false;
            RunButton.IsEnabled = true;
            ContinueButton.IsEnabled = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MontyHall.Continue();
            ContinueButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MontyHall?.Cancel();
        }
    }
}
