# QPangReborn (QPang / MangaFighter emulator)

This is our attempt at an QPang emulator. We have decided to open-source our prototype emulators because we are not actively working on the project anymore. Hopefully someone else can continue with our work or assist us.

You can find QPang and MangaFighter installations in our GDrive folder. There is also a patch for QPang 20120502.

https://drive.google.com/drive/folders/0B94To1DD6pC7YnVPYkYtZzlBUTg

## How do I run QPang?

1. Download and install `QPangSetup_NL 20120502.exe` from the GDrive folder.
2. Download all files from the `20120502 Patch #2` folder and place them in the QPang installation folder.
3. Rename or remove the "HShield" directory (I recommend to rename it so you have a backup).
4. Execute `Play QPang.bat`.

## How do I run the emulators?

1. Clone the GitHub project.
2. Open the `.sln` in VS2017. (Make sure you have [.NETCore 2.0](https://www.microsoft.com/net/core) installed as well)
3. Right-click on the solution in the `Solution Explorer` view and press `Set StartUp Projects...`.
4. Select `Multiple startup projects`.
5. Set the action for `Server.Auth` and `Server.Lobby` to `Start`.
6. Press `Apply` and `OK`.

## Links

- https://www.facebook.com/qpangreborn (Dutch)
- https://qpangreborn.tumblr.com (Dutch)
- https://www.qpang.nl
- https://soundcloud.com/qpangreborn

## Resources

- [Reverse Engineering Dead Protocols](https://media.ccc.de/v/31c3_-_5956_-_en_-_saal_2_-_201412281400_-_cyber_necromancy_-_joseph_tartaro_-_matthew_halchyshak)
- [How do modders revive online features in old console games without the server files?](https://www.reddit.com/r/ReverseEngineering/comments/1uognx/how_do_modders_revive_online_features_in_old/cekbbt4/)
- [Is it still possible to reverse engineer an old MMORPG with just the game client?](http://forum.ragezone.com/f860/reverse-engineer-mmorpg-game-client-1079352/)
