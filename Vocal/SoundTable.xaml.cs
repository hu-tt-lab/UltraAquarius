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
        Click,
        Free
    }

    /// <summary>
    /// 音のパラメータ
    /// </summary>
    public class Sonant
    {
        public ToneType Tone { get; set; } = ToneType.PureTone;
        public double Decibel { get; set; } = 60;
        public double Frequency { get; set; } = 100;
        public double Duration { get; set; } = 100;
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
                foreach (var e in Rows)
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
            Rows.Add(new Variable { Sound = rhs });
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
        public ObservableCollection<double> Decibel { get; set; } = new ObservableCollection<double> { 50, 60, 70, 80 };
        /// <summary>
        /// Frequency Label
        /// </summary>
        public ObservableCollection<double> Frequency { get; set; } = new ObservableCollection<double> { 100, 200, 400, 800 };

        /// <summary>
        /// Raw Data
        /// </summary>
        public ObservableCollection<Variable> Rows { get; set; } = new ObservableCollection<Variable> { };

    }

    /// <summary>
    /// write csv file
    /// </summary>
    public class SonantWriter : IDisposable
    {

        class Mapper : ClassMap<Sonant>
        {
            public Mapper()
            {
                Map(x => x.Decibel).Index(0);
                Map(x => x.Frequency).Index(1);
                Map(x => x.Duration).Index(2);
            }
        }

        public SonantWriter(string name)
        {
            stream_ = new StreamWriter(name, false, Encoding.UTF8);
            writer_ = new CsvWriter(stream_);
            writer_.Configuration.HasHeaderRecord = true;
            writer_.Configuration.RegisterClassMap<Mapper>();
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

        TextWriter stream_;
        CsvWriter writer_;

    }

    /// <summary>
    /// sound wave data
    /// </summary>
    public abstract class SoundWave
    {
        public SoundWave(double sampling, double duration, ToneType type)
        {
            SamplingRate = sampling;
            Duration = duration;
            Type = type;
        }
        public abstract IEnumerable<double> Wave { get; }

        public double SamplingRate { get; }
        public double Duration { get; }
        public double Size { get { return (int)(SamplingRate * Duration); } }
        public ToneType Type { get; }
    }

    /// <summary>
    /// pure tone wave data
    /// </summary>
    public class PureWave : SoundWave
    {
        public PureWave(double frequency, double amplitude,
            double sampling, double duration) : base(sampling, duration, ToneType.PureTone)
        {
            Gain = amplitude;
            Frequency = frequency;
        }

        public override IEnumerable<double> Wave
        {
            get
            {
                for (var i = 0; i < Size; ++i)
                {
                    yield return Gain * Math.Sin(i * Frequency * 2 * Math.PI / SamplingRate);
                }
            }
        }

        public double Frequency { get; }
        public double Gain { get; }

    }

    /// <summary>
    /// tone pip wave data
    /// </summary>
    public class PipWave : SoundWave
    {
        public PipWave(double frequency, double amplitude,
            double sampling, double duration) : base(sampling, duration, ToneType.TonePip)
        {
            Gain = amplitude;
            Frequency = frequency;
        }

        public override IEnumerable<double> Wave
        {
            get
            {
                var size = Size / 2;
                for (var i = 0; i < size; ++i)
                {
                    yield return Gain * (i * Math.Sin(i * Frequency * 2 * Math.PI / SamplingRate)) / size;
                }
                for (var i = size; i < Size; ++i)
                {
                    yield return Gain * ((Size - i) * Math.Sin(i * Frequency * 2 * Math.PI / SamplingRate)) / size;
                }
            }
        }

        public double Frequency { get; }
        public double Gain { get; }

    }

    /// <summary>
    /// tone pip wave data
    /// </summary>
    public class BurstWave : SoundWave
    {
        public BurstWave(double frequency, double amplitude,
            double sampling, double duration) : base(sampling, duration, ToneType.ToneBurst)
        {
            Gain = amplitude;
            Frequency = frequency;
        }

        public override IEnumerable<double> Wave
        {
            get
            {
                var fade = Size / 10;
                for (var i = 0; i < fade; ++i)
                {
                    yield return Gain * (i * Math.Sin(i * Frequency * 2 * Math.PI / SamplingRate)) / fade;
                }
                for (var i = fade; i < Size - fade; ++i)
                {
                    yield return Gain * Math.Sin(i * Frequency * 2 * Math.PI / SamplingRate);
                }
                for (var i = Size - fade; i < Size; ++i)
                {
                    yield return Gain * ((Size - i) * Math.Sin(i * Frequency * 2 * Math.PI / SamplingRate)) / fade;
                }
            }
        }

        public double Frequency { get; }
        public double Gain { get; }

    }

    public class Trigger : SoundWave
    {
        public Trigger(double level, double sampling, double duration) : base(sampling, duration, ToneType.Free)
        {
            Level = level;
        }

        public double Level { get; }

        public override IEnumerable<double> Wave
        {
            get
            {
                var onset = Size / 10;
                for (var i = 0; i < onset; ++i)
                {
                    yield return Level;
                }
                for (var i = onset; i < Size; ++i)
                {
                    yield return 0;
                }
            }
        }
    }

}
