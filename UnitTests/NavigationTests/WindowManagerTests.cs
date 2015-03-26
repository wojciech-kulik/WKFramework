using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using UnitTests.NavigationTests.Mocks;
using WKFramework.Utils.UnitTests;
using WKFramework.Utils.UnitTests.Helpers;

namespace UnitTests.NavigationTests
{
    [TestClass]
    public class WindowManagerTests
    {
        WindowManagerMock _manager = new WindowManagerMock();

        [TestMethod]
        public void ShowWindowWithoutViewModel()
        {
            var manager = new WindowManagerMock();

            UnitTestHelper.ShowWindowInThread(() =>
            {
                manager.ShowWindow<PersonDetailsViewModel>();
                PersonDetailsView.WaitForClose();
                Assert.IsInstanceOfType(PersonDetailsView.Window.DataContext, typeof(PersonDetailsViewModel));
            });   
        }

        [TestMethod]
        public void ShowWindowWithViewModel()
        {
            UnitTestHelper.ShowWindowInThread(() =>
            {
                var viewModel = new PersonDetailsViewModel() { City = "Los Angeles" };
                _manager.ShowWindow(viewModel);
                PersonDetailsView.WaitForClose();

                Assert.AreSame(viewModel, PersonDetailsView.Window.DataContext);
                Assert.AreEqual("Los Angeles", (PersonDetailsView.Window.DataContext as PersonDetailsViewModel).City);
            });
        }

        [TestMethod]
        public void ShowDialogWithoutViewModel()
        {
            UnitTestHelper.ShowWindowInThread(() =>
            {
                PersonView.Result = false;
                var result = _manager.ShowDialog<PersonViewModel>();

                Assert.AreEqual(false, result);
                Assert.IsInstanceOfType(PersonView.Window.DataContext, typeof(PersonViewModel));
            });
        }

        [TestMethod]
        public void ShowDialogWithViewModel()
        {
            UnitTestHelper.ShowWindowInThread(() =>
            {
                PersonView.Result = true;
                var viewModel = new PersonViewModel() { FirstName = "Jack" };
                var result = _manager.ShowDialog(viewModel);

                Assert.AreSame(viewModel, PersonView.Window.DataContext);
                Assert.AreEqual("Jack", (PersonView.Window.DataContext as PersonViewModel).FirstName);
                Assert.AreEqual(true, result);

                PersonView.Result = null;
                result = _manager.ShowDialog(viewModel);
                Assert.AreEqual(false, result);
            });
        }

        [TestMethod]
        public void CreatingWindowListFromAssembly()
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
            AssertExt.ThrowsException<ArgumentOutOfRangeException>(() => manager.ShowDialog<ContactsViewModel>());
            AssertExt.ThrowsException<ArgumentOutOfRangeException>(() => manager.ShowDialog(vm));
        }

        [TestMethod]
        public void CustomViewModelFactory()
        {
            var manager = new WindowManagerMock(new CustomViewModelFactory());

            UnitTestHelper.ShowWindowInThread(() =>
            {
                manager.ShowDialog<PersonViewModel>();
                Assert.AreEqual("[PVW] Changed by factory", (PersonView.Window.DataContext as PersonViewModel).LastName);

                manager.ShowDialog<PersonDetailsViewModel>();
                Assert.AreEqual("[PDVW] Changed by factory", (PersonView.Window.DataContext as PersonDetailsViewModel).City);

                manager.ShowWindow<PersonViewModel>();
                PersonView.WaitForClose();
                Assert.AreEqual("[PVW] Changed by factory", (PersonView.Window.DataContext as PersonViewModel).LastName);

                manager.ShowWindow<PersonDetailsViewModel>();
                PersonDetailsView.WaitForClose();
                Assert.AreEqual("[PDVW] Changed by factory", (PersonView.Window.DataContext as PersonDetailsViewModel).City);
            });
        }

        [TestMethod]
        public void GetAssemblyWithViews()
        {
            //it will return null for UnitTests, added to get 100% code coverage
            new WindowManagerMock().GetBaseAssemblyWithViews();
        }
    }
}
