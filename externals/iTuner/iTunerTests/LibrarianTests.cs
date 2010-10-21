//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTunerTests
{
	using System;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using iTuner.iTunes;


	/// <summary>
	/// </summary>

	[TestClass]
	public class LibrarianTests : TestBase
	{

		/// <summary>
		/// </summary>

		[TestMethod]
		public void CreateLibrary ()
		{
			Controller controller = new Controller();
			Librarian librarian = Librarian.Create(controller);
		}
	}

}
