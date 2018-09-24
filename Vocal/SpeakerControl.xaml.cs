using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;


namespace Vocal
{
    /// <summary>
    /// SpeakerControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SpeakerControl : UserControl
    {
        public SpeakerControl()
        {
            InitializeComponent();

            Path = new ObservableCollection<string>(Directory
                .EnumerateFiles(Directory.GetCurrentDirectory())
                .Where(x => Regex.IsMatch(x, @".csv$")));

            // Data Binding
            CalibrationList.ItemsSource = Path;

        }

        public ObservableCollection<string> Path { get; set; }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "(*.csv)|*.*";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!Path.Contains(dialog.FileName))
                    {
                        Path.Add(dialog.FileName);
                        if (CalibrationList.SelectedIndex < 0)
                        {
                            CalibrationList.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        public event EventHandler Changed;

        private void SelectionChanged(object sender, EventArgs e)
        {
            Changed?.Invoke(sender, e);
        }

        public string Selected { get { return CalibrationList.SelectedItem.ToString(); } }

    }
}
