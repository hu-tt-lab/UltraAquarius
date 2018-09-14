using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace Vocal
{

    /// <summary>
    /// 音の種類
    /// </summary>
    public enum ToneType
    {
        PureTone,
        TonePip,
        ToneBurst,
        Click
    }

    /// <summary>
    /// 音のパラメータ
    /// </summary>
    public class Sonant
    {
        public ToneType Tone { get; set; } = ToneType.PureTone;
        public Double Decibel { get; set; } = 60;
        public Double Frequency { get; set; } = 100;
        public Double Duration { get; set; } = 100;
    }

    /// <summary>
    /// SoundTable.xaml の相互作用ロジック
    /// </summary>
    public partial class SoundTable : UserControl
    {
        public class Variable
        {
            public bool Valid { get; set; } = true;
            public Sonant Sound { get; set; } = new Sonant();
        }

        public SoundTable()
        {
            // Initialize Components
            InitializeComponent();

            // Data Binding
            (TableView.Columns[2] as DataGridComboBoxColumn).ItemsSource = Decibel;
            (TableView.Columns[3] as DataGridComboBoxColumn).ItemsSource = Frequency;
            TableView.ItemsSource = Rows;
        }

        ///// <summary>
        ///// Sound Table
        ///// </summary>
        public IEnumerable<Sonant> Table
        {
            get
            {
                foreach(var e in Rows)
                {
                    if (e.Valid)
                    {
                        yield return e.Sound;
                    }
                }
            }
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
        /// Add row to end
        /// </summary>
        /// <param name="rhs">sound parameter</param>
        public void Add(Sonant rhs)
        {
            Rows.Add(new Variable { Sound=rhs });
        }

        /// <summary>
        /// Insert row
        /// </summary>
        /// <param name="index">index to insert object into<param>
        public void Insert(int index)
        {
            Rows.Insert(index, Rows[index]);
        }
        /// <summary>
        /// Insert row to end.
        /// </summary>
        public void Insert()
        {
            Insert(Rows.Count - 1);
        }
        
        /// <summary>
        /// Push row
        /// </summary>
        public void Push()
        {
            var index = TableView.SelectedIndex;
            if (index >= 0)
            {
                Insert(index);
            }
            else
            {
                Rows.Add(new Variable());
            }
        }

        /// <summary>
        /// Decibel Label
        /// </summary>
        public ObservableCollection<Double> Decibel { get; set; } = new ObservableCollection<double> { 50, 60, 70, 80 };
        /// <summary>
        /// Frequency Label
        /// </summary>
        public ObservableCollection<Double> Frequency { get; set; } = new ObservableCollection<double> { 100, 200, 400, 800 };

        /// <summary>
        /// Raw Data
        /// </summary>
        public ObservableCollection<Variable> Rows { get; set; } = new ObservableCollection<Variable> { };

    }

    class SonantWriter: IDisposable
    {
        class Mapper : CsvClassMap<Sonant>
        {
            Mapper()
            {
                Map(x => x.Tone.ToString()).Index(0);
                Map(x => x.Decibel).Index(1);
                Map(x => x.Frequency).Index(2);
                Map(x => x.Duration).Index(3);
            }
        }

        public SonantWriter(string name)
        {
            stream_ = new StreamWriter(name);
            writer_ = new CsvWriter(stream_);
            writer_.Configuration.HasHeaderRecord = true;
            writer_.Configuration.RegisterClassMap(typeof(Mapper));
        }

        public void Write(IEnumerable<Sonant> rhs)
        {
            writer_.WriteRecords(rhs);
        }

        public void Write(Sonant rhs)
        {
            writer_.WriteRecord(rhs);
        }

        public void Dispose()
        {
            writer_.Dispose();
            stream_.Dispose();
        }

        StreamWriter stream_;
        CsvWriter writer_;
    }
}
