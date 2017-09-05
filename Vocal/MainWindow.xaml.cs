using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Data;
using CsvHelper;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Vocal
{
    public enum Decibel
    {
        dB10,
        dB20,
        dB30,
        dB40,
        dB50,
        dB60,
        dB70,
        dB80,
        dB90
    }

    public enum Tone
    {
        PureTone,
        TonePip,
        ToneBurst
    }

    public class Sound
    {
        public int frequency { get; set; }
        public TimeCourse.Duration duration { get; set; }
        public Decibel decibel { get; set; }
        public Tone tone { get; set; }
        public Sound()
        {
            frequency = 1000;
            duration = new TimeCourse.Duration(TimeSpan.FromMilliseconds(500));
            decibel = Decibel.dB50;
            tone = Tone.PureTone;
        }
        public Sound(Sound rhs)
        {
            frequency = rhs.frequency;
            duration = new TimeCourse.Duration(rhs.duration.value);
            decibel = rhs.decibel;
            tone = rhs.tone;
        }

    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        enum Mode
        {
            Active,
            Idle,
            Buzy,
        }

        private Random random;
        private Mode mode;

        public MainWindow()
        {
            InitializeComponent();
            soundTable.ItemsSource = new[]
            {
                new Sound()
            };
            random = new Random();

            var files = Directory.EnumerateFiles(Directory.GetCurrentDirectory());
            calibration.ItemsSource = files.Where(x => x.Contains("calibration.csv"));
            calibration.SelectedIndex = 0;

        }

        public void SetIdle()
        {
            mode = Mode.Idle;
            SoundList.IsEnabled = true;
            Option.IsEnabled = true;
            start.IsEnabled = true;
            stop.IsEnabled = false;
        }
        public void SetActive()
        {
            mode = Mode.Active;
            SoundList.IsEnabled = false;
            Option.IsEnabled = false;
            start.IsEnabled = false;
            stop.IsEnabled = true;
        }
        public void SetBuzy()
        {
            mode = Mode.Buzy;
            SoundList.IsEnabled = false;
            Option.IsEnabled = false;
            start.IsEnabled = false;
            stop.IsEnabled = false;
        }

        public List<Sound> sounds
        {
            get
            {
                var result = new List<Sound>();
                foreach (var itr in soundTable.Items)
                {
                    result.Add((Sound)itr);
                }
                return result;
            }
        }

        public Dictionary<Double, List<Double>> calibrationTable
        {
            get
            {
                var stream = new CsvReader(new StreamReader(calibration.SelectedValue as string));
                var table = new Dictionary<Double, List<Double>>();

                while (stream.Read())
                {
                    var row = stream.CurrentRecord.Where(x => x != "").Select(x => Double.Parse(x)).ToList();
                    table[row[0]] = row.Skip(1).ToList();
                }

                return table;
            }
        }

        private async void OnStartClick(object sender, RoutedEventArgs e)
        {
            SetActive();
            var interval = (duration: new TimeCourse.Duration(double.Parse(intervalDuration.Text)), waggle: new TimeCourse.Duration(double.Parse(intervalWaggle.Text)));
            var trial = int.Parse(trialCount.Text);
            progress.Value = 0;
            progress.Maximum = trial;
            progress.Minimum = 0;

            var soundList = randomCheck.IsChecked == true ? sounds.OrderBy(i => Guid.NewGuid()).ToList() : sounds.ToList();
            var calibrationData = calibrationTable;

            try
            {
#if !DEBUG
                var device = new Device();
#endif

                var stream = Open();
                stream.WriteLine("start sound..." + stream.NewLine);

                for (var i = 0; i < trial; i++)
                {
                    foreach (var itr in soundList)
                    {
                        var duration = interval.duration + random.NextDouble() * interval.waggle;
                        stream.WriteLine("Trial Count : {0:d}", i);
                        stream.WriteLine("Tone : {0:g}", itr.tone);
                        stream.WriteLine("Frequency : {0:f3} [hz]", itr.frequency);
                        stream.WriteLine("Decibel : {0:g} [dB]", itr.decibel.ToString());
                        stream.WriteLine("Duration : {0:f3} [ms]", itr.duration.milliseconds);
                        stream.WriteLine("Interval : {0:f3} [ms]", duration.milliseconds);
                        stream.WriteLine();
                        Update(stream);

#if !DEBUG
                        var signal = new SoundWave.PureTone(0.3, itr.frequency, itr.duration, device.samplingRate) * calibrationTable[itr.frequency][(int)itr.decibel];
                        device.Output(signal.values, 3.5);
#else
                        var k = calibrationTable[itr.frequency][(int)itr.decibel];
                        var signal = (new SoundWave.PureTone(0.3, itr.frequency, itr.duration, 10000)) * k;

#endif
                        await Task.Delay(duration.value);

                        if (Mode.Active != mode)
                        {
                            stream.WriteLine("stop sound..." + stream.NewLine);
                            Update(stream);
                            return;
                        }
                    }
                    progress.Value = i;
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
            finally
            {
                SetIdle();
            }
        }

        private void OnStop(object sender, RoutedEventArgs e)
        {
            SetBuzy();
        }

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            var value = sounds;
            if (value.Count == 0)
            {
                value.Add(new Sound());
            }
            else
            {
                value.Add(new Sound(value[value.Count - 1]));
            }
            soundTable.ItemsSource = value.ToArray();
        }
        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var value = sounds;
            var selected = soundTable.SelectedIndex;
            if (selected >= 0)
            {
                value.RemoveAt(selected);
                soundTable.ItemsSource = value.ToArray();
            }
        }

        public StringWriter Open()
        {
            return new StringWriter(new StringBuilder(console.Text));
        }
        public void Update(StringWriter stream)
        {
            console.Text = stream.ToString();
            console.ScrollToEnd();
        }

    }
}
