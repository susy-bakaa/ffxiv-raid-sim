This patch focuses on a handful of usability and reset-related issues, plus a few important fixes for **M12S P2: Idyllic Dream** (especially around reenactment clones and hitbox visuals). The tutorial popups also got a refresh.

## v.0.7.2 Changelog

### Fixes & Improvements

-   Updated and refreshed the **tutorial popups**.
-   Fixed an issue where **resetting too quickly** after dying to an **out-of-bounds trigger** could cause the screen to get stuck fully black.
-   Fixed an issue where changing **keybinds for hotbar slots** would not visually update the slot until other UI actions occurred.
-   Fixed **Lindwurm hitbox visuals** in **M12S P2: Idyllic Dream** not resetting correctly if you reset the timeline while its size was temporarily altered.
-   Fixed an issue where **boss clones** did not execute their animations or disappear correctly in both **Reenactment mechanics** in **M12S P2: Idyllic Dream**.
-   Fixed a rare issue where the **target bar** could remain stuck on-screen after resetting a timeline.

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

Quick small hotfix-esque patch for few issues. As always, let us know if you spot any problems through [GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D)!