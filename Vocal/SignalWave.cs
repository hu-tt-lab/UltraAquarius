using System;
using System.Collections.Generic;
using System.Linq;

namespace Vocal
{

    /// <summary>
    /// sound wave data
    /// </summary>
    public abstract class SignalWave
    {
        public SignalWave(double sampling, double duration)
        {
            SamplingRate = sampling;
            Duration = duration;
        }
        public abstract IEnumerable<double> Wave { get; }

        public double SamplingRate { get; }
        public double Duration { get; }
        public double Size { get { return (int)(SamplingRate * Duration); } }

    }

    /// <summary>
    /// pure tone wave data
    /// </summary>
    public class PureWave : SignalWave
    {
        public PureWave(double frequency, double amplitude,
            double sampling, double duration) : base(sampling, duration)
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
    /// tone pip wave data
    /// </summary>
    public class PipWave : SignalWave
    {
        public PipWave(double frequency, double amplitude,
            double sampling, double duration) : base(sampling, duration)
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
    /// tone pip wave data
    /// </summary>
    public class BurstWave : SignalWave
    {
        public BurstWave(double frequency, double amplitude,
            double sampling, double duration) : base(sampling, duration)
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

    public class ClickWave : SignalWave
    {
        public ClickWave(double amplitude, double sampling, double duration): base(sampling, duration)
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

    public class Trigger : SignalWave
    {
        public Trigger(double level, double sampling, double duration) : base(sampling, duration)
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

}
