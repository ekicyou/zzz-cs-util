//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Input;
	using iTuner.iTunes;
	using Resx = Properties.Resources;
	using WinForms = System.Windows.Forms;


	/// <summary>
	/// Interaction logic for AppWindow.xaml
	/// </summary>

	internal partial class AppWindow : FadingWindow, IDisposable
	{
		private class MenuMap : Dictionary<string, IconMenuItem> { }

		private WinForms.NotifyIcon trayIcon;
		private WinForms.Timer trayIconTimer;
		private int trayIconClickCount;

		private AboutBox aboutBox;
		private TrackerWindow tracker;
		private Controller controller;
		private Librarian librarian;
		private KeyManager manager;
		private SplashWindow splash = null;
		private bool isDisposed;
		private object syncdis;

		private ExportDialog exportDialog = null;
		private SynchronizeDialog syncDialog = null;


		//========================================================================================
		// Constructors
		//========================================================================================

		/// <summary>
		/// Initialize a new instance including the internal iTunes controller and
		/// hot key manager.
		/// </summary>

		public AppWindow ()
			: base()
		{
			InitializeComponent();

			// since this window instantiates other windows, we must set the shutdown mode
			// otherwise the application will hang and not shutdown properly
			Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

			// FadingWindow initialization
			this.mainBorder.Opacity = 0.0;
			this.AnimatedElement = mainBorder;
			this.Visibility = Visibility.Hidden;

			if (DesignerProperties.GetIsInDesignMode(this))
			{
				// if in VS or Blend designers do not start loading window or iTunes
				return;
			}

			trayIcon = new WinForms.NotifyIcon();
			trayIcon.Text = Resx.ApplicationTitle;
			trayIcon.ContextMenu = CreateContextMenu();
			trayIcon.MouseDown += new WinForms.MouseEventHandler(DoTrayIconMouseDown);
			trayIcon.Visible = true;

			if (Controller.IsHostRunning)
			{
				// since iTunes is running, we can instantiate its controller very quickly
				// without blocking the UI thread, so do it explicitly here

				DoLoadingWork(null, null);
				DoLoadingCompleted(null, null);
			}
			else
			{
				// by instantiating iTunes in a background thread, we are able to show animated
				// progress in SplashWindow; otherwise, creating a new Controller blocks
				// the UI thread

				splash = new SplashWindow();
				splash.Show();

				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += new DoWorkEventHandler(DoLoadingWork);
				worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DoLoadingCompleted);
				worker.RunWorkerAsync();
			}
		}


		#region Lifecycle

		/// <summary>
		/// If iTunes is not currently running then we start it in a background thread
		/// so we don't block the UI thread.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoLoadingWork (object sender, DoWorkEventArgs e)
		{
			try
			{
				controller = new Controller();
				librarian = controller.Librarian;
			}
			catch (IncompatibleException exc)
			{
				MessageBox.Show(exc.Message, "Error starting iTuner",
					MessageBoxButton.OK, MessageBoxImage.Error);

				controller = null;
				throw;
			}
		}


		/// <summary>
		/// Now that iTunes is started, we can continue initializing the iTuner UI
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoLoadingCompleted (object sender, RunWorkerCompletedEventArgs e)
		{
			if (controller == null)
			{
				this.Close();
				return;
			}

			controller.LyricsProgressReport += new LyricEngineProgress(DoLyricsProgressReport);
			controller.LyricsUpdated += new TrackHandler(DoLyricsUpdated);
			controller.Quiting += new EventHandler(DoQuiting);
			controller.TrackPlaying += new TrackHandler(DoTrackPlaying);
			controller.TrackStopped += new TrackHandler(DoTrackStopped);

			librarian.CollectionChanged +=
				new NotifyCollectionChangedEventHandler(DoTaskCollectionChanged);

			if (splash != null)
			{
				splash.Hide();
				splash = null;
			}

			DataContext = controller;

			manager = new KeyManager();
			manager.KeyPressed += new HotKeyHandler(ExecuteHotKeyAction);

			trayIconTimer = new WinForms.Timer();
			trayIconTimer.Interval = WinForms.SystemInformation.DoubleClickTime;
			trayIconTimer.Tick += new EventHandler(DoTrayIconTick);
			trayIconClickCount = 0;

			tracker = new TrackerWindow(this.controller);

			EventManager.RegisterClassHandler(
			typeof(EditBlock), EditBlock.BeginEditEvent,
			new RoutedEventHandler(DoBeginEdit));

			EventManager.RegisterClassHandler(
				typeof(EditBlock), EditBlock.CompleteEditEvent,
				new RoutedEventHandler(DoCompleteEdit));

			this.isDisposed = false;
			this.syncdis = new object();

			SetNotifyIcon(controller.CurrentTrack);
		}


		/// <summary>
		/// Dispose of all resources in an orderly fashion.
		/// </summary>

		public override void Dispose ()
		{
			if (aboutBox != null)
			{
				aboutBox.Dispose();
				aboutBox = null;
			}

			if (tracker != null)
			{
				tracker.Dispose();
				tracker = null;
			}

			if (librarian != null)
			{
				librarian.CollectionChanged -=
					new NotifyCollectionChangedEventHandler(DoTaskCollectionChanged);

				librarian.Dispose();
				librarian = null;
			}

			if (controller != null)
			{
				controller.LyricsProgressReport -= new LyricEngineProgress(DoLyricsProgressReport);
				controller.LyricsUpdated -= new TrackHandler(DoLyricsUpdated);
				controller.Quiting -= new EventHandler(DoQuiting);
				controller.TrackPlaying -= new TrackHandler(DoTrackPlaying);
				controller.TrackStopped -= new TrackHandler(DoTrackStopped);

				// if running from IDE force iTunes to shutdown, force COM detach. Poking around
				// in the debugger tends to destabalize the COM interface so we shut it down when
				// invoked from the Debugger to avoid any confusion.
				controller.Dispose(System.Diagnostics.Debugger.IsAttached);

				controller = null;
			}

			if (manager != null)
			{
				manager.KeyPressed -= new HotKeyHandler(ExecuteHotKeyAction);
				manager.Dispose();
				manager = null;
			}

			if (trayIcon != null)
			{
				trayIcon.ContextMenu.Popup -= new EventHandler(SetMenuItemStates);
				trayIcon.ContextMenu.Dispose();
				trayIcon.ContextMenu = null;

				trayIcon.MouseDown -= new WinForms.MouseEventHandler(DoTrayIconMouseDown);
				trayIcon.Dispose();
				trayIcon = null;
			}

			base.Dispose();
			isDisposed = true;

			GC.SuppressFinalize(this);
		}

		#endregion Lifecycle

		#region NotifyIcon management

		private void DoTrayIconTick (object sender, EventArgs e)
		{
			trayIconTimer.Stop();
			trayIconClickCount = 0;

			// perform single-click action
			ShowAppWindow();
		}

		private void DoTrayIconMouseDown (object sender, WinForms.MouseEventArgs e)
		{
			if (e.Button == WinForms.MouseButtons.Left)
			{
				trayIconClickCount++;

				if (trayIconClickCount > 1)
				{
					trayIconTimer.Stop();
					trayIconClickCount = 0;

					// perform double-click action
					DoPlayPause(sender, e);
				}
				else
				{
					trayIconTimer.Start();
				}
			}
		}

		#endregion NotifyIcon management

		#region Context menu management

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>

		private WinForms.ContextMenu CreateContextMenu ()
		{
			WinForms.ContextMenu menu = new WinForms.ContextMenu();
			menu.Tag = new MenuMap();

			menu.MenuItems.Add(new IconMenuItem(
				Resx.iTunes, Resx.ActionShowiTunes, new EventHandler(DoShowiTunes)));

			menu.MenuItems.Add(new IconMenuItem(
				Resx.iTuner, Resx.ActionShowiTuner, new EventHandler(DoShowiTuner)));

			menu.MenuItems.Add(new IconMenuItem("-"));

			menu.MenuItems.Add(new IconMenuItem(
				Resx.Library, Resx.MenuClean, new IconMenuItem[]
				{
					MakeMenuItem("cleanthis", menu.Tag,
						Resx.Library, Resx.MenuCleanAlbum, new EventHandler(DoCleanContext)),

					new IconMenuItem("-"),

					MakeMenuItem("scandead", menu.Tag,
						Resx.Phantom, Resx.MenuCleanPhantoms, new EventHandler(DoPhantomScanner)),

					MakeMenuItem("scandup", menu.Tag,
						Resx.Duplicates, Resx.MenuCleanDuplicates, new EventHandler(DoDuplicateScanner)),

					MakeMenuItem("scanempty", menu.Tag,
						Resx.EmptyDirectory, Resx.MenuCleanEmpty, new EventHandler(DoEmptyScanner))
				}));

			menu.MenuItems.Add(new IconMenuItem(
				Resx.Export, Resx.MenuExport, new IconMenuItem[]
				{
					MakeMenuItem("exalbum", menu.Tag,
						Resx.Album, Resx.MenuExportAlbum, new EventHandler(DoExportAlbum)),

					MakeMenuItem("exartist", menu.Tag,
						Resx.Artist, Resx.MenuExportArtist, new EventHandler(DoExportArtist)),

					MakeMenuItem("explaylist", menu.Tag,
						Resx.Playlist, Resx.MenuExportPlaylist, new EventHandler(DoExportPlaylist))
				}));

			menu.MenuItems.Add(MakeMenuItem("sync", menu.Tag,
				Resx.Sync, Resx.MenuSync, new EventHandler(DoSync)));

			menu.MenuItems.Add(new IconMenuItem("-"));

			menu.MenuItems.Add(MakeMenuItem("mute", menu.Tag,
				Resx.Mute, Resx.ActionMute, new EventHandler(DoToggleMute)));

			menu.MenuItems.Add(new IconMenuItem(
				Resx.PrevTrack, Resx.MenuPrevTrack, new EventHandler(DoPrevTrack)));

			menu.MenuItems.Add(new IconMenuItem(
				Resx.NextTrack, Resx.MenuNextTrack, new EventHandler(DoNextTrack)));

			menu.MenuItems.Add(MakeMenuItem("play", menu.Tag,
				Resx.Play, Resx.ActionPlayPause, new EventHandler(DoPlayPause)));

			menu.MenuItems.Add(MakeMenuItem("shuffle", menu.Tag,
				Resx.Shuffle, Resx.MenuShuffle, new EventHandler(DoToggleShuffle)));

			menu.MenuItems.Add(new IconMenuItem("-"));
			menu.MenuItems.Add(new IconMenuItem(Resx.MenuAbout, new EventHandler(DoAbout)));
			menu.MenuItems.Add(new IconMenuItem(Resx.MenuOptions, new EventHandler(DoOptions)));
			menu.MenuItems.Add(new IconMenuItem(Resx.MenuExitiTunes, new EventHandler(DoQuitingAll)));
			menu.MenuItems.Add(new IconMenuItem(Resx.MenuExit, new EventHandler(DoQuiting)));


			menu.Popup += new EventHandler(SetMenuItemStates);
			return menu;
		}


		/// <summary>
		/// Construct a new IconMenuItem and add it to the MenuMap attached to the Context
		/// Menu.  This menu map provides a quick and easy way to lookup and configure
		/// menu items in SetMenuitemStates. 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="tag"></param>
		/// <param name="icon"></param>
		/// <param name="text"></param>
		/// <param name="handler"></param>
		/// <returns></returns>

		private IconMenuItem MakeMenuItem (
			string key, object tag, System.Drawing.Icon icon, string text, EventHandler handler)
		{
			IconMenuItem item = new IconMenuItem(icon, text, handler);
			item.Name = key;

			((MenuMap)tag).Add(key, item);

			return item;
		}


		/// <summary>
		/// Set the menu item states just prior to displaying the context menu.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SetMenuItemStates (object sender, EventArgs e)
		{
			WinForms.ContextMenu menu = sender as WinForms.ContextMenu;
			MenuMap map = (MenuMap)menu.Tag;

			// main context menu items
			map["mute"].Checked = controller.IsMuted;
			map["shuffle"].Checked = controller.Shuffle;
			map["play"].Icon = (controller.IsPlaying ? Resx.Pause : Resx.Play);

			// library menu items

			if (librarian.IsActive(Resx.I_ScanContextDuplicates) ||
				librarian.IsActive(Resx.I_ScanContextPhantoms) ||
				(controller.CurrentContext == Controller.PlayerContext.None))
			{
				map["cleanthis"].Enabled = false;
			}
			else
			{
				map["cleanthis"].Enabled = true;
				if (controller.IsMusicalPlaylist)
				{
					map["cleanthis"].Text = Resx.MenuCleanAlbum;
				}
				else
				{
					map["cleanthis"].Text = Resx.MenuCleanPlaylist;
				}
			}

			map["scandead"].Enabled = !librarian.IsActive(Resx.I_ScanPhantoms);
			map["scandup"].Enabled = !librarian.IsActive(Resx.I_ScanDuplicates);
			map["scanempty"].Enabled = !librarian.IsActive(Resx.I_ScanEmptyDirectories);

			bool isMusic = (controller != null)
				&& controller.IsMusicalPlaylist && (controller.CurrentTrack != null)
				&& (controller.Librarian != null) && (controller.Librarian.Catalog != null);

			bool hasPlaylist = (controller.CurrentPlaylist != null);
			bool noDialogs = (syncDialog == null) && (exportDialog == null);
			bool notExporting = !librarian.IsActive(Resx.I_ScanExport);

			// export menu items

			map["exalbum"].Enabled = isMusic && noDialogs && notExporting;
			map["exartist"].Enabled = isMusic && noDialogs && notExporting;
			map["explaylist"].Enabled = isMusic && hasPlaylist && noDialogs && notExporting;

			// synchronize item

			map["sync"].Enabled = isMusic && noDialogs;
		}

		#endregion Context menu management


		//========================================================================================
		// Handlers
		//========================================================================================

		#region Commands and Handlers

		private void ExecuteHotKeyAction (IHotKey key)
		{
			switch (key.Action)
			{
				case HotKeyAction.PlayPause:
					DoPlayPause(null, null);
					break;

				case HotKeyAction.NextTrack:
					DoNextTrack(null, null);
					break;

				case HotKeyAction.PrevTrack:
					DoPrevTrack(null, null);
					break;

				case HotKeyAction.VolumeUp:
					DoVolumeUp(null, null);
					break;

				case HotKeyAction.VolumeDown:
					DoVolumeDown(null, null);
					break;

				case HotKeyAction.Mute:
					DoToggleMute(null, null);
					break;

				case HotKeyAction.ShowiTunes:
					DoShowiTunes(null, (EventArgs)null);
					break;

				case HotKeyAction.ShowiTuner:
					DoShowiTuner(null, null);
					break;

				case HotKeyAction.ShowLyrics:
					DoShowLyrics(null, null);
					break;
			}
		}


		private void DoAbout (object sender, EventArgs e)
		{
			if (this.IsOpaque && !this.IsPinned)
			{
				this.Hide();
			}

			if (aboutBox == null)
			{
				aboutBox = new AboutBox();
			}

			Taskbar taskbar = new Taskbar();
			Point point = taskbar.GetTangentPosition(trayIcon);
			aboutBox.SetPositionRelateiveTo(point, taskbar.Docking);
			aboutBox.Show();
		}


		/// <summary>
		/// Handler for the EditBlock BeginEdit event, pins the current window so it
		/// cannot fade out while editing.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoBeginEdit (object sender, RoutedEventArgs e)
		{
			IsPinned = true;
		}


		/// <summary>
		/// Handler for the EditBlock CompleteEdit event, unpins the current window so
		/// it can fade out.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoCompleteEdit (object sender, RoutedEventArgs e)
		{
			IsPinned = false;
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


		private void DoLyricsProgressReport (ISong song, int stage)
		{
			if (!this.Dispatcher.CheckAccess())
			{
				this.Dispatcher.BeginInvoke((Action)delegate
				{
					DoLyricsProgressReport(song, stage);
				});

				return;
			}

			ITrack track = controller.CurrentTrack;
			if (track != null)
			{
				if (track.GetHashCode() == song.GetHashCode())
				{
					if ((stage >= 0) && (stage <= 5))
					{
						taskPanel.LyricsState = stage.ToString();
					}
					else
					{
						taskPanel.LyricsState = String.Empty;
					}
				}
			}
		}


		private void DoLyricsUpdated (ITrack song)
		{
			if (!this.Dispatcher.CheckAccess())
			{
				this.Dispatcher.BeginInvoke((Action)delegate
				{
					DoLyricsUpdated(song);
				});

				return;
			}

			ITrack track = controller.CurrentTrack;
			if (track != null)
			{
				bool hasLyrics = !String.IsNullOrEmpty(track.Lyrics);
				taskPanel.LyricsState = (hasLyrics ? String.Empty : "0");
			}
			else
			{
				taskPanel.LyricsState = "0";
			}
		}


		private void DoOptions (object sender, EventArgs e)
		{
			OptionsDialog dialog = new OptionsDialog();
			dialog.ShowDialog();
		}


		private void DoPlayPause (object sender, EventArgs e)
		{
			controller.TogglePlayPause();
		}


		private void DoNextTrack (object sender, EventArgs e)
		{
			controller.NextTrack();
		}


		private void DoPrevTrack (object sender, EventArgs e)
		{
			controller.PreviousTrack();
		}


		private void DoShowiTunes (object sender, EventArgs e)
		{
			controller.ShowiTunes();
		}


		private void DoShowiTuner (object sender, EventArgs e)
		{
			ShowAppWindow();
		}


		private void DoShowLyrics (object sender, RoutedEventArgs e)
		{
			ITrack track = controller.CurrentTrack;
			if (track != null)
			{
				string lyrics = track.Lyrics;
				if (!String.IsNullOrEmpty(lyrics))
				{
					string path = System.IO.Path.GetTempFileName();
					System.IO.File.WriteAllText(path, track.MakeLyricReportHeader() + lyrics);
					System.Diagnostics.Process.Start("Notepad.exe", path);
				}
			}
		}


		private void DoToggleMute (object sender, EventArgs e)
		{
			controller.ToggleMute();
			controlPanel.IsMuted = controller.IsMuted;
		}


		private void DoToggleShuffle (object sender, EventArgs e)
		{
			Logger.WriteLine(Logger.Level.Debug, "App", "DoToggleShuffle");
			controller.Shuffle = !controller.Shuffle;
		}


		private void DoTrackPlaying (ITrack track)
		{
			if (!tracker.Dispatcher.CheckAccess())
			{
				//Logger.WriteLine(Logger.Level.Debug, "DEBUG", "DoTrackPlaying.BeginInvoke");
				tracker.Dispatcher.BeginInvoke((Action)delegate
				{
					DoTrackPlaying(track);
				});

				return;
			}

			//Logger.WriteLine(Logger.Level.Debug, "DEBUG", "DoTrackPlaying");

			// TODO: why do we need to do this?  the 'track' parameter is not alway populated
			// for some reason.  Is this when we BeginInvoke?  Why?
			track = controller.CurrentTrack;

			//Logger.WriteLine(Logger.Level.Debug, "DEBUG", String.Format("track is null? ({0})", track == null ? "Yes" : "No"));

			if ((track == null) || String.IsNullOrEmpty(track.Lyrics))
			{
				taskPanel.LyricsState = "0";
			}
			else
			{
				taskPanel.LyricsState = String.Empty;
			}

			SetNotifyIcon(track);

			// if already showing main app window, don't need to show tracker
			if (this.IsOpaque)
			{
				return;
			}

			Taskbar taskbar = new Taskbar();
			Point point = taskbar.GetTangentPosition(trayIcon);
			tracker.SetPositionRelateiveTo(point, taskbar.Docking);
			tracker.Show();
		}


		private void DoTrackStopped (ITrack track)
		{
			SetNotifyIcon(track);
		}


		private void DoVolumeUp (object sender, EventArgs e)
		{
			controller.VolumeUp();
		}


		private void DoVolumeDown (object sender, EventArgs e)
		{
			controller.VolumeDown();
		}


		private void DoQuiting (object sender, EventArgs e)
		{
			lock (syncdis)
			{
				if (!isDisposed)
				{
					Logger.WriteLine(Resx.ApplicationProduct, "Quiting");

					trayIcon.Visible = false;
					this.Close();
					this.Dispose();
				}
			}
		}


		private void DoQuitingAll (object sender, EventArgs e)
		{
			controller.Dispose(true);
			DoQuiting(sender, e);
		}

		#endregion Commands and Handlers

		#region Export Handlers

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoExportAlbum (object sender, EventArgs e)
		{
			string album = controller.CurrentTrack.Album;
			string artist = controller.CurrentTrack.Artist;

			PersistentIDCollection list = librarian.Catalog.FindTracksByAlbum(album, artist);
			list.Album = album;
			list.Artist = artist;
			list.Name = album;

			DoExport(list);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoExportArtist (object sender, EventArgs e)
		{
			string artist = controller.CurrentTrack.Artist;

			PersistentIDCollection list = librarian.Catalog.FindTracksByArtist(artist);
			list.Artist = artist;
			list.Name = artist;

			DoExport(list);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoExportPlaylist (object sender, EventArgs e)
		{
			Playlist playlist = controller.CurrentPlaylist;

			PersistentIDCollection list =
				librarian.Catalog.FindTracksByPlaylist(playlist.PersistentID);

			list.Name = controller.CurrentPlaylist.Name;

			DoExport(list);

			list.Clear();
			list = null;

			playlist.Dispose();
			playlist = null;
		}


		private void DoExport (PersistentIDCollection list)
		{
			if (this.IsOpaque && !this.IsPinned)
			{
				this.Hide();
			}

			try
			{
				exportDialog = new ExportDialog(controller, list);
				exportDialog.ShowDialog();
			}
			finally
			{
				exportDialog = null;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoSync (object sender, EventArgs e)
		{
			if (this.IsOpaque && !this.IsPinned)
			{
				this.Hide();
			}

			syncDialog = new SynchronizeDialog(controller);
			syncDialog.ShowDialog();

			syncDialog = null;
		}

		#endregion Export Handlers

		#region Scanner Handlers

		private void DoCleanContext (object sender, EventArgs e)
		{
			librarian.CleanContext(controller.IsMusicalPlaylist);
		}


		private void DoEmptyScanner (object sender, EventArgs e)
		{
			librarian.ScanEmptyFolders();
		}


		private void DoDuplicateScanner (object sender, EventArgs e)
		{
			librarian.ScanDuplicates();
		}


		private void DoPhantomScanner (object sender, EventArgs e)
		{
			librarian.ScanPhantomTracks();
		}

		#endregion Scanner Handlers


		//========================================================================================
		// Methods
		//========================================================================================

		#region Window Control Methods

		/// <summary>
		/// Ensures that the play panel is displayed the next time the window is shown.
		/// </summary>

		protected override void OnHideCompleted ()
		{
			trackPanel.ResetView();
			taskPanel.ResetView();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ShowAppWindow ()
		{
			if ((tracker != null) && tracker.IsOpaque)
			{
				tracker.Hide();
			}

			if ((aboutBox != null) && aboutBox.IsOpaque)
			{
				aboutBox.Hide();
			}

			// there is no event to indicate when Shuffled changes so we need to peek
			taskPanel.IsShuffled = controller.Shuffle;

			Taskbar taskbar = new Taskbar();
			Point point = taskbar.GetTangentPosition(trayIcon);
			base.SetPositionRelateiveTo(point, taskbar.Docking);

			Show();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ShowKeyEditor (object sender, RoutedEventArgs e)
		{
			HotKeyEditor editor = new HotKeyEditor(manager);

			Taskbar taskbar = new Taskbar();
			switch (taskbar.Docking)
			{
				case Taskbar.Dock.Bottom:
				case Taskbar.Dock.Right:
					editor.Top = this.Top - (editor.Height * 0.50);
					editor.Left = this.Left - (editor.Width * 0.60);
					break;

				case Taskbar.Dock.Top:
					editor.Top = this.Top + (this.Height * 0.50);
					editor.Left = this.Left - (editor.Width * 0.80);
					break;

				case Taskbar.Dock.Left:
					editor.Top = this.Top - (editor.Height * 0.50);
					editor.Left = this.Left + (this.Width * 0.30);
					break;
			}

			this.IsPinned = true;
			manager.IsEnabled = false;

			editor.ShowDialog();

			editor.Dispose();
			editor = null;

			this.IsPinned = false;
			manager.IsEnabled = true;

			taskPanel.ResetEditKeys();
		}


		/// <summary>
		/// When a task is added to or removed from the task list, we want to update
		/// the notify icon to indicate whether a task is running.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DoTaskCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!this.Dispatcher.CheckAccess())
			{
				this.Dispatcher.BeginInvoke((Action)delegate
				{
					DoTaskCollectionChanged(sender, e);
				});

				return;
			}

			SetNotifyIcon(controller.CurrentTrack);
		}


		/// <summary>
		/// 
		/// </summary>

		private void SetNotifyIcon (ITrack track)
		{
			if ((librarian == null) || (controller == null) || (trayIcon == null))
			{
				// TODO: not sure why but some have seen exceptions coming into this method from
				// DoTaskCollectionChanged so this null-check and subsequent return is meant to
				// avoid that until I figure out why it's happening on a few machines.
				return;
			}

			bool isActive = librarian.ActiveCount > 0;

			if (controller.IsPlaying)
			{
				trayIcon.Icon = isActive ? Resx.PlayActive : Resx.Play;
			}
			else
			{
				trayIcon.Icon = isActive ? Resx.PauseActive : Resx.Pause;
			}

			if (track == null)
			{
				trayIcon.Text = Resx.ApplicationTitle;
				//Logger.WriteLine(Logger.Level.Debug, "DEBUG", "SetNotifyIcon track is null");
			}
			else
			{
				string text = String.Format("{0}\n{1}\n{2}",
					track.Title, track.Artist, track.Album).Trim();

				//Logger.WriteLine(Logger.Level.Debug, "DEBUG", String.Format("SetNotifyIcon.Text ({0})", text));

				if (text.Length == 0)
				{
					text = Resx.ApplicationTitle;
				}
				else if (text.Length > 63)				
				{
					// NotifyIcon.Text cannot be longer than 64 characters
					text = text.Substring(0, 60).Trim() + "...";
				}

				trayIcon.Text = text;
			}
		}

		#endregion Window Control Methods
	}
}
