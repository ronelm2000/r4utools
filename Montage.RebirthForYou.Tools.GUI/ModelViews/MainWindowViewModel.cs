using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using DynamicData;
using DynamicData.Binding;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Entities.Exceptions;
using Montage.RebirthForYou.Tools.CLI.Impls.Exporters;
using Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Deck;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Octokit;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.GUI.ModelViews
{
    public class MainWindowViewModel : ReactiveObject
    {
        #region Private Fields
        private Dictionary<string,CardEntry> _database = new Dictionary<string,CardEntry>();
        private ObservableCollection<CardEntry> databaseResults = new ObservableCollection<CardEntry>();
        private ObservableCollection<CardEntry> deckResults = new ObservableCollection<CardEntry>(new CardEntry[] { });
        private IContainer ioc;
        private Predicate<R4UCard> filter = (card) => true;
        private string deckName;
        private string deckRemarks;
        private string isSaved;
        #endregion

        #region Observers
        private readonly IObservable<string> _deckNameObserver;
        private readonly IObservable<string> _deckRemarkObserver;
        public readonly IObservable<string> SavedObserver;

        #endregion

        public ObservableCollection<Bitmap> Items { get; set; }
        public ObservableCollection<CardEntry> DatabaseResults
        {
            get => databaseResults;
            set => this.RaiseAndSetIfChanged(ref databaseResults, value);
        }
        public ObservableCollection<CardEntry> DeckResults
        {
            get => deckResults;
            set => this.RaiseAndSetIfChanged(ref deckResults, value);
        }
        public string DeckName
        {
            get => deckName;
            set => this.RaiseAndSetIfChanged(ref deckName, value);
        }
        public string DeckRemarks
        {
            get => deckRemarks;
            set => this.RaiseAndSetIfChanged(ref deckRemarks, value);
        }
        public string Saved
        {
            get => isSaved;
            set => this.RaiseAndSetIfChanged(ref isSaved, value);
        }

        public CardEntry SelectedDatabaseCardEntry { get; internal set; }
        public Predicate<R4UCard> Filter { 
            get => filter; 
            set => this.RaiseAndSetIfChanged(ref filter, value); 
        }
        public MainWindowViewModel()
        {
        }
        public MainWindowViewModel(IContainer ioc)
        {
            this.ioc = ioc;

            _deckNameObserver = this.WhenValueChanged(x => x.DeckName);
            _deckRemarkObserver = this.WhenValueChanged(x => x.DeckRemarks);
            SavedObserver = this.WhenValueChanged(dc => dc.Saved);
            _deckNameObserver.Subscribe(s => this.Saved = "*");
            _deckRemarkObserver.Subscribe(s => this.Saved = "*");

            //    dataContext.WhenValueChanged(x => x.DeckRemarks).Subscribe(s => _dataContext().Saved = "*");
            //    dataContext.WhenValueChanged(dc => dc.Saved).Subscribe(ChangeWindowTitle);
        }

        internal async Task InitializeDatabase()
        {
            using (var db = ioc.GetInstance<CardDatabaseContext>())
            using (_ = this.SuppressChangeNotifications())
            {
                await db.Database.MigrateAsync();
                await new UpdateVerb().Run(ioc);
                await foreach (var card in GetCardDatabase(db))
                {
                    this._database.Add(card.Card.Serial, card);
                    this.DatabaseResults.Add(card);
                }
            }
        }

        internal void RemoveDeckCard(CardEntry cardEntry)
        {
            DeckResults.Remove(cardEntry);
            SortDeck();
        }

        internal void AddDeckCard(CardEntry cardEntry)
        {
            if (IsDeckConstructionValid(cardEntry))
            {
                DeckResults.Add(cardEntry);
                SortDeck();
            }
        }

        private bool IsDeckConstructionValid(CardEntry cardEntry)
        {
            return DeckResults.Where(cr => cr.Card.Name.JP == cardEntry.Card.Name.JP).Count() < 4
                && DeckResults.Count < 50
                && ((cardEntry.Card.Type == CardType.Rebirth) ? DeckResults.Count(cr => cr.Card.Type == CardType.Rebirth) < 8 : true)
                && ((cardEntry.Card.Type == CardType.Partner) ? DeckResults.Count(cr => cr.Card.Type == CardType.Partner) < 3 : true)
                ;
        }

        private void SortDeck()
        {
            var newSort = deckResults.OrderBy(x => x.Card.Type) //
                .ThenBy(x => x.Card.Color) //
                .ThenBy(x => x.Card.Cost) //
                .ThenBy(x => x.Card.Serial) //
                .ToList();
            deckResults.Clear();
            deckResults.AddRange(newSort);
        }

        public async Task ApplyFilter(Predicate<R4UCard> filter)
        {
            if (ioc == null) return;
            await Task.CompletedTask;
            databaseResults.Clear();
            databaseResults.AddRange(_database.Values.Where(ce => filter(ce.Card)).ToList());
        }

        public IAsyncEnumerable<CardEntry> GetCardDatabase(CardDatabaseContext db)
        {
            if (ioc == null) return new CardEntry[] { }.ToAsyncEnumerable();
            return db.R4UCards.SelectAwait<R4UCard, CardEntry>(c => CardEntry.From(c));
        }

        internal async Task<(string Title, string Details)> SaveDeck(string saveFilePath)
        {
            var exporter = ioc.GetInstance<LocalDeckJSONExporter>();
            try
            {
                await exporter.Export(GenerateDeck(deckResults, deckName, deckRemarks), null, Fluent.IO.Path.Get(saveFilePath));
                Saved = "";
                var nonNulldeckName = string.IsNullOrWhiteSpace(deckName) ? "An unnamed deck" : deckName;
                return ("Success", $"{deckName} was saved successfully!");
            }
            catch (Exception e)
            {
                Log.Error("Deck failed to be exported.", e);
                return ("Failed", "You aren't supposed to see this message, please contact me on Discord / GitHub to file a bug.");
            }
        }

        internal async Task<(string Title, string Details)> LoadDeck(string loadFilePath)
        {
            try
            {
                var exporter = ioc.GetInstance<LocalDeckJSONParser>();
                var deck = await exporter.Parse(loadFilePath);
                var deckName = string.IsNullOrWhiteSpace(deck.Name) ? "An unnamed deck": deck.Name;
                ApplyDeck(deck);
                return ("Success", $"{deckName} was loaded successfully!");
            }
            catch (DeckParsingException e)
            {
                return ("Failed", e.Message);
            }
        }

        private void ApplyDeck(R4UDeck deck)
        {
            DeckResults.Clear();
            var range = deck.Ratios.Keys
                .SelectMany(c => Enumerable.Range(0, deck.Ratios[c]).Select(i => c.Serial))
                .Select(serial => _database[serial]);
            DeckResults.AddRange(range);
            DeckName = deck.Name;
            DeckRemarks = deck.Remarks;
            Saved = "";
        }

        private static R4UDeck GenerateDeck(ObservableCollection<CardEntry> deckResults, string deckName, string deckRemarks)
        {
            var result = new R4UDeck();
            result.Name = deckName;
            result.Remarks = deckRemarks;
            result.Ratios = deckResults.Select(ce => ce.Card).GroupBy(card => card).ToDictionary(g => g.Key, g => g.Count());
            return result;
        }

        internal async Task<(string Title, string Details)> Export<T>() where T : IDeckExporter
        {
            var newDeckName = (string.IsNullOrWhiteSpace(deckName)) ? "[Placeholder for a Deck Name]" : deckName; 
            var deck = GenerateDeck(deckResults, newDeckName, deckRemarks);
            var exporter = ioc.GetInstance<T>();
            await exporter.Export(deck, new MockInfo());
            return ("Success", $"{deck.Name} was exported successfully!");
        }
    }

    internal class MockInfo : IExportInfo
    {
        public string Source => null;
        public string Destination => "./Export/";
        public string Parser => "";
        public string Exporter => "";
        public string OutCommand => "";
        public IEnumerable<string> Flags => new[] { "upscaling" };
        public bool NonInteractive => true;
    }

    public class CardEntry : ReactiveObject
    {
        private IImage imageSource;
        private string text;

        public IImage ImageSource {
            get => imageSource;
            set => this.RaiseAndSetIfChanged(ref imageSource, value);
        }
        public string Text {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }

        public string Name => Card.Name.AsNonEmptyString();
        public string ATKDEF => $"{Card.ATK}/{Card.DEF}";
        public string Traits => $"{Card.Traits.Select(t => t.AsNonEmptyString()).ConcatAsString("\n")}";
        public string Effects => Card.Effect.Select(mls => mls.AsNonEmptyString()).ConcatAsString("\n");
        public string Flavor => Card.Flavor?.AsNonEmptyString();
        public R4UCard Card { get; set; }

        public CardEntry()
        {
        }

        public static async ValueTask<CardEntry> From(R4UCard card)
        {
            if (!card.IsCached)
                await new CacheVerb().AddCachedImageAsync(card);
            return new CardEntry()
            {
                Card = card,
                Text = card.Name?.AsNonEmptyString() ?? "",
                ImageSource = new Bitmap(await card.GetImageStreamAsync())
            };
        }
    }

}
