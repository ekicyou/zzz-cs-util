//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner
{
	using System;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Media.Animation;
	using System.Windows.Threading;


	//********************************************************************************************
	// class FadingWindow
	//********************************************************************************************

	/// <summary>
	/// Base class for windows that can fade-in and fade-out.
	/// Also provides fade-out cancellation by moving the mouse over the window
	/// and static pinning to "keep" the window visible.
	/// </summary>

	internal class FadingWindow : Window, IDisposable
	{

		// time in milliseconds when fade-out begins after fade-in completes
		private readonly TimeSpan DefaultFadeOutDelay = TimeSpan.FromMilliseconds(3000);

		// time in milliseconds when fade-out begins after mouse leaves the window
		private readonly TimeSpan LeaveFadeOutDelay = TimeSpan.FromMilliseconds(2000);

		// standard Windows 7 offset of windows from taskbar
		protected const int DefaultWindowMargin = 2;

		// hidden/visible Opacity levels
		private const double HiddenOpacity = 0.0;
		private const double VisibleOpacity = 1.0;

		private bool isPinned;
		private bool hasMouse;
		private bool isDisposed;
		private DispatcherTimer timer;
		private FrameworkElement element;

		private Storyboard fadeInStoryboard;
		private Storyboard fadeOutStoryboard;


		//========================================================================================
		// Constructors
		//========================================================================================

		/// <summary>
		/// This constructor must be called by implementors or the fading functionality
		/// will not work.
		/// </summary>

		public FadingWindow ()
			: base()
		{
			this.Opacity = 0.0;

			// by setting it hidden, this hides the window from the Alt-Tab program switcher
			this.Visibility = Visibility.Hidden;

			DoubleAnimation animation = new DoubleAnimation(
				HiddenOpacity, VisibleOpacity, new Duration(TimeSpan.FromMilliseconds(300)));
			animation.BeginTime = TimeSpan.FromMilliseconds(100);
			animation.AutoReverse = false;

			this.fadeInStoryboard = new Storyboard();
			this.fadeInStoryboard.Children.Add(animation);
			this.fadeInStoryboard.Completed += new EventHandler(ShowCompleted);

			animation = new DoubleAnimation(
				VisibleOpacity, HiddenOpacity, new Duration(TimeSpan.FromMilliseconds(500)));
			animation.BeginTime = TimeSpan.FromMilliseconds(100);
			animation.AutoReverse = false;

			this.fadeOutStoryboard = new Storyboard();
			this.fadeOutStoryboard.Children.Add(animation);
			this.fadeOutStoryboard.Completed += new EventHandler(HideCompleted);

			this.timer = new DispatcherTimer();
			this.timer.Tick += new EventHandler(InitiateFadeOut);
			this.timer.Interval = DefaultFadeOutDelay;
		}


		/// <summary>
		/// Clean up all references and event handlers.
		/// </summary>

		public virtual void Dispose ()
		{
			if (!isDisposed)
			{
				if (IsOpaque)
				{
					this.Close();
				}

				if (timer != null)
				{
					timer.Tick -= new EventHandler(InitiateFadeOut);
					timer.Stop();
					timer = null;
				}

				if (fadeInStoryboard != null)
				{
					fadeInStoryboard.Completed -= new EventHandler(ShowCompleted);
					fadeInStoryboard.Stop();
					fadeInStoryboard.Children.Clear();
					fadeInStoryboard = null;
				}

				if (fadeOutStoryboard != null)
				{
					fadeOutStoryboard.Completed -= new EventHandler(HideCompleted);
					fadeOutStoryboard.Stop();
					fadeOutStoryboard.Children.Clear();
					fadeOutStoryboard = null;
				}

				element = null;

				isDisposed = true;

				GC.SuppressFinalize(this);
			}
		}


		//========================================================================================
		// Properties
		//========================================================================================

		/// <summary>
		/// Sets the primary element to be animated.  This should be a Border or similar
		/// FrameworkElement and not the Window itself.
		/// </summary>

		public FrameworkElement AnimatedElement
		{
			set
			{
				element = value;

				DoubleAnimation animation = fadeInStoryboard.Children[0] as DoubleAnimation;
				if (animation != null)
				{
					Storyboard.SetTargetName(animation, element.Name);
					Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.OpacityProperty));
				}

				animation = fadeOutStoryboard.Children[0] as DoubleAnimation;
				if (animation != null)
				{
					Storyboard.SetTargetName(animation, element.Name);
					Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.OpacityProperty));
				}
			}
		}


		/// <summary>
		/// Gets a Boolean value indicating whether this window is currently in a visible
		/// and opaque state.
		/// </summary>

		public bool IsOpaque
		{
			get
			{
				return IsVisible && element.Opacity == VisibleOpacity;
			}
		}


		/// <summary>
		/// Gets or sets the pinned state of the window.  If pinned then fade-outs are
		/// disabled, otherwise fade-outs are enabled.
		/// </summary>

		public bool IsPinned
		{
			get
			{
				return isPinned;
			}

			set
			{
				// if unpinning a pinned window and the mouse is not over this window
				// then we can restart the fade-out timer
				if (isPinned && !value)
				{
					if (!hasMouse)
					{
						timer.Interval = LeaveFadeOutDelay;
						timer.Start();
					}
				}

				isPinned = value;
			}
		}


		//========================================================================================
		// Methods
		//========================================================================================

		/// <summary>
		/// Show the window, fading in opacity.
		/// </summary>

		public new void Show ()
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(new Action(delegate { Show(); }));
				return;
			}

			try
			{
				this.Visibility = Visibility.Visible;

				// stop fadeout if in progress
				fadeOutStoryboard.Stop(element);

				// pull it forward and show the window
				Topmost = true;
				Opacity = VisibleOpacity;

				if ((element.Opacity > 0) && (element.Opacity < VisibleOpacity))
				{
					// if animating then just complete it immediately
					ShowCompleted(this, new EventArgs());
				}
				else if (element.Opacity == 0)
				{
					fadeInStoryboard.Begin(element, true);
				}
			}
			catch (Exception exc)
			{
				ExceptionDialog dialog = new ExceptionDialog(new SmartException(exc), "Fading Window");
				dialog.ShowDialog();
				dialog = null;
			}
		}


		private void ShowCompleted (object sender, EventArgs e)
		{
			element.Opacity = VisibleOpacity;

			if (!hasMouse)
			{
				timer.Interval = DefaultFadeOutDelay;
				timer.Start();
			}
		}


		//----------------------------------------------------------------------------------------

		/// <summary>
		/// Hide the window, fading-out opacity.
		/// </summary>

		public new void Hide ()
		{
			// stop fadein if in progress
			fadeInStoryboard.Stop(element);

			// allow it to be eclipsed
			Topmost = false;

			// start only if fully visible to avoid flicker
			if (element.Opacity == VisibleOpacity)
			{
				fadeOutStoryboard.Begin(element, true);
			}
			else
			{
				// just hide immediatley
				HideCompleted(this, new EventArgs());
			}
		}


		public void HideNow ()
		{
			HideCompleted(null, null);
			hasMouse = false;
			isPinned = false;
		}


		private void HideCompleted (object sender, EventArgs e)
		{
			element.Opacity = HiddenOpacity;
			this.Opacity = HiddenOpacity;
			this.Visibility = Visibility.Hidden;

			OnHideCompleted();
		}


		/// <summary>
		/// Allows inheritors to perform additiona logic immediately after the window
		/// has completely faded out.
		/// </summary>

		protected virtual void OnHideCompleted ()
		{
		}


		//----------------------------------------------------------------------------------------

		/// <summary>
		/// When the mouse is visible to this window, that means it has a "virtual capture"
		/// of the window and prevents it from fading.
		/// </summary>
		/// <param name="e"></param>

		protected override void OnMouseMove (MouseEventArgs e)
		{
			hasMouse = true;

			if (this.Opacity != VisibleOpacity)
			{
				fadeInStoryboard.Stop(element);
				fadeOutStoryboard.Stop(element);
				ShowCompleted(this, new EventArgs());
			}

			if (timer.IsEnabled)
			{
				timer.Stop();
			}

			base.OnMouseMove(e);
		}


		/// <summary>
		/// When the mouse is no longer visible to this window, that means it releases
		/// its "virtual capture", allowing subsequent fading.
		/// </summary>
		/// <param name="e"></param>

		protected override void OnMouseLeave (MouseEventArgs e)
		{
			hasMouse = false;

			if (!isPinned)
			{
				timer.Interval = LeaveFadeOutDelay;
				timer.Start();
			}

			base.OnMouseLeave(e);
		}


		private void InitiateFadeOut (object sender, EventArgs e)
		{
			timer.Stop();
			Hide();
		}


		//----------------------------------------------------------------------------------------

		/// <summary>
		/// Positions the window just above a specified coordinate, centering
		/// the window over that position.
		/// </summary>
		/// <param name="x">The X coordinate over which the window is to be centered.</param>
		/// <param name="y">The Y coordinate over which the window is to be placed.</param>

		public void SetPositionRelateiveTo (Point point, Taskbar.Dock docking)
		{
			double limitted;			// max width or height limit depending on docking
			double centered;			// vertical or horizontal center depending on docking

			if ((docking == Taskbar.Dock.Bottom) || (docking == Taskbar.Dock.Top))
			{
				limitted = SystemParameters.WorkArea.Width - Width - DefaultWindowMargin;
				centered = point.X - (Width / 2);

				Left = (centered > limitted ? limitted : centered);

				if (docking == Taskbar.Dock.Bottom)
				{
					Top = point.Y - Height - DefaultWindowMargin;
				}
				else
				{
					Top = point.Y + DefaultWindowMargin;
				}
			}
			else
			{
				limitted = SystemParameters.WorkArea.Height - Height - DefaultWindowMargin;
				centered = point.Y - (Height / 2);

				Top = (centered > limitted ? limitted : centered);

				if (docking == Taskbar.Dock.Left)
				{
					Left = point.X + DefaultWindowMargin;
				}
				else
				{
					Left = point.X - Width - DefaultWindowMargin;
				}
			}
		}
	}
}
