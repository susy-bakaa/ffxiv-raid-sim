This hotfix has some changes to **UMAD P1: Tele-trouncing**, improves cursor handling across all platforms and fixes a few issues.

## v.0.7.5 Changelog

### Changes

- Updated **arrow (Tele-portent) status effect** assignment in **UMAD P1: Tele-trouncing** to use the same limited variations as the real game.
- Updated **UMAD status effect** sorting to better match the game.
- Updated the **Double-trouble Trap** status effect description to be more accurate.
- Further improved **cursor position logic** from last update.
  
  **NOTE:** Cursor handling now works on all platforms using the same backend code.
- Further improved **alternative bot name** functionality.
  
  **NOTE:** Names should now load almost instantly and remain correctly selected. Hopefully at least...

### Bug Fixes

- Fixed an issue where **ranged bots** could sometimes get clipped by the **Thunder AOE** in **UMAD P1: Tele-trouncing**.
  
  **NOTE:** They learned to press Sprint now :)

## Known Issues

-   **Linux windowed mode** can behave erratically and sometimes changes shape during scene loads.  
    **Workaround:** Don’t maximize the window, or play in fullscreen instead of windowed.
-   Bots may **fail** certain mechanics/timelines if you use one of the new naming schemes.  
    **Workaround:** Revert to the original naming style (Option 1).
	
	**NOTE:** Not fully confirmed yet, but bot names have been hardcoded for a long time in some places, so if you hit any issues like this, tell me ASAP so I can patch them.

---

Hotfix/bugfix patch this time. As always, let us know if you spot any problems through [GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D)!