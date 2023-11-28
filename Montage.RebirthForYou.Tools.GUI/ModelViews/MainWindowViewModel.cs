using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using DynamicData.Binding;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Entities.Exceptions;
using Montage.RebirthForYou.Tools.CLI.Impls.Exporters;
using Montage.RebirthForYou.Tools.CLI.Impls.Exporters.Deck;
using Montage.RebirthForYou.Tools.CLI.Impls.Exporters.TTS;
using Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Deck;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Montage.RebirthForYou.Tools.GUI.Dialogs;
using Montage.RebirthForYou.Tools.GUI.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using Octokit;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.GUI.ModelViews
{
    public class MainWindowViewModel : ReactiveObject
    {
        #region Private Fields
        private readonly FilePickerSaveOptions _saveFileDialogOptions;
        private readonly FilePickerOpenOptions _openFileDialogOptions;

        private readonly ILogger Log;

        private ConcurrentDictionary<string,CardEntryModel> _database = new ConcurrentDictionary<string,CardEntryModel>();
        private ObservableCollection<CardEntryModel> databaseResults = new ObservableCollection<CardEntryModel>();
        private ObservableCollection<CardEntryModel> deckResults = new ObservableCollection<CardEntryModel>(new CardEntryModel[] { });
        private IContainer ioc;

        private Predicate<R4UCard> filter = (card) => true;
        private string deckName;
        private string deckRemarks;
        private string isSaved;
        private bool isShareXFlagged = true;
        private bool willSendViaTCP = true;
        #endregion

        #region Observers
        private readonly IObservable<string> _deckNameObserver;
        private readonly IObservable<string> _deckRemarkObserver;
        private readonly IObservable<bool> _isShareXFlaggedObserver;
        private readonly IObservable<bool> _willSendViaTCPObserver;
        public readonly IObservable<string> SavedObserver;

        #endregion

        public ObservableCollection<Bitmap> Items { get; set; }
        public ObservableCollection<CardEntryModel> DatabaseResults
        {
            get => databaseResults;
            set => this.RaiseAndSetIfChanged(ref databaseResults, value);
        }
        public ObservableCollection<CardEntryModel> DeckResults
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
        public bool IsShareXFlagged {
            get => isShareXFlagged; 
            set => this.RaiseAndSetIfChanged(ref isShareXFlagged, value); 
        }
        public bool WillSendViaTCP
        {
            get => willSendViaTCP;
            set => this.RaiseAndSetIfChanged(ref willSendViaTCP, value);
        }

        public CardEntryModel SelectedDatabaseCardEntry { get; internal set; }
        public Predicate<R4UCard> Filter { 
            get => filter; 
            set => this.RaiseAndSetIfChanged(ref filter, value); 
        }
        public ReactiveCommand<Unit,Unit> SaveDeckCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadDeckCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportViaTTSCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportViaDeckImageCommand { get; }
        public ReactiveCommand<Unit, Unit> ImportDeckCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenAboutCommand { get; }


        public MainWindow Parent { get; internal set; }

        public MainWindowViewModel()
        {
        }
        public MainWindowViewModel(IContainer ioc)
        {
            this.ioc = ioc;
            Log = Serilog.Log.ForContext<MainWindowViewModel>();
            _deckNameObserver = this.WhenValueChanged(x => x.DeckName);
            _deckRemarkObserver = this.WhenValueChanged(x => x.DeckRemarks);
            _isShareXFlaggedObserver = this.WhenValueChanged(x => x.IsShareXFlagged);
            _willSendViaTCPObserver = this.WhenValueChanged(x => x.WillSendViaTCP);
            SavedObserver = this.WhenValueChanged(dc => dc.Saved);

            _deckNameObserver.Subscribe(s => this.Saved = "*");
            _deckRemarkObserver.Subscribe(s => this.Saved = "*");

            SaveDeckCommand = ReactiveCommand.CreateFromTask(SaveDeck);
            LoadDeckCommand = ReactiveCommand.CreateFromTask(LoadDeck);
            ExitCommand = ReactiveCommand.Create(Exit);
            ExportViaTTSCommand = ReactiveCommand.CreateFromTask(async()=> await ExportWithResult<TTSDeckExporter>());
            ExportViaDeckImageCommand = ReactiveCommand.CreateFromTask(async () => await ExportWithResult<LocalDeckImageExporter>());
            ImportDeckCommand = ReactiveCommand.CreateFromTask(ImportDeck);
            OpenAboutCommand = ReactiveCommand.CreateFromTask(OpenAbout);

            var fileTypeChoices = new FilePickerFileType[]
            {
                new FilePickerFileType("Rebirth For You (R4U) Deck")
                {
                    Patterns = new[] { "*.r4udek" }
                }
            };

            _saveFileDialogOptions = new FilePickerSaveOptions
            {
                FileTypeChoices = fileTypeChoices,
                DefaultExtension = "Rebirth For You (R4U) Deck",
                ShowOverwritePrompt = true,
                Title = "Save R4U Deck..."
            };

            _openFileDialogOptions = new FilePickerOpenOptions
            {
                FileTypeFilter = fileTypeChoices,
                AllowMultiple = false,
                Title = "Load R4U Deck..."
            };

            //    dataContext.WhenValueChanged(x => x.DeckRemarks).Subscribe(s => _dataContext().Saved = "*");
            //    dataContext.WhenValueChanged(dc => dc.Saved).Subscribe(ChangeWindowTitle);
        }

        private async Task OpenAbout()
        {
            await ioc.GetService<AboutDialog>().ShowDialog(Parent);
        }

        public void ToggleFlag(string flagID)
        {
            Log.Information("Toggling {flag}", flagID);
            switch (flagID.ToLower())
            {
                case "sharex":
                    IsShareXFlagged = !IsShareXFlagged;
                    break;
                case "sendviatcp":
                    WillSendViaTCP = !WillSendViaTCP;
                    break;
                default: break;
            } 
        }

        internal async Task InitializeDatabase()
        {
            await Task.Yield();
            var updateCommand = new UpdateVerb();
            updateCommand.OnStarting += UpdateCommand_OnStarting;
            Log.Information("Updating DB...");
            await updateCommand.Run(ioc);
            Log.Information("Adding DB to UI...");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Parent.LoadingTextbox.Text = "[Initializing Database]";
                this.Parent.LoadingProgressBar.Value = 99;
            });

            await using (var db = ioc.GetInstance<CardDatabaseContext>())
            {
                GetCardDatabase(db).ForAll(card =>
                {
                    _database.AddOrUpdate(card.Card.Serial, card, (k, o) => o);
                    Dispatcher.UIThread.InvokeAsync(() => DatabaseResults.Add(card));
                    card.SubscribeOnImageLoaded(async () =>
                    {
                        await ApplyFilter(Filter);
                    });
                });
            }

            Log.Information("Completed!");
        }

        internal async Task InitializeSettings()
        {
            await using var db = ioc.GetInstance<CardDatabaseContext>();
            await db.Database.MigrateAsync();

            Setting sharedXFlagSettings = await db.Settings.FindAsync("gui.flags.sharex.enabled") ?? (await db.Settings.AddAsync(new Setting { Key = "gui.flags.sharex.enabled", Value = "true" })).Entity;
            Setting sendtcpFlagSettings = await db.Settings.FindAsync("gui.flags.sendtcp.enabled") ?? (await db.Settings.AddAsync(new Setting { Key = "gui.flags.sendtcp.enabled", Value = "true" })).Entity;
            IsShareXFlagged = bool.Parse(sharedXFlagSettings.Value);
            WillSendViaTCP = bool.Parse(sendtcpFlagSettings.Value);

            _willSendViaTCPObserver.SubscribeAsync(async r => await SaveFlag(sendtcpFlagSettings, r));
            _isShareXFlaggedObserver.SubscribeAsync(async r => await SaveFlag(sharedXFlagSettings, r));
            await db.SaveChangesAsync();
        }

        private async Task SaveFlag(Setting flagSetting, bool newValue)
        {
            await using var db = ioc.GetInstance<CardDatabaseContext>();
            Setting findSetting = await db.FindAsync<Setting>(flagSetting.Key);
            if (findSetting != null)
                findSetting.Value = newValue.ToString();
            await db.SaveChangesAsync();
        }

        private async Task UpdateCommand_OnStarting(UpdateVerb sender, UpdateEventArgs args)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Parent.LoadingTextbox.Text = args.Status;
                this.Parent.LoadingProgressBar.Value = args.UpdateProgress * 100;
            });
        }

        #region View Methods
        internal void AddDeckCard(CardEntryModel cardEntry)
        {
            Log.Debug("Attempting to add: {serial}", cardEntry.Card.Serial);
            if (IsDeckConstructionValid(cardEntry))
            {
                DeckResults.Add(cardEntry);
                SortDeck();
            } else
            {
                Log.Debug("Invalid.");
            }
        }
        internal void RemoveDeckCard(CardEntryModel cardEntry)
        {
            DeckResults.Remove(cardEntry);
            SortDeck();
        }
        private async Task SaveDeck(CancellationToken token)
        {
            if (!Parent.StorageProvider.CanSave)
                return;

            var saveFile = await Parent.StorageProvider.SaveFilePickerAsync(_saveFileDialogOptions);
            if (saveFile is null)
                return;
            
            var result = await SaveDeck(saveFile, token);
            var prms = GenerateStandardMessageParams(result.Title, result.Details);
            await MessageBoxManager.GetMessageBoxStandard(prms).ShowAsPopupAsync(Parent);//(this);
        }
        private async Task LoadDeck(CancellationToken token)
        {
            if (!Parent.StorageProvider.CanOpen)
                return;

            var loadedFile = (await Parent.StorageProvider.OpenFilePickerAsync(_openFileDialogOptions)).FirstOrDefault();
            if (loadedFile is null or default(IStorageFile))
                return;

            var result = await LoadDeck(loadedFile, token);
            var prms = GenerateStandardMessageParams(result.Title, result.Details);
            var msgBox = MessageBoxManager.GetMessageBoxStandard(prms);// (result.Title, result.Details);
            await msgBox.ShowAsPopupAsync(Parent);// (this);
        }
        private async Task ImportDeck()
        {
            var dialog = ioc.GetService<RemoteImportDialog>();
            var url = await dialog.GetResult(Parent);
            if (!string.IsNullOrWhiteSpace(url))
            {
                MessageBoxStandardParams resultParams;
                try
                {
                    Log.Information("Inserted: {resultMessage}", url);
                    var deck = (await new ExportVerb { Source = url, NonInteractive = true, Exporter = "null" }.ParseDeck(ioc)).Deck;
                    await ApplyDeck(deck);
                    resultParams = GenerateStandardMessageParams("Success!", $"[{deck.Name}] was loaded successfully!");
                }
                catch (NotImplementedException e) when (e.Message == "NO_PARSER")
                {
                    resultParams = GenerateStandardMessageParams("Failed!", $"Could not find a compatible importer. Please make sure that the inputted URL is correct.");
                }
                catch (DeckParsingException e)
                {
                    resultParams = GenerateStandardMessageParams("Failed!", e.Message);
                }
                catch (Exception e)
                {
                    Log.Error("Logging an unacconted error during import.", e);
                    resultParams = GenerateStandardMessageParams("Failed!", $"Failed because of an unknown error: {e.Message}");
                }
                var resultMsgBox = MessageBoxManager.GetMessageBoxStandard(resultParams);
                await resultMsgBox.ShowAsPopupAsync(Parent);
            }
        }
        private void Exit()
        {
            Parent.Close();   
        }
        private async Task ExportWithResult<T>() where T : IDeckExporter
        {
            var result = await Export<T>();
            await MessageBoxManager.GetMessageBoxStandard(result.Title, result.Details).ShowAsPopupAsync(Parent);
        }
        #endregion

        private bool IsDeckConstructionValid(CardEntryModel cardEntry)
        {
            if (cardEntry.Card.Type == CardType.Partner)
            {
                return DeckResults.Count(cr => cr.Card.Type == CardType.Partner) < 3;
            }
            else if (DeckResults.Where(cr => cr.Card.Type != CardType.Partner).Count() >= 50 || DeckResults.Where(cr => cr.Card.Name.AsJPThenEN() == cardEntry.Card.Name.AsJPThenEN()).Count() >= 4)
            {
                return false;
            }
            else if (cardEntry.Card.Type == CardType.Rebirth)
            {
                return DeckResults.Count(cr => cr.Card.Type == CardType.Rebirth) < 8;
            }
            else
            {
                return true;
            }
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

        // Task _currentApplyFilter = null;
        CancellationTokenSource _currentApplyFilterCancelToken = default;
        SemaphoreSlim _applyFilterLock = new SemaphoreSlim(1, 1);
        public async Task ApplyFilter(Predicate<R4UCard> filter) => await ApplyFilter(filter, TimeSpan.Zero);

        public async Task ApplyFilter(Predicate<R4UCard> filter, TimeSpan delayTimeSpan)
        {
            Filter = filter;
            if (_currentApplyFilterCancelToken != null)
            {
                _currentApplyFilterCancelToken.Cancel();
                _currentApplyFilterCancelToken.Dispose();
            }
            _currentApplyFilterCancelToken = new CancellationTokenSource();
            try
            {
                var cancelToken = _currentApplyFilterCancelToken.Token;
                await Task.Delay(delayTimeSpan, cancelToken);
                await ApplyFilter(filter, cancelToken);
            } catch (TaskCanceledException)
            {
                // Do nothing, this was expected.
            }
        }
        public async Task ApplyFilter(Predicate<R4UCard> filter, CancellationToken token)
        {
            bool isLocked = false;
            try
            {
                if (ioc == null) return;
                var values = _database.Values.AsParallel()
                    .WithCancellation(token)
                    .Where(ce => filter(ce.Card))
                    .OrderBy(ce => ce.Card.Serial);

                isLocked = await _applyFilterLock.WaitAsync(-1, token);
                if (!token.IsCancellationRequested)
                {
                    databaseResults.Clear();
                    databaseResults.AddRange(values);
                }
                await new ValueTask<bool>(true);
            }finally
            {
                if (isLocked) _applyFilterLock.Release();
                //if (!token.IsCancellationRequested)
                //    token.Dispose(); // Some bug occurs if I tried to dispose the token.
            }
        }

        public ParallelQuery<CardEntryModel> GetCardDatabase(CardDatabaseContext db)
        {
            if (ioc == null) return new CardEntryModel[] { }.AsParallel();
            return db.R4UCards
                .Include(c => c.Set)
                .AsParallel()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .Select(c => new CardEntryModel(this, c));
        }

        internal async Task<(string Title, string Details)> SaveDeck(IStorageFile saveFilePath, CancellationToken token)
        {
            var exporter = ioc.GetInstance<LocalDeckJSONExporter>();
            try
            {
                await exporter.Export(GenerateDeck(deckResults, deckName, deckRemarks), null, saveFilePath.OpenWriteAsync, token);
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

        internal async Task<(string Title, string Details)> LoadDeck(IStorageFile loadFilePath, CancellationToken token)
        {
            try
            {
                var exporter = ioc.GetInstance<LocalDeckJSONParser>();
                await using var stream = await loadFilePath.OpenReadAsync().WaitAsync(token);
                var deck = await exporter.Parse(stream, token);
                var deckName = string.IsNullOrWhiteSpace(deck.Name) ? "An unnamed deck": deck.Name;
                await ApplyDeck(deck);
                return ("Success", $"{deckName} was loaded successfully!");
            }
            catch (DeckParsingException e)
            {
                return ("Failed", e.Message);
            }
        }

        private async Task ApplyDeck(R4UDeck deck)
        {
            DeckResults.Clear();
            var range = deck.Ratios.Keys
                .SelectMany(c => Enumerable.Range(0, deck.Ratios[c]).Select(i => c.Serial))
                .Select(serial => _database[serial]);
            await range .Select(ce => ce.LoadImageAsync())
                        .ProcessAllAsync(new SemaphoreSlim(initialCount: 1, maxCount: 30));
            DeckResults.AddRange(range);
            DeckName = deck.Name;
            DeckRemarks = deck.Remarks;
            Saved = "";
            SortDeck();
        }

        private static R4UDeck GenerateDeck(ObservableCollection<CardEntryModel> deckResults, string deckName, string deckRemarks)
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
            var outCommand = isShareXFlagged ? "sharex" : "";
            var flags = Array.Empty<string>().AsEnumerable();
            if (willSendViaTCP && typeof(T) == typeof(TTSDeckExporter)) flags = flags.Append("sendtcp");
            await exporter.Export(deck, new GUIBasedExportInfo(outCommand, flags));
            return ("Success", $"{deck.Name} was exported successfully!");
        }
        private MessageBoxStandardParams GenerateStandardMessageParams(string title, string details)
        {
            var result = new MessageBoxStandardParams {
                MinWidth = 400,
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = details,
                ShowInCenter = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (title == "Failed!") result.Icon = Icon.Error;
            return result;
        }

        internal async Task SearchCombos(CardEntryModel card)
        {
            var nameReferencesRegex = new Regex(@"(?:“|”|"")([^”""]+)(?:“|”|"")");
            var traitReferenceRegex = new Regex(@"(?:<)([^>]+)(?:>)");
            var orQueries = card.Card.Effect
                .SelectMany(e => nameReferencesRegex.Matches(e.EN))
                .Select(m => m.Groups[1].Value)
                .SelectMany(reff => new CardQuery[] { new CardQuery { Name = reff }, new CardQuery { Effect = reff } })
                .ToList();

           orQueries.AddRange(card.Card.Effect
                .SelectMany(e => traitReferenceRegex.Matches(e.EN))
                .Select(m => m.Groups[1].Value)
                .SelectMany(reff => new CardQuery[] { new CardQuery { Effect = $"<{reff}>" }, new CardQuery { Traits = new string[] { reff } } })
                );

            orQueries.Add(new CardQuery { Effect = card.Card.Name.EN });
            orQueries.Add(new CardQuery { Name = card.Card.Name.EN });
            var seriesQueries = card.Card.Set.TitleCodes
                    .SelectMany(tc => new CardQuery[]
                    {
                        new CardQuery { Serial = $"{tc}/" }
                    })
                    .ToList();
            var newFilter = new CardQuery
            {
                And = new CardQuery[] {
                    new CardQuery { Or = seriesQueries.ToArray() },
                    new CardQuery { Or = orQueries.ToArray() }
                }
            };
            await ApplyFilter(newFilter.ToQuery());
        }
    }

    internal class GUIBasedExportInfo : IExportInfo
    {
        public string Source => null;
        public string Destination => "./Export/";
        public string Parser => "";
        public string Exporter => "";
        public string OutCommand { get; }
        public IEnumerable<string> Flags { get; private set; }
        public bool NonInteractive => true;

        public GUIBasedExportInfo(string outCommand, IEnumerable<string> flags)
        {
            OutCommand = outCommand;
            Flags = new[] { "upscaling" }.Concat(flags);                                                                                
        }
    }
}
