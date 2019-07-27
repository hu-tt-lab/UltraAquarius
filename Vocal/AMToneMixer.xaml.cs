using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Codeplex.Data;

namespace Vocal
{


    public class AmTone
    {
        public double Decibel { get; set; } = 60;
        public double Frequency { get; set; } = 4000;
        public double Duration { get; set; } = 100;
        public double Modulation { get; set; } = 400;
    }

    /// <summary>
    /// PureToneMixer.xaml の相互作用ロジック
    /// </summary>
    public partial class AmToneMixer : UserControl
    {

        public class Variable
        {
            public string Name { get; set; }
            public AmTone Signal { get; set; }
        }

        public AmToneMixer()
        {
            // Initialize Components
            InitializeComponent();
            Speaker.Search();

            // Data Binding
            (TableView.Columns[1] as DataGridComboBoxColumn).ItemsSource = Decibel;
            (TableView.Columns[2] as DataGridComboBoxColumn).ItemsSource = Frequency;
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
            var id = values.Count != 0?values.Select(x => int.Parse(Regex.Match(x.Name, @"\d+").Value)).Max() + 1:0;
            Rows.Add(new Variable { Name = string.Format("am{0:d}", id), Signal = new AmTone() });
        }
        public List<Variable> Save()
        {
            return Rows.ToList();
        }
        public void Load(List<Variable> rhs)
        {
            Rows.Clear();
            foreach (var i in rhs)
            {
                Rows.Add(i);
            }

        }
        public void Load(object[] rhs)
        {
            Rows.Clear();
            foreach (var i in rhs)
            {
                var json = DynamicJson.Parse(i.ToString());
                Rows.Add(new Variable { Name = json.Name, Signal = json.Signal });
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

        public AmTone Find(string key)
        {
            if (!IsLocked()) throw new Exception("Table must be Locked.");
            var variables = Rows.Where(x => x.Name == key).ToList();
            if (variables.Count != 1) throw new KeyNotFoundException(string.Format("table does not have {0:g}", key));
            return variables[0].Signal;
        }

        public double Correction(double frequency, double decibel)
        {
            return Speaker.SelectedTable[frequency, decibel];
        }

        /// <summary>
        /// Decibel Label
        /// </summary>
        public ObservableCollection<double> Decibel { get; set; } = new ObservableCollection<double> { 0 };
        /// <summary>
        /// Frequency Label
        /// </summary>
        public ObservableCollection<double> Frequency { get; set; } = new ObservableCollection<double> { 0 };

        /// <summary>
        /// Raw Data
        /// </summary>
        public ObservableCollection<Variable> Rows { get; set; } = new ObservableCollection<Variable> { };

        /// <summary>
        ///  call when changed calibration data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChangeCalibration(object sender, EventArgs e)
        {
            Decibel.Clear();
            foreach (var x in Speaker.SelectedTable.Column)
            {
                Decibel.Add(x);
            }
            Frequency.Clear();
            foreach (var x in Speaker.SelectedTable.Row)
            {
                Frequency.Add(x);
            }
        }
    }
}
