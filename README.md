# ffxiv-raid-sim

A singleplayer raid and fight mechanic simulator with bots, made in the Unity game engine from scratch, for the critically acclaimed MMORPG Final Fantasy XIV: Online by Square Enix. This program will get more timelines and fights added to it as I develop it further. I will also accept any external thirdparty contributions possibly developed by others. Current main focus is Blue Mage specific fights and mechanics with the odd ultimate mechanic thrown in to the mix.

# Supported fights

- Omega: Alphascape
	- Alphascape 4.0 (Savage) (O12S)
		- Phase 2
			- Hello World 1 ([BLU](https://www.icy-veins.com/ffxiv/blue-mage-omega-raid-guide))
			- Hello World 2 ([BLU](https://www.icy-veins.com/ffxiv/blue-mage-omega-raid-guide))
- Alexander: Midas
	- Alexander - The Burden of the Son (Savage) (A8S)
		- Gavel ([BLU](https://www.icy-veins.com/ffxiv/blue-mage-brute-justice-raid-guide))
- AAC Light-heavyweight Tier
	- AAC Light-heavyweight M4 (Savage) (M4S)
		- Sunrise Sabbath ([Rinon](https://www.youtube.com/watch?v=1lrk5FbNIPc))
		- Sunrise Sabbath ([AutoCad/Uptime](https://raidplan.io/plan/OnQXobwatopL1G8u))

# In development fights

Timelines don't work yet for these or the bots are not finished and will die to mechanics.

- None currently

# Planned fights

- Omega: Alphascape
	- Alphascape 4.0 (Savage) (O12S)
		- Phase 2
			- Patch ([BLU](https://www.icy-veins.com/ffxiv/blue-mage-omega-raid-guide))
- Alexander: The Creator
	- Alexander - The Soul of the Creator (Savage) (A12S)
		- Temporal Stasis ([BLU](https://www.icy-veins.com/ffxiv/blue-mage-alexander-prime-raid-guide))
- The Epic of Alexander (Ultimate) (TEA)
	- Phase 4 (Perfect Alexander)
		- The Final Word
		- Fate Calibration α
		- Fate Calibration β
- The Unending Coil of Bahamut (Ultimate) (UCOB)
	- Phase 3 (Bahamut Prime)
		- Heavensfall
- The Weapon's Refrain (Ultimate) (UWU)
	- Phase 3 (Titan)
		- Gaols
	- Phase 5 (The Ultima Weapon)
		- Ultimate Predation
		- Ultimate Suppression
		- Ultimate Annihilation

# Installation

To use this program, either open the [web version](https://susy-bakaa.github.io/unityweb/raidsim/index.html) or just head over to the releases section and download the latest archive file for your respective operating system. If you wish to try out the program before downloading you may do so with the web version, but be warned though, as the web version is currently not properly tested and might run into sever issues. For the standalone version, you need to extract said archive with some tool such as 7zip and then open up the raidsim.exe or raidsim.x86_64 to launch the program. The build available for Linux is not well tested so it might have some issues. If you instead want to download the source code or expand upon this program, check the git instructions down below.

# Git Usage Instructions

This repository uses [gdrive](https://github.com/prasmussen/gdrive) for LFS storage. To learn more about installing gdrive follow [this guide](https://medium.com/machine-learning-intuition/tutorial-storing-large-a-i-models-with-gdrive-don-t-use-git-lfs-a1aaccdc5b26). To get started, edit `gdrive_default.txt` located inside the `.gdrive` directory on the project root. Each line corresponds to one variable. After you're done save it as `gdrive_config.txt` into the same folder.

Config file explanation:
1. Disk your repository is on
2. Path to your local repository (In a format the windows cd command can read)
3. Name of the archive used with Google Drive
4. File ID of your existing archive on Google Drive (Must be shared to everyone with link)
5. Folder ID of the folder where your archive will sit in (Must be shared to everyone with link)
6. File used for ignored folders and file types (By default `.gdriveignore`)

After configuring the settings simply run `gdrive_util.bat` located in the root folder after commiting other changes to GitHub and follow the instructions on the cmd window to manage your Google Drive backups of large files. If you do not have access to the original archive used in this project, as in you're an external contributor, you may manually download the asset archive from here: [ffxiv-raid-sim.tar.gz](https://drive.google.com/file/d/1ybYaJ8LGnHwY5jeCv1Zr6B5fT7FHL51i/view?usp=drive_link)

# Credits

Please check out the [credits file](https://github.com/susy-bakaa/ffxiv-raid-sim/blob/main/credits.md) for all of the credits!