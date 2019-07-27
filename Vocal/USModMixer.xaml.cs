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
    public enum USModWindowType
    {
        Sine,
        Liner
    }

    public class USMod
    {
        public USModWindowType WindowType { get; set; } = USModWindowType.Sine;
        public double Voltage { get; set; } = 0.5;
        public double Frequency { get; set; } = 500000;
        public double Waves { get; set; } = 1000;
        public double WindowWaves { get; set; } = 100;

        public USMod() { }

        public USMod(JsonUSMod rhs)
        {
            WindowType = (USModWindowType)Enum.Parse(typeof(USModWindowType), rhs.WindowType, true);
            Voltage = rhs.Voltage;
            Frequency = rhs.Frequency;
            Waves = rhs.Waves;
            WindowWaves = rhs.WindowWaves;
        }

    }

    public class JsonUSMod
    {
        public string WindowType { get; set; } = "Sine";
        public double Voltage { get; set; } = 0.5;
        public double Frequency { get; set; } = 500000;
        public double Waves { get; set; } = 1000;
        public double WindowWaves { get; set; } = 100;

        public JsonUSMod(USMod rhs)
        {
            WindowType = rhs.WindowType.ToString();
            Voltage = rhs.Voltage;
            Frequency = rhs.Frequency;
            Waves = rhs.Waves;
            WindowWaves = rhs.WindowWaves;
        }
    }

    /// <summary>
    /// PureToneMixer.xaml の相互作用ロジック
    /// </summary>
    public partial class USModMixer : UserControl
    {

        public class Variable
        {
            public string Name { get; set; }
            public USMod Signal { get; set; }
        }

        public USModMixer()
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
            Rows.Add(new Variable { Name = string.Format("usm{0:d}", id), Signal = new USMod() });
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
                var tmp = new USMod();
                var json = DynamicJson.Parse(i.ToString());
                tmp.Frequency = (double)json.Signal.Frequency;
                tmp.Voltage = (double)json.Signal.Voltage;
                tmp.Waves = (double)json.Signal.Waves;
                tmp.WindowType = (USModWindowType)Enum.Parse(typeof(USModWindowType), json.Signal.WindowType, true); ;
                tmp.WindowWaves = (double)json.Signal.WindowWaves;
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

        public USMod Find(string key)
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
