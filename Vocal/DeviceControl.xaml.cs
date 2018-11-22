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
    public partial class DeviceControl : UserControl
    {
        public DeviceControl()
        {
            InitializeComponent();
            SoundChannelBox.ItemsSource = Channels;
            TriggerChannelBox.ItemsSource = Channels;
            FunGeneChannelBox.ItemsSource = Channels;


        }

        ResourceManager RM = new ResourceManager();
        public FunGene Fungene = new FunGene();

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

        public string FunGeneChannel
        {
            get
            {
                return string.Format("{0:g}/{1:g}", Identifer, FunGeneChannelBox.SelectedValue.ToString());
            }
        }

        public ObservableCollection<string> Channels { get; set; } = new ObservableCollection<string> { "ao0", "ao1", "ao2"};

        private void OnGetResourseClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var resources = Fungene.GetResourse();
            ResourceComboBox.ItemsSource = Enumerable.Range(0, resources.GetLength(0))
            .Select(i => new VisaResuorce { Resource = resources[i] })
            .ToList();
        }

        private void ResourceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var name = (VisaResuorce)ResourceComboBox.SelectedItem;
                FunGeneIDBox.Text = Fungene.Open(name.Resource);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
            
        }
        
    }
    public class VisaResuorce
    {
        public string Resource { get; set; }
    }
}
