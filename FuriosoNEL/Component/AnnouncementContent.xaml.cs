/*
<FuriosoNEL>
Copyright (C) <2025>  <FuriosoNEL>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
*/
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Net.Http;
using System.Text.Json;
using FuriosoNEL.type;
using Serilog;

namespace FuriosoNEL.Component
{
    public sealed partial class AnnouncementContent : UserControl
    {
        public AnnouncementContent()
        {
            InitializeComponent();
            this.Loaded += AnnouncementContent_Loaded;
        }

        private void AnnouncementContent_Loaded(object sender, RoutedEventArgs e)
        {
            ContentText.Text = "暂无公告";     
        }
    }
}
