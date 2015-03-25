using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WKFramework.Utils.UnitTests
{
    public static class UnitTestHelper
    {
        public static void ShowWindowInThread(Action action)
        {
            var thread = new Thread(() =>
            {
                action();
                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}
