using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.NavigationTests.Mocks;
using WKFramework.WPF.Navigation;

namespace UnitTests.NavigationTests
{
    [TestClass]
    public class NavigationServiceTests
    {
        private WindowManagerMock _windowManager = new WindowManagerMock();
        private NavigationService _navigationService;

        public NavigationServiceTests()
        {
            _navigationService = new NavigationService(_windowManager);
        }

        [TestMethod]
        public void GetWindow()
        {
            Assert.IsInstanceOfType(_navigationService.GetWindow<PersonDetailsViewModel>(), typeof(NavigationService<PersonDetailsViewModel>));
            Assert.IsInstanceOfType(_navigationService.GetWindow<PersonViewModel>(), typeof(NavigationService<PersonViewModel>));
        }

        [TestMethod]
        public void ThrowsIfWindowNotFound()
        {
            AssertExt.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _navigationService.GetWindow<ContactsViewModel>()
                    .ShowWindow();
            });
        }

        [TestMethod]
        public void ShowWindowWithParams()
        {
            _navigationService.GetWindow<PersonDetailsViewModel>()
                .WithParam(vm => vm.City, "My city")
                .WithParam(vm => vm.Address, "Address")
                .ShowWindow();
            PersonDetailsView.WaitForClose();

            Assert.IsInstanceOfType(PersonDetailsView.Window.DataContext, typeof(PersonDetailsViewModel));
            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreEqual("My city", viewModel.City);
            Assert.AreEqual("Address", viewModel.Address);
        }

        [TestMethod]
        public void SetupViewModel()
        {
            _navigationService.GetWindow<PersonDetailsViewModel>()
                .SetupViewModel(vm => vm.City = "Los Santos")
                .ShowWindow();

            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreEqual("Los Santos", viewModel.City);
            PersonDetailsView.WaitForClose();
        }

        [TestMethod]
        public void DoIfAcceptedAndShowWindow()
        {
            _navigationService.GetWindow<PersonDetailsViewModel>()
                .DoIfAccepted(vm => vm.City = "Los Santos")
                .ShowWindow();

            PersonDetailsView.WaitForClose();
            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreNotEqual("Los Santos", viewModel.City);
        }

        [TestMethod]
        public void DoIfAcceptedAndShowDialog()
        {
            PersonDetailsView.Result = false;
            var result = _navigationService.GetWindow<PersonDetailsViewModel>()
                            .DoIfAccepted(vm => vm.City = "Los Santos")
                            .ShowDialog();

            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreNotEqual("Los Santos", viewModel.City);


            PersonDetailsView.Result = true;
            result = _navigationService.GetWindow<PersonDetailsViewModel>()
                            .DoIfAccepted(vm => vm.City = "Los Santos")
                            .ShowDialog();
            viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreEqual("Los Santos", viewModel.City);
        }

        [TestMethod]
        public void DoAfterCloseAndShowWindow()
        {
            _navigationService.GetWindow<PersonDetailsViewModel>()
                .DoAfterClose(vm => vm.Age = 25)
                .ShowWindow();

            PersonDetailsView.WaitForClose();
            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreEqual(25, viewModel.Age);
        }

        [TestMethod]
        public void DoAfterCloseAndShowDialog()
        {
            _navigationService.GetWindow<PersonDetailsViewModel>()
                .DoAfterClose(vm => vm.Age = 25)
                .ShowDialog();

            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreEqual(25, viewModel.Age);
        }

        [TestMethod]
        public void DoIfCancelledAndShowDialogCancelled()
        {
            PersonDetailsView.Result = false;
            _navigationService.GetWindow<PersonDetailsViewModel>()
                .DoIfCancelled(vm => vm.Age = 25)
                .ShowDialog();

            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreEqual(25, viewModel.Age);
        }

        [TestMethod]
        public void DoIfCancelledAndShowDialogNotCancelled()
        {
            PersonDetailsView.Result = true;
            _navigationService.GetWindow<PersonDetailsViewModel>()
                .DoIfCancelled(vm => vm.Age = 25)
                .ShowDialog();

            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreNotEqual(25, viewModel.Age);
        }

        [TestMethod]
        public void DoIfCancelledAndShowWindow()
        {
            _navigationService.GetWindow<PersonDetailsViewModel>()
                .DoIfCancelled(vm => vm.Age = 25)
                .ShowWindow();

            PersonDetailsView.WaitForClose();
            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreNotEqual(25, viewModel.Age);
        }

        [TestMethod]
        public void CombinedFluentActions()
        {
            _navigationService.GetWindow<PersonDetailsViewModel>()
                .WithParam(vm => vm.City, "My city")
                .DoAfterClose(vm => vm.Age = 24)
                .ShowWindow();

            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreEqual("My city", viewModel.City);

            PersonDetailsView.WaitForClose();
            Assert.AreEqual(24, viewModel.Age);
        }

        [TestMethod]
        public void DefaultConstructor()
        {
            var navigation = new NavigationService();
            var navigationWithVM = navigation.GetWindow<PersonDetailsViewModel>();
            AssertExt.ThrowsException<ArgumentOutOfRangeException>(() => navigationWithVM.ShowWindow());
        }

        [TestMethod]
        public void ConstructorWithViewModelFactory()
        {
            var navigation = new NavigationService(new ViewModelFactory());
            var navigationWithVM = navigation.GetWindow<PersonDetailsViewModel>();
            AssertExt.ThrowsException<ArgumentOutOfRangeException>(() => navigationWithVM.ShowWindow());
        }

        [TestMethod]
        public void ConstructorWithViewModelFactoryAndWindowManager()
        {
            var vmFactory = new ViewModelFactory();
            var windowManager = new WindowManagerMock(vmFactory);
            var navigation = new NavigationService(vmFactory, windowManager);

            navigation.GetWindow<PersonDetailsViewModel>()
                .WithParam(vm => vm.Age, 20)
                .ShowDialog();

            var viewModel = PersonDetailsView.Window.DataContext as PersonDetailsViewModel;
            Assert.AreEqual(20, viewModel.Age);
        }

        [TestMethod]
        public void NullArguments()
        {
            _navigationService.GetWindow<PersonDetailsViewModel>()
                .SetupViewModel(null)
                .WithParam(vm => vm.Address, null)
                .DoAfterClose(null)
                .DoIfCancelled(null)
                .DoIfAccepted(null)
                .ShowDialog();
        }
    }
}
