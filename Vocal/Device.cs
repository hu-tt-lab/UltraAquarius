using NationalInstruments.DAQmx;

namespace Vocal
{
    public class Device
    {
        public string[] channels { get; }
        public string triggerChannel { get { return channels[1]; } }
        public string signalChannel { get { return channels[0]; } }
        public double samplingRate { get; }

        private Task task_;

        public Device(string[] channels, double samplingRate)
        {
            this.channels = channels;
            this.samplingRate = samplingRate;
            task_ = new Task();
            task_.AOChannels.CreateVoltageChannel(signalChannel, "", -10, 10, AOVoltageUnits.Volts);
            task_.AOChannels.CreateVoltageChannel(triggerChannel, "", -10, 10, AOVoltageUnits.Volts);
            task_.Timing.ConfigureSampleClock("", samplingRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples);

            task_.Done += (sender, e) => task_.Stop();

        }
        public Device(double samplingRate = 100000) : this(new string[2] { "Dev/ao0", "Dev/ao1" }, samplingRate) { }
        public void Output(double[] wave, double[] trigger)
        {
            var data = new double[wave.Length, 2];
            for (var i = 0; i < wave.Length; i++)
            {
                data[0, i] = wave[i];
                data[1, i] = trigger[i];
            }

            var stream = new AnalogMultiChannelWriter(task_.Stream);
            stream.WriteMultiSample(false, data);

            task_.Start();
            task_.WaitUntilDone();
        }
        public void Output(double[] wave, double trigger)
        {
            var value = new double[wave.Length];
            for (var i = 0; i < value.Length / 2; i++) { value[i] = trigger; }
            Output(wave, value);
        }

    }
}
