using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.WPF.Navigation
{
    public interface IWindowManager
    {
        Dictionary<string, Type> KnownWindows { get; }

        void ShowWindow<TViewModel>();

        bool? ShowDialog<TViewModel>();

        void ShowWindow(object viewModel);

        bool? ShowDialog(object viewModel);
    }
}
