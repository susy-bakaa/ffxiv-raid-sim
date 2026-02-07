This update adds the newest Savage mechanic to the simulator: **M12S P2: Idyllic Dream**, which is currently the **longest and most complex** piece of content in the whole sim so far. Alongside the new timeline, this update also introduces a chat window, new actions to healers and tanks, improvements to core mechanical systems, and a whole lot of other fixes.

## Changelog

### New Features

-   Added a new timeline: **M12S P2: Idyllic Dream**.

	**NOTE:** This timeline supports both **NA** and **EU Pastebin/Hector** strategies, with optional **Banana Codex** positioning for some parts of it.
-   Added new usable actions:
    -   **Esuna** for Healers
    -   **Generic Tank Mitigation** for Tanks  
    
	**NOTE:** These actions are currently only available in timelines that either require them or have some use for them. They currently share controller bindings with the **3rd Movement Ability**.
-   Added a **chat system** and **chat window**.
    
	**NOTE:** This exists as groundwork for possible future changes making **FFXIV macros** be supported by the sim. For now, certain timelines can post automatic info into chat, and it includes commands to control the timeline/program. This will be expanded later.
-   Added more **bot name display options** (by request).

    **NOTE:** You can now:
    - Match nameplate color to the bot’s role
    - Use role-based unique naming with few different options to choose from
-   Added a **tooltip system** for timeline specific settings.

    **NOTE:** Currently only implemented in **M12S P2: Idyllic Dream**, but this system may be expanded to other timelines later.

### Changes

-   Minimap now **remembers visibility** and stays hidden/visible between reloads.
-   Major behind-the-scenes changes to multiple systems. If something is not working let me know!

### Fixes & Improvements

-   Improvements and fixes to most **tether behavior** across all timelines.
-   Improvements and fixes to most **knockback behavior** across all timelines.
-   Fixed **gap closers** always snapping you to the edge of a boss hitbox (and sometimes even pushing you backwards if you were already inside one).
-   Fixed an issue where **Status Effects** could be removed too quickly, leaving **visual effects** behind.
-   Fixed an issue in the timeline **FRU P2: Diamond Dust**, where the Status Effect **Thin Ice** was not being applied at all.

    **NOTE:** Somehow I had at somepoint just disabled the mechanic? No idea why or how that happened. Oops.

---

## Known Issues

-   **Linux windowed mode** can behave erratically and sometimes changes shape during scene loads.  
    **Workaround:** Don’t maximize the window, or play in fullscreen instead of windowed.
-   Mouse cursor can sometimes get stuck invisible after right clicking in certain conditions.  
    **Workaround:** Left click once anywhere and it should bring it back.
-   Player camera may "freak out" with certain rapid input combinations.  
    **Workaround:** Quick full reload of the timeline fixes it.
-   In **M12S P2: Idyllic Dream** the boss clone tether visuals post arena split may not disappear if the tethered player previously picked up more than one tether/mechanic.  
    **Note:** Visual-only issue and should not occur under normal play.
-   Bots may **fail** certain mechanics/timelines if you use one of the new naming schemes.  
    **Workaround:** Revert to the original naming style (Option 1).
	
	**NOTE:** Not fully confirmed yet, but bot names have been hardcoded for a long time in some places, so if you hit any issues like this, tell me ASAP so I can patch them.

---

One of the longer changelogs once again, it's been a while since the last one. As always, let us know if you spot any problems through [GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D)!