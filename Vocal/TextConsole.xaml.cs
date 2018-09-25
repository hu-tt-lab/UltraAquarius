using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
    /// TextConsole.xaml の相互作用ロジック
    /// </summary>
    public partial class TextConsole : UserControl
    {
        public TextConsole()
        {
            InitializeComponent();
            writer = new StringWriter(new StringBuilder(Console.Text));
        }

        public TextConsole Return()
        {
            writer.WriteLine();
            return this;
        }

        public TextConsole WriteLine(string text)
        {
            writer.WriteLine(text);
            return this;
        }
        public TextConsole WriteLine(string text, object x)
        {
            writer.WriteLine(text, x);
            return this;
        }
        public TextConsole WriteLine(string text, object x, object y)
        {
            writer.WriteLine(text, x, y);
            return this;
        }
        public TextConsole WriteLine(string text, object x, object y, object z)
        {
            writer.WriteLine(text, x, y, z);
            return this;
        }
        public TextConsole WriteLine(string text, object x, object y, object z, object w)
        {
            writer.WriteLine(text, x, y, z, w);
            return this;
        }

        public void End()
        {
            Console.Text = writer.ToString();
            Console.ScrollToEnd();
        }

        public void Flush()
        {
            writer.Flush();
            Console.Text = writer.ToString();
        }

        StringWriter writer;
    }
}
