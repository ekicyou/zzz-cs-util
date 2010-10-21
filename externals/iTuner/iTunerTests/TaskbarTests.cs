//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTunerTests
{
	using System;
	using System.IO;
	using System.Windows;
	using WinForms = System.Windows.Forms;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using iTuner;


	/// <summary>
	/// </summary>

	[TestClass]
	public class TaskbarTests : TestBase
	{

		/// <summary>
		/// </summary>

		[TestMethod]
		public void GetTaskbarInfo ()
		{
			WinForms.NotifyIcon icon = new WinForms.NotifyIcon();

			Taskbar bar = new Taskbar();
			Point point = bar.GetTangentPosition(icon);
		}


		[TestMethod]
		public void PathTests ()
		{
			string path = @"C:\Music\AC/DC\Alternating Current\01 Live Wire.mp3";
			string clean;
			
			clean = PathHelper.Clean(path);
			clean = PathHelper.CleanDirectoryPath(Path.GetDirectoryName(path));
			clean = PathHelper.CleanFileName(Path.GetFileNameWithoutExtension(path));
			clean = PathHelper.CleanFileName("AC/DC");
		}
	}
}
