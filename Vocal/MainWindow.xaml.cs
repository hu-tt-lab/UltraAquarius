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
            Option.IsEnabled = true;
            Start.IsEnabled = true;
            Stop.IsEnabled = false;
            Mixer.UnLock();
            Output.IsEnabled = true;
        }
        public void SetActive()
        {
            mode = Mode.Active;
            Option.IsEnabled = false;
            Start.IsEnabled = false;
            Stop.IsEnabled = true;
            Mixer.Lock(Configure.SamplingRate);
            Output.IsEnabled = false;
        }
        public void SetBuzy()
        {
            mode = Mode.Buzy;
            Option.IsEnabled = false;
            Start.IsEnabled = false;
            Stop.IsEnabled = false;
            Mixer.Lock(Configure.SamplingRate);
            Output.IsEnabled = false;
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

            try
            {
                // set window configure
                SetActive();
                var trial = Trial;
                Progress.Value = 0;
                Progress.Maximum = trial;
                Progress.Minimum = 0;
                Cancellation = new CancellationTokenSource();

                Console.WriteLine("Start to output")
                    .WriteLine("Device number: {0:g}", Configure.Identifer)
                    .WriteLine("Channel Mode: Sound = {0:g}, Trigger = {1:g}", Configure.SoundChannel, Configure.TriggerChannel)
                    .WriteLine("Trigger Level: {0:f1}", TriggerLevel)
                    .Return()
                    .End();

                var signals = Output.List.Select(x => (Name: x.Name, Signal: Mixer.Get(x.Name, x.Type)));
                var duration = signals.Max(x => x.Signal.Duration);
                var trigger = new Trigger(TriggerLevel, Configure.SamplingRate, duration);

                // create device buffer
                using (var device = new Device(Configure.SamplingRate, duration, Configure.SoundChannel, Configure.TriggerChannel))
                {
                    var table = new List<(string Name, SignalWave Signal)>[trial];
                    if (Random.SelectedIndex == 1)
                    {
                        var seq = signals.OrderBy(x => Guid.NewGuid()).ToList();
                        for (var i = 0; i < trial; ++i)
                        {
                            table[i] = seq;
                        }
                    }
                    else if (Random.SelectedIndex == 2)
                    {
                        for (var i = 0; i < trial; ++i)
                        {
                            table[i] = signals.OrderBy(x => Guid.NewGuid()).ToList();
                        }
                    }
                    else
                    {
                        var seq = signals.ToList();
                        for (var i = 0; i < trial; ++i)
                        {
                            table[i] = seq;
                        }
                    }

                    for (var i = 0; i < trial; ++i)
                    {
                        foreach (var signal in table[i])
                        {
                            device.Output(signal.Signal.Wave, trigger.Wave);
                            Console.WriteLine("Output Sound!")
                                .WriteLine("Name: {0:g}", signal.Name)
                                .Return()
                                .End();
                            await Task.Delay(Interval.Interval, Cancellation.Token);
                        }
                        Progress.Value = i + 1;
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

            SetIdle();
        }

        private void OnStop(object sender, RoutedEventArgs e)
        {
            Cancellation.Cancel();
            SetBuzy();
        }

    }
}
