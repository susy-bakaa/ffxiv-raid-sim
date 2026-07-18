This update adds the simulator's first **UCOB** timeline and includes a round of internal changes in preparation for the larger **0.8.0** update. The main focus this time was finishing the new timeline and cleaning up things behind the scenes.

## v.0.7.7 Changelog

### Changes

- Added a new timeline: **UCOB P3: Heavensfall**.
  This is the first **UCOB** timeline added to the simulator so far.

  Supports the following common main strategies:

  - [LPDU](https://ff14.toolboxgaming.space/?id=141496754100071&preview=1)
  - [NAUR](https://raidplan.io/plan/NGpZ9S-3kiLsDzAY)
  - [JP Elemental](https://ffxiv.tuufless.com/elemental/ucob/03_bahamut/)
  - [Materia Raiding](https://ff14.toolboxgaming.space/?id=740246169786361&preview=1)

### Bug Fixes

* Fixed the links opened by the **Main Strategy** picker in **UMAD P1: Tele-Trouncing**.

### Other

* Made a large number of internal changes and updates to prepare for the upcoming **0.8.0** update.

## Known Issues

-   **Linux windowed mode** can behave erratically and sometimes changes shape during scene loads.  
    **Workaround:** Don’t maximize the window, or play in fullscreen instead of windowed.
-   Bots may **fail** certain mechanics/timelines if you use one of the new naming schemes.  
    **Workaround:** Revert to the original naming style (Option 1).
	
	**NOTE:** Not fully confirmed yet, but bot names have been hardcoded for a long time in some places, so if you hit any issues like this, tell me ASAP so I can patch them.

---

Been reprogging UCOB recently and it is a fun fight. As always, let us know if you spot any problems through [GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D)!
