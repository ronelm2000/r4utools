using Avalonia.Media;
using Avalonia.Media.Imaging;
using MessageBox.Avalonia.ViewModels;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Montage.RebirthForYou.Tools.CLI.Utilities.Components;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.GUI.ModelViews
{
    public class CardInfoDialogModel : ReactiveObject, ICardInfoDialogModel
    {
        private R4UCard _card;
        private bool _isJP;
        private readonly AsyncLazy<IImage> _imageSource;
        private readonly ObservableAsPropertyHelper<string> __cardEffects;

        public IImage ImageSource => _imageSource.DesignValue;
        public R4UCard Card
        {
            get => _card;
            set => this.RaiseAndSetIfChanged(ref _card, value);
        }

        public string CardName => Card.Name.Default;
        public string CardTraits { get; }

        public string CardEffects
        {
            get => __cardEffects.Value;
        }

        public bool IsJP
        {
            get => _isJP;
            set => this.RaiseAndSetIfChanged(ref _isJP, value);
        }

        public CardInfoDialogModel(R4UCard card)
        {
            this._card = card;
            this.CardTraits = Card.Traits
                .Select(t => t.Default)
                .ConcatAsString("\n");
            this.IsJP = false; 
            this.__cardEffects = this.WhenAny(
                    t => t.IsJP,
                    t => _card.Effect.Select(eff => (t.Value) ? eff.JP : eff.EN).ConcatAsString("\n")
                    )
                .ToProperty(this, t => t.CardEffects)
                ;
            _imageSource = new AsyncLazy<IImage>(async () => await LoadImage());
        }

        private async Task<IImage> LoadImage()
        {
            if (!Card.IsCached)
                await new CacheVerb().AddCachedImageAsync(Card);
            await using (var imageStream = await Card.GetImageStreamAsync())
                return new Bitmap(imageStream);
        }
    }
}
