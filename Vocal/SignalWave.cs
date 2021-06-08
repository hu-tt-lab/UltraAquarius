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
        public UltrasoundWave(UltrasoundWaveform waveform, double frequency, double voltage, double waves, double duty, double prf, int Pulses, double sampling, double duration)
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
                var period = SamplingRate / PRF;
                for (var i = 0; i < Size; ++i)
                {
                        if ((i % period) < (period / 4))
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

        public double Frequency { get; }
        public double Triggered { get; }
        public double Voltage { get; }
        public double Waves { get; }
        public double Duty { get; }
        public double PRF { get; }
        public UltrasoundWaveform Waveform { get;}

    }
    /// <summary>
    /// ultrasound moduration Trigger wavegenerator
    /// </summary>
    public class WindowSine : SignalWave
    {
        public WindowSine(USModWindowType windowType, double frequency, double voltage, double waves, double windowWaves, double sampling, double duration)
            : base(sampling, duration, 0)
        {
            WindowType = windowType;
            Level = 1.0;
            Frequency = frequency;
            Voltage = voltage;
            Waves = waves;
            WindowWaves = windowWaves;
        }

        public double Level { get; set; }

        public override IEnumerable<double> Wave
        {
            get
            {
                var windowLength_ = WindowWaves / Frequency * SamplingRate;
                for (var i = 0; i < Size; ++i)
                {
                    if (i <= windowLength_)
                    {
                        yield return Level * Math.Sin(0.25 * Frequency / WindowWaves * 2 * Math.PI * i / SamplingRate);
                    }
                    else if (windowLength_ < i & i < Size - windowLength_)
                    {
                        yield return Level;
                    }
                    else
                    {
                        yield return Level * Math.Sin(0.25 * Frequency / WindowWaves * 2 * Math.PI * (Size - i) / SamplingRate);
                    }


                }
            }
        }

        public USModWindowType WindowType { get; } = USModWindowType.Sine;
        public double Voltage { get; set; } = 0.5;
        public double Frequency { get; set; } = 500000;
        public double Waves { get; set; } = 1000;
        public double WindowWaves { get; set; } = 100;
    }

    public class WindowLiner : SignalWave
    {
        public WindowLiner(USModWindowType windowType, double frequency, double voltage, double waves, double windowWaves, double sampling, double duration)
            : base(sampling, duration, 0)
        {
            WindowType = windowType;
            Level = 1.0;
            Frequency = frequency;
            Voltage = voltage;
            Waves = waves;
            WindowWaves = windowWaves;
        }

        public double Level { get; set; }

        public override IEnumerable<double> Wave
        {
            get
            {
                var windowLength_ = WindowWaves / Frequency * SamplingRate;
                for (var i = 0; i < Size; ++i)
                {
                    if (i <= windowLength_)
                    {
                        yield return Level * i / windowLength_;
                    }
                    else if (windowLength_ < i & i < Size - windowLength_)
                    {
                        yield return Level;
                    }
                    else
                    {
                        yield return Level * (Size - i) / windowLength_;
                    }


                }
            }
        }

        public USModWindowType WindowType { get; } = USModWindowType.Sine;
        public double Voltage { get; set; } = 0.5;
        public double Frequency { get; set; } = 500000;
        public double Waves { get; set; } = 1000;
        public double WindowWaves { get; set; } = 100;
    }
    /// <summary>
    /// UltrasoundRepeatModu Trigger wavegenerator
    /// </summary>
    public class USWindowHamming_rep : SignalWave
    {
        public USWindowHamming_rep(USHumWindowType windowType, double frequency, double voltage, double waves, double windowWaves, double prf, int pulses, double sampling, double duration)
            : base(sampling, duration, 0)
        {
            WindowType = windowType;
            Level = 1.0;
            Frequency = frequency;
            Voltage = voltage;
            Waves = waves;
            WindowWaves = windowWaves;
            PRF = prf;
            Pulses = pulses;
        }

        public double Level { get; set; }

        public override IEnumerable<double> Wave
        {
            get
            {
                var period = SamplingRate / PRF;
                var windowLength = WindowWaves / Frequency * SamplingRate;
                var wave_period_in_pulse = SamplingRate * Waves / Frequency;
                for (var i = 0; i < Size; ++i)
                {
                    var position_in_pulse_section = i % period;
                    if ((position_in_pulse_section) < (wave_period_in_pulse))
                    {
                        if((position_in_pulse_section) <= (windowLength))
                        {
                            yield return Level * Math.Sin( 0.5 * Math.PI * position_in_pulse_section / windowLength);
                        }
                        else if((windowLength) < (position_in_pulse_section) & (position_in_pulse_section ) < (period-windowLength))
                        {
                            yield return Level;
                        }
                        else
                        {
                            yield return Level * Math.Sin(0.5 * Math.PI * (wave_period_in_pulse - position_in_pulse_section) / windowLength);
                        }
                            
                    }
                    else
                    {
                        yield return 0;
                    }
                }
            }
        }

        public USHumWindowType WindowType { get; } = USHumWindowType.Sine;
        public double Voltage { get; set; } = 0.5;
        public double Frequency { get; set; } = 500000;
        public double Waves { get; set; } = 20;
        public double WindowWaves { get; set; } = 5;
        public double PRF { get; set; } = 1500;
        public double Pulses { get; set; } = 100;
    }

    /// <summary>
    /// Waveforms trigger Wave generator
    /// </summary>
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

    public class SquarePulse : SignalWave
    {
        public SquarePulse(double voltage, int waves, double interval, double sampling, double duration)
    : base(sampling, (interval + duration) * waves, 0)
        {
            Voltage = voltage;
            Waves = waves;
            Interval = interval;
        }
        public override IEnumerable<double> Wave
        {
            get
            {
                var repeat_time = (int)((Duration/Waves) * SamplingRate);
                var pulse_time = (int)((Duration/Waves - Interval) * SamplingRate);

                for (var i = 0; i < Size; ++i)
                {
                    if (i % repeat_time <= pulse_time)
                    {
                        yield return Voltage;
                    }
                    else
                    {
                        yield return 0;
                    }
                }
            }
        }

        public double Voltage { get; }
        public double Waves { get; }
        public double Interval { get; }
    }

    public class FrontEdgeSawPulse : SignalWave
    {
        public FrontEdgeSawPulse(double voltage, int waves, double interval, double sampling, double duration)
    : base(sampling, (interval + duration) * waves, 0)
        {
            Voltage = voltage;
            Waves = waves;
            Interval = interval;
        }
        public override IEnumerable<double> Wave
        {
            get
            {
                var repeat_time = (int)((Duration / Waves) * SamplingRate);
                var pulse_time = (int)((Duration / Waves - Interval) * SamplingRate);

                for (var i = 0; i < Size; ++i)
                {
                    var period = i % repeat_time;
                    if (period <= pulse_time)
                    {
                        yield return Voltage*(1- ((double)period /pulse_time));
                    }
                    else
                    {
                        yield return 0;
                    }
                }
            }
        }

        public double Voltage { get; }
        public double Waves { get; }
        public double Interval { get; }
    }

    public class LastEdgeSawPulse : SignalWave
    {
        public LastEdgeSawPulse(double voltage, int waves, double interval, double sampling, double duration)
    : base(sampling, (interval + duration) * waves, 0)
        {
            Voltage = voltage;
            Waves = waves;
            Interval = interval;
        }
        public override IEnumerable<double> Wave
        {
            get
            {
                var repeat_time = (int)((Duration / Waves) * SamplingRate);
                var pulse_time = (int)((Duration / Waves - Interval) * SamplingRate);

                for (var i = 0; i < Size; ++i)
                {
                    var period = i % repeat_time;
                    if (period <= pulse_time)
                    {
                        yield return Voltage * ((double)period / pulse_time);
                    }
                    else
                    {
                        yield return 0;
                    }
                }
            }
        }

        public double Voltage { get; }
        public double Waves { get; }
        public double Interval { get; }
    }

    public class TrianglePulse : SignalWave
    {
        public TrianglePulse(double voltage, int waves, double interval, double sampling, double duration)
    : base(sampling, (interval + duration) * waves, 0)
        {
            Voltage = voltage;
            Waves = waves;
            Interval = interval;
        }
        public override IEnumerable<double> Wave
        {
            get
            {
                var repeat_time = (int)((Duration / Waves) * SamplingRate);
                var pulse_time = (int)((Duration / Waves - Interval) * SamplingRate);

                for (var i = 0; i < Size; ++i)
                {
                    var period = i % repeat_time;
                    if (period <= pulse_time)
                    {
                        if (period < 0.5 * pulse_time)
                        {
                            yield return Voltage * (period / (0.5 * pulse_time));
                        }
                        else
                        {
                            yield return Voltage * ( 2 - period / (0.5 * pulse_time));
                        }
                    }
                    else
                    {
                        yield return 0;
                    }
                }
            }
        }

        public double Voltage { get; }
        public double Waves { get; }
        public double Interval { get; }
    }
    public class MagneticWave : SignalWave
    {
        public MagneticWave(MagneticWaveform waveform, double voltage, int waves, double raiseDuration, double fallDuration, double interval, double sampling, double duration)
            : base(sampling, (interval + duration) * waves, 0)
        {
            Waveform = waveform;
            Level = 4.5;
            RaiseDuration = raiseDuration;
            FallDuration = fallDuration;
            Voltage = voltage;
            Waves = waves;
            Interval = interval;
        }

        public double Level { get; set; }

        public override IEnumerable<double> Wave
        {
            get
            {
                for (var i = 0; i < Size; ++i)
                {
                    if (i  < Size / 10)
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

        public MagneticWaveform Waveform { get;} = MagneticWaveform.Pulse;
        public double Voltage { get;} = 1;
        public double RaiseDuration { get;} = 500;
        public double FallDuration { get;} = 500;
        public double Interval { get;} = 10;
        public int Waves { get; } = 1;

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

