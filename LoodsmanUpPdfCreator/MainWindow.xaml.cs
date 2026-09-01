using Ascon.Plm.Loodsman.PluginSDK;
using LoodsmanUpPdfCreator.Infrastructure.Loodsman;
using LoodsmanUpPdfCreator.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LoodsmanUpPdfCreator
{

    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        //public MainWindow(INetPluginCall client, IReadOnlyList<string> selectedObjectIds)
        //{
        //    InitializeComponent();
        //    _viewModel = new MainWindowViewModel(client, selectedObjectIds);
        //    _treeRefresh = new ClientTreeRefreshNotifier((IntPtr)client.PluginCall.ClientHandle);
        //}
        public MainWindow(INetPluginCall client, ObservableCollection<string> selectedObjectIds)
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel(client, selectedObjectIds);

        }
    }
}
