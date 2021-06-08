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
            Magnetic.Lock();
            USMod.Lock();
            USHum.Lock();
            UserDefined.Lock();
        }

        public void UnLock()
        {
            PureTone.Unlock();
            ClickTone.Unlock();
            ModulationTone.Unlock();
            Ultrasound.Unlock();
            Magnetic.Unlock();
            USMod.Unlock();
            USHum.Unlock();
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
            else if (type == SignalType.USMod)
            {
                var x = USMod.Find(name);
                switch (x.WindowType)
                {
                    case USModWindowType.Sine:
                        return new WindowSine(x.WindowType, x.Frequency, x.Voltage, x.Waves, x.WindowWaves, SamplingRate, x.Waves/x.Frequency);
                    case USModWindowType.Liner:
                        return new WindowLiner(x.WindowType, x.Frequency, x.Voltage, x.Waves, x.WindowWaves, SamplingRate, x.Waves / x.Frequency);
                }
            }
            else if (type == SignalType.USHum)
            {
                var x = USHum.Find(name);

            }
            else if (type == SignalType.Magnetic)
            {
                var x = Magnetic.Find(name);
                //switch (x.Waveform)
                //{
                return new MagneticWave(x.Waveform, x.Voltage, x.Waves, x.RaiseDuration / 1000000, x.FallDuration / 1000000, x.Interval / 1000000, SamplingRate, x.Duration / 1000000);
                /*case MagneticWaveform.FrontEdgeSawPulse:
                    return new FrontEdgeSawPulse(x.Voltage, x.Waves, x.Interval/1000000, SamplingRate, x.Duration / 1000000);
                case MagneticWaveform.LastEdgeSawPulse:
                    return new LastEdgeSawPulse(x.Voltage, x.Waves, x.Interval / 1000000, SamplingRate, x.Duration / 1000000);
                case MagneticWaveform.SquarePulse:
                    return new SquarePulse(x.Voltage, x.Waves, x.Interval / 1000000, SamplingRate, x.Duration / 1000000);
                case MagneticWaveform.TrianglePulse:
                    return new TrianglePulse(x.Voltage, x.Waves, x.Interval / 1000000, SamplingRate, x.Duration / 1000000);
                */
                //}
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
