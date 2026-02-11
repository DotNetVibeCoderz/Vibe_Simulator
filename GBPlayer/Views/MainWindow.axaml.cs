using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GBPlayer.ViewModels;
using GBPlayer.Core;

namespace GBPlayer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (DataContext is MainWindowViewModel vm)
            {
                var gbKey = vm.MapKey(e.Key);
                if (gbKey.HasValue)
                {
                    vm.HandleKeyDown(gbKey.Value);
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
             if (DataContext is MainWindowViewModel vm)
            {
                var gbKey = vm.MapKey(e.Key);
                if (gbKey.HasValue)
                {
                    vm.HandleKeyUp(gbKey.Value);
                }
            }
        }
    }
}
