This small hotfix addresses a few issues that popped up after the last major update. Mostly the biggest fix is for **targeting** in **WebGL** and few other smaller ones for audio/visual bugs. Everything should now behave more as expected across all platforms.

## Changelog:

### Bug Fixes

- Fixed an issue where **character targeting** was not working in the **WebGL build**.
**NOTE:** This was caused by the new multithreaded targeting implementation which I just for now reverted for WebGL, because it does not support multiple threads.

- Fixed the **software cursor** rendering behind other elements and it should now correctly appear **above everything**.

- Fixed a few **audio system issues**, including sounds not playing or behaving inconsistently.

- Fixed a UI bug where the **social buttons** overlapped with the **fullscreen button** on WebGL.

---

If anything else seems off, as always, report any issues you may come across [here on GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues), contact me through DMs on Discord or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D).