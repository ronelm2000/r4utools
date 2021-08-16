using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Montage.RebirthForYou.Tools.GUI.ModelViews
{
    public class AboutDialogModel : ReactiveObject
    {
        #region Private Fields
        private ObservableCollection<AboutModelRow> _rows;
        #endregion

        #region Public Properties
        public ObservableCollection<AboutModelRow> Rows
        {
            get => _rows;
            set => this.RaiseAndSetIfChanged(ref _rows, value);
        }
        #endregion

        public AboutDialogModel()
        {
            Rows = new ObservableCollection<AboutModelRow>(new AboutModelRow[]
            {
                new AboutModelRow { Header = "Sample", Value = "Example Value" },
                new AboutModelRow { Header = "Sample", Value = "Example Value" },
                new AboutModelRow { Header = "Sample", Value = "Example Value" }
            });
        }

    }

    public class AboutModelRow
    {
        public string Header;
        public string Value;
    }
}
