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
    /// FunGeneControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FunGeneControl : UserControl
    {
        public FunGeneControl()
        {
            InitializeComponent();
        }

        ResourceManager RM = new ResourceManager();
        public FunGene Fungene = new FunGene();

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
