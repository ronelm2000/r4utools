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
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Octokit;
using ReactiveUI;
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
        private List<CardEntry> _database = new List<CardEntry>();
        private ObservableCollection<CardEntry> databaseResults = new ObservableCollection<CardEntry>();
        private ObservableCollection<CardEntry> deckResults = new ObservableCollection<CardEntry>(new CardEntry[] { });
        private IContainer ioc;
        private Predicate<R4UCard> filter = (card) => true;
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
        }

        internal async Task InitializeDatabase()
        {
            using (var db = ioc.GetInstance<CardDatabaseContext>())
            {
                await db.Database.MigrateAsync();
                await foreach (var card in GetCardDatabase(db))
                {
                    this._database.Add(card);
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
            return DeckResults.Where(cr => cr.Card.Name.JP == cardEntry.Card.Name.JP).Count() < 4;
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
            databaseResults.AddRange(_database.Where(ce => filter(ce.Card)).ToList());
        }

        public IAsyncEnumerable<CardEntry> GetCardDatabase(CardDatabaseContext db)
        {
            if (ioc == null) return new CardEntry[] { }.ToAsyncEnumerable();
            return db.R4UCards.SelectAwait<R4UCard, CardEntry>(c => CardEntry.From(c));
        }
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

        public string ATKDEF => $"{Card.ATK}/{Card.DEF}";
        public string Traits => $"{Card.Traits.Select(t => t.AsNonEmptyString()).ConcatAsString("\n")}";
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
