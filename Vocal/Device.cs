using System;
using NationalInstruments.DAQmx;

namespace Vocal
{
    public class Device : IDisposable
    {
        public string[] Channels { get; }
        public string TriggerChannel { get { return Channels[1]; } }
        public string SignalChannel { get { return Channels[0]; } }
        public double SamplingRate { get; }

        private Task task_;
        private double[,] data_;

        public Device(string[] channels, double samplingRate, TimeSpan timeout)
        {
            Channels = channels;
            SamplingRate = samplingRate;
            data_ = new double[2, (int)(samplingRate * timeout.TotalSeconds)];

            var voltage = (max: 10, min: -10);
            task_ = new Task();
            task_.AOChannels.CreateVoltageChannel(SignalChannel, "", voltage.min, voltage.max, AOVoltageUnits.Volts);
            task_.AOChannels.CreateVoltageChannel(TriggerChannel, "", voltage.min, voltage.max, AOVoltageUnits.Volts);
            task_.Timing.ConfigureSampleClock("", samplingRate,
                SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, data_.GetLength(1));

            task_.Done += (sender, e) => task_.Stop();

        }
        public Device(TimeSpan timeout, double samplingRate = 100000) :
            this(new string[2] { "Dev1/ao0", "Dev1/ao1" }, samplingRate, timeout) { }

        public void Output(double[] wave, double[] trigger)
        {
            data_.Initialize();
            for (var i = 0; i < wave.Length; i++)
            {
                data_[0, i] = wave[i];
                data_[1, i] = trigger[i];
            }

            var stream = new AnalogMultiChannelWriter(task_.Stream);
            stream.WriteMultiSample(false, data_);
            task_.Start();
            task_.WaitUntilDone();

        }
        public void Output(double[] wave, double trigger)
        {
            data_.Initialize();
            for (var i = 0; i < wave.Length; i++)
            {
                data_[0, i] = wave[i];
                data_[1, i] = trigger;
            }
            var stream = new AnalogMultiChannelWriter(task_.Stream);
            stream.WriteMultiSample(false, data_);
            task_.Start();
            task_.WaitUntilDone();
        }

        public void Dispose()
        {
            task_.Dispose();
        }
    }
}
