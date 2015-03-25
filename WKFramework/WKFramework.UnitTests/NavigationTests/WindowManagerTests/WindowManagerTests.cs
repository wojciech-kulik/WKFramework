using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.NavigationTests.Mocks;
using WKFramework.Utils.UnitTests;

namespace UnitTests.NavigationTests.WindowManagerTests
{
    [TestClass]
    public class WindowManagerTests
    {
        [TestMethod]
        public void AttachViewModelAndShow()
        {
            var manager = new WindowManagerMock();

            UnitTestHelper.ShowWindowInThread(() =>
                {
                    PersonDetailsView.ClosedEvent = (w) => Assert.IsTrue(w.DataContext is PersonDetailsViewModel);
                    manager.ShowWindow<PersonDetailsViewModel>();
                });

            UnitTestHelper.ShowWindowInThread(() =>
                {
                    var viewModel = new PersonDetailsViewModel() { City = "Los Angeles" };
                    PersonDetailsView.ClosedEvent = (w) =>
                    {
                        Assert.AreSame(viewModel, w.DataContext);
                        Assert.AreEqual("Los Angeles", (w.DataContext as PersonDetailsViewModel).City);
                    };
                    manager.ShowWindow(viewModel);
                });   
        }
    }
}
