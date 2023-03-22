using GalaSoft.MvvmLight;
using System;
using System.Threading;
using System.Threading.Tasks;

using OxyPlot;
using OxyPlot.Axes;

using LineSeries = OxyPlot.Series.LineSeries;
using System.Collections.ObjectModel;

namespace FreezePunch.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            TempDataPoints = new ObservableCollection<DataPoint>();
            HumiDataPoints = new ObservableCollection<DataPoint>();
           
            Model = new PlotModel(){Title = "Simple Example",Subtitle = "using OxyPlot"};
            var series1 = new LineSeries { Title = "Flow", MarkerType = MarkerType.None, YAxisKey = "flow" };
            var series2 = new LineSeries { Title = "CO2", MarkerType = MarkerType.None, YAxisKey = "ppm" };
            series1.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;
            series2.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;
            var dateTimeAxis1 = new DateTimeAxis();
            dateTimeAxis1.Title = "Time";
            Model.Axes.Add(dateTimeAxis1);
            Model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "SLM", Key = "flow" });
            Model.Axes.Add(new LinearAxis { Position = AxisPosition.Right, Title = "ppm", Key = "ppm" });
            Model.Series.Add(series1);
            Model.Series.Add(series2);

            Random rd = new Random();
            Task.Run(
                () =>
                    {
                        while (true)
                        {
                            series1.Points.Add(DateTimeAxis.CreateDataPoint(DateTime.Now, rd.Next(2, 20)));
                            series2.Points.Add(DateTimeAxis.CreateDataPoint(DateTime.Now, rd.Next(100, 500)));
                            if (series1.Points.Count > 600)
                            {
                                series1.Points.RemoveAt(0);
                                series2.Points.RemoveAt(0);
                            }
                            Model.InvalidatePlot(true);
                            Thread.Sleep(1000);
                        }
                    });
        }

        private PlotModel _model;
        /// <summary>
        /// PlotModel
        /// </summary>
        public PlotModel Model
        {
            get { return _model; }
            set { Set(ref _model, value); }
        }

        private ObservableCollection<DataPoint> _tempDataPoints;
        /// <summary>
        /// ÎÂ¶È
        /// </summary>
        public ObservableCollection<DataPoint> TempDataPoints
        {
            get { return _tempDataPoints; }
            set { Set(ref _tempDataPoints, value); }
        }

        private ObservableCollection<DataPoint> _humiDataPoints;
        /// <summary>
        /// Êª¶È
        /// </summary>
        public ObservableCollection<DataPoint> HumiDataPoints
        {
            get { return _humiDataPoints; }
            set { Set(ref _humiDataPoints, value); }
        }
    }
}