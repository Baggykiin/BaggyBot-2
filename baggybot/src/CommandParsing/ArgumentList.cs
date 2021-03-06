﻿using System.Collections.Generic;

namespace BaggyBot.CommandParsing
{
	class ArgumentList
	{
		private readonly List<string> arguments = new List<string>();
		private readonly Dictionary<string, int> argumentIndices = new Dictionary<string, int>(1);
		private int currentIndex = 0;

		public string this[string argName]
		{
			get
			{
				return arguments[argumentIndices[argName]];
			}
			set
			{
				var lastIndex = arguments.Count;
				argumentIndices.Add(argName, lastIndex);
				arguments.Add(value);
			}
		}

		public bool IsFull => currentIndex >= arguments.Count;

		public void AssignArgument(string value)
		{
			if (currentIndex >= arguments.Count)
			{
				throw new InvalidCommandException("Unexpected command parameter.", value);
			}
			arguments[currentIndex++] = value;
		}
	}
}
