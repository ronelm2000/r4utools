using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Impls.Exporters.TTS;
using Montage.RebirthForYou.Tools.GUI.ModelViews;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.GUI
{
    public class MainWindow : Window
    {
        private readonly TextBox _searchBarTextBox;
        private readonly ItemsRepeater _decklistRepeater;
        private readonly ScrollViewer _deckScroller;
        private readonly ItemsRepeater _databaseRepeater;
        private readonly ScrollViewer _databaseScroller;
        private readonly Func<MainWindowViewModel> _dataContext;
        private readonly IContainer _ioc;
        private readonly SaveFileDialog _saveFileDialog;
        private readonly OpenFileDialog _openFileDialog;

        public ILogger Log { get; }

        public MainWindow(IContainer ioc) : this()
        {
            // Do stuff here.
            //var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("title", "orem ipsum dolor sit amet, consectetur adipiscing elit, sed...");
            // messageBoxStandardWindow.Show();
            _ioc = ioc;
            Log = Serilog.Log.ForContext<MainWindow>();
            DataContext = new MainWindowViewModel(ioc);
            Dispatcher.UIThread.InvokeAsync(MainWindow_Initialized);
        }

        private async Task MainWindow_Initialized()//object sender, EventArgs e)
        {
            await Task.CompletedTask;
            _searchBarTextBox.IsEnabled = false;
            await _dataContext().InitializeDatabase();
            _searchBarTextBox.IsEnabled = true;
        }

        /// <summary>
        /// This method is needed to make the WYSIWYG editor work.
        /// The actual execution method uses the one with IOC.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _searchBarTextBox = this.FindControl<TextBox>("searchBarTextBox");

            _decklistRepeater = this.FindControl<ItemsRepeater>("decklistRepeater");
            _deckScroller = this.FindControl<ScrollViewer>("deckScroller");

            _decklistRepeater.PointerPressed += RepeaterClick;

            _deckScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            _deckScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            _decklistRepeater.Layout = new UniformGridLayout
            {
                Orientation = Orientation.Horizontal,
                //MinItemHeight = 100,
                //MinColumnSpacing = -25,
                MaximumRowsOrColumns = 10
            };

            _databaseRepeater = this.FindControl<ItemsRepeater>("databaseRepeater");
            _databaseScroller = this.FindControl<ScrollViewer>("databaseScroller");

            _databaseScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            _databaseScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            _databaseRepeater.Layout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 4
            };

            _saveFileDialog = new SaveFileDialog
            {
                DefaultExtension = "r4udek",
                Title = "Save R4U Deck..."
            };
            _saveFileDialog.Filters.Add(new FileDialogFilter { Extensions = new[] { "r4udek" }.ToList(), Name = "Rebirth For You (R4U) Deck" });

            _openFileDialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Title = "Load R4U Deck..."
            };
            _openFileDialog.Filters.Add(new FileDialogFilter { Extensions = new[] { "r4udek" }.ToList(), Name = "Rebirth For You (R4U) Deck" });

            DataContext = new MainWindowViewModel();
            _dataContext = () => DataContext as MainWindowViewModel;

            _searchBarTextBox.GetObservable(TextBox.TextProperty).Subscribe(SearchBarText_OnTextChanged); //text => { /* Will be called with Text each time it's changed */ });

#if DEBUG
            //this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void RepeaterClick(object sender, PointerPressedEventArgs e)
        {
            var item = (e.Source as StyledElement)?.DataContext as CardEntry;
            _dataContext().DeckResults = _dataContext().DeckResults;
        }

        private Dictionary<Border, IBrush> oldBrush = new Dictionary<Border, IBrush>();

        public void Item_OnPointerEnter(object sender, PointerEventArgs e)
        {
            var border = e.Source as Border;
            var item = border?.DataContext as CardEntry;
            var gradientBrush = new LinearGradientBrush
            {
                SpreadMethod = GradientSpreadMethod.Pad,
                StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                Opacity = 1d
            };
            var stops = gradientBrush.GradientStops;// new GradientStops();
            stops.Add(new GradientStop(Color.FromUInt32(0x7391C8FF), 0)); // #7391C8
            stops.Add(new GradientStop(Color.FromUInt32(0x52688FFF), 1)); // #52688F
            oldBrush[border] = border.Background;
            border.Background = gradientBrush;

            _dataContext().SelectedDatabaseCardEntry = item;
        }

        public void Item_OnPointerLeave(object sender, PointerEventArgs e)
        {
            var border = e.Source as Border;
            border.Background = oldBrush?[border] ?? null;
        }

        public void Item_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _dataContext().AddDeckCard(_dataContext().SelectedDatabaseCardEntry);
        }

        public void DeckItem_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            Log.Information("Trying to remove...");
            var border = e.Source as Image;
            var cardEntry = border?.DataContext as CardEntry;
            Log.Information("Got: {serial}", cardEntry?.Card.Serial);
            _dataContext().RemoveDeckCard(cardEntry);
        }
        private async void SearchBarText_OnTextChanged(string newText)
        {
            await _dataContext().ApplyFilter((card) => (card.Name.EN ?? "").Contains(newText) || (card.Name.JP ?? "").Contains(newText));
        }

        public async void SaveDeckMenuItem_Pressed(object sender, PointerPressedEventArgs args)
        {
            var saveFilePath = await _saveFileDialog.ShowAsync(this);
            if (!String.IsNullOrWhiteSpace(saveFilePath))
            {
                var result = await _dataContext().SaveDeck(saveFilePath);
                await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(result.Title, result.Details).ShowDialog(this);
            }
        }

        public async void LoadDeckMenuItem_Pressed(object sender, PointerPressedEventArgs args)
        {
            var loadFilePath = await _openFileDialog.ShowAsync(this);
            if (loadFilePath?.Length > 0)
            {
                var result = await _dataContext().LoadDeck(loadFilePath?[0]);
                await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(result.Title, result.Details).ShowDialog(this);
            }
        }

        public async void ExportTTSMenuItem_Pressed(object sender, PointerPressedEventArgs args)
        {
            var result = await _dataContext().Export<TTSDeckExporter>();
            await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(result.Title, result.Details).ShowDialog(this);
        }
        public int DeckItemWidth => (int)(_deckScroller.Width / 10f);
    }
}
