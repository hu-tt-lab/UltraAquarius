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
using Codeplex.Data;


namespace Vocal
{
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

        public MainWindow()
        {
            InitializeComponent();

            // set configure
            try
            {
                using (var reader = new StreamReader("info.json"))
                {
                    var config = DynamicJson.Parse(reader.ReadToEnd());

                    Trial = (int)config.trial;
                    TriggerLevel = config.trigger;

                    Configure.Identifer = config.device.identifer;
                    Configure.SamplingRate = config.device.samplingRate;

                    Interval.Duration = config.interval.duration;
                    Interval.Waggle = config.interval.waggle;

                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message).Return().End();
            }

            

        }

        // manager to stop an async thread
        private CancellationTokenSource Cancellation;

        // output trigger level
        public double TriggerLevel { get { return double.Parse(triggerVoltage.Text); } set { triggerVoltage.Text = value.ToString(); } }
        // output count
        public int Trial { get { return int.Parse(trialCount.Text); } set { trialCount.Text = value.ToString(); } }

        private async void OnStartClick(object sender, RoutedEventArgs e)
        {
            // set window configure
            SetActive();
            var trial = Trial;
            progress.Value = 0;
            progress.Maximum = trial;
            progress.Minimum = 0;
            Cancellation = new CancellationTokenSource();

            Console.WriteLine("Start to output")
                .WriteLine("Device number: {0:g}", Configure.Identifer)
                .WriteLine("Channel Mode: Sound = {0:g}, Trigger = {1:g}", Configure.SoundChannel, Configure.TriggerChannel)
                .WriteLine("Trigger Level: {0:f1}", TriggerLevel)
                .Return()
                .End();

            // create sound set
            var waves = Table.Table.Select(new Func<Sonant, SoundWave>(x =>
            {
                var amplitude = Speaker.SelectedTable[x.Frequency, x.Decibel];
                switch (x.Tone)
                {
                    case ToneType.PureTone:
                        return new PureWave(x.Frequency, amplitude, Configure.SamplingRate, x.Duration);
                    case ToneType.TonePip:
                        return new PipWave(x.Frequency, amplitude, Configure.SamplingRate, x.Duration);
                    case ToneType.ToneBurst:
                        return new BurstWave(x.Frequency, amplitude, Configure.SamplingRate, x.Duration);
                    default:
                        throw new ArgumentException("this parameter is invalid.");
                }
            }))
            .Select(x => (sound: x, trigger: new Trigger(TriggerLevel, Configure.SamplingRate, x.Duration)));


            try
            {
                // create device buffer
                var duration = waves.Max(x => x.sound.Duration);
                using (var device = new Device(Configure.SamplingRate, duration, Configure.SoundChannel, Configure.TriggerChannel))
                {
                    var table = new List<(SoundWave, Trigger)>[trial];
                    if (Random.SelectedIndex == 1)
                    {
                        var seq = waves.OrderBy(x => Guid.NewGuid()).ToList();
                        for (var i = 0; i < trial; ++i)
                        {
                            table[i] = seq;
                        }
                    }
                    else if (Random.SelectedIndex == 2)
                    {
                        for (var i = 0; i < trial; ++i)
                        {
                            table[i] = waves.OrderBy(x => Guid.NewGuid()).ToList();
                        }
                    }
                    else
                    {
                        var seq = waves.ToList();
                        for (var i = 0; i < trial; ++i)
                        {
                            table[i] = seq;
                        }
                    }

                    for (var i = 0; i < trial; ++i)
                    {
                        foreach (var (signal, trigger) in table[i])
                        {
                            device.Output(signal.Wave, trigger.Wave);
                            Console.WriteLine("Output Sound!")
                                .Return()
                                .End();
                            await Task.Delay(Interval.Interval, Cancellation.Token);
                        }
                        progress.Value = i + 1;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Stop to output sound").End();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }

            // write sound list
            var root = "result";
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);
            using (var stream = new SonantWriter(string.Format("{0:g}/result.csv", root)))
            {
                stream.Write(Table.Table);
            }

            SetIdle();
        }

        private void OnStop(object sender, RoutedEventArgs e)
        {
            Cancellation.Cancel();
            SetBuzy();
        }

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            Table.Push();
        }
        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            Table.Pop();
        }

        private void OnChanged(object sender, EventArgs e)
        {
            Table.Decibel.Clear();
            foreach(var x in Speaker.SelectedTable.Column)
            {
                Table.Decibel.Add(x);
            }
            Table.Frequency.Clear();
            foreach (var x in Speaker.SelectedTable.Row)
            {
                Table.Frequency.Add(x);
            }
        }

    }
}
