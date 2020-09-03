using AngleSharp.Dom;
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
using DynamicData.Binding;
using Lamar;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;
using MessageBox.Avalonia.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Impls.Exporters.TTS;
using Montage.RebirthForYou.Tools.GUI.ModelViews;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.GUI.Dialogs
{
    public class RemoteImportDialog : Window
    {
        private readonly IContainer _ioc;
        private readonly TextBox _importURLTextBox;
        private readonly Button _importButton;

        #region Public Properties
        public ILogger Log { get; }
        #endregion

        public RemoteImportDialog(IContainer ioc) : this()
        {
            // Do stuff here.
            //var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("title", "orem ipsum dolor sit amet, consectetur adipiscing elit, sed...");
            // messageBoxStandardWindow.Show();
            _ioc = ioc;
            Log = Serilog.Log.ForContext<MainWindow>();
            DataContext = _ioc.GetService<MainWindowViewModel>();
        }

        private void MainWindow_Initialized()//object sender, EventArgs e)
        {
            //_searchBarTextBox.IsEnabled = true;
        }

        /// <summary>
        /// This method is needed to make the WYSIWYG editor work.
        /// The actual execution method uses the one with IOC.
        /// </summary>
        public RemoteImportDialog()
        {
            InitializeComponent();
            var dataContext = new MainWindowViewModel();
            DataContext = dataContext;
            _importURLTextBox = this.FindControl<TextBox>("importURLTextBox");
            _importButton = this.FindControl<Button>("importButton");
            //_dataContext = () => DataContext as MainWindowViewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Task<string> GetResult(Window parent)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            _importButton.Click += (obj, args) =>
            {
                tcs.SetResult(_importURLTextBox.Text);
                this.Close();
            };
            this.Closed += (obj, args) =>
            {
                tcs.TrySetResult(null);
            };
            this.ShowDialog(parent);
            return tcs.Task;
        }

    }
}
