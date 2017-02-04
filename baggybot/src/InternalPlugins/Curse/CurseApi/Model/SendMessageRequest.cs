﻿namespace BaggyBot.InternalPlugins.Curse.CurseApi.Model
{
	public class SendMessageRequest : RequestObject
	{
		public string ClientID { get; set; }
		public string MachineKey { get; set; }
		public string Body { get; set; }
		public string AttachmentID { get; set; }
		public int AttachmentRegionID { get; set; }
	}

}
