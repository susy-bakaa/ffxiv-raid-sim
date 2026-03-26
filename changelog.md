This update expands macro support, improves action handling to feel closer to the real game, and brings a bunch of accuracy tweaks and quality-of-life improvements to **M12S P2: Idyllic Dream**. It also fixes several major issues introduced in earlier builds.

## v.0.7.3 Changelog

### New Features

-   Added macro wait support:
    -   **`/wait X`**
    -   **`<wait.X>`**
-   Added **4+2+2 melee uptime stack** option to **M12S P2: Idyllic Dream**.
-   Added a simple **action queue/buffer system**, allowing you to buffer actions more like in-game.
-   Added **export/import** support for **M12S P2: Idyllic Dream** timeline configuration.  
    
	**NOTE:** This only exports/imports shared configuration values. Strategy-specific "personal preference" options are intentionally left untouched. This feature will be added to future timelines where it makes sense.

### Changes

-   Tweaked existing commands and added a few new ones.
-   Tweaked **Lindwurm's** default boss hitbox size in **M12S P2 Idyllic Dream** to better match the game.
-   Tweaked **Mana Burst AOE** size in **M12S P2 Idyllic Dream** to better match the game.
-   Tweaked intercardinal **player clone positions** in **M12S P2 Idyllic Dream** to better match the game.
-   Changed the default audio volume to **50%**.
-   Changed the font used by **Chat** and the **Macro Editor**.  
    
	**NOTE:** This font has much better compatibility with FFXIV glyphs. Some may still be missing, but most should now render correctly.

### Bug Fixes

-   Fixed an issue where actions ignored **range checks**, allowing them to execute from any distance.
-   Fixed an issue where **dashes and gap closers** could cause the player to fall through the arena under certain conditions.
-   Fixed an issue in **M12S P2: Idyllic Dream** where boss clone tether visuals post arena split could become incorrect.  
    
	**NOTE:** This was a side effect of making tether logic more accurate. Players can now only pick up **one tether**, and if multiple are picked up they will be randomized across the party so everyone ends up with one.

## Known Issues

-   **Linux windowed mode** can behave erratically and sometimes changes shape during scene loads.  
    **Workaround:** Don’t maximize the window, or play in fullscreen instead of windowed.
-   Bots may **fail** certain mechanics/timelines if you use one of the new naming schemes.  
    **Workaround:** Revert to the original naming style (Option 1).
	
	**NOTE:** Not fully confirmed yet, but bot names have been hardcoded for a long time in some places, so if you hit any issues like this, tell me ASAP so I can patch them.

---

A bit bigger patch for few issues and new changes. As always, let us know if you spot any problems through [GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D)!