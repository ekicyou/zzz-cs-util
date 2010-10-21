//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner.iTunes
{
	using System;
	using System.Text;

	
	/// <summary>
	/// Abstract base class for all lyrics providers.
	/// </summary>
	/// <remarks>
	/// Consecutive failures are allowed only up to a maximum threshold.  At each successfull
	/// discovery, the failure count is reset to zero.  If the consecutive failures reach
	/// the threshold then this provider is permanently disabled (for this Process session).
	/// </remarks>

	internal abstract class LyricsProviderBase : ILyricsProvider
	{
		private const int MaxFailures = 5;

		protected int failures = 0;
		protected string name;


		/// <summary>
		/// Gets a Boolean value indicating whether this provider has successfully
		/// connected.  This should be checked after each request.  If not connected
		/// then the provide should be ignored.
		/// </summary>

		public bool IsConnected
		{
			get { return failures < MaxFailures; }
		}


		/// <summary>
		/// Gets the name of this lyrics provider.  Inheritors must set the protected
		/// <i>name</i> field in their constructors.
		/// </summary>

		public string Name
		{
			get { return name; }
		}


		/// <summary>
		/// Retrieve the lyrics for the given song
		/// </summary>
		/// <param name="song">The song whose lyrics are to be fetched</param>
		/// <returns>The lyrics or an empty string if the lyrics could not be found</returns>

		public abstract string RetrieveLyrics (ISong song);


		/// <summary>
		/// Clean up the lyrics and encode into Unicode to preserve special characters.
		/// </summary>
		/// <param name="lyrics"></param>
		/// <returns></returns>

		protected string Encode (string text)
		{
			Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
			string s = Encoding.UTF8.GetString(encoding.GetBytes(text));
			s = s.Replace("\r", String.Empty);
			text = s.Replace("\n", Environment.NewLine);
			return text.Trim();
		}
	}
}
