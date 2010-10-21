//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTunerTests
{
	using System;
	using System.Linq;
	using System.Xml.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using iTuner.iTunes;


	/// <summary>
	/// </summary>

	[DeploymentItem(@"ThirdParty\", @"ThirdParty")]
	[TestClass]
	public class TaggerTests : TestBase
	{

		/// <summary>
		/// </summary>

		[TestMethod]
		public void RetrieveEmbeddedTags ()
		{
			Tagger tagger = new Tagger();
			Track track = new Track(null);
			track.Location = @"C:\iTuner\Research\genpuid\09-Little Martha.mp3";
			track.IsBuffered = true;

			tagger.RetrieveTags(track);

			Assert.IsNotNull(track);
			Assert.IsNotNull(track.UniqueID);
		}


		[TestMethod]
		public void LinqAll ()
		{
			string xml = @"
 <genpuid songs=""1"" xmlns:mip=""http://musicip.com/ns/mip-1.0#"">
   <track file=""C:\iTuner\Research\genpuid\09-Little Martha.mp3""
 	     puid=""019e0c33-0450-cdf8-211c-1ba69d6f935b"">
     <title>Little Martha</title>
     <artist>
       <name>Allman Brothers Band</name>
     </artist>
     <puid-list>
       <puid id=""019e0c33-0450-cdf8-211c-1ba69d6f935b""/>
     </puid-list>
     <mip:first-release-date>1972</mip:first-release-date>
     <mip:genre-list>
       <mip:genre>
         <name>Rock</name>
       </mip:genre>
     </mip:genre-list>
   </track>
 </genpuid>";

			Track tags = null;

			XElement root = XElement.Parse(xml, LoadOptions.None);
			XNamespace ns = root.GetDefaultNamespace();
			XNamespace mip = root.GetNamespaceOfPrefix("mip");

			var track = root.Element(ns + "track");
			if (track != null)
			{
				XAttribute attribute = track.Attribute(ns + "puid");
				if (attribute != null)
				{
					tags = new Track(null);
					tags.UniqueID = attribute.Value;

					XElement element = track.Element(ns + "artist");
					if (element != null)
					{
						element = element.Element(ns + "name");
						if (element != null)
						{
							tags.Artist = element.Value;
						}
					}

					element = track.Element(ns + "title");
					if (element != null)
					{
						tags.Title = element.Value;
					}

					element = track.Element(mip + "first-release-date");
					if (element != null)
					{
						int year;
						if (Int32.TryParse(element.Value, out year))
						{
							tags.Year = year;
						}
					}

					element = track.Element(mip + "genre-list");
					if (element != null)
					{
						element = element.Descendants(ns + "name").Single();
						if (element != null)
						{
							tags.Genre = element.Value;
						}
					}
				}
			}
		}
	}

}
