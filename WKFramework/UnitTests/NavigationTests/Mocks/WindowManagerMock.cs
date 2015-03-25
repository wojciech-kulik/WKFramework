using System.Reflection;
using WKFramework.WPF.Navigation;

namespace UnitTests.NavigationTests.Mocks
{
    public class WindowManagerMock : WindowManager
    {
        protected override Assembly GetAssemblyWithViews()
        {
            return Assembly.GetExecutingAssembly();
        }
    }
}
