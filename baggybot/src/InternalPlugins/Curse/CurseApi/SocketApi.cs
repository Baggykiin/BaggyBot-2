﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.InternalPlugins.Curse.CurseApi.ExtensionMethods;
using BaggyBot.InternalPlugins.Curse.CurseApi.SocketModel;
using Newtonsoft.Json;

namespace BaggyBot.InternalPlugins.Curse.CurseApi
{
	public delegate void MessageReceivedEvent(MessageResponse message);

	class SocketApi
	{
		private readonly ClientWebSocket webSocket= new ClientWebSocket();

		public event MessageReceivedEvent OnMessageReceived;

		public void Connect(Uri wsUri, string authToken)
		{
			webSocket.Options.SetRequestHeader("Origin", "https://www.curse.com");
			webSocket.Options.SetRequestHeader("Cookie", "CurseAuthToken=" + WebUtility.UrlEncode(authToken));
			webSocket.ConnectAsync(wsUri, CancellationToken.None).Wait();
		}

		public void Login(string machineKey, string sessionId, int userId)
		{
			SendMessage(LoginRequest.Create(machineKey, sessionId, userId));
		}

		public async void SendMessage(SocketRequest message)
		{
			await webSocket.SendMessage(JsonConvert.SerializeObject(message));
		}

		public void Listen()
		{
			Task.Run(() =>
			{
				while (webSocket.CloseStatus == null)
				{
					var message = webSocket.ReceiveMessage().Result;
					var parsed = SocketResponse.Deserialise(message);

					ProcessMessage(parsed);
				}
			});
		}

		private void ProcessMessage(SocketResponse message)
		{
			switch (message.TypeID)
			{
				case ResponseType.UserActivityChange:
					break;
				case ResponseType.ChannelReference:
					break;
				case ResponseType.ChatMessage:
					OnMessageReceived?.Invoke((MessageResponse)message.Body);
					break;
				case ResponseType.Login:
					break;
				case ResponseType.UnknownChange:
					break;
				default:
					break;
			}
		}
	}
}
