using Ivi.Visa.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows;


namespace Vocal
{
    
    /// <summary>
    /// DeviceControl.xaml の相互作用ロジック
    /// </summary>
    public partial class DAQControl : UserControl
    {
        
        public DAQControl()
        {
            InitializeComponent();
            SoundChannelBox.ItemsSource = AnalogChannels;
            TriggerChannelBox.ItemsSource = DigitalChannels;
            ExDeviceChannelBox.ItemsSource = AnalogChannels;
            ExDeviceChannel2Box.ItemsSource = AnalogChannels;
            ALLTriggerChannelBox.ItemsSource = AnalogChannels;

        }


        public string Identifer
        {
            get { return DeviceBox.Text; }
            set { DeviceBox.Text = value; }
        }

        public double SamplingRate
        {
            get
            {
                return double.Parse(SamplingRateBox.Text);
            }
            set
            {
                SamplingRateBox.Text = value.ToString();
            }
        }

        public string SoundChannel
        {
            get
            {
                return SoundChannelBox.SelectedValue.ToString();
            }
        }
        public string TriggerChannel
        {
            get
            {
                return TriggerChannelBox.SelectedValue.ToString();
            }
        }
        public string ALLTriggerChannel
        {
            get
            {
                return ALLTriggerChannelBox.SelectedValue.ToString();
            }
        }
        public string ExDeviceChannel
        {
            get
            {
                return ExDeviceChannelBox.SelectedValue.ToString();
            }
        }
        public string ExDeviceChannel2
        {
            get
            {
                return ExDeviceChannel2Box.SelectedValue.ToString();
            }
        }

        public ObservableCollection<string> AnalogChannels { get; set; } = new ObservableCollection<string> { "ao0", "ao1", "ao2", "ao3" };
        public ObservableCollection<string> DigitalChannels { get; set; } = new ObservableCollection<string> { "Port0" };
    }
       
}
