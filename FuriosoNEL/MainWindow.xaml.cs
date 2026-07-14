/*
<FuriosoNEL>
Copyright (C) <2025>  <FuriosoNEL>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
*/
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using FuriosoNEL.Page;
using FuriosoNEL.Utils;
using FuriosoNEL.Handlers.Plugin;
using FuriosoNEL.type;
using Serilog;

namespace FuriosoNEL
{
    public sealed partial class MainWindow : Window
    {
        static MainWindow? _instance;
        AppWindow? _appWindow;
        string _currentBackdrop = "";
        bool _mainNavigationInitialized;
        public static Microsoft.UI.Dispatching.DispatcherQueue? UIQueue => _instance?.DispatcherQueue;

        public MainWindow()
        {
            InitializeComponent();
            _instance = this;
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);
            _appWindow.Title = "Furioso NEL";
            ApplyThemeFromSettings();
            InitializeMainNavigationIfNeeded();
        }

        private static readonly Dictionary<string, (Type Page, string Title)> Pages = new()
        {
            ["HomePage"] = (typeof(HomePage), HomePage.PageTitle),
            ["AccountPage"] = (typeof(AccountPage), AccountPage.PageTitle),
            ["NetworkServerPage"] = (typeof(NetworkServerPage), NetworkServerPage.PageTitle),
            ["RentalServerPage"] = (typeof(RentalServerPage), RentalServerPage.PageTitle),
            ["PluginsPage"] = (typeof(PluginsPage), PluginsPage.PageTitle),
            ["GamesPage"] = (typeof(GamesPage), GamesPage.PageTitle),
            ["SkinPage"] = (typeof(SkinPage), SkinPage.PageTitle),
            ["ToolsPage"] = (typeof(ToolsPage), ToolsPage.PageTitle),
            ["AboutPage"] = (typeof(AboutPage), AboutPage.PageTitle),
        };

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeMainNavigationIfNeeded();
        }

        private void AddNavItem(Symbol icon, string key)
        {
            if (!Pages.TryGetValue(key, out var info)) return;
            NavView.MenuItems.Add(new NavigationViewItem { Icon = new SymbolIcon(icon), Content = info.Title, Tag = key });
        }

        void InitializeMainNavigationIfNeeded()
        {
            if (_mainNavigationInitialized) return;
            _mainNavigationInitialized = true;

            NavView.MenuItems.Clear();
            AddNavItem(Symbol.Home, "HomePage");
            AddNavItem(Symbol.People, "AccountPage");
            AddNavItem(Symbol.World, "NetworkServerPage");
            AddNavItem(Symbol.Remote, "RentalServerPage");
            AddNavItem(Symbol.AllApps, "PluginsPage");
            AddNavItem(Symbol.Play, "GamesPage");
            AddNavItem(Symbol.AllApps, "SkinPage");
            AddNavItem(Symbol.Setting, "ToolsPage");
            AddNavItem(Symbol.ContactInfo, "AboutPage");

            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "HomePage")
                {
                    NavView.SelectedItem = navItem;
                    ContentFrame.Navigate(typeof(HomePage));
                    break;
                }
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else if (args.SelectedItem is NavigationViewItem { Tag: string key } && Pages.TryGetValue(key, out var info))
            {
                ContentFrame.Navigate(info.Page);
            }
        }

        private void NavView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (NavView.PaneDisplayMode == NavigationViewPaneDisplayMode.Left)
            {
                NavView.OpenPaneLength = e.NewSize.Width * 0.2; 
            }
        }

        void ApplyThemeFromSettings()
        {
            try
            {
                var mode = FuriosoNEL.Manager.SettingManager.Instance.Get().ThemeMode?.Trim().ToLowerInvariant() ?? "system";
                ElementTheme t = ElementTheme.Default;
                if (mode == "light") t = ElementTheme.Light;
                else if (mode == "dark") t = ElementTheme.Dark;
                RootGrid.RequestedTheme = t;
                NavView.RequestedTheme = t;
                ContentFrame.RequestedTheme = t;
                var actual = t == ElementTheme.Default ? RootGrid.ActualTheme : t;
                UpdateTitleBarColors(actual);

                var bd = FuriosoNEL.Manager.SettingManager.Instance.Get().Backdrop?.Trim().ToLowerInvariant() ?? "mica";
                if (bd != _currentBackdrop)
                {
                    if (bd == "acrylic")
                    {
                        SystemBackdrop = new DesktopAcrylicBackdrop();
                        RootGrid.Background = null;
                    }
                    else
                    {
                        SystemBackdrop = new MicaBackdrop();
                        RootGrid.Background = null;
                    }
                    _currentBackdrop = bd;
                }
            }
            catch (Exception ex) { Log.Warning(ex, "应用主题失败"); }
        }

        public static void ApplyThemeFromSettingsStatic()
        {
            _instance?.ApplyThemeFromSettings();
        }

        void UpdateTitleBarColors(ElementTheme theme)
        {
            try
            {
                var tb = _appWindow?.TitleBar;
                if (tb == null) return;
                var fg = ColorUtil.ForegroundForTheme(theme);
                var bg = ColorUtil.Transparent;
                tb.ForegroundColor = fg;
                tb.InactiveForegroundColor = fg;
                tb.ButtonForegroundColor = fg;
                tb.ButtonInactiveForegroundColor = fg;
                tb.BackgroundColor = bg;
                tb.InactiveBackgroundColor = bg;
                tb.ButtonHoverForegroundColor = fg;
                tb.ButtonPressedForegroundColor = fg;
                tb.ButtonBackgroundColor = ColorUtil.Transparent;
                tb.ButtonInactiveBackgroundColor = ColorUtil.Transparent;
                tb.ButtonHoverBackgroundColor = ColorUtil.HoverBackgroundForTheme(theme);
                tb.ButtonPressedBackgroundColor = ColorUtil.PressedBackgroundForTheme(theme);
            }
            catch (Exception ex) { Log.Warning(ex, "更新标题栏颜色失败"); }
        }
    }
}
