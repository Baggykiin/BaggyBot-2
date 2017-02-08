﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins.MessageFormatters;

namespace BaggyBot.Plugins
{
	public abstract class Plugin : IDisposable
	{
		// events
		public abstract event Action<ChatMessage> OnMessageReceived;
		public abstract event Action<ChatUser, ChatUser> OnNameChange;
		public abstract event Action<ChatUser, ChatChannel, ChatUser, string> OnKick;
		public abstract event Action<ChatChannel, ChatUser, string> OnKicked;
		public abstract event Action<ChatUser, ChatChannel> OnJoinChannel;
		public abstract event Action<ChatUser, ChatChannel> OnPartChannel;
		public abstract event Action<string, Exception> OnConnectionLost;
		public abstract event Action<ChatUser, string> OnQuit;

		public string ServerType => GetType().GetCustomAttribute<ServerTypeAttribute>().ServerType;
		/// <summary>
		/// Should return true if the client is currently connected to the server.
		/// </summary>
		public abstract bool Connected { get; }
		/// <summary>
		/// A list of channels the client is currently a part of.
		/// </summary>
		public abstract IReadOnlyList<ChatChannel> Channels { get; protected set; }

		/// <summary>
		/// Should be set to true if users should be mentioned
		/// by prefixing their username with '@' character.
		/// </summary>
		public bool AtMention { get; protected set; }
		/// <summary>
		/// Should be set to true if the plugin supports line feeds within messages.
		/// </summary>
		public bool AllowsMultilineMessages { get; protected set; }
		/// <summary>
		/// Should be set to false if the server does not correctly handle special
		/// unicode characters (such as mathematical symbols).
		/// </summary>
		public bool SupportsUnicode { get; protected set; } = true;

		/// <summary>
		/// The operators defined for this client.
		/// </summary>
		public IReadOnlyList<Operator> Operators => ServerConfiguration.Operators;
		/// <summary>
		/// Any message formatters placed in here will format outgoing and incoming messages for this plugin.
		/// </summary>
		public List<IMessageFormatter> MessageFormatters { get; } = new List<IMessageFormatter>();
		/// <summary>
		/// Gets the unique name of this server as defined in the configuration file.
		/// </summary>
		public string ServerName => ServerConfiguration.ServerName;

		/// <summary>
		/// Tries to look up user based by their username.
		/// Will generally only be used when looking up a user by their Unique ID is not possible,
		/// for instance because the username is user input.
		/// </summary>
		public abstract ChatUser FindUser(string username);
		public abstract MessageSendResult SendMessage(ChatChannel target, string message);
		public abstract void JoinChannel(ChatChannel channel);
		public abstract void Part(ChatChannel channel, string reason = null);
		public abstract void Quit(string reason);
		public abstract bool Connect();
		public abstract void Disconnect();
		public abstract void Dispose();

		/// <summary>
		/// The server configuration as defined in the configuration file.
		/// </summary>
		private ServerCfg ServerConfiguration { get; }

		protected Plugin(ServerCfg config)
		{
			ServerConfiguration = config;
		}

		/// <summary>
		/// Look up a channel by its name
		/// </summary>
		public virtual ChatChannel FindChannel(string name)
		{
			var matches = Channels.Where(c => c.Name == name).ToArray();
			if (matches.Length == 0) throw new ArgumentException("Invalid channel name.");
			if (matches.Length == 1) return matches[0];
			throw new ArgumentException("Ambiguous channel name");
		}

		/// <summary>
		/// Look up a channel by its ID
		/// </summary>
		public ChatChannel GetChannel(string channelId)
		{
			if (channelId == null) throw new ArgumentNullException(nameof(channelId));
			return Channels.First(c => c.Identifier == channelId);
		}

		/// <summary>
		/// Send a message to every operator defined for this client
		/// </summary>
		public virtual void NotifyOperators(string message)
		{
			foreach (var op in Operators)
			{
				SendMessage(FindChannel(op.Nick), message);
			}
		}

		/// <summary>
		/// Replies to a specific user in a channel.
		/// </summary>
		public virtual MessageSendResult Reply(ChatChannel channel, ChatUser user, string message)
		{
			return SendMessage(channel, $"{(AtMention ? "@" : "")}{user.AddressableName}, {message}");
		}
	}
}
