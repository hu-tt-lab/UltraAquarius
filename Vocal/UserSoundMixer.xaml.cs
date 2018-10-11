using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
namespace Vocal
{
    /// <summary>
    /// UserSoundMixer.xaml の相互作用ロジック
    /// </summary>
    public partial class UserSoundMixer : UserControl
    {

        public class Variable
        {
            public string Name { get; set; }
            public string FilePath { get; set; }
        }

        public UserSoundMixer()
        {
            InitializeComponent();
            TableView.DataContext = Rows;
        }

        /// <summary>
        /// Delete selected row
        /// </summary>
        public void Pop()
        {
            var index = TableView.SelectedIndex;
            if (index >= 0)
            {
                Rows.RemoveAt(index);
            }
        }

        /// <summary>
        /// Push row
        /// </summary>
        public void Push()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "(*.csv)|*.*";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!Rows.Select(x=>x.FilePath).Contains(dialog.FileName))
                    {
                        var values = Rows.Where(x => Regex.IsMatch(x.Name, @"\d+")).ToList();
                        var id = values.Count != 0 ? values.Select(x => int.Parse(Regex.Match(x.Name, @"\d+").Value)).Max() + 1 : 0;
                        Rows.Add(new Variable { Name = string.Format("u{0:d}", id), FilePath = dialog.FileName});
                    }
                }
            }
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            Push();
        }
        private void OnDelete(object sender, RoutedEventArgs e)
        {
            Pop();
        }

        public void Lock()
        {
            var keys = Rows.Select(x => x.Name);
            if (keys.Count() != keys.Distinct().Count()) throw new Exception("The key must be unique.");

            Key = true;
            IsEnabled = false;
        }

        public void Unlock()
        {
            Key = false;
            IsEnabled = true;
        }

        bool IsLocked()
        {
            return Key;
        }

        public string Find(string key)
        {
            if (!IsLocked()) throw new Exception("Table must be Locked.");
            var variables = Rows.Where(x => x.Name == key).ToList();
            if (variables.Count != 1) throw new KeyNotFoundException(string.Format("table does not have {0:g}", key));
            return variables[0].FilePath;
        }

        /// <summary>
        /// Raw Data
        /// </summary>
        public ObservableCollection<Variable> Rows { get; set; } = new ObservableCollection<Variable> { };

        private bool Key = false;
    }
}
