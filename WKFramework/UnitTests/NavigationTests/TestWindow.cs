using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace UnitTests.NavigationTests
{
    public class TestWindow : Window
    {
        private static AutoResetEvent _closeMonitor;
        public static Window Window { get; private set; }
        public static bool? Result { get; set; }

        public static void WaitForClose()
        {
            _closeMonitor.WaitOne();
        }

        public TestWindow()
        {
            _closeMonitor = new AutoResetEvent(false);
            Window = this;
            Loaded += TestWindow_Loaded;
        }

        void TestWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = Result;
            }
            catch { }

            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _closeMonitor.Set();
            base.OnClosed(e);
        }
    }
}
