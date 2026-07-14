/*
<FuriosoNEL>
Copyright (C) <2025>  <FuriosoNEL>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
*/
using System.Collections.Generic;
using System.Linq;
using FuriosoNEL.type;
using FuriosoNEL.Entities.Web.NEL;
using FuriosoNEL.Manager;

namespace FuriosoNEL.Handlers.Game;

public class QueryGameSession
{
    public object Execute()
    {
        List<EntityQueryGameSessions> list = (from interceptor in GameManager.Instance.GetQueryInterceptors()
            select new EntityQueryGameSessions
            {
                Id = "interceptor-" + interceptor.Id,
                ServerName = interceptor.Server,
                Guid = interceptor.Name.ToString(),
                CharacterName = interceptor.Role,
                ServerVersion = interceptor.Version,
                StatusText = "Running",
                ProgressValue = 0,
                Type = "Interceptor",
                LocalAddress = interceptor.LocalAddress
            }).ToList();
        return new { type = "query_game_session", items = list };
    }
}
