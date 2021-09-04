using Avalonia.Media;
using Montage.RebirthForYou.Tools.CLI.Entities;

namespace Montage.RebirthForYou.Tools.GUI.ModelViews.Interfaces
{
    internal interface ICardInfoDialogModel
    {
        R4UCard Card { get; set; }
        string CardName { get; }
        IImage ImageSource { get; }
        string CardTraits { get; }
        string CardEffects { get; }
        bool IsJP { get; }
        bool HasFlavor { get; }
        string CardFlavor { get; }
        FontStyle CardFlavorFontStyle { get; }
    }
}