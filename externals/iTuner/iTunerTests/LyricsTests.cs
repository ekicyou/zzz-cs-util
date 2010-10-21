//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTunerTests
{
	using System;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using iTunesLib;
	using iTuner.iTunes;


	/// <summary>
	/// </summary>

	[TestClass]
	public class LyricsTests : TestBase
	{
		private class MockSong : ISong
		{
			private string lyrics;

			public MockSong (string artist, string title)
			{
				this.Artist = artist;
				this.Title = title;
				this.lyrics = String.Empty;
			}

			public string Artist { get; set; }
			public string Title { get; set; }
			public string Lyrics { get { return lyrics; } set { lyrics = value; } }
			public void CacheLyrics (string lyrics) { this.lyrics = lyrics; }
		}


		[TestMethod]
		public void TestProviders ()
		{
			MockSong song = new MockSong("Cut Copy", "Time Stands Still");

			ILyricsProvider provider;
			string lyrics;

			provider = new ChartLyricsLyricsProvider();
			lyrics = provider.RetrieveLyrics(song);

			provider = new LyrdbLyricsProvider();
			lyrics = provider.RetrieveLyrics(song);

			provider = new LyricsPluginLyricsProvider();
			lyrics = provider.RetrieveLyrics(song);
		}


		[TestMethod]
		public void GetLyrics ()
		{
			MockSong song = new MockSong("Cut Paste", "Time Stands Still");

			//engine.RetrieveLyrics(song);

			// TODO: use ManualResetEvent to wait for this to complete...
		}


		private void DoLyricsUpdated (ITrack track)
		{
			throw new NotImplementedException();
		}


		private void DoProgressReport (ISong song, int stage)
		{
			throw new NotImplementedException();
		}
	}
}
