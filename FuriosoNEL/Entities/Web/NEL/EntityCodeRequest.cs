/*
<FuriosoNEL>
Copyright (C) <2025>  <FuriosoNEL>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
*/
using System.Text.Json.Serialization;

namespace FuriosoNEL.Entities.Web.NEL;

public class EntityCodeRequest
{
	[JsonPropertyName("phone")]
	public required string Phone { get; set; }

	[JsonPropertyName("code")]
	public required string Code { get; set; }
}
