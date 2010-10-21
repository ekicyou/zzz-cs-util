//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner.iTunes
{
	using System;
	using System.IO;


	/// <summary>
	/// 
	/// </summary>

	internal class M3UPlaylistWriter : PlaylistWriterBase
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="name"></param>
		/// <param name="createSubdirectories"></param>

		public M3UPlaylistWriter (string root, string name, bool createSubdirectories)
			: base(root, name, ".m3u", createSubdirectories)
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="track"></param>

		public override void Add (Track track, string path)
		{
			//#EXTINF:123,Sample Artist - Sample title
			//C:\Documents and Settings\I\My Music\Sample.mp3

			WriteLine(String.Format("#EXTINF:{0},{1} - {2}", track.Duration, track.Artist, track.Name));

			if (createSubdirectories)
			{
				WriteLine(path);
			}
			else
			{
				WriteLine(Path.GetFileName(path));
			}
		}


		/// <summary>
		/// 
		/// </summary>

		protected override void WriteFooter ()
		{
		}


		/// <summary>
		/// 
		/// </summary>

		protected override void WriteHeader ()
		{
			WriteLine("#EXTM3U");
		}
	}
}
