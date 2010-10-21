
========================================================================================
iTuner Version 1.2.3782 Release
10 May 2010

Copyright © 2010 Steven M. Cohn.  All Rights Reserved.
========================================================================================

System Requirements
-------------------

	1.2.3767 and later compatible with iTunes 9.1 and later
	- Tested with 9.1.0.79

	All previous version compatible with iTunes 9.0.x version only
	- Tested with 9.0.2.25 OK
	- Tested with 9.0.3.15 OK
	
	Windows 7 or Windows Vista
	Microsoft .NET Framework 3.5


Change Log
----------

1.2.3782 (10 May 2010)

	- Added Options dialog, initially for auto-scanners
	- Fix #6533 to handle key sequence conflicts
	- Fix #6557, duplicate of #6533
	- Fix to TerseCatalog to ignore DTD when offline
	- Fix to recognize incompatible iTunes versions

1.2.3769 Beta 3c (28 May 2010)

	- Added new hot key action to Show Lyrics, Alt+Win+L
	- Added double-click play/pause handler for NotifyIcon
	- Fix #6511 to properly handle unknown playlists
	- Fix #6512 to add missing MessageBox icons to source control
	- Fix to allow escaped spaces in music library path
	- Fix memory leak in Export/Synchronize dialogs
	- Fix memory leak in MovableWindow base class

1.2.3768 Beta 3b (26 Apr 2010)

	- Fix to reveal proper name of contextual scanners
	- Fix to disable automated scanners using config
	- Fix to cancel scenarios of export/sync dialogs

1.2.3767 Beta 3 (25 Apr 2010)

	- Complete rewrite of iTunes wrappers to handle COM interrupts
	- Automated import of files added to Library folders
	- Optimized catalog improve performance and memory use
	- Automated scanners can be disabled in configuration (UI coming)
	- Added Task panel behind main buttons to show running tasks
	- Added Check for Upgrades feature on startup and About box
	- Added custom Notify icons to indicate active background task
	- Improved MovableWindow, replacing custom code with DragMove
	- Issue: Export/Sync sometimes hangs or does not complete

1.2.3738 Beta 2 (27 Mar 2010)

	- Synchronize one or more iTunes playlists with a USB MP3 player
	- Fix #6240 to allow multiple playlists with the same name
	- Fix to auto-detect folder capabilities feature of synchronizer
	- Fix to properly position windows relative to docked taskbar
	- Fix to Synchronize dialog when using playlist in folder layout
	- Fix to Export dialog to properly italicize "No encoder" item
	- Fix to Export dialog to correctly interperet encoder ComboBox 

1.2.3735 Beta (24 Mar 2010)

	- USB MP3 Player / iTunes Playlist synchronization

1.1.3711 Release (01 Mar 2010)

	- Automated Librarian - albums cleaned as tracks are played
	- Export and convert tracks - by artist, album, or playlist
	- Fix to ChartLyricsLyricsProvider to govern request intervals
	- Fix to improve start-up time, deferred initialization to background
	- Fix to recognize musical playlists by scanning perceived file types

1.1.3707 Beta 3 (24 Feb 2010)

	- Automated Librarian
	- Export and convert tracks - by artist, album, or playlist
	- Fix to ChartLyricsLyricsProvider to govern request intervals
	- Fix to improve start-up time, deferred initialization to background
	- Fix to recognize musical playlists by scanning perceived file types

1.1.3703 Beta 2 (20 Feb 2010)

	- Use NetworkStatus to avoid online providers while machine is offline
	- Fix to CurrentTrack tracking status
	- Fix to avoid memory leak while switching tracks
	- Fix to avoid memory leak while running DuplicateScanner
	- Fix to hide windows from Alt-Tab program switcher control
	- Fix to avoid empty notify icon tooltip text
	- Fix to avoid double-drawing text in context menu

1.1.3699 Beta (16 Feb 2010)

	- Initial implementation of Librarian
	    - Remove dead phantom tracks
	    - Remove duplicate entires with the assistance of genpuid
	    - Remove empty directories left by iTunes Library Organizer
	    - "Clean" item added to notify icon context menu
	- Shows current track title, artist, and album name in Notify icon tooltip
	- Adding optional output logging... we recommend BareTail
	- Adding application configuration file
	- May drag and move ExceptionDialog
	- Enhanced About box

1.0.3692 Patch (09 Feb 2010)

	- Implementation of basic track information editor.  Double-click title, artist, or
	  album name from main window or popup window to modify the field value.  The edit
	  control was implemented by the new EditBlock UserControl.
	- Fix to FadingWindow to restart fading timers when window is unpinned while mouse
	  cursor is not hovering over window.
	- Fix to add localizable resources for lyrics report header.
	- Fix to SplashWindow to include full version information.

1.0.3690 Release (08 Feb 2010)

	- Initial release

.
