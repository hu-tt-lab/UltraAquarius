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
            SoundChannelBox.ItemsSource = Channels;
            TriggerChannelBox.ItemsSource = Channels;
            FunGeneChannelBox.ItemsSource = Channels;
            FunGeneTriggerChannelBox.ItemsSource = Channels;

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
                return string.Format("{0:g}/{1:g}", Identifer, SoundChannelBox.SelectedValue.ToString());
            }
        }
        public string TriggerChannel
        {
            get
            {
                return string.Format("{0:g}/{1:g}", Identifer, TriggerChannelBox.SelectedValue.ToString());
            }
        }
        public string FunGeneTriggerChannel
        {
            get
            {
                return string.Format("{0:g}/{1:g}", Identifer, FunGeneTriggerChannelBox.SelectedValue.ToString());
            }
        }
        public string FunGeneChannel
        {
            get
            {
                return string.Format("{0:g}/{1:g}", Identifer, FunGeneChannelBox.SelectedValue.ToString());
            }
        }

        public ObservableCollection<string> Channels { get; set; } = new ObservableCollection<string> { "ao0", "ao1", "ao2", "ao3" };
    }
       
}
