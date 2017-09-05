﻿using System;
using TimeCourse;

namespace SoundWave
{
    public class SoundWave
    {
        public double[] values { get; set; }

        public SoundWave(double[] values)
        {
            this.values = values;
        }
        public SoundWave(Duration duration, double samplingRate) : this(new double[(duration * samplingRate).value.Ticks]) { }

    }

    public class PureTone : SoundWave
    {
        public PureTone(double amplitude, double frequency, Duration duration, double samplingRate): base(duration, samplingRate)
        {
            for(var i =0; i < values.Length; ++i)
            {
                values[i] = amplitude*Math.Sin(Math.PI * 2 * i * (frequency / samplingRate));
            }
        }
    }

    public class TonePip : PureTone
    {
        public TonePip(double amplitude, double frequency, Duration duration, double samplingRate): base(amplitude, frequency, duration, samplingRate)
        {
            var size = values.Length / 2;
            for(var i = 0; i < size; ++i)
            {
                var k = (double)i / size;
                values[i] = values[i] * k;
                values[values.Length - 1 - size] = values[values.Length - 1 - size] * k;
            }
        }
    }

}
