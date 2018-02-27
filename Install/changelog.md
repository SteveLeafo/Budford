# Budford Change Log

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

