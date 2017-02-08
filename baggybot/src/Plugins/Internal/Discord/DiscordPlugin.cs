﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;
using Discord;
using Mono.CSharp;


namespace BaggyBot.Plugins.Internal.Discord
{
	public class DiscordPlugin : Plugin
	{
		public override string ServerType => "discord";

		public override event Action<ChatMessage> OnMessageReceived;
		public override event Action<ChatUser, ChatUser> OnNameChange;
		public override event Action<ChatUser, ChatChannel, ChatUser, string> OnKick;
		public override event Action<ChatChannel, ChatUser, string> OnKicked;
		public override event Action<string, Exception> OnConnectionLost;
		public override event Action<ChatUser, string> OnQuit;
		public override event Action<ChatUser, ChatChannel> OnJoinChannel;
		public override event Action<ChatUser, ChatChannel> OnPartChannel;

		private DiscordClient client;
		private Server server;
		private string token;

		public DiscordPlugin(ServerCfg cfg) : base(cfg)
		{
			token = cfg.Password;
			client = new DiscordClient();
			client.MessageReceived += (s, e) =>
			{
				if (!e.Message.IsAuthor)
				{
					var user = BuildUser(e.User);
					var channel = BuildChannel(e.Channel);
					OnMessageReceived?.Invoke(new ChatMessage(user, channel, e.Message.Text));
				}
			};
		}

		private ChatChannel BuildChannel(Channel discordChannel)
		{
			return new ChatChannel(discordChannel.Id.ToString(), discordChannel.Name, discordChannel.IsPrivate);
		}
		private ChatUser BuildUser(User discordUser)
		{
			return new ChatUser(discordUser.Name, discordUser.Id.ToString(), name: discordUser.Nickname);
		}

		public override void Disconnect()
		{
			client.Disconnect();
		}

		public override void Dispose()
		{
			client.Dispose();
		}

		public override bool Connect()
		{
			client.Connect(token, TokenType.Bot).Wait();

			while (!client.Servers.Any())
			{
				Thread.Sleep(100);
			}
			server = client.Servers.First();
			return true;
		}

		public override IReadOnlyList<ChatChannel> Channels { get; protected set; }
		public override bool Connected { get; }
		public new StatsDatabaseManager StatsDatabase { get; set; }

		public override ChatUser FindUser(string name)
		{
			var matches = server.FindUsers(name).ToArray();
			if (matches.Length == 0) throw new ArgumentException("Invalid username");
			if (matches.Length == 1)return BuildUser(matches[0]);
			throw new ArgumentException("Ambiguous username");
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message)
		{
			var channel = server.GetChannel(ulong.Parse(target.Identifier));
			var result = channel.SendMessage(message).Result;
			return MessageSendResult.Success;
		}

		public override void JoinChannel(ChatChannel channel)
		{
			throw new NotImplementedException();
		}

		public ChatUser DoWhoisCall(string nick)
		{
			throw new NotImplementedException();
		}

		public void Reconnect()
		{
			throw new NotImplementedException();
		}

		public override void Part(ChatChannel channel, string reason = null)
		{
			throw new NotImplementedException();
		}

		public override void Quit(string reason)
		{
			throw new NotImplementedException();
		}
	}
}