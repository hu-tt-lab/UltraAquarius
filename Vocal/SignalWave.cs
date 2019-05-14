using System;
using System.Collections.Generic;
using System.Linq;

namespace Vocal
{
    

    /// <summary>
    /// sound wave generator
    /// </summary>
    public abstract class SignalWave
    {
        public SignalWave(double sampling, double duration, double decibel)
        {
            SamplingRate = sampling;
            Duration = duration;
            Decibel = decibel;
        }
        public abstract IEnumerable<double> Wave { get; }

        public double SamplingRate { get; }
        public double Duration { get; }
        public double Size { get { return (int)(SamplingRate * Duration); } }
        public double Decibel { get; }

    }

    /// <summary>
    /// pure tone wave generator
    /// </summary>
    public class PureWave : SignalWave
    {
        public PureWave(double frequency, double amplitude, double decibel,
            double sampling, double duration) : base(sampling, duration, decibel)
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
    /// tone pip wave generator
    /// </summary>
    public class PipWave : SignalWave
    {
        public PipWave(double frequency, double amplitude, double decibel,
            double sampling, double duration) : base(sampling, duration, decibel)
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
    /// tone pip wave generator
    /// </summary>
    public class BurstWave : SignalWave
    {
        public BurstWave(double frequency, double amplitude, double decibel,
            double sampling, double duration) : base(sampling, duration, decibel)
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

    /// <summary>
    /// click tone wave generator
    /// </summary>
    public class ClickWave : SignalWave
    {
        public ClickWave(double amplitude, double decibel, double sampling, double duration)
            : base(sampling, duration, decibel)
        {
            Gain = amplitude;
        }

        public override IEnumerable<double> Wave
        {
            get
            {
                for (var i= 0; i < Size; ++i)
                {
                    yield return Gain;
                }
            }
        }

        public double Gain { get; }
    }


    /// <summary>
    /// Amplitude Modulation Wave
    /// </summary>
    public class AmplitudeModulationWave : SignalWave
    {
        public AmplitudeModulationWave(double frequency, double amplitude, double modulation, double decibel, double sampling, double duration)
            : base(sampling, duration, decibel)
        {
            Frequency = frequency;
            Gain = amplitude;
            Modulation = modulation;
        }

        public override IEnumerable<double> Wave
        {
            get
            {
                for (var i = 0; i < Size; ++i)
                {
                    yield return Gain * Math.Sin(i * Frequency * 2 * Math.PI / SamplingRate) * Math.Sin(i * Modulation * 2 * Math.PI / SamplingRate);
                }
            }
        }

        public double Frequency { get; }
        public double Gain { get; }
        public double Modulation { get; }

    }

    /// <summary>
    /// trigger wave generator
    /// </summary>
    public class Trigger : SignalWave
    {
        public Trigger(double level, double sampling, double duration) : base(sampling, duration, 0)
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

    /// <summary>
    /// Ultrasound Trigger wave generator
    /// </summary>
    public class UltrasoundWave : SignalWave
    {
        public UltrasoundWave(UltrasoundWaveform waveform, double frequency, double voltage, int waves, double duty, double prf, int Pulses, double sampling, double duration)
            : base(sampling, duration, 0)
        {
            Waveform = waveform;
            Level = 4.5;
            Frequency = frequency;
            PRF = prf;
            Triggered = Pulses;
            Voltage = voltage;
            Waves = waves;
            Duty = duty;
        }

        public double Level { get; set;}
       
        public override IEnumerable<double> Wave
        {
            get
            {
                for (var i = 0; i < Size; ++i)
                {
                    if (i * Frequency / SamplingRate > Triggered)
                    {
                        yield return 0;
                    }
                    else
                    {
                        if (Math.Cos(i * Frequency * 2 * Math.PI / SamplingRate - Math.PI / 4) > Math.Cos(Math.PI / 4))
                        {
                            yield return Level;
                        }
                        else
                        {
                            yield return 0;
                        }
                    }
                    
                }
            }
        }

        public double Frequency { get; }
        public double Triggered { get; }
        public double Voltage { get; }
        public double Waves { get; }
        public double Duty { get; }
        public double PRF { get; }
        public UltrasoundWaveform Waveform { get;}

    }
    public class SawWave : SignalWave
    {
        public SawWave(double frequency, double voltage, int waves, double sampling, double duration)
    : base(sampling, duration, 0)
        {
            Frequency = frequency;
            Voltage = voltage;
            Waves = waves;
        }

        public double Level { get; set; }

        public override IEnumerable<double> Wave
        {
            get
            {
                for (var i = 0; i < Size; ++i)
                {
                    var x = i / SamplingRate * Frequency;
                    yield return Voltage * (x - Math.Floor(x + 0.5));
                }
            }
        }

        public double Frequency { get; }
        public double Voltage { get; }
        public double Waves { get; }

    }

    public class SquareWave : SignalWave
    {
        public SquareWave(double frequency, double voltage, int waves, double duty, double sampling, double duration)
    : base(sampling, duration, 0)
        {
            Frequency = frequency;
            Voltage = voltage;
            Waves = waves;
            Duty = duty;
        }

        public double Level { get; set; }

        public override IEnumerable<double> Wave
        {
            get
            {
                for (var i = 0; i < Size; ++i)
                {
                    var x = i / SamplingRate * Frequency;
                    yield return (Voltage * (x - Math.Floor(x + 0.5)) - Voltage / 2) - (Voltage * (x - Duty/100 - Math.Floor(x + 0.5 - Duty/100)) - Voltage / 2);
                }
            }
        }

        public double Frequency { get; }
        public double Voltage { get; }
        public double Waves { get; }
        public double Duty { get; }
    }
    public class TriangleWave : SignalWave
    {
        public TriangleWave(double frequency, double voltage, int waves, double sampling, double duration)
    : base(sampling, duration, 0)
        {
            Frequency = frequency;
            Voltage = voltage;
            Waves = waves;
        }

        public double Level { get; set; }

        public override IEnumerable<double> Wave
        {
            get
            {
                for (var i = 0; i < Size; ++i)
                {
                    var x = i / SamplingRate * Frequency;
                    yield return (2 * Voltage * Math.Abs(Math.Round(x - 0.25) - (x -0.25)) - Voltage / 2);
                }
            }
        }

        public double Frequency { get; }
        public double Voltage { get; }
        public double Waves { get; }
    }
    /// <summary>
    /// trigger wave generator
    /// </summary>
    public class NonUse : SignalWave
    {
        public NonUse(double sampling, double duration): base(sampling, duration, 0) {}

        public override IEnumerable<double> Wave
        {
            get
            {
                for (var i = 0; i < Size; ++i)
                {
                    yield return 0;
                }

            }
        }
    }
}

