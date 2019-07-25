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
        public string Port { get; }

        private string ClockSource { get; }
        private string Name { get; }

        private string ToOutputChannel(string channel)
        {
            return string.Format("{0:g}/{1:g}", Name, channel);
        }
        private string ToClockChannel(string channel)
        {
            return string.Format("/{0:g}/{1:g}", Name, channel);
        }

        private (Task analog, Task digital) task_;
        private double[,] data_;
        private NationalInstruments.DigitalWaveform trigger_;

        public Device(string device, double samplingRate, double duration, string port, params string[] channels)
        {
            Name = device;
            Channels = channels;
            SamplingRate = samplingRate;
            Duration = duration;
            data_ = new double[Channels.Length, Capacity + 10];
            trigger_ = new NationalInstruments.DigitalWaveform(data_.GetLength(1), 8, NationalInstruments.DigitalState.ForceDown);
            ClockSource = "ao/SampleClock";
            Port = string.Format("{0:g}/line0:7", port);
            var voltage = (max: 10, min: -10);

#if DEBUG
#else
            task_ = (analog: new Task(), digital: new Task());
            foreach (var e in channels)
            {
                task_.analog.AOChannels.CreateVoltageChannel(ToOutputChannel(e), "", voltage.min, voltage.max, AOVoltageUnits.Volts);
            }
            task_.analog.Timing.ConfigureSampleClock("", SamplingRate, SampleClockActiveEdge.Rising,
                            SampleQuantityMode.FiniteSamples, data_.GetLength(1));
            task_.analog.Done += (sender, e) => { task_.digital.Stop(); task_.analog.Stop(); };

            task_.digital.DOChannels.CreateChannel(ToOutputChannel(Port), "", ChannelLineGrouping.OneChannelForAllLines);
            task_.digital.Timing.ConfigureSampleClock(ToClockChannel(ClockSource), samplingRate, SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples, data_.GetLength(1));
#endif
        }

        public void Output(int number, params IEnumerable<double>[] waves)
        {
            for (var u = 0; u < data_.GetLength(0); ++u)
            {
                for(var v = 0; v < data_.GetLength(1); ++v)
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
            var state = new NationalInstruments.DigitalState[trigger_.Signals.Count];
            for (var i = 0; i < state.Length; ++i)
            {
                if (((1<<number-1) & (1 << (state.Length - i - 1))) != 0) state[i] = NationalInstruments.DigitalState.ForceUp;
                else state[i] = NationalInstruments.DigitalState.ForceDown;
            }

            for (var t = 0; t < trigger_.Samples.Count / 10; ++t)
            {
                for (var i = 0; i < trigger_.Samples[t].States.Count; ++i)
                {
                    trigger_.Samples[t].States[i] = state[i];
                }
            }
#if DEBUG
#else
            var digital = new DigitalSingleChannelWriter(task_.digital.Stream);
            digital.WriteWaveform(false, trigger_);
            var analog = new AnalogMultiChannelWriter(task_.analog.Stream);
            analog.WriteMultiSample(false, data_);
            task_.digital.Start();
            task_.analog.Start();

            task_.analog.WaitUntilDone();
#endif
        }

        public void Dispose()
        {
#if DEBUG
#else
            task_.analog.Dispose();
            task_.digital.Dispose();
#endif
        }
    }
}
