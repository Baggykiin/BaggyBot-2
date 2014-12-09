﻿using System;
using System.Linq;

namespace BaggyBot.Commands
{
	class Say : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }
		private readonly IrcInterface ircInterface;

		public Say(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command){
			var msg = String.Join(" ", command.Args.Skip(1));
			Logger.Log("Saying: " + msg);

			ircInterface.SendMessage(command.Args[0], msg);
		}
	}
}
