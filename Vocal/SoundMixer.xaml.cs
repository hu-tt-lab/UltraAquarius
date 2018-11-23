using System;
using System.Collections.Generic;
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


namespace Vocal
{
    /// <summary>
    /// SoundMixer.xaml の相互作用ロジック
    /// </summary>
    public partial class SoundMixer : UserControl
    {
        public SoundMixer()
        {
            InitializeComponent();
        }

        public void Lock(double sampling)
        {
            SamplingRate = sampling;
            PureTone.Lock();
            ClickTone.Lock();
            ModulationTone.Lock();
            Ultrasound.Lock();
            UserDefined.Lock();
        }

        public void UnLock()
        {
            PureTone.Unlock();
            ClickTone.Unlock();
            ModulationTone.Unlock();
            Ultrasound.Unlock();
            UserDefined.Unlock();
        }

        public SignalWave Get(string name, SignalType type)
        {
            if (type == SignalType.Pure)
            {
                var x = PureTone.Find(name);
                var amplitude = PureTone.Correction(x.Frequency, x.Decibel);
                switch (x.Tone)
                {
                    case PureToneType.PureTone:
                        return new PureWave(x.Frequency, amplitude, x.Decibel, SamplingRate, x.Duration / 1000);
                    case PureToneType.ToneBurst:
                        return new BurstWave(x.Frequency, amplitude, x.Decibel, SamplingRate, x.Duration / 1000);
                    case PureToneType.TonePip:
                        return new PipWave(x.Frequency, amplitude, x.Decibel, SamplingRate, x.Duration / 1000);
                }
            }
            else if (type == SignalType.Click)
            {
                var x = ClickTone.Find(name);
                var amplitude = ClickTone.Correction(x.Decibel);
                return new ClickWave(amplitude, x.Decibel, SamplingRate, x.Duration / 1000);
            }
            else if (type == SignalType.Modulation)
            {
                var x = ModulationTone.Find(name);
                var amplitude = ModulationTone.Correction(x.Frequency, x.Decibel);
                return new AmplitudeModulationWave(x.Frequency, amplitude, x.Modulation, x.Decibel, SamplingRate, x.Duration / 1000);
            }
            else if (type == SignalType.Ultrasound)
            {
                var x = Ultrasound.Find(name);
                return new UltrasoundWave(x.Waveform, x.Frequency, x.Voltage, x.Waves, x.Duty, x.PRF, x.Pulses, SamplingRate, (x.Pulses / x.PRF));
            }
            else if (type == SignalType.User)
            {
                var x = UserDefined.Find(name);
                throw new ArgumentException("application does not support this sound type.");

            }

            throw new ArgumentException("application does not support this sound type.");
        }

        public double SamplingRate { get; set; }

    }
}
