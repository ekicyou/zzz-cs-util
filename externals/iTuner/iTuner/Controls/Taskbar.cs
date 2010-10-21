//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner
{
	using System;
	using System.Drawing;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Windows.Forms;
	using WPF = System.Windows;


	/// <summary>
	/// Provider helper methods to determine the recommended position and direction of
	/// our tray application windows relative to the application notify icon.  This
	/// depends on the current docking location of the Windows taskbar.
	/// </summary>

	internal class Taskbar
	{

		/// <summary>
		/// Indicates the docking position of the taskbar.
		/// </summary>
		/// <remarks>
		/// This is equivalent to ABE_LEFT, ABE_TOP, ABE_RIGHT, and ABE_BOTTOM.
		/// </remarks>

		public enum Dock : int
		{
			Left = 0,
			Top,
			Right,
			Bottom
		}


		private IntPtr traybar;
		private Interop.RECT bounds;
		private Dock docking;


		#region Interop

		private static class Interop
		{

			/// <summary>
			/// SHAppBarMessage message commands.
			/// </summary>

			public enum ABMessage : int
			{
				ABM_NEW = 0,
				ABM_REMOVE,
				ABM_QUERYPOS,
				ABM_SETPOS,
				ABM_GETSTATE,
				ABM_GETTASKBARPOS,
				ABM_ACTIVATE,
				ABM_GETAUTOHIDEBAR,
				ABM_SETAUTOHIDEBAR,
				ABM_WINDOWPOSCHANGED,
				ABM_SETSTATE
			}


			/// <summary>
			/// The RECT structure defines the coordinates of the upper-left and lower-right
			/// corners of a rectangle.
			/// </summary>

			[StructLayout(LayoutKind.Sequential)]
			public struct RECT
			{
				public int left;
				public int top;
				public int right;
				public int bottom;
			}


			/// <summary>
			/// Contains information about a system appbar message. This structure is used
			/// with the SHAppBarMessage function
			/// </summary>

			[StructLayout(LayoutKind.Sequential)]
			public struct APPBARDATA
			{
				public int cbSize;
				public IntPtr hWnd;
				public int uCallbackMessage;

				/// <summary>
				/// A value that specifies an edge of the screen. This member is used when
				/// sending the ABM_GETAUTOHIDEBAR, ABM_QUERYPOS, ABM_SETAUTOHIDEBAR, and
				/// ABM_SETPOS messages. This member can be one of the following values.
				/// </summary>

				public int uEdge;

				/// <summary>
				/// A RECT structure to contain the bounding rectangle, in screen coordinates,
				/// of an appbar or the Microsoft Windows taskbar. This member is used when
				/// sending the ABM_GETTASKBARPOS, ABM_QUERYPOS, and ABM_SETPOS messages
				/// </summary>

				public RECT rc;

				public IntPtr lParam;
			}


			/// <summary>
			/// The EnumChildProc function is an application-defined callback function used with
			/// the EnumChildWindows function. It receives the child window handles. The WNDENUMPROC
			/// type defines a pointer to this callback function. EnumChildProc is a placeholder for
			/// the application-defined function name. 
			/// </summary>
			/// <param name="hwnd">
			/// Handle to a child window of the parent window specified in EnumChildWindows.
			/// </param>
			/// <param name="lParam">
			/// Specifies the application-defined value given in EnumChildWindows.
			/// </param>
			/// <returns>
			/// To continue enumeration, the callback function must return TRUE; to stop enumeration,
			/// it must return FALSE.
			/// </returns>

			public delegate bool EnumChildProc (IntPtr hwnd, IntPtr lParam);


			/// <summary>
			/// The EnumChildWindows function enumerates the child windows that belong to the
			/// specified parent window by passing the handle to each child window, in turn,
			/// to an application-defined callback function. EnumChildWindows continues until
			/// the last child window is enumerated or the callback function returns FALSE.
			/// </summary>
			/// <param name="hWnd">
			/// Handle to the parent window whose child windows are to be enumerated. If this
			/// parameter is NULL, this function is equivalent to EnumWindows. 
			/// </param>
			/// <param name="callback">
			/// Pointer to an application-defined callback function. For more information, see
			/// EnumChildProc.
			/// </param>
			/// <param name="lParam">
			/// Specifies an application-defined value to be passed to the callback function.
			/// </param>
			/// <returns>Not used</returns>

			[DllImport("user32")]
			public static extern int EnumChildWindows (IntPtr hWnd, EnumChildProc callback, int lParam);


			/// <summary>
			/// 
			/// </summary>
			/// <param name="hwndParent"></param>
			/// <param name="hwndChildAfter"></param>
			/// <param name="lpszClass"></param>
			/// <param name="lpszName"></param>
			/// <returns></returns>

			[DllImport("User32", SetLastError = true)]
			public static extern IntPtr FindWindowEx (
				IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, int lpszName);


			/// <summary>
			/// The GetClassName function retrieves the name of the class to which the specified
			/// window belongs.
			/// </summary>
			/// <param name="hWnd">
			/// Handle to the window and, indirectly, the class to which the window belongs.
			/// </param>
			/// <param name="className">
			/// Pointer to the buffer that is to receive the class name string.
			/// </param>
			/// <param name="maxCount">
			/// Specifies the length, in TCHAR, of the buffer pointed to by the lpClassName parameter.
			/// The class name string is truncated if it is longer than the buffer and is always
			/// null-terminated.
			/// </param>
			/// <returns>
			/// If the function succeeds, the return value is the number of TCHAR copied to the
			/// specified buffer.  If the function fails, the return value is zero. To get extended
			/// error information, call GetLastError.
			/// </returns>

			[DllImport("user32.dll")]
			public static extern bool GetClassName (IntPtr hWnd, StringBuilder className, int maxCount);


			/// <summary>
			/// The GetWindowRect function retrieves the dimensions of the bounding rectangle of the
			/// specified window. The dimensions are given in screen coordinates that are relative
			/// to the upper-left corner of the screen. 
			/// </summary>
			/// <param name="hWnd">Handle to the window. </param>
			/// <param name="lpRect">
			/// Pointer to a structure that receives the screen coordinates of the upper-left and
			/// lower-right corners of the window. 
			/// </param>
			/// <returns>
			/// If the function succeeds, the return value is nonzero.
			/// If the function fails, the return value is zero. To get extended error
			/// information, call GetLastError.
			/// </returns>

			[DllImport("User32", SetLastError = true)]
			public static extern int GetWindowRect (IntPtr hWnd, ref RECT lpRect);


			/// <summary>
			/// The MapWindowPoints function converts (maps) a set of points from a coordinate
			/// space relative to one window to a coordinate space relative to another window. 
			/// </summary>
			/// <param name="hWndFrom">
			/// Handle to the window from which points are converted. If this parameter is NULL or
			/// HWND_DESKTOP, the points are presumed to be in screen coordinates.
			/// </param>
			/// <param name="hWndTo">
			/// Handle to the window to which points are converted. If this parameter is NULL or
			/// HWND_DESKTOP, the points are converted to screen coordinates.
			/// </param>
			/// <param name="lpPoints">
			/// Pointer to an array of POINT structures that contain the set of points to be
			/// converted. The points are in device units. This parameter can also point to a
			/// RECT structure, in which case the cPoints parameter should be set to 2.
			/// </param>
			/// <param name="cPoints">
			/// Specifies the number of POINT structures in the array pointed to by the
			/// lpPoints parameter.
			/// </param>
			/// <returns>
			/// <para>
			/// If the function succeeds, the low-order word of the return value is the number of
			/// pixels added to the horizontal coordinate of each source point in order to compute
			/// the horizontal coordinate of each destination point; the high-order word is the
			/// number of pixels added to the vertical coordinate of each source point in order to
			/// compute the vertical coordinate of each destination point. 
			/// </para>
			/// <para>
			/// If the function fails, the return value is zero. Call SetLastError prior to calling
			/// this method to differentiate an error return value from a legitimate "0" return value. 
			/// </para>
			/// </returns>

			[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			public static extern int MapWindowPoints (
				IntPtr hWndFrom, IntPtr hWndTo, ref RECT lpPoints, UInt32 cPoints);


			/// <summary>
			/// Sends an appbar message to the system.
			/// </summary>
			/// <param name="dwMessage">Appbar message value to send.</param>
			/// <param name="pData">
			/// The address of an APPBARDATA structure. The content of the structure depends
			/// on the value set in the dwMessage parameter.
			/// </param>
			/// <returns></returns>

			[DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
			public static extern uint SHAppBarMessage (
				int dwMessage, ref APPBARDATA pData);
		}

		#endregion Interop

		#region HelperWindow

		/// <summary>
		/// A native window used just to grab a valid Window handle.
		/// </summary>

		private class HelperWindow : NativeWindow, IDisposable
		{
			public HelperWindow ()
			{
				// create a generic window with no class name
				base.CreateHandle(new CreateParams());
			}


			public void Dispose ()
			{
				base.DestroyHandle();
				GC.SuppressFinalize(this);
			}
		}

		#endregion HelperWindow


		//========================================================================================
		// Constructor
		//========================================================================================

		/// <summary>
		/// Initialize a new instance.
		/// </summary>

		public Taskbar ()
		{
			this.traybar = IntPtr.Zero;
			this.docking = GetDocking(out this.bounds);
		}


		/// <summary>
		/// Discover the docking location of the Windows taskbar.
		/// </summary>
		/// <returns>The screen edge where the taskbar is docked.</returns>

		private Dock GetDocking (out Interop.RECT bounds)
		{
			Interop.APPBARDATA data = new Interop.APPBARDATA();
			data.cbSize = Marshal.SizeOf(data);

			using (HelperWindow window = new HelperWindow())
			{
				data.hWnd = window.Handle;
				Interop.SHAppBarMessage((int)Interop.ABMessage.ABM_GETTASKBARPOS, ref data);
			}

			bounds = data.rc;

			return (Dock)data.uEdge;
		}


		//========================================================================================
		// Properties
		//========================================================================================

		/// <summary>
		/// Gets the docking location of the Windows taskbar.  This is the edge of the screen
		/// to which the taskbar is docked.
		/// </summary>

		public Dock Docking
		{
			get { return docking; }
		}

	
		//========================================================================================
		// Methods
		//========================================================================================

		/// <summary>
		/// Gets a screen coordinate, as a Point struct, indicating the suggestion starting
		/// offset from the given notify icon opposite to the docking edge of the taskbar.
		/// <para>
		/// In other words, if the taskbar is docked to the bottom of the screen, the returned
		/// coordinates are just above the icon; if the taskbar is docked to the left of the
		/// screen, the returned coordinates are just to the right of the icon.
		/// </para>
		/// </summary>
		/// <param name="icon"></param>

		public WPF.Point GetTangentPosition (NotifyIcon icon)
		{
			IntPtr handle = GetNativeHandle(icon);
			if (handle == IntPtr.Zero)
			{
				return new WPF.Point();
			}

			Rectangle rectangle = GetScreenRect(handle);

			WPF.Point point = new WPF.Point();

			switch (docking)
			{
				case Dock.Bottom:
					point.X = rectangle.Left;
					point.Y = bounds.top;
					break;

				case Dock.Top:
					point.X = rectangle.Left;
					point.Y = bounds.bottom;
					break;

				case Dock.Left:
					point.X = bounds.right;
					point.Y = rectangle.Top;
					break;

				case Dock.Right:
					point.X = bounds.left;
					point.Y = rectangle.Top;
					break;
			}

			return point;
		}


		/// <summary>
		/// Get the handle of the native window associated with the NotifyIcon.  This is done
		/// by reflecting in to a private member field of the icon.
		/// </summary>
		/// <param name="icon"></param>
		/// <returns></returns>

		private IntPtr GetNativeHandle (NotifyIcon icon)
		{
			FieldInfo field = icon.GetType().GetField("window",
				BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

			NativeWindow window = field.GetValue(icon) as NativeWindow;

			return (window == null ? IntPtr.Zero : window.Handle);
		}


		/// <summary>
		/// Determine the screen bounding rectangle of the NotifyIcon native window.
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>

		private Rectangle GetScreenRect (IntPtr handle)
		{
			// find the system tray by its well known name
			IntPtr tray = Interop.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", 0);
			if (tray == IntPtr.Zero)
			{
				return Rectangle.Empty;
			}

			// the find system tray toolbar contained within the tray window
			traybar = IntPtr.Zero;
			Interop.EnumChildWindows(tray, new Interop.EnumChildProc(FilterWindows), 0);
			if (traybar.Equals(IntPtr.Zero))
			{
				return Rectangle.Empty;
			}

			Interop.RECT rect = new Interop.RECT();

			// get the window rectangle of the notify icon; this is local-relative coordinates
			// with top/left at 0,0 and right/bottom equal to width/height.

			if (Interop.GetWindowRect(handle, ref rect) == 0)
			{
				return Rectangle.Empty;
			}

			// translate the local-relative window rectangle to screen coordinates

			if (Interop.MapWindowPoints(traybar, IntPtr.Zero, ref rect, 2) == 0)
			{
				return Rectangle.Empty;
			}

			return new Rectangle(
				rect.left, rect.top,			// x, y
				rect.right - rect.left,			// width
				rect.bottom - rect.top);		// height
		}


		/// <summary>
		/// A callback function used by EnumChildWindows.
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>

		private bool FilterWindows (IntPtr handle, IntPtr lParam)
		{
			StringBuilder builder = new StringBuilder(256);
			Interop.GetClassName(handle, builder, builder.Capacity);
			if (builder.ToString().StartsWith("ToolbarWindow32"))
			{
				traybar = handle;
				return false;
			}

			return true;
		}
	}
}
