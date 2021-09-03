using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Flurl.Http;
using MessageBox.Avalonia.ViewModels;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Montage.RebirthForYou.Tools.CLI.Utilities.Components;
using Montage.RebirthForYou.Tools.GUI.ModelViews;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.GUI.ModelViews.Test
{
    public class TestCardInfoDialogModel : ReactiveObject, ICardInfoDialogModel
    {
        private R4UCard _card;
        private string _cardName;
        private AsyncLazy<IImage> _imageSource;
        private string _cardTraits;
        private string _cardEffects;
        private bool _isJP;

        public IImage ImageSource => _imageSource.Value;// ().Result;//_imageSource.Value;

        public R4UCard Card
        {
            get => _card;
            set => this.RaiseAndSetIfChanged(ref _card, value);
        }

        public string CardName
        {
            get => _cardName;
            set => this.RaiseAndSetIfChanged(ref _cardName, value);
        }

        public string CardTraits
        {
            get => _cardTraits;
            set => this.RaiseAndSetIfChanged(ref _cardTraits, value);
        }

        public string CardEffects
        {
            get => __cardEffects.Value;
        }

        public bool IsJP
        {
            get => _isJP;
            set => this.RaiseAndSetIfChanged(ref _isJP, value);
        }

        private ObservableAsPropertyHelper<string> __cardEffects;

        public TestCardInfoDialogModel()
        {
            //Serilog.Log.Logger ??= Montage.RebirthForYou.Tools.CLI.Program.BootstrapLogging().CreateLogger();
            this.Card = CreateTestCard();
            this.CardName = $"{_card.Name.EN}\n({_card.Name.JP})";
            this.CardTraits = _card.Traits.Select(t => t.Default).ConcatAsString("\n");
            //this.CardEffects = _card.Effect.Select(t => t.EN).ConcatAsString("\n");
            this.IsJP = false;
            this.__cardEffects = this.WhenAny(
                    t => t.IsJP, 
                    t => _card.Effect.Select(eff => (t.Value) ? eff.JP : eff.EN).ConcatAsString("\n")
                    )
                .ToProperty(this, t => t.CardEffects)
                ;

                //.S
            this._imageSource = new AsyncLazy<IImage>(LoadImage);
        }

        private static R4UCard CreateTestCard()
        {
            var base64JSON = "ICAgIHsKICAgICAgIlNlcmlhbCI6ICJJTUMvMDAxVC0wMDEiLAogICAgICAiQWx0ZXJuYXRlcyI6IFsKICAgICAgICB7CiAgICAgICAgICAiU2VyaWFsIjogIklNQy8wMDFULTAwMVMiLAogICAgICAgICAgIlJhcml0eSI6ICJURFx1MDAyQiIsCiAgICAgICAgICAiSW1hZ2VzIjogWwogICAgICAgICAgICAiaHR0cHM6Ly9zMy1hcC1ub3J0aGVhc3QtMS5hbWF6b25hd3MuY29tL3JlYmlydGgtZnkuY29tL3dvcmRwcmVzcy93cC1jb250ZW50L2ltYWdlcy9jYXJkbGlzdC9JTVREL2ltYzAwMXQtMDAxcy5wbmciCiAgICAgICAgICBdCiAgICAgICAgfSwKICAgICAgICB7CiAgICAgICAgICAiU2VyaWFsIjogIklNQy8wMDFULTAwMVNOIiwKICAgICAgICAgICJSYXJpdHkiOiAiU05SIiwKICAgICAgICAgICJJbWFnZXMiOiBbCiAgICAgICAgICAgICJodHRwczovL3MzLWFwLW5vcnRoZWFzdC0xLmFtYXpvbmF3cy5jb20vcmViaXJ0aC1meS5jb20vd29yZHByZXNzL3dwLWNvbnRlbnQvaW1hZ2VzL2NhcmRsaXN0L0lNVEQvaW1jMDAxdC0wMDFzbi5wbmciCiAgICAgICAgICBdCiAgICAgICAgfQogICAgICBdLAogICAgICAiTmFtZSI6IHsKICAgICAgICAiRU4iOiAibmV3IGdlbmVyYXRpb25zLCBVenVraSIsCiAgICAgICAgIkpQIjogIm5ldyBnZW5lcmF0aW9ucyBcdTUzNkZcdTY3MDgiCiAgICAgIH0sCiAgICAgICJUcmFpdHMiOiBbCiAgICAgICAgewogICAgICAgICAgIkVOIjogIkN1dGUiLAogICAgICAgICAgIkpQIjogIlx1MzBBRFx1MzBFNVx1MzBGQ1x1MzBDOCIKICAgICAgICB9CiAgICAgIF0sCiAgICAgICJUeXBlIjogIkNoYXJhY3RlciIsCiAgICAgICJDb2xvciI6ICJCbHVlIiwKICAgICAgIlJhcml0eSI6ICJURCIsCiAgICAgICJDb3N0IjogMiwKICAgICAgIkFUSyI6IDIsCiAgICAgICJERUYiOiA1LAogICAgICAiRWZmZWN0IjogWwogICAgICAgIHsKICAgICAgICAgICJFTiI6ICJbQVVUT10oTWVtYmVyKVtSZUNvbWJvXTpXaGVuIHRoaXMgY2hhcmFjdGVyIHN1cHBvcnRzLCB0aGUgc3VwcG9ydGVkIGNoYXJhY3RlciBnZXRzIFx1MDAyQjIvXHUwMEIxMCB1bnRpbCBlbmQgb2YgdGhhdCBhdHRhY2suIElmIHlvdSBoYXZlIDMgb3IgbW9yZSBtZW1iZXJzLCB0aGVuIGl0IGdldHMgYW4gYWRkaXRpb25hbCBcdTAwMkIxL1x1MDBCMTAuXG4oW1JlQ29tYm9dOkFjdGl2ZSBpZiB5b3UgaGF2ZSBhIFJlYmlydGggc2V0KSIsCiAgICAgICAgICAiSlAiOiAiXHUzMDEwXHU4MUVBXHUzMDExXHUzMDEwXHUzMEUxXHUzMEYzXHUzMEQwXHUzMEZDXHUzMDExXHUzMDEwUmVcdTMwQjNcdTMwRjNcdTMwRENcdTMwMTFcdUZGMUFcdTMwNTNcdTMwNkVcdTMwQURcdTMwRTNcdTMwRTlcdTMwNENcdTMwQjVcdTMwRERcdTMwRkNcdTMwQzhcdTMwNTdcdTMwNUZcdTY2NDJcdTMwMDFcdTMwNTNcdTMwNkVcdTMwQTJcdTMwQkZcdTMwQzNcdTMwQUZcdTRFMkRcdTMwMDFcdTMwQjVcdTMwRERcdTMwRkNcdTMwQzhcdTMwNTVcdTMwOENcdTMwNUZcdTMwQURcdTMwRTNcdTMwRTlcdTMwOTJcdUZGMEJcdUZGMTIvXHUwMEIxXHVGRjEwXHUzMDAyXHUzMDQyXHUzMDZBXHUzMDVGXHUzMDZFXHUzMEUxXHUzMEYzXHUzMEQwXHUzMEZDXHUzMDRDXHVGRjEzXHU2NzlBXHU0RUU1XHU0RTBBXHUzMDQ0XHUzMDhCXHUzMDZBXHUzMDg5XHUzMDAxXHUzMDU1XHUzMDg5XHUzMDZCXHVGRjBCXHVGRjExL1x1MDBCMVx1RkYxMFx1MzAwMlxuXHVGRjA4XHUzMDEwUmVcdTMwQjNcdTMwRjNcdTMwRENcdTMwMTFcdUZGMUFcdTMwNDJcdTMwNkFcdTMwNUZcdTMwNkVSZVx1MzBEMFx1MzBGQ1x1MzBCOVx1MzA0Q1x1MzBCQlx1MzBDM1x1MzBDOFx1MzA1N1x1MzA2Nlx1MzA0Mlx1MzA4Q1x1MzA3MFx1NjcwOVx1NTJCOVx1RkYwOSIKICAgICAgICB9CiAgICAgIF0sCiAgICAgICJJbWFnZXMiOiBbCiAgICAgICAgImh0dHBzOi8vczMtYXAtbm9ydGhlYXN0LTEuYW1hem9uYXdzLmNvbS9yZWJpcnRoLWZ5LmNvbS93b3JkcHJlc3Mvd3AtY29udGVudC9pbWFnZXMvY2FyZGxpc3QvSU1URC9pbWMwMDF0LTAwMS5wbmciCiAgICAgIF0sCiAgICAgICJMYW5ndWFnZSI6ICJKYXBhbmVzZSIsCiAgICAgICJTZXQiOiB7CiAgICAgICAgIlJlbGVhc2VDb2RlIjogIklNQy8wMDFUIiwKICAgICAgICAiTmFtZSI6ICJUSEUgSURPTE1AU1RFUiBDSU5ERVJFTExBIEdJUkxTIFRoZWF0ZXIgVHJpYWwgRGVjayIKICAgICAgfQogICAgfQ==";
            return JsonSerializer.Deserialize<R4UCard>(Convert.FromBase64String(base64JSON));
        }

        private async Task<IImage> LoadImage()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            await using (var stream = assets.Open(new Uri("avares://deckbuilder4u/Assets/IMC_001T_001.jpg")))
                return new Bitmap(stream);
        }
    }
}
