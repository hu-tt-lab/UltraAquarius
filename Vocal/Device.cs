using System;
using NationalInstruments.DAQmx;

namespace Vocal
{
    public class Device : IDisposable
    {
        public string[] channels { get; }
        public string triggerChannel { get { return channels[1]; } }
        public string signalChannel { get { return channels[0]; } }
        public double samplingRate { get; }

        private Task task_;
        private double[,] data_;

        public Device(string[] channels, double samplingRate, TimeSpan timeout)
        {
            this.channels = channels;
            this.samplingRate = samplingRate;
            this.data_ = new double[2, (int)(samplingRate * timeout.TotalSeconds)];
#if !DEBUG
            var voltage = (max: 10, min: -10);
            task_ = new Task();
            task_.AOChannels.CreateVoltageChannel(signalChannel, "", voltage.min, voltage.max, AOVoltageUnits.Volts);
            task_.AOChannels.CreateVoltageChannel(triggerChannel, "", voltage.min, voltage.max, AOVoltageUnits.Volts);
            task_.Timing.ConfigureSampleClock("", samplingRate,
                SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, data_.GetLength(1));

            task_.Done += (sender, e) => task_.Stop();
#endif

        }
        public Device(TimeSpan timeout, double samplingRate = 100000) : this(new string[2] { "Dev1/ao0", "Dev1/ao1" }, samplingRate, timeout) { }

        public void Output(double[] wave, double[] trigger)
        {
            data_.Initialize();
            for (var i = 0; i < wave.Length; i++)
            {
                data_[0, i] = wave[i];
                data_[1, i] = trigger[i];
            }

#if !DEBUG
            var stream = new AnalogMultiChannelWriter(task_.Stream);
            stream.WriteMultiSample(false, data_);

            task_.Start();
            task_.WaitUntilDone();
#endif

        }
        public void Output(double[] wave, double trigger)
        {
            var value = new double[wave.Length];
            for (var i = 0; i < value.Length / 2; i++) { value[i] = trigger; }
            Output(wave, value);
        }

        public void Dispose()
        {
#if !DEBUG
            task_.Dispose();
#endif
        }
    }
}
