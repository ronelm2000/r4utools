using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Lamar;
using Lamar.Scanning.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Impls.Exporters.TTS;
using Montage.RebirthForYou.Tools.GUI.Models;
using Montage.RebirthForYou.Tools.GUI.ModelViews;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.GUI
{
    public partial class MainWindow : Window
    {
        private readonly TextBox _searchBarTextBox;
        private readonly ItemsRepeater _decklistRepeater;
        private readonly ScrollViewer _deckScroller;
//        private readonly ItemsRepeater _databaseRepeater;
        private readonly ListBox _databaseScroller;
        private readonly Func<MainWindowViewModel> _dataContext;
        private readonly IContainer _ioc;
        private Dictionary<Border, IBrush> oldBrush = new Dictionary<Border, IBrush>();
        private string _originalTitle;

        #region Public Properties
        public ILogger Log { get; }
        public TextBox SearchBarTextbox => _searchBarTextBox;
        public ProgressBar LoadingProgressBar { get; }
        public Border LoadingBox { get; }
        public TextBlock LoadingTextbox { get; }
        #endregion

        public MainWindow(IContainer ioc) : this()
        {
            _ioc = ioc;
            Log = Serilog.Log.ForContext<MainWindow>();
            DataContext = _ioc.GetService<MainWindowViewModel>();
            var dataContext = _dataContext();
            Dispatcher.UIThread.InvokeAsync(MainWindow_Initialized);
            //Task.Run(MainWindow_Initialized);
            dataContext.DeckResults.CollectionChanged += (s, e) => _dataContext().Saved = "*";
            dataContext.SavedObserver.Subscribe(ChangeWindowTitle);
        }

        private void MainWindow_Initialized()//object sender, EventArgs e)
        {
            _searchBarTextBox.IsEnabled = false;
            _dataContext().Saved = "";
            var context = _dataContext();
            context.Parent = this;
            Task.Run(async() =>
            {
                try
                {
                    await context.InitializeSettings();
                    await context.InitializeDatabase();
                } catch (Exception e)
                {
                    Log.Error("Error Occurred while Loading: {exception}", e);
                } finally
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LoadingBox.IsVisible = false;
                        SearchBarTextbox.IsEnabled = true;
                    });
                }
            });
            //_searchBarTextBox.IsEnabled = true;
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

            _deckScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            _deckScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            _decklistRepeater.Layout = new UniformGridLayout
            {
                Orientation = Orientation.Horizontal,
                //MinItemHeight = 100,
                //MinColumnSpacing = -25,
                MaximumRowsOrColumns = 10
            };

//            _databaseRepeater = this.FindControl<ItemsRepeater>("databaseRepeater");
            _databaseScroller = this.FindControl<ListBox>("databaseScroller");
//            _databaseScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
//            _databaseScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
//            _databaseScroller.ScrollChanged += _databaseScroller_ScrollChanged;
            /*
            _databaseRepeater.Layout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 4
            };
            */

            var dataContext = new MainWindowViewModel();
            DataContext = dataContext;
            _dataContext = () => DataContext as MainWindowViewModel;
            
            _searchBarTextBox.GetObservable(TextBox.TextProperty).Subscribe(SearchBarText_OnTextChanged); //text => { /* Will be called with Text each time it's changed */ });

            _originalTitle = this.Title;

            LoadingProgressBar = this.FindControl<ProgressBar>("loadingProgressBar");
            LoadingBox = this.FindControl<Border>("loadingBox");
            LoadingTextbox = this.FindControl<TextBlock>("loadingTextbox");

            Closing += MainWindow_Closing;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void _databaseScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _databaseScroller.InvalidateVisual();
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = _dataContext().Saved == "*";
            if (_dataContext().Saved == "*")
            {
                var msgBoxParams = new MessageBoxStandardParams
                {
                    ContentTitle = "Save", 
                    ContentMessage = "You still have unsaved changes. Are you sure you want to exit?", 
                    ButtonDefinitions = ButtonEnum.YesNo,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Width = 300,
                    SizeToContent = SizeToContent.Height
                };
                var window = MessageBoxManager.GetMessageBoxStandard(msgBoxParams);
                var dialogResult = await window.ShowAsPopupAsync(this);
                if (dialogResult == ButtonResult.Yes)
                {
                    _dataContext().Saved = "";
                    this.Close();
                }
            }
        }

        private void ChangeWindowTitle(string saved)
        {
            this.Title = _originalTitle + saved;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Item_OnSetVisible(object sender, VisualTreeAttachmentEventArgs e)
        {
            var border = sender as Border;
            var item = border?.DataContext as CardEntryModel;
            item.IsLoading = true;
            Log.Information("Loading in Database: {card}", item.Card.Serial);
        }

        public void Item_OnPointerEnter(object sender, PointerEventArgs e)
        {
            var border = e.Source as Border;
            var item = border?.DataContext as CardEntryModel;
            var gradientBrush = new LinearGradientBrush
            {
                SpreadMethod = GradientSpreadMethod.Pad,
                StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                Opacity = 1d
            };
            var stops = gradientBrush.GradientStops;// new GradientStops();
            stops.Add(new GradientStop(Avalonia.Media.Color.FromUInt32(0x7391C8FF), 0)); // #7391C8
            stops.Add(new GradientStop(Avalonia.Media.Color.FromUInt32(0x52688FFF), 1)); // #52688F
            oldBrush[border] = border.Background;
            border.Background = gradientBrush;

            item.IsLoading = true;

            _dataContext().SelectedDatabaseCardEntry = item;
        }

        public void Item_OnPointerLeave(object sender, PointerEventArgs e)
        {
            var border = sender as Border;
            border.Background = oldBrush?[border] ?? null;
        }

        public void Item_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _dataContext().AddDeckCard(_dataContext().SelectedDatabaseCardEntry);
        }

        public void DeckItem_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left && e.Source is Image border && e.KeyModifiers == KeyModifiers.None)
            {
                Log.Information("Trying to remove...");
                var cardEntry = border?.DataContext as CardEntryModel;
                Log.Information("Got: {serial}", cardEntry?.Card.Serial);
                _dataContext().RemoveDeckCard(cardEntry);
            }
        }
        private async void SearchBarText_OnTextChanged(string newText)
        {
            var query = CardQuery.Parse(newText) ?? new CardQuery
            {
                Or = new CardQuery[]
                {
                    new CardQuery { Name = newText },
                    new CardQuery { Serial = newText }
                }
            };
            await _dataContext().ApplyFilter(query.ToQuery(), TimeSpan.FromSeconds(1));
        }

        private void ToggleFormat(string formatCode)
        {
            _dataContext().DeckFormat = DeckFormats.Parse(formatCode);
        }

        private void DatabaseImage_ResourcesChanged(object sender, ResourcesChangedEventArgs e)
        {
            /*
            Log.Information("Database Image Changed");
            var image = sender as Image;
            image.InvalidateVisual();
            Dispatcher.UIThread.Post(() =>
            {
                //                if (image.Source is Bitmap bitmap)
                image.InvalidateVisual();


                image.IsVisible = false;
                image.InvalidateVisual();
                image.IsVisible = true;
            });
            */
            // image.InvalidateVisual();
        }

        internal async Task RefreshView()
        {
        }

        public int DeckItemWidth => (int)(_deckScroller.Width / 10f);
    }
}
