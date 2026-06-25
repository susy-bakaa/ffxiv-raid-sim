This patch has some more changes to **UMAD P1: Tele-trouncing**, implements the post hotfix behavior that the real game got and updates strategies to the latest versions.

## v.0.7.6 Changelog

### Changes

- Added the newly released **LPDU P1 raidplan** strat as an option to the final Mystery Magic.
- Added an option to set a simulated ping value for the server tick simulation in the global settings.
- Updated **arrow (Tele-portent) AOE** in **UMAD P1: Tele-trouncing** to use the same logic for moving the characters as the real game after the hotfix.

  **NOTE:** Your character is now teleported relative to the middle of the arrow debuff.
- Updated all of the existing strategies to their latest versions and positions.
- Changed how player characters are fundamentally updated and added experimental server tick simulation.
  
  **NOTE:** This is a really big change and it's still super early and experimental, currently it has been only enabled for **UMAD P1: Tele-trouncing** and no other timelines yet. I will be doing further work on this in the future and implementing it to more timelines. It changes the feeling of mechanics closer to what they feel in-game, the ping option is also currently only related to this change.

## Known Issues

-   **Linux windowed mode** can behave erratically and sometimes changes shape during scene loads.  
    **Workaround:** Don’t maximize the window, or play in fullscreen instead of windowed.
-   Bots may **fail** certain mechanics/timelines if you use one of the new naming schemes.  
    **Workaround:** Revert to the original naming style (Option 1).
	
	**NOTE:** Not fully confirmed yet, but bot names have been hardcoded for a long time in some places, so if you hit any issues like this, tell me ASAP so I can patch them.

---

If anyone sees this, please tell LPDU to stop changing the strats... As always, let us know if you spot any problems through [GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D)!
