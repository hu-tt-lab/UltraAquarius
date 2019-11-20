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
using System.Runtime.Serialization;

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
                    .WriteLine("Channel Mode: Sound = {0:g}, Trigger = {1:g}, ExDevice = {2:g} AllTrigger = {3:g}", Configure.SoundChannel, Configure.TriggerChannel, Configure.ExDeviceChannel, Configure.ALLTriggerChannel)
                    .WriteLine("Trigger Level: {0:f1} Function Generator Trigger Level: {1:f1}", TriggerLevel, FunGeneTriggerLevel)
                    .Return()
                    .End();

                var signals = Output.List.Select(x => (Name: x.Name, Number: x.Number, Type: x.Type, Signal: Mixer.Get(x.Name, x.Type))).ToList();
                var duration = signals.Max(x => x.Signal.Duration);
                var trigger = new Trigger(TriggerLevel, Configure.SamplingRate, duration);
                var nonuse = new NonUse(Configure.SamplingRate, duration);
                //ABR Mode
                if (Random.SelectedIndex == 3) 
                {
                    Progress.Maximum = trial* signals.Count();
                    // create device buffer
                    using (var device = new Device(Configure.Identifer, Configure.SamplingRate, duration, Configure.TriggerChannel, Configure.SoundChannel, Configure.ExDeviceChannel, Configure.ExDeviceChannel2, Configure.ALLTriggerChannel))
                    {//Digital Trigger, Sound channel, ExDevice channel, ExDevice channel2, All Trigger channel
                        
                        var table = new List<int>[1];
                        table[0] = Enumerable.Range(0, signals.Count()).ToList();

                        foreach (var index in table[0])
                        {
                            var signal = signals[index];
                            Console.WriteLine("Output Sound!")
                                .WriteLine("Name: {0:g}", signal.Name)
                                .WriteLine("Type: {0:g}", signal.Type)
                                .Return()
                                .End();
                            //device.output's argument is (Digital Trigger, Sound channel, ExDevice channel,ExDevice channel2, All Trigger channel)
                            if (signal.Type == SignalType.Ultrasound)
                            {
                                Fungene.Oscillation("TRIGGER");
                                Fungene.BurstSyncType("BURSTSYNC");
                                Fungene.Parameter(Mixer.Ultrasound.Find(signal.Name));
                                for (var i = 0; i < trial; ++i)
                                {
                                    device.Output(signal.Number, nonuse.Wave, signal.Signal.Wave, nonuse.Wave, trigger.Wave);
                                    Progress.Value += 1;
                                    await Task.Delay(Interval.Interval, Cancellation.Token);
                                }
                            }
                            else if (signal.Type == SignalType.USMod)
                            {
                                Fungene.Oscillation("AMSC");
                                Fungene.Parameter(Mixer.USMod.Find(signal.Name));
                                for (var i = 0; i < trial; ++i)
                                {
                                    device.Output(signal.Number, nonuse.Wave, nonuse.Wave, signal.Signal.Wave, trigger.Wave);
                                    Progress.Value += 1;
                                    await Task.Delay(Interval.Interval, Cancellation.Token);
                                }
                            }
                            else if (signal.Type == SignalType.Magnetic)
                            {
                                Fungene.Oscillation("TRIGGER");
                                Fungene.BurstSyncType("BURSTSYNC");
                                Fungene.Parameter(Mixer.Magnetic.Find(signal.Name));
                                for (var i = 0; i < trial; ++i)
                                {
                                    device.Output(signal.Number, nonuse.Wave, signal.Signal.Wave, nonuse.Wave, trigger.Wave);
                                    Progress.Value += 1;
                                    await Task.Delay(Interval.Interval, Cancellation.Token);
                                }
                            }
                            else
                            {
                                for (var i = 0; i < trial; ++i)
                                {
                                    device.Output(signal.Number, signal.Signal.Wave, nonuse.Wave, nonuse.Wave, trigger.Wave);
                                    Progress.Value += 1;
                                    await Task.Delay(Interval.Interval, Cancellation.Token);
                                }
                            }
                            // TDT needs two triggers to expel data
                            device.Output(signal.Number, nonuse.Wave, nonuse.Wave, nonuse.Wave, trigger.Wave);
                            await Task.Delay(Interval.Interval, Cancellation.Token);
                            device.Output(signal.Number, nonuse.Wave, nonuse.Wave, nonuse.Wave, trigger.Wave);
                            await Task.Delay(Interval.Interval, Cancellation.Token);
                        }

                        // log to json
                        var nowDate = DateTime.Now.ToString("yyyyMMdd-HHmm-");
                        using (var writer = new StreamWriter(string.Format("{0:g}result.json", nowDate)))
                        {
                            var contents = MakeJsonOrderData(table, signals);
                            writer.WriteLine(contents);
                        }
                        using (var writer = new StreamWriter(string.Format("{0:g}Settings.json", nowDate)))
                        {
                            var contents = MakeJsonSettingData();
                            writer.WriteLine(contents);
                        }
                        device.Dispose();
                    }
                }
                // Ordinary(Plexon,Micam)Mode
                else
                {

                    if (signals.Any(x => x.Type == SignalType.Ultrasound))
                    {
                        var name = (VisaResuorce)FGConfigure.ResourceComboBox.SelectedItem;
                        Fungene.Open(name.Resource);
                        Fungene.Oscillation("TRIGGER");
                        Fungene.BurstSyncType("BURSTSYNC");
                        Fungene.OnOff("ON");
                    }
                    else if (signals.Any(x => x.Type == SignalType.Magnetic))
                    {
                        var name = (VisaResuorce)FGConfigure.ResourceComboBox.SelectedItem;
                        Fungene.Open(name.Resource);
                        Fungene.Oscillation("TRIGGER");
                        Fungene.BurstSyncType("BURSTSYNC");
                        Fungene.OnOff("OFF");
                    }
                    else if (signals.Any(x => x.Type == SignalType.USMod))
                    {
                        var name = (VisaResuorce)FGConfigure.ResourceComboBox.SelectedItem;
                        Fungene.Open(name.Resource);
                        Fungene.Oscillation("AMSC");
                        Fungene.OnOff("ON");
                    }

                    // create device buffer
                    using (var device = new Device(Configure.Identifer, Configure.SamplingRate, duration, Configure.TriggerChannel, Configure.SoundChannel, Configure.ExDeviceChannel, Configure.ExDeviceChannel2, Configure.ALLTriggerChannel))
                    {//Digital Trigger, Sound channel, ExDevice channel, ExDevice channel2, All Trigger channel
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
                                //device.output's argument is (Digital Trigger, Sound channel, ExDevice channel,ExDevice channel2, All Trigger channel)
                                if (signal.Type == SignalType.Ultrasound)
                                {
                                    Fungene.Oscillation("TRIGGER");
                                    Fungene.BurstSyncType("BURSTSYNC");
                                    Fungene.Parameter(Mixer.Ultrasound.Find(signal.Name));
                                    device.Output(signal.Number, nonuse.Wave, signal.Signal.Wave, nonuse.Wave, trigger.Wave);
                                }
                                else if (signal.Type == SignalType.USMod)
                                {
                                    Fungene.Oscillation("AMSC");
                                    Fungene.Parameter(Mixer.USMod.Find(signal.Name));
                                    device.Output(signal.Number, nonuse.Wave, nonuse.Wave, signal.Signal.Wave, trigger.Wave);
                                }
                                else if (signal.Type == SignalType.Magnetic)
                                {
                                    Fungene.Oscillation("TRIGGER");
                                    Fungene.BurstSyncType("BURSTSYNC");
                                    Fungene.Parameter(Mixer.Magnetic.Find(signal.Name));
                                    device.Output(signal.Number, nonuse.Wave, signal.Signal.Wave, nonuse.Wave, trigger.Wave);
                                }
                                else
                                {
                                    device.Output(signal.Number, signal.Signal.Wave, nonuse.Wave, nonuse.Wave, trigger.Wave);
                                }

                                await Task.Delay(Interval.Interval, Cancellation.Token);
                            }
                            Progress.Value = i + 1;
                        }

                        // log to json
                        var nowDate = DateTime.Now.ToString("yyyyMMdd-HHmm-");
                        using (var writer = new StreamWriter(string.Format("{0:g}result.json", nowDate)))
                        {
                            var contents = MakeJsonOrderData(table, signals);
                            writer.WriteLine(contents);
                        }
                        using (var writer = new StreamWriter(string.Format("{0:g}Settings.json", nowDate)))
                        {
                            var contents = MakeJsonSettingData();
                            writer.WriteLine(contents);
                        }
                        device.Dispose();
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
        
        public void LoadMixer()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog openDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Title = "名前を指定して読み込み",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                    FileName = @"Mixer.json",
                    Filter = "jsonファイル(*.json)|*.json|すべてのファイル(*.*)|*.*",
                    FilterIndex = 1

                };
                if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var reader = new StreamReader(openDialog.FileName))
                    {
                        var json = DynamicJson.Parse(reader.ReadToEnd());
                        Mixer.PureTone.Load((object[])json.PureTone);
                        Mixer.ClickTone.Load((object[])json.ClickTone);
                        Mixer.ModulationTone.Load((object[])json.ModulationTone);
                        Mixer.Magnetic.Load((object[])json.Magnetic);
                        Mixer.Ultrasound.Load((object[])json.Ultrasound);
                        Mixer.USMod.Load((object[])json.USMod);
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        public void SaveMixer()
        {
            try
            {
                System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog
                {
                    Title = "名前を指定して保存",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                    FileName = @"Mixer.json",
                    Filter = "jsonファイル(*.json)|*.json|すべてのファイル(*.*)|*.*",
                    FilterIndex = 1,
                    CheckFileExists = false

                };
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var writer = new StreamWriter(saveDialog.FileName))
                    {
                        var contents = DynamicJson.Parse(DynamicJson.Serialize(
                            new
                            {
                                PureTone = Mixer.PureTone.Save().Select(x => new { Name = x.Name, Signal = new JsonPureTone(x.Signal) }).ToList(),
                                ClickTone = Mixer.ClickTone.Save().Select(x => new { Name = x.Name, Signal = x.Signal }).ToList(),
                                ModulationTone = Mixer.ModulationTone.Save().Select(x => new { Name = x.Name, Signal = x.Signal }).ToList(),
                                Magnetic = Mixer.Magnetic.Save().Select(x => new { Name = x.Name, Signal = new JsonMagnetic(x.Signal) }).ToList(),
                                Ultrasound = Mixer.Ultrasound.Save().Select(x => new { Name = x.Name, Signal = new JsonUltrasound(x.Signal) }).ToList(),
                                USMod = Mixer.USMod.Save().Select(x => new { Name = x.Name, Signal = new JsonUSMod(x.Signal) }).ToList()
                            }
                        ));
                        writer.WriteLine(contents);
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
}

        public void SaveOutputList()   
        {
            try
            {
                System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog
                {
                    Title = "名前を指定して保存",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                    FileName = @"OutputList.json",
                    Filter = "jsonファイル(*.json)|*.json|すべてのファイル(*.*)|*.*",
                    FilterIndex = 1,
                    CheckFileExists = false

                };
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var writer = new StreamWriter(saveDialog.FileName))
                    {
                        var contents = DynamicJson.Parse(DynamicJson.Serialize(
                            Output.Save().Select(x => new OutputList.JsonOptional(x)).ToList()
                        ));
                        writer.WriteLine(contents);
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
}

        public void LoadOutputList()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog openDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Title = "名前を指定して読み込み",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                    FileName = @"OutputList.json",
                    Filter = "jsonファイル(*.json)|*.json|すべてのファイル(*.*)|*.*",
                    FilterIndex = 1

                };
                if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var reader = new StreamReader(openDialog.FileName))
                    {
                        var json = DynamicJson.Parse(reader.ReadToEnd());
                        Output.Load((object[])json);
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void OnMixerSave(object sender, RoutedEventArgs e)
        {
            SaveMixer();
        }
        private void OnMixerLoad(object sender, RoutedEventArgs e)
        {
            LoadMixer();
        }

        private void OnOutputListSave(object sender, RoutedEventArgs e)
        {
            SaveOutputList();
        }
        private void OnOutputListLoad(object sender, RoutedEventArgs e)
        {
            LoadOutputList();
        }

        private void OnHelp(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://tt-lab.ist.hokudai.ac.jp/gitbucket/toda/UltraAquarius/blob/master/README.md");
        }

        private dynamic MakeJsonOrderData(List<int>[] table, List<(string Name, int Number, SignalType Type, SignalWave Signal)> signals)
        {
            return DynamicJson.Serialize(table.Select(x => x.Select(i =>
            {
                var variable = signals[i];
                if (variable.Type == SignalType.Click)
                {
                    var wave = variable.Signal as ClickWave;
                    return (object)(new { number = variable.Number, name = variable.Name, duration = wave.Duration, decibel = wave.Decibel, type = variable.Type.ToString() });
                }
                else if (variable.Type == SignalType.Pure)
                {
                    var pure = variable.Signal as PureWave;
                    if (pure != null)
                    {
                        return new { number = variable.Number, name = variable.Name, duration = pure.Duration, decibel = pure.Decibel, frequency = pure.Frequency, type = variable.Type.ToString(), tonetype = "PureTone" };
                    }
                    var pip = variable.Signal as PipWave;
                    if (pip != null)
                    {
                        return new { number = variable.Number, name = variable.Name, duration = pip.Duration, decibel = pip.Decibel, frequency = pip.Frequency, type = variable.Type.ToString(), tonetype = "TonePip" };
                    }
                    var burst = variable.Signal as BurstWave;
                    if (burst != null)
                    {
                        return new { number = variable.Number, name = variable.Name, duration = burst.Duration, decibel = burst.Decibel, frequency = burst.Frequency, type = variable.Type.ToString(), tonetype = "ToneBurst" };
                    }
                    else
                    {
                        throw new ArgumentException("this param is invalid.");
                    }
                }
                else if (variable.Type == SignalType.Modulation)
                {
                    var signal = variable.Signal as AmplitudeModulationWave;
                    return new { number = variable.Number, name = variable.Name, duration = signal.Duration, decibel = signal.Decibel, frequency = signal.Frequency, type = variable.Type.ToString(), modulation = signal.Modulation };
                }
                else if (variable.Type == SignalType.Ultrasound)
                {
                    var signal = variable.Signal as UltrasoundWave;
                    return new { number = variable.Number, name = variable.Name, voltage = signal.Voltage, frequency = signal.Frequency, waves = signal.Waves, duty = signal.Duty, prf = signal.PRF, pulses = signal.Triggered, type = variable.Type.ToString() };
                }
                else if (variable.Type == SignalType.USMod)
                {
                    var sinewindow = variable.Signal as WindowSine;
                    if (sinewindow != null)
                    {
                        return new { number = variable.Number, name = variable.Name, voltage = sinewindow.Voltage, frequency = sinewindow.Frequency, waves = sinewindow.Waves, windowwaves = sinewindow.WindowWaves, type = variable.Type.ToString(), windowtype = sinewindow.WindowType.ToString() };
                    }
                    var linerwindow = variable.Signal as WindowLiner;
                    if (linerwindow != null)
                    {
                        return new { number = variable.Number, name = variable.Name, voltage = linerwindow.Voltage, frequency = linerwindow.Frequency, waves = linerwindow.Waves, windowwaves = linerwindow.WindowWaves, type = variable.Type.ToString(), windowtype = linerwindow.WindowType.ToString() };
                    }
                    else
                    {
                        throw new ArgumentException("this param is invalid.");
                    }
                }
                else if (variable.Type == SignalType.Magnetic)
                {
                    var magpulse = variable.Signal as MagneticWave;
                    if (magpulse != null)
                    {
                        return new { number = variable.Number, name = variable.Name, voltage = magpulse.Voltage, duration = (magpulse.Duration / magpulse.Waves - magpulse.Interval), raisedur = magpulse.RaiseDuration, falldur = magpulse.FallDuration, interval = magpulse.Interval, waves = magpulse.Waves, type = variable.Type.ToString(), signaltype = magpulse.Waveform.ToString() };
                    }
                    /*
                    var frontsaw = variable.Signal as FrontEdgeSawPulse;
                    if (frontsaw != null)
                    {
                        return new {number = variable.Number, name = variable.Name, voltage = frontsaw.Voltage, duration = (frontsaw.Duration/ frontsaw.Waves - frontsaw.Interval), interval = frontsaw.Interval, waves = frontsaw.Waves, type = variable.Type.ToString(), signaltype = "FrontEdgeSawPulse" };
                    }
                    var lastsaw = variable.Signal as LastEdgeSawPulse;
                    if (lastsaw != null)
                    {
                        return new {number = variable.Number, name = variable.Name, voltage = lastsaw.Voltage, duration = (lastsaw.Duration / lastsaw.Waves - lastsaw.Interval), interval = lastsaw.Interval, waves = lastsaw.Waves, type = variable.Type.ToString(), signaltype = "LastEdgeSawPulse" };
                    }
                    var square = variable.Signal as SquarePulse;
                    if (square != null)
                    {
                        return new {number = variable.Number, name = variable.Name, voltage = square.Voltage, duration = (square.Duration / square.Waves - square.Interval), interval = square.Interval, waves = square.Waves, type = variable.Type.ToString(), signaltype = "SquarePulse" };
                    }
                    var triangle = variable.Signal as TrianglePulse;
                    if (triangle != null)
                    {
                        return new {number = variable.Number, name = variable.Name, voltage = triangle.Voltage, duration = (triangle.Duration / triangle.Waves - triangle.Interval), interval = triangle.Interval, waves = triangle.Waves, type = variable.Type.ToString(), signaltype = "TrianglePulse" };
                    }
                    */
                    else
                    {
                        throw new ArgumentException("this param is invalid.");
                    }
                }
                else
                {
                    throw new ArgumentException("this type is invalid.");
                }
            }).ToList()).ToList().SelectMany(x => x));
        }
        private dynamic MakeJsonSettingData() {

            return DynamicJson.Serialize(new
            {
                Interval = Interval.Duration,
                Waggle = Interval.Waggle,
                Samplingrate = Configure.SamplingRate,
                Trial = Trial,
                Mode = Random.Text,
                VocalVersion = this.Title
            }
            );
        }
    }
}
