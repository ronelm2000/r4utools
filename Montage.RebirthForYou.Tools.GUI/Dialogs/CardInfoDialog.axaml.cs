using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Montage.RebirthForYou.Tools.GUI.Dialogs
{
    public partial class CardInfoDialog : Window
    {
        public CardInfoDialog()
        {
            InitializeComponent();
#if DEBUG
 //           this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
