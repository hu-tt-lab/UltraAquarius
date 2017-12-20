using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
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
        public enum Mode
        {
            Active,
            Idle,
            Buzy,
        }

        private CancellationTokenSource cancellation;

        public MainWindow()
        {
            InitializeComponent();
            soundTable.ItemsSource = new[]
            {
                new Sound()
            };

            var files = Directory.EnumerateFiles(Directory.GetCurrentDirectory());
            calibration.ItemsSource = files.Where(x => x.Contains("calibration.csv"));
            calibration.SelectedIndex = 0;

        }

        private Mode mode;
        public Mode State
        {
            get
            {
                return mode;
            }
            set
            {
                switch (mode)
                {
                    case Mode.Active:
                        SetActive();
                        break;
                    case Mode.Buzy:
                        SetBuzy();
                        break;
                    case Mode.Idle:
                        SetIdle();
                        break;
                }
            }
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

        public List<Sound> Sounds
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

        public Dictionary<Double, List<Double>> CalibrationTable
        {
            get
            {
                using (var stream = new CsvReader(new StreamReader(calibration.SelectedValue as string)))
                {
                    var table = new Dictionary<Double, List<Double>>();

                    while (stream.Read())
                    {
                        var row = stream.CurrentRecord.Where(x => x != "").Select(x => Double.Parse(x)).ToList();
                        table[row[0]] = row.Skip(1).ToList();
                    }

                    return table;
                }
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
            cancellation = new CancellationTokenSource();
            var stream = Open();
            var random = new Random();
            Func<Tone, double, double, TimeCourse.Duration, double, SoundWave.SoundWave> mixer = (tone, ampliude, frequency, duration, samplingRate) =>
           {
               switch (tone)
               {
                   case Tone.PureTone:
                       return new SoundWave.PureTone(ampliude, frequency, duration, samplingRate);
                   case Tone.ToneBurst:
                       return new SoundWave.TonePip(ampliude, frequency, duration, samplingRate);
                   case Tone.TonePip:
                       return new SoundWave.TonePip(ampliude, frequency, duration, samplingRate);
               }
               throw new Exception("このパターンの波形はありません");
           };

            try
            {
                using (var device = new Device(TimeSpan.FromSeconds(1)))
                {

                    var sounds = Sounds;
                    var calibration = CalibrationTable;
                    var waves = sounds.Select(x => mixer(x.tone, calibration[x.frequency][(int)x.decibel], x.frequency, x.duration, device.SamplingRate)).ToList();
                    var trigger = Double.Parse(triggerVoltage.Text);
                    stream.WriteLine("start sound...");
                    stream.WriteLine("Device Sampling Rate : {0:f3}[Hz]" + stream.NewLine, device.SamplingRate);

                    var indecies = Enumerable.Range(0, trial).Select(x => new List<int>()).ToList();
                    if (Random.SelectedIndex == 1)
                    {
                        var seq = Enumerable.Range(0, waves.Count).OrderBy(x => Guid.NewGuid()).ToList();
                        for (var i = 0; i < indecies.Count; ++i)
                        {
                            indecies[i] = seq;
                        }
                    }
                    else if (Random.SelectedIndex == 2)
                    {
                        var seq = Enumerable.Range(0, waves.Count);
                        for (var i = 0; i < indecies.Count; ++i)
                        {
                            indecies[i] = seq.OrderBy(x => Guid.NewGuid()).ToList();
                        }
                    }
                    else
                    {
                        var seq = Enumerable.Range(0, waves.Count).ToList();
                        for (var i = 0; i < indecies.Count; ++i)
                        {
                            indecies[i] = seq;
                        }
                    }

                    for (var i = 0; i < trial; ++i)
                    {
                        foreach (var itr in indecies[i])
                        {
                            var duration = interval.duration + random.NextDouble() * interval.waggle;
                            stream.WriteLine("Trial Count : {0:d}", i);
                            stream.WriteLine("Tone : {0:g}", sounds[itr].tone);
                            stream.WriteLine("Frequency : {0:f3} [hz]", sounds[itr].frequency);
                            stream.WriteLine("Decibel : {0:g} [dB]", sounds[itr].decibel.ToString());
                            stream.WriteLine("Duration : {0:f3} [ms]", sounds[itr].duration.milliseconds);
                            stream.WriteLine("Interval : {0:f3} [ms]", sounds[itr].duration.milliseconds);
                            stream.WriteLine();
                            Update(stream);

                            device.Output(waves[itr].values, trigger);
                            await Task.Delay(duration.value, cancellation.Token);
                        }
                        progress.Value = i + 1;
                     }
                }
            }
            catch (TaskCanceledException)
            {
                stream.WriteLine("stop sound...");
                Update(stream);
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
            cancellation.Cancel();
            SetBuzy();
        }

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            var value = Sounds;
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
            var value = Sounds;
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
