//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner.iTunes
{
	using System;


	/// <summary>
	/// Instantiate a new playlist writer.
	/// </summary>

	internal static class PlaylistFactory
	{

		public const string NoFormat = "None";


		/// <summary>
		/// Initiate a new playlist writer based on the specified key.
		/// </summary>
		/// <param name="key">The key identifying the writer.</param>
		/// <param name="root">The root path of the exported playlist.</param>
		/// <param name="name">The base name of this playlist file.</param>
		/// <param name="createSubdirectories">
		/// A Boolean value indicating whether files in this playlist are located relative
		/// to the root (<b>true</b>) or are all located within the root itself (<b>false</b>).
		/// </param>
		/// <returns>An IPlaylistWriter instance.</returns>

		public static IPlaylistWriter CreateWriter (
			string key, string root, string name, bool createSubdirectories)
		{
			IPlaylistWriter writer = null;

			switch (key)
			{
				case "M3U":
					writer = new M3UPlaylistWriter(root, name, createSubdirectories);
					break;

				case "PLS":
					writer = new PLSPlaylistWriter(root, name, createSubdirectories);
					break;

				case "WPL":
					writer = new WPLPlaylistWriter(root, name, createSubdirectories);
					break;

				case "ZPL":
					writer = new ZPLPlaylistWriter(root, name, createSubdirectories);
					break;
			}

			return writer;
		}
	}
}
