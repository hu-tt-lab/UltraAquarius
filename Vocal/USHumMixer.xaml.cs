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
    /// 音の種類
    /// </summary>
    public enum USHumWindowType
    {
        Sine
    }

    public class USHum
    {
        public USHumWindowType WindowType { get; set; } = USHumWindowType.Sine;
        public double Voltage { get; set; } = 0.5;
        public double Frequency { get; set; } = 500000;
        public double Waves { get; set; } = 20;
        public double WindowWaves { get; set; } = 5;
        public double PRF { get; set; } = 1500;
        public int Pulses { get; set; } = 150;

        public USHum() { }

        public USHum(JsonUSHum rhs)
        {
            WindowType = (USHumWindowType)Enum.Parse(typeof(USHumWindowType), rhs.WindowType, true);
            Voltage = rhs.Voltage;
            Frequency = rhs.Frequency;
            Waves = rhs.Waves;
            WindowWaves = rhs.WindowWaves;
            PRF = rhs.PRF;
            Pulses = rhs.Pulses;

        }

    }

    public class JsonUSHum
    {
        public string WindowType { get; set; } = "Sine";
        public double Voltage { get; set; } = 0.5;
        public double Frequency { get; set; } = 500000;
        public double Waves { get; set; } = 20;
        public double WindowWaves { get; set; } = 5;
        public double PRF { get; set; } = 1500;
        public int Pulses { get; set; } = 150;

        public JsonUSHum(USHum rhs)
        {
            WindowType = rhs.WindowType.ToString();
            Voltage = rhs.Voltage;
            Frequency = rhs.Frequency;
            Waves = rhs.Waves;
            WindowWaves = rhs.WindowWaves;
            PRF = rhs.PRF;
            Pulses = rhs.Pulses;
        }
    }

    /// <summary>
    /// PureToneMixer.xaml の相互作用ロジック
    /// </summary>
    public partial class USHumMixer : UserControl
    {

        public class Variable
        {
            public string Name { get; set; }
            public USHum Signal { get; set; }
        }

        public USHumMixer()
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
            Rows.Add(new Variable { Name = string.Format("ush{0:d}", id), Signal = new USHum() });
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
                var tmp = new USHum();
                var json = DynamicJson.Parse(i.ToString());
                tmp.Frequency = (double)json.Signal.Frequency;
                tmp.Voltage = (double)json.Signal.Voltage;
                tmp.Waves = (double)json.Signal.Waves;
                tmp.WindowType = (USHumWindowType)Enum.Parse(typeof(USHumWindowType), json.Signal.WindowType, true); ;
                tmp.WindowWaves = (double)json.Signal.WindowWaves;
                tmp.Pulses = (int)json.Signal.Pulses;
                tmp.PRF = (double)json.Signal.PRF;
                Rows.Add(new Variable { Name = json.Name, Signal = tmp});
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

        public USHum Find(string key)
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
