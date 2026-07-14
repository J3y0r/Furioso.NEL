using Microsoft.UI.Xaml.Controls;
using FuriosoNEL.Manager;

namespace FuriosoNEL.Component;

public class ThemedContentDialog : ContentDialog
{
    public ThemedContentDialog()
    {
        RequestedTheme = SettingManager.Instance.GetAppTheme();
    }
}
