﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CppIncludeChecker
{
	class Logger
	{
		private TextWriter _text_writer;

		public Logger(string filename)
		{
			_text_writer = File.CreateText(filename);
		}

		public void Dispose()
		{
			_text_writer.Close();
		}

		public void Log(string text, List<string> outputs = null, List<string> errors = null)
		{
			text = string.Format("{0}: {1}", DateTime.Now.ToLongTimeString(), text);

			Console.WriteLine(text);
			Debug.WriteLine(text);
			if (outputs != null)
			{
				foreach (string line in outputs)
				{
					Console.WriteLine(line);
					Debug.WriteLine(line);
				}
			}
			if (errors != null)
			{
				foreach (string line in errors)
				{
					Console.WriteLine(line);
					Debug.WriteLine(line);
				}
			}
		}

		public void LogToFile(string text, List<string> outputs = null, List<string> errors = null)
		{
			_text_writer.WriteLine(text);
			if (outputs != null)
			{
				foreach (string line in outputs)
				{
					_text_writer.WriteLine(line);
				}
			}
			if (errors != null)
			{
				foreach (string line in errors)
				{
					_text_writer.WriteLine(line);
				}
			}
		}
	}
}