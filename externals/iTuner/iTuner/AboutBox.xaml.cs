//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner
{
	using System;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Media;
	using Resx = Properties.Resources;
	using System.Windows.Documents;


	/// <summary>
	/// Interaction logic for About.xaml
	/// </summary>

	internal partial class AboutBox : FadingWindow
	{

		public AboutBox ()
		{
			this.InitializeComponent();

			titleBlock.Text = App.NameVersion;

			this.mainBorder.Opacity = 0.0;
			this.AnimatedElement = mainBorder;

		}


		/// <summary>
		/// Close the window when the Esc key is pressed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoKeyDown (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				Hide();
			}
		}


		private void DoEnterLink (object sender, MouseEventArgs e)
		{
			Hyperlink link = sender as Hyperlink;
			if (link != null)
			{
				link.Foreground = Brushes.Blue;
			}
		}

		private void DoLeaveLink (object sender, MouseEventArgs e)
		{
			Hyperlink link = sender as Hyperlink;
			if (link != null)
			{
				link.Foreground = Brushes.Black;
			}
		}

		private void DoClickLink (object sender, RoutedEventArgs e)
		{
			Hide();
			NavigateTo(Resx.ApplicationURI);
		}


		/// <summary>
		/// Invoke the Internet browser, opening the PayPal donation page.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoDonate (object sender, RoutedEventArgs e)
		{
			Hide();
			NavigateTo(Resx.PayPalDonateUri);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="uri"></param>

		private void NavigateTo (string uri)
		{
			try
			{
				// start with default browser
				System.Diagnostics.Process.Start(uri);
			}
			catch (System.ComponentModel.Win32Exception exc)
			{
				if (exc.ErrorCode == -2147467259)
				{
					MessageWindow.Show(
						Resx.NoBrowserMessage, Resx.NoBrowserCaption,
						MessageBoxButton.OK, MessageWindowImage.Error);
				}
			}
			catch
			{
				// no-op
			}
		}


		private void DoClickUpgrade (object sender, RoutedEventArgs e)
		{
			UpgradeHelper.CheckUpgrades(this.Dispatcher, true);
		}

		/// <summary>
		/// The OK button provides a way for the user to quickly dismiss the About box 
		/// without having to wait for it to fade.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoOK (object sender, RoutedEventArgs e)
		{
			Hide();
		}
	}
}