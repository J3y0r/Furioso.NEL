/*
<FuriosoNEL>
Copyright (C) <2025>  <FuriosoNEL>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
*/

using System;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using FuriosoNEL.Entities.Web.NetGame;
using FuriosoNEL.Handlers.Game.NetServer;
using FuriosoNEL.Manager;
using FuriosoNEL.Utils;
using Serilog;
using Windows.ApplicationModel.DataTransfer;
using static FuriosoNEL.Utils.StaTaskRunner;

namespace FuriosoNEL.Component
{
    public sealed partial class JoinServerContent : UserControl
    {
        private string _serverId = string.Empty;
        private string _serverName = string.Empty;
        private List<RoleItem> _roles = new();

        public ContentDialog? ParentDialog { get; set; }

        public JoinServerContent()
        {
            InitializeComponent();
            try
            {
                var mode = SettingManager.Instance.Get().ThemeMode?.Trim().ToLowerInvariant() ?? "system";
                ElementTheme t = ElementTheme.Default;
                if (mode == "light") t = ElementTheme.Light;
                else if (mode == "dark") t = ElementTheme.Dark;
                RequestedTheme = t;
            }
            catch { }
        }

        public void Initialize(string serverId, string serverName, List<RoleItem> roles)
        {
            _serverId = serverId;
            _serverName = serverName;
            _roles = roles;

            var accounts = UserManager.Instance.GetAuthorizedAccounts();
            AccountCombo.ItemsSource = accounts.Select(a => new OptionItem { Label = a.Label, Value = a.Id }).ToList();
            if (AccountCombo.Items.Count > 0) AccountCombo.SelectedIndex = 0;

            RefreshRoles();
        }

        void RefreshRoles()
        {
            RoleCombo.ItemsSource = _roles.Select(r => new OptionItem { Label = r.Name, Value = r.Id }).ToList();
            if (RoleCombo.Items.Count > 0) RoleCombo.SelectedIndex = 0;
        }

        public class OptionItem
        {
            public string Label { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        string SelectedAccountId => AccountCombo.SelectedValue as string ?? string.Empty;
        string SelectedRoleId => RoleCombo.SelectedValue as string ?? string.Empty;

        private async void AccountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var id = SelectedAccountId;
            if (string.IsNullOrWhiteSpace(id)) return;
            await RunOnStaAsync(() => new SelectAccount().Execute(id));
            var r = await RunOnStaAsync(() => new OpenServer().ExecuteForAccount(id, _serverId));
            if (r.Success)
            {
                _roles = r.Items;
                DispatcherQueue.TryEnqueue(RefreshRoles);
            }
        }

        private async void AddRole_Click(object sender, RoutedEventArgs e)
        {
            var content = new AddRoleContent();
            content.SetServerId(_serverId);
            var dlg = DialogService.Create(XamlRoot, "添加角色", content, "添加", null, "关闭");
            if (await dlg.ShowAsync() != ContentDialogResult.Primary) return;

            var newRoles = await content.CreateRoleAsync();
            if (newRoles != null)
            {
                _roles = newRoles;
                RefreshRoles();
            }
        }

        public async Task<bool> LaunchAsync()
        {
            var accId = SelectedAccountId;
            var roleId = SelectedRoleId;
            if (string.IsNullOrWhiteSpace(accId) || string.IsNullOrWhiteSpace(roleId))
            {
                NotificationHost.ShowGlobal("请选择账号和角色", ToastLevel.Error);
                return false;
            }

            NotificationHost.ShowGlobal("正在准备游戏资源...", ToastLevel.Success);
            Log.Information("启动游戏: Server={Server}, Role={Role}", _serverId, roleId);

            var result = await Task.Run(async () => await new JoinGame().Execute(accId, _serverId, _serverName, roleId));
            if (result.Success)
            {
                NotificationHost.ShowGlobal("启动成功", ToastLevel.Success);
                var copyText = SettingManager.Instance.GetCopyIpText(result.Ip, result.Port);
                if (copyText != null)
                {
                    var dp = new DataPackage();
                    dp.SetText(copyText);
                    Clipboard.SetContent(dp);
                    Clipboard.Flush();
                    NotificationHost.ShowGlobal("地址已复制", ToastLevel.Success);
                }
            }
            return result.Success;
        }
    }
}
