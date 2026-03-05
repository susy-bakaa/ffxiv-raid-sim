This is a **big update** that expands **M12S P2: Idyllic Dream** with a pile of new strategies, adds a much more FFXIV-like **action + macro workflow**, and fixes a bunch of long-standing annoyances. It also introduces a new RNG system and more options for practice and self-testing.

## v0.7.0 Changelog

### New Features

-   Added multiple new strategies to **M12S P2: Idyllic Dream**:
    -   [**JP Game8 / Nukemaru**](https://xivjpraids.com/7.0_dawntrail/savage_raids/m12s_2/)
    -   [**Brain Deluxe Uptime Flavour (jN9)**](https://raidplan.io/plan/FBZLq8AaMOU0NjN9)
    -   [**Zenith Uptime Idyllic (9xd)**](https://raidplan.io/plan/jwJzsjj4-KmsZ9xd)
    -   [**Emergency Meeting (Lt2)**](https://raidplan.io/plan/buBeeLVDS9lTlLt2)
-   Added proper **floor textures** to the **M12S arena**.
-   Added a new **RNG implementation** + settings for controlling how randomness behaves.  
    
	**NOTE:** This should fix the "same RNG" button not working.
-   Added an option to **hide the bots** so you can genuinely test memory/knowledge.
    
	**NOTE:** Some timelines might be really hard or impossible to complete with these settings, so try them out!
-   Added **/mark** (or **/mk**) command to manually mark yourself or party members with the sign of your choice.
-   Added the **Command Panel** for custom commands/macros.
-   Added **Action Menu** (default key: **P**), which let's you drag actions onto hotbars.
-   Added **Macro Menu** (default key: **K**), which let's you create or import FFXIV macros from the game and drag them onto hotbars.
-	Added the **Main Menu Bar** that has quick access to few menus and features.

### Changes

-   **Completely rewrote the action system** to support macros and make future development easier.  
    -   Players can now place **any actions** onto their hotbars (restrictions still apply to action usability).
    -   The available action set has been greatly expanded.
    -   Keybinds are now tied to **hotbar slots** (FFXIV-style), not specific actions.
    -   **For Controllers,** the action used on **right stick click** must now be placed in **slot 11** (2nd last) on the main hotbar. **Sprint** will be there by default.
    
	**NOTE:** Some of the new actions available are experimental and some are not required for anything and purely optional. The keybind change also means you will need to redo your keybinds.
-   Renamed some strat dropdowns and updated help texts to include the new strats.
-   Updated windowed mode naming logic and added **experimental Linux support** for the custom window name behavior.

### Fixes & Optimizations

-   Fixed a few small misc issues and optimized some functions.
-   Fixed the out-of-bounds hitbox in **M12S P2: Idyllic Dream**.
-   Fixed **waymarks** not showing up when nothing had been saved yet.
-   Fixed waymarks not refreshing when changing main strategy while set to **"same as main strategy."**
-   Fixed mouse cursor sometimes getting stuck invisible when right clicking in certain conditions.
-   Fixed the player camera getting stuck in an infinite rotation loop with certain quick input combinations.
-   Fixed **Automarkers** getting permanently disabled under certain conditions.
-   Fixed a problematic extension method causing unpredictable behavior.
-   Fixed some bosses having Unity default physics enabled in a few timelines, causing rare sliding issues.

## Known Issues

-   **Linux windowed mode** can behave erratically and sometimes changes shape during scene loads.  
    **Workaround:** Don’t maximize the window, or play in fullscreen instead of windowed.
-   Chat and macro window may not display some characters supported by FFXIV (placeholder boxes).  
    **Workaround:** Remove those characters or live with the boxes for now.  
    
	**NOTE:** Planned fix is changing to a font with full FFXIV glyph coverage.
-   Gap closers/dashes on certain arenas may cause the player to sink/fall through the ground.  
    **Workaround:** Avoid those broken actions.  
    
	**NOTE:** Should currently only happen in the Demo timeline.
-   In **M12S P2: Idyllic Dream** the boss clone tether visuals post arena split may not disappear if the tethered player previously picked up more than one tether/mechanic.  
    
	**NOTE:** Visual-only issue and should not occur under normal play.
-   Bots may **fail** certain mechanics/timelines if you use one of the new naming schemes.  
    **Workaround:** Revert to the original naming style (Option 1).
	
	**NOTE:** Not fully confirmed yet, but bot names have been hardcoded for a long time in some places, so if you hit any issues like this, tell me ASAP so I can patch them.

---

Biggest changelog since last summer probably, it's been a while since we made this big changes. As always, let us know if you spot any problems through [GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D)!