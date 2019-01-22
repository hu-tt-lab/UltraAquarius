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
using Ivi.Visa.Interop;

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

        public FunGene Fungene = new FunGene();
        public ResourceManager RM = new ResourceManager();
        public FormattedIO488 DMM = new FormattedIO488();

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
        // output Function Generator trigger level
        public double FunGeneTriggerLevel { get { return double.Parse(fungenetriggerVoltage.Text); } set { fungenetriggerVoltage.Text = value.ToString(); } }
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
                    .WriteLine("Channel Mode: Sound = {0:g}, Trigger = {1:g}, FunGene = {2:g}", Configure.SoundChannel, Configure.TriggerChannel, Configure.FunGeneChannel)
                    .WriteLine("Trigger Level: {0:f1} Function Generator Trigger Level: {1:f1}", TriggerLevel, FunGeneTriggerLevel)
                    .Return()
                    .End();
                var name = (VisaResuorce)Configure.ResourceComboBox.SelectedItem;
                Fungene.Open(name.Resource);
                Fungene.Oscillation("TRIGGER");
                Fungene.BurstSyncType("BURSTSYNC");
                Fungene.OnOff("ON");
                var signals = Output.List.Select(x => (Name: x.Name, Type: x.Type, Signal: Mixer.Get(x.Name, x.Type))).ToList();
                var duration = signals.Max(x => x.Signal.Duration);
                var trigger = new Trigger(TriggerLevel, Configure.SamplingRate, duration);
                var nonuse = new NonUse(Configure.SamplingRate, duration);

                // create device buffer
                using (var device = new Device(Configure.SamplingRate, duration, Configure.SoundChannel, Configure.TriggerChannel,Configure.FunGeneChannel))
                {
                    var table = new List<int>[trial];
                    if (Random.SelectedIndex == 1)
                    {
                        var seq = Enumerable.Range(0, signals.Count()).OrderBy(x => Guid.NewGuid()).ToList();
                        for (var i = 0; i < trial; ++i)
                        {
                            table[i] = seq;
                        }
                    }
                    else if (Random.SelectedIndex == 2)
                    {
                        for (var i = 0; i < trial; ++i)
                        {
                            table[i] = Enumerable.Range(0, signals.Count()).OrderBy(x => Guid.NewGuid()).ToList();
                        }
                    }
                    else
                    {
                        var seq = Enumerable.Range(0, signals.Count()).ToList();
                        for (var i = 0; i < trial; ++i)
                        {
                            table[i] = seq;
                        }
                    }

                    for (var i = 0; i < trial; ++i)
                    {
                        foreach (var index in table[i])
                        {
                            var signal = signals[index];
                            Console.WriteLine("Output Sound!")
                                .WriteLine("Name: {0:g}", signal.Name)
                                .WriteLine("Type: {0:g}", signal.Type)
                                .Return()
                                .End();
                            if (signal.Type == SignalType.Ultrasound)
                            {
                                Fungene.Parameter(Mixer.Ultrasound.Find(signal.Name));
                                device.Output(nonuse.Wave, trigger.Wave, signal.Signal.Wave);
                            }
                            else
                            {
                                device.Output(signal.Signal.Wave, trigger.Wave, nonuse.Wave);
                            }
                            
                            await Task.Delay(Interval.Interval, Cancellation.Token);
                        }
                        Progress.Value = i + 1;
                    }

                    // log to json
                    using (var writer = new StreamWriter(string.Format("{0:g}result.json", DateTime.Now.ToString("yyyyMMdd-HHmm-"))))
                    {
                        var contents = DynamicJson.Serialize(table.Select(x => x.Select(i =>
                        {
                            var variable = signals[i];
                            if (variable.Type == SignalType.Click)
                            {
                                var wave = variable.Signal as ClickWave;
                                return (object)(new { name = variable.Name, duration = wave.Duration, decibel = wave.Decibel, type = variable.Type.ToString() });
                            }
                            else if (variable.Type == SignalType.Pure)
                            {
                                var pure = variable.Signal as PureWave;
                                if (pure != null)
                                {
                                    return new { name = variable.Name, duration = pure.Duration, decibel = pure.Decibel, frequency = pure.Frequency, type = variable.Type.ToString() };
                                }
                                var pip = variable.Signal as PipWave;
                                if (pip != null)
                                {
                                    return new { name = variable.Name, duration = pip.Duration, decibel = pip.Decibel, frequency = pip.Frequency, type = variable.Type.ToString() };
                                }
                                var burst = variable.Signal as BurstWave;
                                if (burst != null)
                                {
                                    return new { name = variable.Name, duration = burst.Duration, decibel = burst.Decibel, frequency = burst.Frequency, type = variable.Type.ToString() };
                                }
                                else
                                {
                                    throw new ArgumentException("this param is invalid.");
                                }
                            }
                            else if (variable.Type == SignalType.Modulation)
                            {
                                var signal = variable.Signal as AmplitudeModulationWave;
                                return new { name = variable.Name, duration = signal.Duration, decibel = signal.Decibel, frequency = signal.Frequency, type = variable.Type.ToString(), modulation = signal.Modulation };
                            }
                            else if (variable.Type == SignalType.Ultrasound)
                            {
                                var signal = variable.Signal as UltrasoundWave;
                                return new { name = variable.Name, voltage = signal.Voltage, frequency = signal.Frequency, waves = signal.Waves, duty = signal.Duty, prf = signal.PRF, pulses = signal.Triggered, type = variable.Type.ToString() };
                            }
                            else
                            {
                                throw new ArgumentException("this type is invalid.");
                            }
                        }).ToList()).ToList().SelectMany(x=>x));
                        writer.WriteLine(contents);
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
