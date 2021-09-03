using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Montage.RebirthForYou.Tools.CLI.Utilities.Components;
using Montage.RebirthForYou.Tools.GUI.Dialogs;
using Montage.RebirthForYou.Tools.GUI.ModelViews;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.GUI.Models
{
    public class CardEntryModel : ReactiveObject
    {
        //private IImage imageSource;
        private string text;

        private readonly AsyncLazy<IImage> _imageSource;
        private readonly AsyncLazy<IImage> _imageSourceLarge;

        public IImage ImageSource => _imageSource.Value;
        public IImage FullImageSource => _imageSourceLarge.Value;

        public string Text {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }

        public string Name => Card.Name.AsNonEmptyString();
        public string ATKDEF => $"{Card.ATK}/{Card.DEF}";
        public string Traits => $"{Card.Traits.Select(t => t.AsNonEmptyString()).ConcatAsString("\n")}";
        public string Effects => Card.Effect?.Select(mls => mls.AsNonEmptyString()).ConcatAsString("\n");
        public string Flavor => Card.Flavor?.AsNonEmptyString();
        public R4UCard Card { get; set; }

        public ReactiveCommand<Unit, Unit> DuplicateCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchCombosCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowCardInfoCommand { get; }

        public CardEntryModel()
        {
        }
        
        public CardEntryModel(R4UCard card)
        {
            Card = card;
            Text = $"{card.Name?.AsNonEmptyString() ?? ""}\n({card.Serial})";
            _imageSource = new AsyncLazy<IImage>(async () => await LoadImage());
            _imageSourceLarge = new AsyncLazy<IImage>(async () => await LoadLargeImage());
        }

        public CardEntryModel(MainWindowViewModel model, R4UCard card) : this(card)
        {
            DuplicateCommand = ReactiveCommand.Create(() => model.AddDeckCard(this));// ExportWithResult<LocalDeckImageExporter>());
            SearchCombosCommand = ReactiveCommand.CreateFromTask(async () => await model.SearchCombos(this));
            ShowCardInfoCommand = ReactiveCommand.Create(() =>
            {
                new CardInfoDialog
                {
                    DataContext = new CardInfoDialogModel(card),
                    Width = 700,
                    SizeToContent = Avalonia.Controls.SizeToContent.Height
                }.ShowDialog(model.Parent);
            });
        }

        private async Task<IImage> LoadImage()
        {
            if (!Card.IsCached)
                await new CacheVerb().AddCachedImageAsync(Card);
            await using (var imageStream = await Card.GetImageStreamAsync())
                return Bitmap.DecodeToWidth(imageStream, 100);
        }

        private async Task<IImage> LoadLargeImage()
        {
            if (!Card.IsCached)
                await new CacheVerb().AddCachedImageAsync(Card);
            await using (var imageStream = await Card.GetImageStreamAsync())
                return new Bitmap(imageStream);
        }

        public async Task<IImage> LoadImageAsync() => await _imageSource;

        internal void SubscribeOnImageLoaded(Action action)
        {
            _imageSource.OnChanged = () => Dispatcher.UIThread.InvokeAsync(action);
        }
        internal void SubscribeOnImageLoaded(Task action)
        {
            _imageSource.OnChanged = () => action;
        }
    }

}
