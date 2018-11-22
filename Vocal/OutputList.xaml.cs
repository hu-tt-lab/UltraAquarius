using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Vocal
{
    /// <summary>
    /// Signal type
    /// </summary>
    public enum SignalType {
        Pure,
        Click,
        Modulation,
        Ultrasound,
        User
    }

    /// <summary>
    /// OutputList.xaml の相互作用ロジック
    /// </summary>
    public partial class OutputList : UserControl
    {
        public class Optional
        {
            public bool Valid { get; set; } = true;
            public string Variable { get; set; }
            public SignalType Type { get; set; }

            public Optional(Optional rhs)
            {
                Valid = rhs.Valid;
                Variable = rhs.Variable;
                Type = rhs.Type;
            }

            public Optional() { }
        }

        public OutputList()
        {
            InitializeComponent();

            // Data Binding
            TableView.DataContext = Rows;
        }

        public IEnumerable<(string Name, SignalType Type)> List
        {
            get
            {
                foreach(var e in Rows)
                {
                    if (e.Valid == true)
                    {
                        yield return (Name: e.Variable, Type: e.Type);
                    }
                }
            }
        }

        public ObservableCollection<Optional> Rows { get; set; } = new ObservableCollection<Optional>();

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
        /// Add row to end
        /// </summary>
        /// <param name="rhs">sound parameter</param>
        public void Add(string rhs)
        {
            Rows.Add(new Optional{ Variable = rhs });
        }

        /// <summary>
        /// Insert row
        /// </summary>
        /// <param name="index">index to insert object into<param>
        public void Insert(int index)
        {
            Rows.Insert(index, new Optional(Rows[index]));
        }
        /// <summary>
        /// Insert row to end.
        /// </summary>
        public void Insert()
        {
            Insert(Rows.Count - 1);
        }

        /// <summary>
        /// Push row
        /// </summary>
        public void Push()
        {
            var index = TableView.SelectedIndex;
            if (index >= 0)
            {
                Insert(index);
            }
            else
            {
                Rows.Add(new Optional());
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

    }
}
