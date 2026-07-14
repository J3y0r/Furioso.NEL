/*
<FuriosoNEL>
Copyright (C) <2025>  <FuriosoNEL>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
*/
using System;
using Codexus.Cipher.Protocol;

namespace FuriosoNEL.type;

internal static class AppState
{
    private static WPFLauncher? _x19;
    public static WPFLauncher X19 => _x19 ??= new WPFLauncher();
    
    public static Services? Services;
    public static bool Debug;
    public static string AutoDisconnectOnBan;
}
