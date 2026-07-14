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
using Codexus.Development.SDK.Entities;
using FuriosoNEL.Entities.Web.RentalGame;
using FuriosoNEL.Handlers.Game.NetServer;
using FuriosoNEL.Handlers.Game.RentalServer;
using FuriosoNEL.Manager;
using FuriosoNEL.Utils;
using Serilog;
using Windows.ApplicationModel.DataTransfer;
using static FuriosoNEL.Utils.StaTaskRunner;

namespace FuriosoNEL.Component
{
    public sealed partial class JoinRentalServerContent : UserControl
    {
        private string _serverId = string.Empty;
        private string _serverName = string.Empty;
        private string _mcVersion = string.Empty;
        private bool _hasPassword;
        private List<RentalRoleItem> _roles = new();

        public ContentDialog? ParentDialog { get; set; }

        public JoinRentalServerContent()
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

        public void Initialize(string serverId, string serverName, string mcVersion, bool hasPassword, List<RentalRoleItem> roles)
        {
            _serverId = serverId;
            _serverName = serverName;
            _mcVersion = mcVersion;
            _hasPassword = hasPassword;
            _roles = roles;

            PasswordPanel.Visibility = hasPassword ? Visibility.Visible : Visibility.Collapsed;

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
        string Password => PasswordBox.Password ?? string.Empty;

        private async void AccountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var id = SelectedAccountId;
            if (string.IsNullOrWhiteSpace(id)) return;
            try
            {
                await RunOnStaAsync(() => new SelectAccount().Execute(id));
                var r = await RunOnStaAsync(() => new OpenRentalServer().ExecuteForAccount(id, _serverId));
                if (r.Success)
                {
                    _roles = r.Items;
                    DispatcherQueue.TryEnqueue(RefreshRoles);
                }
            }
            catch (System.Exception ex) { Log.Debug(ex, "切换账号失败"); }
        }

        private async void AddRole_Click(object sender, RoutedEventArgs e)
        {
            var content = new AddRoleContent();
            content.SetServerId(_serverId);
            var dlg = DialogService.Create(XamlRoot, "添加角色", content, "添加", null, "关闭");
            if (await dlg.ShowAsync() != ContentDialogResult.Primary) return;
            if (string.IsNullOrWhiteSpace(content.RoleName)) return;

            var result = await RunOnStaAsync(() => new CreateRentalRole().Execute(_serverId, content.RoleName));
            if (result.Success)
            {
                _roles = result.Items;
                RefreshRoles();
                NotificationHost.ShowGlobal("角色创建成功", ToastLevel.Success);
            }
        }

        public async Task<bool> LaunchAsync()
        {
            var accId = SelectedAccountId;
            var roleId = SelectedRoleId;
            var password = Password;

            if (string.IsNullOrWhiteSpace(accId) || string.IsNullOrWhiteSpace(roleId))
            {
                NotificationHost.ShowGlobal("请选择账号和角色", ToastLevel.Error);
                return false;
            }
            if (_hasPassword && string.IsNullOrWhiteSpace(password))
            {
                NotificationHost.ShowGlobal("请输入服务器密码", ToastLevel.Error);
                return false;
            }

            NotificationHost.ShowGlobal("正在准备游戏资源...", ToastLevel.Success);
            await RunOnStaAsync(() => new SelectAccount().Execute(accId));

            var set = SettingManager.Instance.Get();
            var enabled = set?.Socks5Enabled ?? false;
            var req = new EntityJoinRentalGame
            {
                ServerId = _serverId,
                ServerName = _serverName,
                Role = roleId,
                GameId = _serverId,
                Password = password,
                McVersion = _mcVersion,
                Socks5 = (!enabled || string.IsNullOrWhiteSpace(set?.Socks5Address))
                    ? new EntitySocks5 { Address = string.Empty, Port = 0, Username = string.Empty, Password = string.Empty }
                    : new EntitySocks5 { Address = set!.Socks5Address, Port = set.Socks5Port, Username = set.Socks5Username, Password = set.Socks5Password }
            };

            var result = await Task.Run(async () => await new JoinRentalGame().Execute(req));
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
            else
            {
                NotificationHost.ShowGlobal(result.Message ?? "启动失败", ToastLevel.Error);
            }
            return result.Success;
        }
    }
}
