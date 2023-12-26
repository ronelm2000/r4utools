using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using DynamicData.Binding;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Montage.RebirthForYou.Tools.CLI.Utilities.Components;
using Montage.RebirthForYou.Tools.GUI.Dialogs;
using Montage.RebirthForYou.Tools.GUI.ModelViews;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.GUI.Models
{
    public class CardEntryModel : ReactiveObject
    {
        public static IImage LoadingImage = CardEntryModel.ScaleToWidth(AssetLoader.Open(new Uri("avares://deckbuilder4u/Assets/Card/Loading.jpg")), 120);
        public static IImage NotFoundImage = CardEntryModel.ScaleToWidth(AssetLoader.Open(new Uri("avares://deckbuilder4u/Assets/Card/404.jpg")), 120);

        //private IImage imageSource;
        private string text;
        private bool _isLoading;
        private TaskCompletionSource<bool> _loadTask = new();


        public IObservable<bool> IsLoadingObserver { get; init; }
        public Task<IImage?> ImageSource { get; private set; }
        public Task<IImage?> FullImageSource { get; private set; }


        public string CardName {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
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

        public IEnumerable<string> NeoStandardCodes => Card.NeoStandardCodes;

        public CardEntryModel()
        {
        }

        public CardEntryModel(R4UCard card)
        {
            Card = card;
            CardName = $"{card.Name?.AsNonEmptyString() ?? ""}\n({card.Serial})";
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
                    Width = 750,
                    SizeToContent = Avalonia.Controls.SizeToContent.Height
                }.ShowDialog(model.Parent);
            });

            IsLoading = false;

            IsLoadingObserver = this.WhenValueChanged(d => d.IsLoading);
            IsLoadingObserver.Subscribe(__isLoading =>
            {
                if (__isLoading && !_loadTask.Task.IsCompleted)
                {
                    Log.Information("Loading Image: {card}", card.Serial);
                    _loadTask.SetResult(true);
                }
            });

            ImageSource = LoadImage();
            FullImageSource = LoadLargeImage();
        }

        private async Task<IImage?> LoadImage() => await LoadImage(s => ScaleToWidth(s, 120));
        private async Task<IImage?> LoadLargeImage() => await LoadImage(s => new Bitmap(s));
        private async Task<IImage?> LoadImage(Func<Stream, IImage?> bitmapAlgorithm)
        {
            try
            {
                await _loadTask.Task;

                if (!Card.IsCached)
                    await new CacheVerb().AddCachedImageAsync(Card);

                await using (var imageStream = await Card.GetImageStreamAsync())
                    return bitmapAlgorithm(imageStream);
            }
            catch (Exception)
            {
                return NotFoundImage;
            }
        }

        private static Bitmap ScaleToWidth(Stream stream, int width)
        {
            // return Bitmap.DecodeToWidth(imageStream, 100); // This is currently broken due to an issue in SkiaSharp 2.88.6
            Bitmap bitmap = new(stream);
            var newHeight = width * bitmap.PixelSize.Height / bitmap.PixelSize.Width;
            return bitmap.CreateScaledBitmap(new Avalonia.PixelSize(120, newHeight));
        }
    }

}
