using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using CsvHelper;


namespace Vocal
{
    /// <summary>
    /// Pivot Data Table
    /// </summary>
    /// <typeparam name="type">element type</typeparam>
    public class PivotTable<type>
    {
        // constructor
        public PivotTable(IEnumerable<double> header, IEnumerable<double> index)
        {
            Data = new type[index.Count(), header.Count()];
            Column = header.ToArray();
            Row = index.ToArray();
        }

        // accessor
        public type this[int v, int u]
        {
            get
            {
                return Data[v, u];
            }
            set
            {
                Data[v, u] = value;
            }
        }

        // accessor
        public type this[double v, double u]
        {
            get
            {
                return Data[Array.IndexOf(Row, v), Array.IndexOf(Column, u)];
            }
            set
            {
                Data[Array.IndexOf(Row, v), Array.IndexOf(Column, u)] = value;
            }
        }

        type[,] Data;

        // row label
        public double[] Row { get; }
        // column label
        public double[] Column { get; }
    }

    /// <summary>
    /// SpeakerControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SpeakerControl : UserControl
    {
        public SpeakerControl()
        {
            InitializeComponent();

            // Data Binding
            CalibrationList.ItemsSource = Path;

        }

        public void Search()
        {
           ;
            foreach (var e in Directory.EnumerateFiles(Directory.GetCurrentDirectory())
                .Where(x => Regex.IsMatch(x, string.Format(@"{0:g}$", Extension))))
            {
                if (!Path.Contains(e))
                {
                    Path.Add(e);
                    Calibrations.Add(read(e));
                    if (CalibrationList.SelectedIndex < 0)
                    {
                        CalibrationList.SelectedIndex = 0;
                    }
                }
            }
        }

        public string Selected { get { return CalibrationList.SelectedItem.ToString(); } }
        public ObservableCollection<string> Path { get; set; } = new ObservableCollection<string>();
        public List<PivotTable<double>> Calibrations { get; } = new List<PivotTable<double>>();
        public PivotTable<double> SelectedTable { get { return Calibrations[CalibrationList.SelectedIndex];  } }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = string.Format("(*{0:g})|*.*", Extension);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!Path.Contains(dialog.FileName))
                    {
                        Path.Add(dialog.FileName);
                        Calibrations.Add(read(dialog.FileName));
                        if (CalibrationList.SelectedIndex < 0)
                        {
                            CalibrationList.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        public event EventHandler Changed;

        private void SelectionChanged(object sender, EventArgs e)
        {
            Changed?.Invoke(sender, e);
        }

        private PivotTable<double> read(string path)
        {
            PivotTable<double> table;
            using (var stream = new StreamReader(path))
            using (var parser = new CsvReader(stream))
            {
                // read header
                parser.Read();
                var column = parser.Context.Record.Where(x => x != "").Select(x => double.Parse(x));

                // read table
                var index = new List<double>();
                var data = new List<IEnumerable<double>>();
                while (parser.Read())
                {
                    var row = parser.Context.Record.Where(x => x != "")
                        .Select(x => double.Parse(x))
                        .ToList();
                    index.Add(row[0]);
                    data.Add(row.Skip(1));
                }

                //create table
                table = new PivotTable<double>(column, index);
                foreach (var (row, x) in data.Select((e, i) => (i, e)))
                {
                    foreach (var (col, v) in x.Select((e, i) => (i, e)))
                    {
                        table[row, col] = v;
                    }
                }

            }
            return table;
        }

        public String Extension { get; set; } = ".csv";
    }
}
