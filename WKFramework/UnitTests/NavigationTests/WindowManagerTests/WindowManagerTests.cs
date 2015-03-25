using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using UnitTests.NavigationTests.Mocks;
using WKFramework.Utils.UnitTests;
using WKFramework.Utils.UnitTests.Helpers;

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
                manager.ShowWindow<PersonDetailsViewModel>();
                PersonDetailsView.WaitForClose();

                Assert.IsInstanceOfType(PersonDetailsView.Window.DataContext, typeof(PersonDetailsViewModel));
            });

            UnitTestHelper.ShowWindowInThread(() =>
            {
                var viewModel = new PersonDetailsViewModel() { City = "Los Angeles" };
                manager.ShowWindow(viewModel);
                PersonDetailsView.WaitForClose();

                Assert.AreSame(viewModel, PersonDetailsView.Window.DataContext);
                Assert.AreEqual("Los Angeles", (PersonDetailsView.Window.DataContext as PersonDetailsViewModel).City);
            });   
        }

        [TestMethod]
        public void AttachViewModelAndShowDialog()
        {
            var manager = new WindowManagerMock();

            UnitTestHelper.ShowWindowInThread(() =>
            {
                PersonView.Result = false;
                var result = manager.ShowDialog<PersonViewModel>();

                Assert.AreEqual(false, result);
                Assert.IsInstanceOfType(PersonView.Window.DataContext, typeof(PersonViewModel));
            });

            UnitTestHelper.ShowWindowInThread(() =>
            {
                PersonView.Result = true;
                var viewModel = new PersonViewModel() { FirstName = "Jack" };
                bool? result = manager.ShowDialog(viewModel);

                Assert.AreSame(viewModel, PersonView.Window.DataContext);
                Assert.AreEqual("Jack", (PersonView.Window.DataContext as PersonViewModel).FirstName);
                Assert.AreEqual(true, result);

                PersonView.Result = null;
                result = manager.ShowDialog(viewModel);
                Assert.AreEqual(false, result);
            });
        }

        [TestMethod]
        public void ListOfWindows()
        {
            var manager = new WindowManagerMock();

            Assert.AreEqual(3, manager.KnownWindows.Count);
            Assert.AreEqual(typeof(PersonDetailsView), manager.KnownWindows["PersonDetails"]);
            Assert.AreEqual(typeof(PersonView), manager.KnownWindows["Person"]);
            Assert.AreEqual(typeof(TestWindow), manager.KnownWindows["TestWindow"]);
        }

        [TestMethod]
        public void ThrowIfNoCorrespondingView()
        {
            var manager = new WindowManagerMock();
            var vm = new ContactsViewModel();

            AssertExt.ThrowsException<ArgumentOutOfRangeException>(() => manager.ShowWindow<ContactsViewModel>());
            AssertExt.ThrowsException<ArgumentOutOfRangeException>(() => manager.ShowWindow(vm));
        }
    }
}
