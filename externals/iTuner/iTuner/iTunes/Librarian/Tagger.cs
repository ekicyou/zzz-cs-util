//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner.iTunes
{
	using System;
	using System.Configuration;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Threading;
	using System.Xml.Linq;


	/// <summary>
	/// Combines MusicDNS genpuid with MusicBrainz Webservice to retrieve extract information
	/// regarding a specific media file.
	/// </summary>
	/// <remarks>
	/// MusicBrainz limits clients requests to no more than one per second.  So we govern
	/// this internally by enforcing up to a one second wait in between retrievals by
	/// blocking for the remaining time.  However, the probability of actually falling into
	/// this is low considering that genpuid usually takes longer than a second anyway.
	/// </remarks>

	internal class Tagger
	{
		private const string LogCategory = "Tagger";

		private const int GenTimeout = 1000 * 60;
		private const int MinWaitTime = 1000;

		private const string BrainzTrackUri = "http://musicbrainz.org/ws/1/track/?type=xml&puid={0}";
		private const string BrainzReleaseUri = "http://musicbrainz.org/ws/1/release/{0}/?type=xml&inc=counts+tracks+tags";
		private static string DefaultDnsKey = "e4230822bede81ef71cde723db743e27";

		private DateTime lastdttm;


		//========================================================================================
		// Constructor
		//========================================================================================

		/// <summary>
		/// Initialize the class, overriding default settings with user configuration.
		/// </summary>

		static Tagger ()
		{
			string key = ConfigurationManager.AppSettings["DnsKey"];
			if (key != null)
			{
				key = key.Trim();
				try
				{
					// test if this is a proper guid
					Guid guid = new Guid(key);
					// passed the test so set the default
					DefaultDnsKey = key;
				}
				catch
				{
					// no-op
				}
			}
		}


		/// <summary>
		/// Initialize a new instance.
		/// </summary>

		public Tagger ()
		{
			lastdttm = DateTime.MinValue;
		}


		//========================================================================================
		// Methods
		//========================================================================================

		/// <summary>
		/// Retrieve the best possible tag information from a series of providers including
		/// genpuid MusicDns and MusicBrainz.
		/// </summary>
		/// <param name="path">Full path of the media file to analyze.</param>
		/// <returns>
		/// A new Tags object containing meta data for the media file.
		/// If metadata could not be found, the Tags.IsPopulated property is <b>false</b>.
		/// </returns>

		public void RetrieveTags (Track track)
		{
			if (!File.Exists(track.Location))
			{
				track.UniqueID = null;
				return;
			}

			if (!NetworkStatus.IsAvailable)
			{
				return;
			}

			// Enforce MusicBrainz min wait threshold of one second.
			// The probability of actually falling into this is low considering that
			// genpuid usually takes longer than a second anyway...
			DateTime now = DateTime.Now;
			TimeSpan span = now.Subtract(lastdttm);
			if (span.TotalMilliseconds < MinWaitTime)
			{
				Thread.Sleep(MinWaitTime - (int)span.TotalMilliseconds);
			}

			lastdttm = DateTime.Now;

			// genpuid can retrieve basic tag information
			RetrieveGenpuidTags(track);

			// we have the basics, now override with more complete information
			if (track.IsAnalyzed)
			{
				RetrieveBrainzTags(track);
			}
		}


		#region MusicDNS genpuid

		/// <summary>
		/// Invoke the genpuid utility in a subprocess with the given music file path.
		/// </summary>
		/// <param name="path">The full path of the music file to analyze.</param>
		/// <returns>The analyzed Track including the PUID and any available metadata tags.</returns>

		private void RetrieveGenpuidTags (Track track)
		{
			ProcessStartInfo info = new ProcessStartInfo();
			info.Arguments = String.Format(@"{0} -xml -rmd=2 ""{1}""", DefaultDnsKey, track.Location);
			info.CreateNoWindow = true;
			info.FileName = @"ThirdParty\genpuid.exe";
			info.RedirectStandardOutput = true;
			info.UseShellExecute = false;

			string xml = null;

			try
			{
				using (Process process = new Process())
				{
					process.StartInfo = info;
					process.Start();

					// PriorityClass must be set after process is started.
					process.PriorityClass = ProcessPriorityClass.BelowNormal;

					// read before waiting
					xml = Encoding.UTF8.GetString(
						process.StandardOutput.CurrentEncoding.GetBytes(
						process.StandardOutput.ReadToEnd()));

					if (process.ExitCode != 0)
					{
						string error = process.StandardError.ReadToEnd();
					}

					// ensures process terminates
					bool ok = process.WaitForExit(GenTimeout);
				}
			}
			catch (Exception exc)
			{
				Logger.WriteLine(LogCategory, "Error analyzing " + track.Location, exc);
				return;
			}

			if (!String.IsNullOrEmpty(xml))
			{
				ExtractGenpuidTags(xml, track);
			}
		}


		/// <summary>
		/// Extract metadata from the genpuid generated XML report.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns>A Tags instance populated with extracted tag information.</returns>

		private void ExtractGenpuidTags (string xml, Track track)
		{
			XElement root;

			try
			{
				root = XElement.Parse(xml, LoadOptions.None);
			}
			catch (Exception exc)
			{
				Logger.WriteLine(LogCategory, "Error parsing analysis", exc);
				Logger.WriteLine(Logger.Level.Error, LogCategory, xml);
				return;
			}

			XNamespace ns = root.GetDefaultNamespace();

			var node = root.Element(ns + "track");
			if (node != null)
			{
				XAttribute attribute = node.Attribute(ns + "puid");
				if (attribute != null)
				{
					// instantiate a new Tags with preferred PUID
					track.UniqueID = attribute.Value;

					// extract Artist if specified
					XElement element = node.Element(ns + "artist");
					if (element != null)
					{
						element = element.Element(ns + "name");
						if (element != null)
						{
							track.Artist = element.Value;
						}
					}

					// extract Title if specified
					element = node.Element(ns + "title");
					if (element != null)
					{
						track.Title = element.Value;
					}

					// mip namespace is included when using the -rmd=2 option

					XNamespace mip = root.GetNamespaceOfPrefix("mip");
					if (mip != null)
					{
						// extract Year if specified
						element = node.Element(mip + "first-release-date");
						if (element != null)
						{
							int year;
							if (Int32.TryParse(element.Value, out year))
							{
								track.Year = year;
							}
						}

						// extract Genre if specified
						element = node.Element(mip + "genre-list");
						if (element != null)
						{
							element = element.Descendants(ns + "name").Single();
							if (element != null)
							{
								track.Genre = element.Value;
							}
						}
					}
				}
			}
		}

		#endregion MusicDNS genpuid

		#region MusicBrainz

		private void RetrieveBrainzTags (Track track)
		{
			string uri = String.Format(BrainzTrackUri, track.UniqueID);
			string xml;

			using (WebClient client = new WebClient())
			{
				try
				{
					xml = client.DownloadString(uri);
				}
				catch (Exception exc)
				{
					Logger.WriteLine(LogCategory, "Exception " + exc.Message, exc);
					xml = null;
				}
			}

			if (!String.IsNullOrEmpty(xml))
			{
				XElement root;

				try
				{
					root = XElement.Parse(xml, LoadOptions.None);
				}
				catch (Exception exc)
				{
					Logger.WriteLine(LogCategory, "Error parsing analysis", exc);
					Logger.WriteLine(Logger.Level.Error, LogCategory, xml);
					return;
				}

				XNamespace ns = root.GetDefaultNamespace();

				var tracklist = root.Element(ns + "track-list");
				if (tracklist != null)
				{
					// if there is one track then we can extract the album otherwise
					// overwrite the album name only if we don't already have one

					int count = tracklist.Elements().Count();
					XElement node = tracklist.Element(ns + "track");

					var taglist =
						from a in root.Elements(ns + "track-list")
						from b in a.Elements(ns + "track")
						select new
						{
							Title = b.Element(ns + "title").Value,
							Artist = b.Elements(ns + "artist").Elements(ns + "name").FirstOrDefault().Value,
							Album = b.Elements(ns + "release-list").Elements(ns + "release").Elements(ns + "title").FirstOrDefault().Value
						};

					var tags = taglist.First();

					if (count == 1)
					{
						track.Album = tags.Album;
						track.Artist = tags.Artist;
						track.Title = tags.Title;						
					}
					else if (count > 1)
					{
						if (String.IsNullOrEmpty(track.Title))
						{
							track.Title = tags.Title;
						}

						if (String.IsNullOrEmpty(track.Artist))
						{
							track.Artist = tags.Artist;
						}

						if (String.IsNullOrEmpty(track.Album))
						{
							track.Album = tags.Album;
						}
					}
				}
			}
		}

		/*
		  <metadata xmlns="http://musicbrainz.org/ns/mmd-1.0#">
			<track-list>
			  <track id="c40615ab-2a8d-4223-baa5-69dfedc0fa4e">
				<title>It's Alright, Ma (I'm Only Bleeding)</title>
				<duration>452093</duration>
				<artist id="72c536dc-7137-4477-a521-567eeb840fa8">
				  <name>Bob Dylan</name>
				  <sort-name>Dylan, Bob</sort-name>
				</artist>
				<release-list>
				  <release id="67e7ab93-d5c1-442f-b8c0-dbfad52e2d34">
					<title>Bringing It All Back Home</title>
					<track-list offset="9"/>
				  </release>
				</release-list>
			  </track>
			  <track id="25128209-e815-48dd-b6ba-e8407a3aff0f">
				<title>It's Alright, Ma (I'm Only Bleeding)</title>
				<duration>452093</duration>
				<artist id="72c536dc-7137-4477-a521-567eeb840fa8">
				  <name>Bob Dylan</name>
				  <sort-name>Dylan, Bob</sort-name>
				</artist>
				<release-list>
				  <release id="ca754c1d-8254-4516-84b6-cb91559196e7">
					<title>Subterranean Homesick Blues</title>
					<track-list offset="9"/>
				  </release>
				</release-list>
			  </track>
			</track-list>
		  </metadata>
		 */

		#endregion MusicBrainz
	}
}
