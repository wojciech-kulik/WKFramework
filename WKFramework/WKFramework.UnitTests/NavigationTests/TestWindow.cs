using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UnitTests.NavigationTests
{
    public class TestWindow : Window
    {
        public static Action<Window> ClosedEvent { get; set; }

        public TestWindow()
        {
            Loaded += (s, args) => Dispatcher.BeginInvoke(new Action(() => Close()));
        }

        protected override void OnClosed(EventArgs e)
        {
            if (ClosedEvent != null)
                ClosedEvent(this);

            Dispatcher.InvokeShutdown();
            base.OnClosed(e);
        }
    }
}
