//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner
{
	using System;


	/// <summary>
	/// Interaction logic for ScannerOptions.xaml
	/// </summary>

	internal partial class ScannerOptions : OptionsPanelBase
	{

		public ScannerOptions ()
		{
			this.InitializeComponent();
			this.DataContext = this;
		}


		public bool DuplicateScannerIsEnabled
		{
			get { return GetOption("DuplicateScannerIsEnabled"); }
			set { SetOption("DuplicateScannerIsEnabled", value); }
		}


		public bool FileWatchScannerIsEnabled
		{
			get { return GetOption("FileWatchScannerIsEnabled"); }
			set { SetOption("FileWatchScannerIsEnabled", value); }
		}


		public bool MaintenanceScannerIsEnabled
		{
			get { return GetOption("MaintenanceScannerIsEnabled"); }
			set { SetOption("MaintenanceScannerIsEnabled", value); }
		}


		public bool PhantomScannerIsEnabled
		{
			get { return GetOption("PhantomScannerIsEnabled"); }
			set { SetOption("PhantomScannerIsEnabled", value); }
		}
	}
}