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

	internal class PLSPlaylistWriter : PlaylistWriterBase
	{
		private const int Version = 2;
		private int count;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="name"></param>
		/// <param name="createSubdirectories"></param>

		public PLSPlaylistWriter (string root, string name, bool createSubdirectories)
			: base(root, name, ".pls", createSubdirectories)
		{
			count = 0;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="track"></param>

		public override void Add (Track track, string path)
		{
			//File2=http://example.com/song.mp3
			//Title2=Remote MP3
			//Length2=286

			count++;

			if (createSubdirectories)
			{
				WriteLine(String.Format("File{0}={1}", count, path));
			}
			else
			{
				WriteLine(String.Format("File{0}={1}", count, Path.GetFileName(path)));
			}

			WriteLine(String.Format("Title{0}={1}", count, track.Name));
			WriteLine(String.Format("Length{0}={1}", count, track.Duration));
		}


		/// <summary>
		/// 
		/// </summary>

		protected override void WriteFooter ()
		{
			WriteLine(String.Format("NumberOfEntries={0}", count));
			WriteLine(String.Format("Version={0}", Version));
		}


		/// <summary>
		/// 
		/// </summary>

		protected override void WriteHeader ()
		{
			WriteLine("[playlist]");
		}
	}
}
