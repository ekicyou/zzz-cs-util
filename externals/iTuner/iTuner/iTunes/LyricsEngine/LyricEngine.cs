//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner.iTunes
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Threading;


	/// <summary>
	/// Defines the progress callback method that consumers can use to keep track of
	/// which provider is currently executing.
	/// </summary>
	/// <param name="song">The song being searched.</param>
	/// <param name="stage">The sequential number of the registered provider about to run.</param>

	internal delegate void LyricEngineProgress (ISong song, int stage);


	/// <summary>
	/// Executes each registered provider looking for the lyrics of a given song.
	/// </summary>

	internal class LyricEngine : IDisposable
	{
		private const string LogCategory = "Lyrics";

		private static LyricEngine engine;

		private bool isConnected;
		private bool isDisposed;
		private Dictionary<int, BackgroundWorker> queue;
		private List<ILyricsProvider> providers;


		//========================================================================================
		// Constructors
		//========================================================================================

		/// <summary>
		/// Private singleton constructor.
		/// </summary>

		private LyricEngine ()
		{
			isConnected = true;
			queue = new Dictionary<int, BackgroundWorker>();

			providers = new List<ILyricsProvider>();

			// add in order of preference and quality
			providers.Add(new ChartLyricsLyricsProvider());
			providers.Add(new LyrdbLyricsProvider());
			providers.Add(new LyricsPluginLyricsProvider());
		}


		/// <summary>
		/// Factory method returning the singleton engine.
		/// </summary>
		/// <returns></returns>

		public static LyricEngine CreateEngine ()
		{
			if (engine == null)
			{
				engine = new LyricEngine();
			}

			return engine;
		}


		#region Lifecycle

		/// <summary>
		/// Destructor.
		/// </summary>

		~LyricEngine ()
		{
			Dispose();
		}
	

		/// <summary>
		/// Ensures the providers are stopped and disposed.
		/// </summary>

		public void Dispose ()
		{
			if (!isDisposed)
			{
				isConnected = false;

				foreach (BackgroundWorker worker in queue.Values)
				{
					if (worker.IsBusy)
					{
						worker.CancelAsync();
					}

					worker.Dispose();
				}

				queue.Clear();
				queue = null;

				providers.Clear();

				isDisposed = true;
			}
		}

		#endregion Lifecycle


		//========================================================================================
		// Properties/Events
		//========================================================================================

		/// <summary>
		/// Fired when the lyrics for a particular song were discovered and the song updated.
		/// </summary>

		public event TrackHandler LyricsUpdated;


		/// <summary>
		/// Fired at each stage of discovery where a stage begins when a new provider is
		/// utilized to retrieve lyrics.
		/// </summary>

		public event LyricEngineProgress LyricsProgressReport;
	

		//========================================================================================
		// Methods
		//========================================================================================

		/// <summary>
		/// Asynchronously searches for the lyrics of the given song.
		/// </summary>
		/// <param name="song">An ISong specifying both the artist and title of the song.</param>

		public void RetrieveLyrics (ISong song)
		{
			// song could be null if we called this method just when switching tracks
			// yes a very small window but nonetheless..
			if (!isConnected || (song == null))
			{
				return;
			}

			int hash = song.GetHashCode();

			lock (engine)
			{
				if (!queue.ContainsKey(hash))
				{
					BackgroundWorker worker = new BackgroundWorker();
					worker.DoWork += new DoWorkEventHandler(RetrieveLyrics);
					worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LyricsRetrieved);
					worker.WorkerSupportsCancellation = true;

					queue.Add(song.GetHashCode(), worker);

					worker.RunWorkerAsync(song);
				}
			}
		}


		/// <summary>
		/// BackgroundWorker DoWork method.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RetrieveLyrics (object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = (BackgroundWorker)sender;

			ISong song = e.Argument as ISong;
			string lyrics = String.Empty;
			int attempts = 0;
			int stage = 1;

			foreach (ILyricsProvider provider in providers)
			{
				if (isConnected && provider.IsConnected)
				{
					Logger.WriteLine(LogCategory,
						String.Format("retrieval stage {0} using {1} provider for '{2}'",
						stage, provider.Name, song.Title));

					if (LyricsProgressReport != null)
					{
						LyricsProgressReport(song, stage);
					}

					lyrics = provider.RetrieveLyrics(song);
					lyrics = (lyrics == null ? String.Empty : lyrics.Trim());

					// if provider successfully contacted server then record attempt
					if (provider.IsConnected)
					{
						attempts++;
					}
					
					// if we found lyrics then we're done!
					if (!String.IsNullOrEmpty(lyrics))
					{
						break;
					}

					if (worker.CancellationPending)
					{
						break;
					}

					stage++;
				}
			}

			// no provider successfully connected so flag total surrender
			if ((attempts == 0) || worker.CancellationPending)
			{
				isConnected = false;
			}

			if (!String.IsNullOrEmpty(lyrics))
			{
				string logtext = String.Format(
					"Setting lyrics for '{0}' by '{1}'", song.Title, song.Artist);

				Logger.WriteAppLog(LogCategory, logtext);
				Logger.WriteLine(LogCategory, logtext);

				song.Lyrics = lyrics;
			}

			e.Result = song;
		}


		/// <summary>
		/// BackgroundWorker RunWorkerCompleted method.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void LyricsRetrieved (object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				return;
			}

			if (e.Error != null)
			{
				// when the Preferences dialog is open, iTunes does not response to COM interops
				return;
			}

			ISong song = e.Result as ISong;
			int hash = song.GetHashCode();

			lock (engine)
			{
				// when iTunes forces a shutdown of iTuner, Dispose might have
				// run immediately prior to this lock, so we need to check for null
				if (queue != null)
				{
					if (queue.ContainsKey(hash))
					{
						queue.Remove(hash);
					}
				}
			}

			// signal regardless of whether lyrics is empty so we can update the UI
			if (LyricsUpdated != null)
			{
				LyricsUpdated(song as Track);
			}
		}
	}
}
