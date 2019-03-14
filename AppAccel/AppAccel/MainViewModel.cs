using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AppAccel
{
    class MainViewModel : INotifyPropertyChanged
    {
        AccelerometerValue AccelerometerValue;
        float X;
        float Y;
        float Z;

        long time;
        int delta_time = 10000;

        PlotModel _plotModel;
        AreaSeries areaSerie;
        DateTimeAxis TimeAxis;

        public PlotModel Model
        {
            get { return _plotModel; }
            set
            {
                _plotModel = value;
                OnPropertyChanged("Model");
            }
        }

        public string XYZ
        {
            get => AccelerometerValue.XYZ;
            set
            {
                if (AccelerometerValue.XYZ != value)
                {
                    AccelerometerValue.XYZ = value;
                    OnPropertyChanged("XYZ");
                }
            }
        }

        public double Vector
        {
            get => AccelerometerValue.Vector;
            set
            {
                if (AccelerometerValue.Vector != value)
                {
                    AccelerometerValue.Vector = value;
                    OnPropertyChanged("Vector");
                }
            }
        }

        public ICommand ControlCommand
        {
            get => new Command(() =>
            {
                if (Accelerometer.IsMonitoring)
                    Accelerometer.Stop();
                else
                {
                    Accelerometer.Start(SensorSpeed.UI);
                    time = GetTime();
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }


        public MainViewModel()
        {
            AccelerometerValue = new AccelerometerValue();

            Model = new PlotModel();

            var minValue = DateTimeAxis.ToDouble(DateTime.Now);
            var maxValue = DateTimeAxis.ToDouble(DateTime.Now.AddSeconds(3));

            TimeAxis = new DateTimeAxis { Position = AxisPosition.Bottom, Minimum = minValue, Maximum = maxValue, StringFormat = "HH:mm:ss" };
            Model.Axes.Add(TimeAxis);
            Model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = 10, StartPosition = 0, AbsoluteMinimum = 0 });
            
            areaSerie = new AreaSeries
            {
                StrokeThickness = 1.0
            };

            Model.Series.Add(areaSerie);

            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            lock (this.Model.SyncRoot)
            {
                this.Update(e.Reading);
            }
            
            Model.InvalidatePlot(true);
        }

        private void Update(AccelerometerData data)
        {
            X = data.Acceleration.X;
            Y = data.Acceleration.Y;
            Z = data.Acceleration.Z;
            Vector = Math.Sqrt(Math.Pow(Convert.ToDouble(X), 2) + Math.Pow(Convert.ToDouble(Y), 2) + Math.Pow(Convert.ToDouble(Z), 2));
            XYZ = $"x = {String.Format("{0:0.00}", X * 10)}, " +
                $"y = {String.Format("{0:0.00}", Y * 10)}, " +
                $"z = {String.Format("{0:0.00}", Z * 10)}";

            if (GetTime() - time < delta_time)
            {
                areaSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), Vector));
            }
            else
            {
                areaSerie.Points.RemoveRange(0, 2);
                delta_time = 100;
                time = GetTime();
            }
            TimeAxis.Minimum = DateTimeAxis.ToDouble(DateTime.Now.AddSeconds(-3));
            TimeAxis.Maximum = DateTimeAxis.ToDouble(DateTime.Now.AddSeconds(3));
        }

        private long GetTime()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
