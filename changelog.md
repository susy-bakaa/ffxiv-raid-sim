This update adds a brand-new timeline for the new 7.51 Ultimate, **UMAD P1: Tele-trouncing**, alongside several small updates & improvements, including role-based auto attacks, internal system reworks and various bug fixes.

## v.0.7.4 Changelog

### New Features

- Added a new timeline: **UMAD P1: Tele-trouncing**. Currently supported strategy options include:

  - [Freaky MGR (gD-)](https://raidplan.io/plan/qD9Y_g1caq3l5gD-)
  - [Merry-Go-Round / Big Box](https://docs.google.com/presentation/d/1-E2rEKa586KKiVNvtt3EAMQY2YAEVBRGcMX0WzORIq8)
  - [Filipino Box (ud5)](https://raidplan.io/plan/5rf2uhud5ztsbud5)
  - [Modified Xolo (X13)](https://raidplan.io/plan/p8JvSSs1_QKMVX13)

### Changes

- Added better **mouse cursor handling on Linux**.
  
  **NOTE:** The cursor should now restore to its previous location after rotating the third-person camera, similar to how it works on Windows.
- Changed **auto attacks** to work based on the selected role.
- Reworked several internal systems.

### Bug Fixes

- Fixed an issue where **alternative bot names** were not always applied correctly.
  
  **NOTE:** There is still a slight delay when hard-loading a timeline for the first time. This is intended due to how the names are applied.
- Fixed various other bugs that appeared during development or were already known.


## Known Issues

-   **Linux windowed mode** can behave erratically and sometimes changes shape during scene loads.  
    **Workaround:** Don’t maximize the window, or play in fullscreen instead of windowed.
-   Bots may **fail** certain mechanics/timelines if you use one of the new naming schemes.  
    **Workaround:** Revert to the original naming style (Option 1).
	
	**NOTE:** Not fully confirmed yet, but bot names have been hardcoded for a long time in some places, so if you hit any issues like this, tell me ASAP so I can patch them.

---

A more miscellaneous patch with new timeline and set of changes. As always, let us know if you spot any problems through [GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D)!