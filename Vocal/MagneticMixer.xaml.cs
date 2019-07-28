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
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Codeplex.Data;

namespace Vocal
{
    /// <summary>
    /// MagneticMixer.xaml の相互作用ロジック
    /// </summary>

        public enum MagneticWaveform
        {
        //SquarePulse,
        //FrontEdgeSawPulse,
        //LastEdgeSawPulse,
        //TrianglePulse,
            Pulse,
            Square,
            Sine
        }

        public class Magnetic
        {
            public MagneticWaveform Waveform { get; set; } = MagneticWaveform.Pulse;
            public double Voltage { get; set; } = 1.0;
            public double Duration { get; set; } = 500;
            public double Frequency { get; set; } = 1000;
            public double RaiseDuration { get; set; } = 50.1;
            public double FallDuration { get; set; } = 50.1;
            public double Interval { get; set; } = 10;
            public int Waves { get; set; } = 1;


            
            public Magnetic(JsonMagnetic rhs)
            {
                Waveform = (MagneticWaveform)Enum.Parse(typeof(MagneticWaveform), rhs.Waveform, true);
                Voltage = rhs.Voltage;
                Duration = rhs.Duration;
                Frequency = rhs.Frequency;
                RaiseDuration = rhs.RaiseDuration;
                FallDuration = rhs.FallDuration;
                Interval = rhs.Interval;
                Waves = rhs.Waves;
            }

            public Magnetic() { }

        }
        
        public class JsonMagnetic
        {
            public string Waveform { get; set; } = "Pulse";
            public double Voltage { get; set; } = 1;
            public double Duration { get; set; } = 500;
            public double Frequency { get; set; } = 1000;
            public double RaiseDuration { get; set; } = 50.1;
            public double FallDuration { get; set; } = 50.1;
            public double Interval { get; set; } = 10;
            public int Waves { get; set; } = 1;

            public JsonMagnetic(Magnetic rhs)
            {
                Waveform = rhs.Waveform.ToString();
                Voltage = rhs.Voltage;
                Duration = rhs.Duration;
                Frequency = rhs.Frequency;
                RaiseDuration = rhs.RaiseDuration;
                FallDuration = rhs.FallDuration;
                Interval = rhs.Interval;
                Waves = rhs.Waves;
            }
    }

        /// <summary>
        /// PureToneMixer.xaml の相互作用ロジック
        /// </summary>
        public partial class MagneticMixer : UserControl
        {

            public class Variable
            {
                public string Name { get; set; }
                public Magnetic Signal { get; set; }
            }

            public MagneticMixer()
            {
                // Initialize Components
                InitializeComponent();

                // Data Binding
                TableView.DataContext = Rows;

            }

            /// <summary>
            /// Delete selected row
            /// </summary>
            public void Pop()
            {
                var index = TableView.SelectedIndex;
                if (index >= 0)
                {
                    Rows.RemoveAt(index);
                }
            }

            /// <summary>
            /// Push row
            /// </summary>
            public void Push()
            {
                var values = Rows.Where(x => Regex.IsMatch(x.Name, @"\d+")).ToList();
                var id = values.Count != 0 ? values.Select(x => int.Parse(Regex.Match(x.Name, @"\d+").Value)).Max() + 1 : 0;
                Rows.Add(new Variable { Name = string.Format("mg{0:d}", id), Signal = new Magnetic() });
            }

            public List<Variable> Save()
            {
                return Rows.ToList();
            }
            public void Load(object[] rhs)
            {
                Rows.Clear();
                foreach (var i in rhs)
                {
                    var tmp = new Magnetic();
                    var json = DynamicJson.Parse(i.ToString());
                    tmp.Waveform = (MagneticWaveform)Enum.Parse(typeof(MagneticWaveform), json.Signal.Waveform, true);
                    tmp.Voltage = (double)json.Signal.Voltage;
                    tmp.Duration = (double)json.Signal.Duration;
                    tmp.Frequency = (double)json.Signal.Frequency;
                    tmp.RaiseDuration = (double)json.Signal.RaiseDuration;
                    tmp.FallDuration = (double)json.Signal.FallDuration;
                    tmp.Interval = (double)json.Signal.Interval;
                    tmp.Waves = (int)json.Signal.Waves;
                    Rows.Add(new Variable { Name = json.Name, Signal = tmp });
                }

            }
            private void OnAdd(object sender, RoutedEventArgs e)
            {
                Push();
            }
            private void OnDelete(object sender, RoutedEventArgs e)
            {
                Pop();
            }

            private bool Key = false;

            public void Lock()
            {
                var keys = Rows.Select(x => x.Name);
                if (keys.Count() != keys.Distinct().Count()) throw new Exception("The key must be unique.");

                Key = true;
                IsEnabled = false;
            }

            public void Unlock()
            {
                Key = false;
                IsEnabled = true;
            }

            bool IsLocked()
            {
                return Key;
            }
            

            public Magnetic Find(string key)
            {
                if (!IsLocked()) throw new Exception("Table must be Locked.");
                var variables = Rows.Where(x => x.Name == key).ToList();
                if (variables.Count != 1) throw new KeyNotFoundException(string.Format("table does not have {0:g}", key));
                return variables[0].Signal;
            }

            /// <summary>
            /// Raw Data
            /// </summary>
            public ObservableCollection<Variable> Rows { get; set; } = new ObservableCollection<Variable> { };

        }
}

