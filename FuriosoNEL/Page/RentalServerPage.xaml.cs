/*
<FuriosoNEL>
Copyright (C) <2025>  <FuriosoNEL>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
*/
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FuriosoNEL.Component;
using FuriosoNEL.Entities.Web.RentalGame;
using FuriosoNEL.Handlers.Game.RentalServer;
using FuriosoNEL.Utils;
using Serilog;
using static FuriosoNEL.Utils.StaTaskRunner;

namespace FuriosoNEL.Page;

public sealed partial class RentalServerPage : Microsoft.UI.Xaml.Controls.Page, INotifyPropertyChanged
{
    public static string PageTitle => "租赁服";

    public ObservableCollection<RentalServerItem> Servers { get; } = new();
    CancellationTokenSource? _cts;
    int _refreshId;
    bool _notLogin;

    public bool NotLogin { get => _notLogin; private set { _notLogin = value; OnPropertyChanged(nameof(NotLogin)); } }

    public RentalServerPage()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) => await RefreshAsync();
    }

    async void RefreshButton_Click(object sender, RoutedEventArgs e) => await RefreshAsync();

    async Task RefreshAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        var myId = Interlocked.Increment(ref _refreshId);

        ListRentalServersResult result;
        try
        {
            result = await RunOnStaAsync(() =>
            {
                if (ct.IsCancellationRequested) return new ListRentalServersResult();
                return new ListRentalServers().Execute(0, 100);
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[RentalServer] RefreshAsync 异常");
            NotLogin = false;
            Servers.Clear();
            return;
        }

        if (myId != _refreshId) return;

        if (result.NotLogin)
        {
            NotLogin = true;
            Servers.Clear();
            return;
        }

        NotLogin = false;
        Servers.Clear();
        foreach (var item in result.Items)
        {
            if (myId != _refreshId || ct.IsCancellationRequested) break;
            Servers.Add(item);
        }
    }

    async void JoinServerButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: RentalServerItem s }) return;

        try
        {
            var openResult = await RunOnStaAsync(() => new OpenRentalServer().Execute(s.EntityId));
            if (!openResult.Success) return;

            var content = new JoinRentalServerContent();
            content.Initialize(s.EntityId, s.Name, s.McVersion, s.HasPassword, openResult.Items);

            var dlg = DialogService.Create(XamlRoot, "加入租赁服", content, "启动", null, "关闭");
            content.ParentDialog = dlg;

            if (await dlg.ShowAsync() == ContentDialogResult.Primary)
                await content.LaunchAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "加入租赁服失败");
            NotificationHost.ShowGlobal("加入失败: " + ex.Message, ToastLevel.Error);
        }
    }

    void ServersGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ServersGrid.ItemsPanelRoot is ItemsWrapGrid panel && e.NewSize.Width > 0)
            panel.ItemWidth = Math.Max(240, (e.NewSize.Width - 24) / 4);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
