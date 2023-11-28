using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Montage.RebirthForYou.Tools.GUI.ModelViews;

namespace Montage.RebirthForYou.Tools.GUI.Dialogs
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            this.InitializeComponent();
            DataContext = new AboutDialogModel();
#if DEBUG
//            this.AttachDevTools();
#endif
        }

        public AboutDialog (IContainer ioc) : this()
        {
            DataContext = ioc.GetService<AboutDialogModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
