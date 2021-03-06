﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BaggyBot.Commands;
using BaggyBot.Configuration;
using BaggyBot.EmbeddedData;
using BaggyBot.Formatting;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.Monitoring;

namespace BaggyBot.MessagingInterface.Handlers
{
	internal class CommandHandler : ChatClientEventHandler
	{
		private readonly Dictionary<string, Command> commands = new Dictionary<string, Command>();

		public override void Initialise()
		{
			var matchingTypes = Assembly.GetExecutingAssembly().DefinedTypes
				.Where(t => typeof(Command).IsAssignableFrom(t)
				&& !t.IsAbstract
				&& t.GetCustomAttribute<DisabledCommandAttribute>() == null
				&& t.GetCustomAttribute<NonAutoGeneratedCommandAttribute>() == null);

			foreach (var type in matchingTypes)
			{
				if (!ConfigManager.Config.Interpreters.Enabled && typeof(ReadEvaluatePrintCommand).IsAssignableFrom(type))
				{
					if (type == typeof(Py))
					{
						var command = new Notify("The interactive Python interpreter is currently disabled. It can be enabled in the configuration file.");
						command.Client = Client;
						commands["py"] = command;
					}
					else if (type == typeof(Cs))
					{
						var command = new Notify("The interactive C# interpreter is currently disabled. It can be enabled in the configuration file.");
						command.Client = Client;
						commands["cs"] = command;
					}
				}
				else
				{
					var command = (Command)Activator.CreateInstance(type);
					command.Client = Client;
					commands[command.Name] = command;
				}

			}

			// Command list must be initialised before we can pass a reference to it to the Help command.
			var help = new Help(commands);
			help.Client = Client;
			commands["help"] = help;
		}

		public override void HandleMessage(MessageEvent ev)
		{
			var match = Bot.CommandIdentifiers.FirstOrDefault(id => ev.Message.Body.StartsWith(id));

			if (match == null)
			{
				return;
			}
			var cmdInfo = CommandArgs.FromMessage(match, ev);
			ProcessCommand(cmdInfo);
		}

		private void HandleExistingCommand(CommandArgs cmdInfo)
		{
			if (!commands[cmdInfo.Command].HasPermission(cmdInfo))
			{
				cmdInfo.ReturnMessage(Messages.CmdNotAuthorised);
				return;
			}

			// Don't gobble up exceptions when debugging
			if (ConfigManager.Config.DebugMode)
			{
				commands[cmdInfo.Command].Use(cmdInfo);
			}
			else
			{
				try
				{
					commands[cmdInfo.Command].Use(cmdInfo);
				}
				catch (Exception e)
				{
					var exceptionMessage = $"An unhandled exception occurred while trying to process your command ({e.GetType().Name}: {e.Message})";
					cmdInfo.ReturnMessage(exceptionMessage);
					// Previously, debugging information (filename and line number) were put in the error message.
					// That's dubm, no reason to bother the user with information that's useless to them. Log the exception instead.
					Logger.LogException(commands[cmdInfo.Command], e, $"processing the command \"{cmdInfo.Command} {cmdInfo.FullArgument}\"");
				}
			}
		}

		private void ProcessCommand(CommandArgs cmdInfo)
		{
			// Inject bot information, but do not return.
			if (new[] { "help", "about", "info", "baggybot", "stats" }.Contains(cmdInfo.Command.ToLower()) && cmdInfo.Args.Length == 0)
			{
				cmdInfo.ReturnMessage(string.Format(Messages.CmdGeneralInfo, Bot.Version, ConfigManager.Config.StatsPage));
			}

			if (commands.ContainsKey(cmdInfo.Command))
			{
				HandleExistingCommand(cmdInfo);
			}
			else
			{
				// This doesn't look like a valid command. Is it a rem being set?
				if (cmdInfo.Command == "rem")
				{
					if (cmdInfo.Args.Length == 1 && cmdInfo.Args[0] == "rem")
					{
						cmdInfo.ReturnMessage("さすが姉さま");
						return;
					}
					if (cmdInfo.Args.Length < 2)
					{
						cmdInfo.Reply($"usage: {Frm.M}{Bot.CommandIdentifiers.First()}{cmdInfo.Command} <key> <message>{Frm.M} -- Creates an alias for a message");
						return;
					}
					Logger.Log(this, "Saving rem");
					var alias = $"{cmdInfo.Args[0]} say {string.Join(" ", cmdInfo.Args.Skip(1))}";
					((Alias)commands["alias"]).Use(CommandArgs.FromPrevious("alias", alias, cmdInfo));
					return;
				}
				// Or perhaps an alias?
				if (((Alias)commands["alias"]).ContainsKey(cmdInfo.Command))
				{
					var aliasedCommand = ((Alias)commands["alias"]).GetAlias(cmdInfo.Command);
					aliasedCommand = aliasedCommand.Replace(" $args", cmdInfo.FullArgument ?? "");

					Logger.Log(this, $"Calling aliased command: -{aliasedCommand}");

					ProcessCommand(CommandArgs.FromPrevious(aliasedCommand, cmdInfo));
				}
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			foreach (var command in commands)
			{
				command.Value.Dispose();
			}
		}
	}
}