using System;
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
    /// IntervalControl.xaml の相互作用ロジック
    /// </summary>
    public partial class IntervalControl : UserControl
    {
        public IntervalControl()
        {
            InitializeComponent();

            Duration = 8000;
            Waggle = 1000;

        }


        /// <summary>
        /// Duration
        /// </summary>
        public double Duration
        {
            get
            {
                return double.Parse(duration.Text);
            }
            set
            {
                duration.Text = value.ToString();
            }
        }

        /// <summary>
        /// Waggle
        /// </summary>
        public double Waggle
        {
            get
            {
                return double.Parse(waggle.Text);
            }
            set
            {
                waggle.Text = value.ToString();
            }
        }

        /// <summary>
        /// rundom engine
        /// </summary>
        Random Random = new Random();

        /// <summary>
        /// Interval
        /// </summary>
        public TimeSpan Interval
        {
            get
            {
                return TimeSpan.FromMilliseconds(Duration + Random.NextDouble() * Waggle);
            }
        }

    }
}
