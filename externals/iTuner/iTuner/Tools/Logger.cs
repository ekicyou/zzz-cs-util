//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner
{
	using System;
	using System.Configuration;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using Resx = Properties.Resources;


	/// <summary>
	/// A simple wrapper of a TextWriterTraceListener to append to a log file.
	/// </summary>

	internal static class Logger
	{
		#region enum Level

		public enum Level : int
		{
			Debug = 0,
			Warn = 1,
			Info = 2,
			Error = 3,
			None = int.MaxValue
		}

		#endregion enum Level

		private static Level level;

		private static bool isApplogEnabled;
		private static TextWriterTraceListener applog;


		//========================================================================================
		// Constructor
		//========================================================================================

		/// <summary>
		/// Initialize a new instance with the given output path.
		/// </summary>

		static Logger ()
		{
			Trace.Listeners.Clear();				// complete control of listeners

			string path = ConfigurationManager.AppSettings["LogFile"];
			if (path != null)
			{
				path = path.Trim();
			}

			// only enable logging if LogFile is specified
			if (!String.IsNullOrEmpty(path))
			{
				string configLevel = ConfigurationManager.AppSettings["LogLevel"];
				if (configLevel == null)
				{
					level = Level.Debug;
				}
				else
				{
					try
					{
						level = (Level)Enum.Parse(typeof(Level), configLevel, true);
					}
					catch
					{
						level = Level.Debug;
					}
				}

				string dirpath;
				string filname;

				if (path.IndexOf(Path.DirectorySeparatorChar) < 0)
				{
					// if no directory specified then place in our local AppData directory
					dirpath = PathHelper.ApplicationDataPath;
					filname = PathHelper.CleanFileName(path);
				}
				else
				{
					dirpath = PathHelper.CleanDirectoryPath(Path.GetDirectoryName(path));
					filname = PathHelper.CleanFileName(Path.GetFileName(path));
				}

				path = Path.Combine(dirpath, filname);

				try
				{
					TextWriterTraceListener listener = new TextWriterTraceListener(path);
					Trace.Listeners.Add(listener);
				}
				catch
				{
					// probably a bad path so ignore logging
				}
			}

			string applogEnabled = ConfigurationManager.AppSettings["AppLogEnabled"];
			if (applogEnabled != null)
			{
				isApplogEnabled = applogEnabled.Trim().ToLower().Equals("true");
			}
			else
			{
				isApplogEnabled = false;
			}
		}


		//========================================================================================
		// Methods
		//========================================================================================

		/// <summary>
		/// Append an empty line to the log file.
		/// </summary>

		public static void WriteLine ()
		{
			if (Trace.Listeners.Count > 0)
			{
				foreach (TraceListener listener in Trace.Listeners)
				{
					listener.WriteLine(String.Empty);
					listener.Flush();
				}
			}
		}


		/// <summary>
		/// Write a single line to the log file with the given category and text.
		/// </summary>
		/// <param name="text">The message text to append to the log.</param>

		public static void WriteLine (string category, string text)
		{
			WriteLine(Level.Info, category, text);
		}


		/// <summary>
		/// Write a single line to the log file with the given category and text,
		/// filtering on the specified level.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="category"></param>
		/// <param name="text"></param>

		public static void WriteLine (Level level, string category, string text)
		{
			if (Trace.Listeners.Count > 0)
			{
				if (level >= Logger.level)
				{
					StringBuilder builder = new StringBuilder();
					builder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff"));
					builder.Append(" ");
					builder.Append(String.Format("{0,-12}", category).Substring(0, 12));
					builder.Append(" ");
					builder.Append(String.Format("{0,-6}", level).Substring(0, 6));
					builder.Append(" ");
					builder.Append(text);

					string message = builder.ToString();

					foreach (TraceListener listener in Trace.Listeners)
					{
						listener.WriteLine(message);
						listener.Flush();
					}
				}
			}
		}


		/// <summary>
		/// Write a formatted exception to the log.
		/// </summary>
		/// <param name="category"></param>
		/// <param name="exc"></param>

		public static void WriteLine (string category, Exception exc)
		{
			if (Trace.Listeners.Count > 0)
			{
				SmartException smart = new SmartException(exc);
				WriteLine(Level.Error, category, new String('=', 80));
				WriteLine(Level.Error, category, smart.Message);
				WriteLine(Level.Error, category, new String('-', 80));
				WriteLine(Level.Error, category, smart.XmlMessage);
			}
		}


		/// <summary>
		/// Write a message and formatted exception to the log.
		/// </summary>
		/// <param name="category"></param>
		/// <param name="message"></param>
		/// <param name="exc"></param>

		public static void WriteLine (string category, string message, Exception exc)
		{
			if (Trace.Listeners.Count > 0)
			{
				WriteLine(Level.Error, category, message);

				SmartException smart = new SmartException(exc);
				WriteLine(Level.Error, category, new String('=', 80));
				WriteLine(Level.Error, category, smart.Message);
				WriteLine(Level.Error, category, new String('-', 80));
				WriteLine(Level.Error, category, smart.XmlMessage);
			}
		}


		/// <summary>
		/// Write a single log to the application log file with the given category and text.
		/// </summary>
		/// <param name="category"></param>
		/// <param name="text"></param>

		public static void WriteAppLog (string category, string text)
		{
			if (isApplogEnabled)
			{
				if (applog == null)
				{
					string path = Path.Combine(PathHelper.ApplicationDataPath, Resx.FilenameAppLog);
					applog = new TextWriterTraceListener(path);
				}

				StringBuilder builder = new StringBuilder();
				builder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				builder.Append(" ");
				builder.Append(String.Format("{0,-12}", category).Substring(0, 12));
				builder.Append(" ");
				builder.Append(text);

				string message = builder.ToString();

				applog.WriteLine(message);
				applog.Flush();
			}
		}
	}
}
