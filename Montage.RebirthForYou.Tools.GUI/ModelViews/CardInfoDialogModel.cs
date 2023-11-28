using Avalonia.Media;
using Avalonia.Media.Imaging;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Montage.RebirthForYou.Tools.CLI.Utilities.Components;
using Montage.RebirthForYou.Tools.GUI.ModelViews.Interfaces;
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
        private readonly ObservableAsPropertyHelper<string> __cardFlavor;
        private readonly ObservableAsPropertyHelper<FontStyle> __cardFlavorFontStyle;

        public IImage ImageSource => _imageSource.DesignValue;
        public R4UCard Card
        {
            get => _card;
            set => this.RaiseAndSetIfChanged(ref _card, value);
        }

        public string CardName { get; }
        public string CardTraits { get; }

        public string CardEffects
        {
            get => __cardEffects.Value;
        }

        public string CardFlavor {
            get => __cardFlavor.Value;
        }
        public FontStyle CardFlavorFontStyle
        {
            get => __cardFlavorFontStyle.Value;
        }

        public bool IsJP
        {
            get => _isJP;
            set => this.RaiseAndSetIfChanged(ref _isJP, value);
        }

        public bool HasFlavor { get; }


        public CardInfoDialogModel(R4UCard card)
        {
            this._card = card;
            this.CardName = $"{_card.Name.EN}\n({_card.Name.JP})";
            this.CardTraits = Card.Traits
                .Select(t => t.Default)
                .ConcatAsString("\n");
            this.IsJP = false;
            this.HasFlavor = !string.IsNullOrWhiteSpace(_card.Flavor?.AsNonEmptyString());
            this.__cardEffects = this.WhenAny(
                    t => t.IsJP,
                    t => _card.Effect.Select(eff => (t.Value) ? eff.JP : eff.EN).ConcatAsString("\n")
                    )
                .ToProperty(this, t => t.CardEffects)
                ;
            this.__cardFlavor = this.WhenAny(
                    t => t.IsJP,
                    t => ((t.Value) ? _card.Flavor?.JP : _card.Flavor?.EN)
                    )
                .ToProperty(this, t => t.CardFlavor)
                ;
            this.__cardFlavorFontStyle = this.WhenAny(
                    t => t.IsJP,
                    t => (t.Value) ? FontStyle.Normal : FontStyle.Italic
                    )
                .ToProperty(this, t => t.CardFlavorFontStyle);

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
