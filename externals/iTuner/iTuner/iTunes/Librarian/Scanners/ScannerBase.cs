//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner.iTunes
{
	using System;
	using System.ComponentModel;
	using System.Configuration;
	using System.IO;
	using Resx = Properties.Resources;


	/// <summary>
	/// Abstract base class for all scanners.
	/// </summary>

	internal abstract class ScannerBase : IScanner, INotifyPropertyChanged
	{

		private static string archivePath = null;	// path of iTuner archive

		private int progressPercentage;
		private Action completedAction;

		/// <summary>
		/// Configuration setting, true if data should persist
		/// </summary>

		protected static bool isLive;


		/// <summary>
		/// The iTunes library catalog provider.
		/// </summary>

		protected ICatalog catalog;


		/// <summary>
		/// Reference to iTunes COM interface.
		/// </summary>

		protected Controller controller;


		/// <summary>
		/// The internal name of this scanner.
		/// </summary>

		protected string name;


		/// <summary>
		/// The user-friendly name of this scanner.
		/// </summary>

		protected string description;


		/// <summary>
		/// True until explicitly cancelled.
		/// </summary>

		protected bool isActive = true;


		//========================================================================================
		// Constructor
		//========================================================================================

		/// <summary>
		/// Initialize the static configuration for all scanners.
		/// </summary>

		static ScannerBase ()
		{
			string mode = ConfigurationManager.AppSettings["LibraryMode"] ?? String.Empty;
			ScannerBase.isLive = !mode.Trim().ToLower().Equals("debug");
		}


		/// <summary>
		/// 
		/// </summary>

		public ScannerBase ()
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="catalog"></param>

		public ScannerBase (string name, Controller controller, ICatalog catalog)
		{
			this.name = name;
			this.controller = controller;
			this.catalog = catalog;
			this.progressPercentage = 0;
			this.completedAction = null;
		}


		//========================================================================================
		// Properties/Events
		//========================================================================================

		/// <summary>
		/// This event is fired when the value of a bindable property is changed.
		/// </summary>

		public event PropertyChangedEventHandler PropertyChanged;


		/// <summary>
		/// This event is fired at regular intervals as the synchronizer progresses.
		/// </summary>

		public event ProgressChangedEventHandler ProgressChanged;


		/// <summary>
		/// Gets the path of the iTuner archive directory.
		/// </summary>

		public string ArchivePath
		{
			get
			{
				if (ScannerBase.archivePath == null)
				{
					ScannerBase.archivePath = GetArchivePath();
				}

				return ScannerBase.archivePath;
			}
		}


		/// <summary>
		/// Gets or sets an Action to invoke at the completion of this scanner.  This
		/// is used to remove disabled scanners from the Librarian <i>disabled</i> collection.
		/// </summary>

		public Action Completed
		{
			get { return completedAction; }
			set { completedAction = value; }
		}

	
		/// <summary>
		/// Gets the user-friendly name of this scanner.  Inheritors must set the
		/// protected <i>description</i> field in their constructors.
		/// </summary>

		public string Description
		{
			get { return description; }
		}


		/// <summary>
		/// Gets the name of this scanner.  Inheritors must set the protected
		/// <i>name</i> field in their constructors.
		/// </summary>

		public string Name
		{
			get { return name; }
		}


		/// <summary>
		/// Gets or sets the percent completed by this scanner.  This is a bindable
		/// property used for the Librarian status panel.
		/// </summary>

		public int ProgressPercentage
		{
			get
			{
				return progressPercentage;
			}

			set
			{
				progressPercentage = value;
				OnPropertyChanged("ProgressPercentage");
			}
		}


		//========================================================================================
		// Methods
		//========================================================================================

		/// <summary>
		/// Cancel execution of this scanner.  This occurs synchronously at the scope
		/// of an atomic task.  Implementors are required to check the <i>isActive</i>
		/// protected member at regular intervals to ensure reasonable immediacy.
		/// </summary>

		public void Cancel ()
		{
			isActive = false;
		}


		/// <summary>
		/// Execute this scanner synchronously.
		/// </summary>
		/// <remarks>
		/// Scanners should be implemented as a sequence or loop of atomic tasks where
		/// the scanner has reasonably small increments between which they can check
		/// for a cancellation condition.
		/// </remarks>

		public abstract void Execute ();


		/// <summary>
		/// Determines the best possible path for the iTuner archive directory.  This
		/// tries a couple of different preferred paths before resorting to the Recycle bin.
		/// </summary>
		/// <returns></returns>

		private string GetArchivePath ()
		{
			// C:\Users\steven\_iTunerArchive
			string root = Environment.GetEnvironmentVariable("USERPROFILE");
			string path = Path.Combine(root, Resx.ArchiveRootPath);
			if (!TryCreateArchive(path))
			{
				// C:\Users\steven\AppData\Local\iTuner\_iTunerArchive
				root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				path = Path.Combine(Path.Combine(root, Resx.ApplicationProduct), Resx.ArchiveRootPath);
				if (!TryCreateArchive(path))
				{
					// C:\_iTunerArchive
					root = @"C:\";
					path = Path.Combine(root, Resx.ArchiveRootPath);
					if (!TryCreateArchive(path))
					{
						// C:\Users\steven\AppData\Local\TEMP\_iTunerArchive
						root = Path.GetTempPath();
						path = Path.Combine(root, Resx.ArchiveRootPath);
						if (!TryCreateArchive(path))
						{
							path = null;
						}
					}
				}
			}

			return path;
		}


		private bool TryCreateArchive (string path)
		{
			bool success = Directory.Exists(path);

			if (!success)
			{
				try
				{
					Directory.CreateDirectory(path);
					success = true;
				}
				catch
				{
					success = false;
				}
			}

			return success;
		}


		/// <summary>
		/// Get a lits of iTunes Tracks given a list of track IDs.
		/// </summary>
		/// <param name="trackIDs"></param>
		/// <returns></returns>

		protected TrackCollection GetTracks (PersistentIDCollection persistentIDs)
		{
			TrackCollection tracks = new TrackCollection();

			foreach (PersistentID persistentID in persistentIDs)
			{
				Track track = controller.LibraryPlaylist.GetTrack(persistentID);
				if (track != null)
				{
					tracks.Add(track);
				}
			}

			return tracks;
		}


		/// <summary>
		/// Notifies binded consumers that the value of the specified property has changed.
		/// </summary>
		/// <param name="name"></param>

		public void OnPropertyChanged (string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

	
		/// <summary>
		/// Notify consumers of updated progress information.
		/// </summary>
		/// <param name="userState">A unique user state.</param>

		protected void UpdateProgress (object userState)
		{
			if (ProgressChanged != null)
			{
				ProgressChanged(
					this, new ProgressChangedEventArgs(progressPercentage, userState));
			}
		}
	}
}
