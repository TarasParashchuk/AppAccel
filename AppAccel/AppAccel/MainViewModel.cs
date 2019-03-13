using OxyPlot;
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
        public event PropertyChangedEventHandler PropertyChanged;
        public AccelerometerValue AccelerometerValue;
        private float X;
        private float Y;
        private float Z;
        private LineSeries series1;
        private PlotModel Plot;
        private int i = 0;

        public PlotModel Model { get; private set; }

        public MainViewModel()
        {
            AccelerometerValue = new AccelerometerValue();

            Plot = new PlotModel ();
            series1 = new LineSeries { Title = "Series 1", MarkerType = MarkerType.Circle };
            Plot.Series.Add(series1);
            this.Model = Plot;
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            
            var data = e.Reading;
            X = data.Acceleration.X;
            Y = data.Acceleration.Y;
            Z = data.Acceleration.Z;
            Vector = Math.Sqrt(Math.Pow(Convert.ToDouble(X), 2) + Math.Pow(Convert.ToDouble(Y), 2) + Math.Pow(Convert.ToDouble(Z), 2));
            XYZ = $"x = {String.Format("{0:0.00}", X * 10)}, " +
                $"y = {String.Format("{0:0.00}", Y * 10)}, " +
                $"z = {String.Format("{0:0.00}", Z * 10)}";
            series1.Points.Add(new DataPoint(i++, Vector));
            
        }
        
        public string XYZ
        {
            get => AccelerometerValue.XYZ;
            set
            {
                if(AccelerometerValue.XYZ != value)
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
                    Accelerometer.Start(SensorSpeed.UI);
            });
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
