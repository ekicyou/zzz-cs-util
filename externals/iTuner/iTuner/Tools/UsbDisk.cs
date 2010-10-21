//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner
{
	using System;
	using System.Text;
	using Resx = Properties.Resources;


	/// <summary>
	/// Represents the displayable information for a single USB disk.
	/// </summary>

	internal class UsbDisk
	{
		public class UsbSpace
		{
			public ulong FreeSpace { get; internal set; }
			public ulong Size { get; internal set; }
		}


		private const int KB = 1024;
		private const int MB = KB * 1000;
		private const int GB = MB * 1000;


		/// <summary>
		/// Initialize a new instance with the given values.
		/// </summary>
		/// <param name="name">The Windows drive letter assigned to this device.</param>

		public UsbDisk (string name)
		{
			this.Name = name;
			this.Model = String.Empty;
			this.Volume = String.Empty;
			this.FreeSpace = 0;
			this.Size = 0;
		}


		/// <summary>
		/// Gets the description of this disk.
		/// </summary>

		public string Description
		{
			get
			{
				string description;

				if (String.IsNullOrEmpty(Volume))
				{
					description = Name;
				}
				else
				{
					description = String.Format(
						Resx.UsbDescription, Name, Volume, Model);
				}

				return description;
			}
		}
	
		
		/// <summary>
		/// Gets the available free space on the disk, specified in bytes.
		/// </summary>

		public ulong FreeSpace
		{
			get;
			internal set;
		}


		/// <summary>
		/// Get the model of this disk.  This is the manufacturer's name.
		/// </summary>
		/// <remarks>
		/// When this class is used to identify a removed USB device, the Model
		/// property is set to String.Empty.
		/// </remarks>

		public string Model
		{
			get;
			internal set;
		}


		/// <summary>
		/// Gets the name of this disk.  This is the Windows identifier, drive letter.
		/// </summary>

		public string Name
		{
			get;
			private set;
		}


		/// <summary>
		/// Gets the total size of the disk, specified in bytes.
		/// </summary>

		public ulong Size
		{
			get;
			internal set;
		}


		/// <summary>
		/// 
		/// </summary>

		public UsbSpace Space
		{
			get
			{
				return new UsbSpace()
				{
					FreeSpace = this.FreeSpace,
					Size = this.Size
				};
			}
		}


		/// <summary>
		/// Get the volume name of this disk.  This is the friently name ("Stick").
		/// </summary>
		/// <remarks>
		/// When this class is used to identify a removed USB device, the Volume
		/// property is set to String.Empty.
		/// </remarks>

		public string Volume
		{
			get;
			internal set;
		}


		/// <summary>
		/// Pretty print the disk.
		/// </summary>
		/// <returns>
		/// A string of the form {name} {volume} ({model}) {free} free of {size}.
		/// </returns>

		public override string ToString ()
		{
			string description;

			if (String.IsNullOrEmpty(Volume))
			{
				description = Name;
			}
			else
			{
				string space = String.Format(
					Resx.FreeSpace, FormatByteCount(FreeSpace), FormatByteCount(Size));

				description = String.Format(
					Resx.UsbLongDescription, Name, Volume, Model, space);
			}

			return description;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>

		public static string FormatByteCount (ulong bytes)
		{
			string format = null;

			if (bytes < KB)
			{
				format = String.Format(Resx.FreeBytes, bytes);
			}
			else if (bytes < MB)
			{
				bytes = bytes / KB;
				format = String.Format(Resx.FreeKB, bytes.ToString("N"));
			}
			else if (bytes < GB)
			{
				double dree = bytes / MB;
				format = String.Format(Resx.FreeMB, dree.ToString("0.#"));
			}
			else
			{
				double gree = bytes / GB;
				format = String.Format(Resx.FreeGB, gree.ToString("0.#"));
			}

			return format;
		}
	}
}
