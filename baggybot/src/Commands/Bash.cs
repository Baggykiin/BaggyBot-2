﻿using BaggyBot.CommandParsing;

namespace BaggyBot.Commands
{
	internal class Bash : StdioBridge
	{
		public override string Name => "bash";
		public override string Usage => "<bash expression>";
		public override string Description => "";

		public Bash()
		{
			Init("bash");
		}

		public override void Use(CommandArgs command)
		{
			EnsureProcess(command.Channel);

			var parser = new CommandParser(new Operation()
				.AddFlag("reset")
				.AddKey("key", null, 'k')
				.AddRestArgument(null));

			var res = parser.Parse(command.FullArgument);
			var key = res.Keys["key"];

			if (res.Flags["reset"])
			{
				Reset(command.Channel);
			}
			else if (key != null)
			{
				SendKey(command.Channel, key);
			}
			else
			{
				Write(command.Channel, command.FullArgument);
			}
		}
	}
}