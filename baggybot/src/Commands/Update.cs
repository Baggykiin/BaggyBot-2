﻿using System;

namespace BaggyBot.Commands
{
	internal class Update : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "update";
		public override string Usage => "[--no-dl]";
		public override string Description => "Downloads a new update and makes me restart to apply it.";

		public override void Use(CommandArgs command)
		{
			// TODO: Implemenent self-updating
			throw new NotImplementedException();
		}
	}
}
