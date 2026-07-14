/*
<FuriosoNEL>
Copyright (C) <2025>  <FuriosoNEL>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
*/
using FuriosoNEL.Manager;

namespace FuriosoNEL.Handlers.Login
{
    public class DeleteAccount
    {
        public object Execute(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
            {
                return new { type = "delete_error", message = "entityId为空" };
            }
            UserManager.Instance.RemoveAvailableUser(entityId);
            UserManager.Instance.RemoveUser(entityId);
            var items = GetAccount.GetAccountItems();
            return new { type = "accounts", items };
        }
    }
}
