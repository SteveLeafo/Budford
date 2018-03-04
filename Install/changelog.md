# Budford Change Log

## V1.0.54
- Fixed an issue that was causing the settings file to not be updated correctly

## V1.0.53
- Added an option to use borderless full screen when launching Cemu in full screen mode.
- Added a plug-in mechanism that is handy for mods and added a couple of mod Plug-ins.
- Changes to the way the settings are saved to make Budford more portable.

## V1.0.52
- When importing a shader cache from a different region, you will now be prompted to chose your game.
- Added two new windows style options Maximized and Minimized.
- Added an option to set an mlc folder.  This will then be passed to Cemu via the -mlc parameter.  Note: only works on Cemu 1.10.0 and later
- Added an option to configure the Download folder (Default is ProgramData)
- Added an option to configure the Save folder (Default is My Documents)
- Issue#4: Fixed the issue that was stopping the default resolution pack from working.
- Issue#4: Fixed the issue where CPU priority sections weren't working

## V1.0.51
- Removed the ability to choose which monitor Cemu will launch on for systems with multiple displays

## V1.0.50
- Added the ability to choose which monitor Cemu will launch on for systems with multiple displays

## V1.0.49
- Added a notification if the official status of a game changes.  You can then see the old and new status of changed games
- Added the ability to configure a hot key to Close Cemu
- Added support for online file to the Cemu version manager
- Improved the Cemu update form - you can now chose to automatically check for new Cemu and Cemu Hook release on start
- Added a new welcome screen and simplified the initial setup for first time users

## V1.0.46
- New per game options: Controller Profile Overrides.  Can change the controller type or disable a controller per game.  Default is to do nothing.
- Fixed an issue with the shader cache importer.  It will now prompt you before importing with a smaller shader cache.
- Fixed an issue where the option to disable shader caches wasn't working.
- Progress windows now stay longer when downloading an extracting files.
- Removed a number of unused options.

## V1.0.45
- Improved FPS override option so it now works with all static packs.
- Fixed the bug where you couldn't launch Cemu without selecting a game first.

## V1.0.44
- Update for Cemu 1.11.5
- Improved name matching with entries on the Compatibility Wiki (Levenshtien distance).
- Added links to the Compatibility Wiki for each game in the context menu and game information form.

## V1.0.43
- New per game option: Advanced | Use Cafe Libs.  Will copy snd_user.rpl and snduser2.rpl to cafeLibs.  (If this options isn't selected, these files fill be deleted).  Dump snd_user.rpl and snduser2.rpl from you Wii U and copy them to My Documents\Budford\CafeLib.

## V1.0.42
- Added support for Operating systems other than Windows.  (Tested on Linux and Mac OS using Mono).
- Added a new Configuration option Cemu | Region Settings | Wine.  If you are not running on windows, are the path to Wine here (usually /usr/bin/wine).

## V1.0.41
- Added a new Tool: Tools | Export to LaunchBox. Launch the tool, select you LaunchBox.exe file and Cemu prifile. Hit export and your games should then appear in LaunchBox.  Even preserves play count etc.

## V1.0.39
- Stability improvements.
- Documentation.

## V1.0.38
- Added an option to automatically download the latest graphic packs on launch.

## V1.0.37
- Added per game comments to the main view.
- Added the ability to take a Cemu screen shot.
- Added the ability to make Cemu full screen.
- Added an option to launch straight into Gamepad view.
- Added an option to hide all WiiULauncher.rpx games.
- Fixed a crash that could happen when using really old version of Cemu.

## V1.0.33
- Improved the way graphic packs are mangaged.

## V1.0.32
- Stability improvements.

## V1.0.29
- Added the ability to choose the Clarity preset when selecting graphic packs.
- Added a global volume option.
- Added an option to scan for new games on start up.
- Added separate configurable process priorites for each of the CPU modes.


## V1.0.26
- Initial public release.

