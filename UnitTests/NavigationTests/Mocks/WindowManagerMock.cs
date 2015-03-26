using System.Reflection;
using WKFramework.WPF.Navigation;

namespace UnitTests.NavigationTests.Mocks
{
    public class WindowManagerMock : WindowManager
    {
        public WindowManagerMock()
        {
        }

        public WindowManagerMock(IViewModelFactory factory)
            : base(factory)
        { }

        protected override Assembly GetAssemblyWithViews()
        {
            return Assembly.GetExecutingAssembly();
        }

        public Assembly GetBaseAssemblyWithViews()
        {
            return base.GetAssemblyWithViews();
        }
    }
}
