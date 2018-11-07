using System;
using System.Linq;
using System.Collections.Generic;
using NationalInstruments.DAQmx;


namespace Vocal
{
    public class Device : IDisposable
    {
        public string[] Channels { get; }
        public double SamplingRate { get; }
        public double Duration { get; }
        public int Capacity { get { return (int)(SamplingRate * Duration); } }
        public int Count { get { return Channels.Length; } }

        private Task task_;
        private double[,] data_;

        public Device(double samplingRate, double duration, params string[] channels)
        {
            Channels = channels;
            SamplingRate = samplingRate;
            Duration = duration;
            data_ = new double[Channels.Length, Capacity + 10];

            var voltage = (max: 10, min: -10);

#if DEBUG
#else
            task_ = new Task();
            foreach (var e in channels)
            {
                task_.AOChannels.CreateVoltageChannel(e, "", voltage.min, voltage.max, AOVoltageUnits.Volts);
            }
            task_.Timing.ConfigureSampleClock("", SamplingRate, SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples, data_.GetLength(1));

            task_.Done += (sender, e) => task_.Stop();
#endif
        }

        public void Output(params IEnumerable<double>[] waves)
        {
            for(var u = 0; u < data_.Rank; ++u)
            {
                for(var v = 0; v < data_.Length / data_.Rank; ++v)
                {
                    data_[u, v] = 0;
                }
            }

            foreach (var (k, v) in waves.Select((value, index) => (index, value)))
            {
                foreach (var (i, e) in v.Select((value, index) => (index, value)))
                {
                    data_[k, i] = e;
                }
            }

#if DEBUG
#else
            var stream = new AnalogMultiChannelWriter(task_.Stream);
            stream.WriteMultiSample(false, data_);
            task_.Start();

            task_.WaitUntilDone();
#endif
        }

        public void Dispose()
        {
#if DEBUG
#else
            task_.Dispose();
#endif
        }
    }
}
