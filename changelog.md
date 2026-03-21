This update focuses on cleaning up a few issues that slipped into **v.0.7.0**, mainly around **M12S P2: Idyllic Dream** and **WebGL**. Nothing huge, but these fixes should offer some nice polish.

## v0.7.1 Changelog

### Fixes & Improvements

-   Updated the **Doom Laser AOE** prefab in **M12S P2: Idyllic Dream** to prevent incorrect aiming.  
    
	**Note:** Since the Dark-element tower is always on the south side of the arena (true north), the prefab can safely be authored facing south, preventing erratic directions if runtime rotation happens to fail.
-   Fixed the **Emergency Meeting** strat in **M12S P2: Idyllic Dream** having **Pyretic** and **Earth** positions reversed for the post-tower debuffs.
-   Fixed the telegraph looking incorrect in **M12S P2: Idyllic Dream** for **Lindwurm's Meteor** in the **WebGL** version.
-   Fixed **default hotbars** not loading correctly on **WebGL**.  
    
	**Note:** This is currently a bit of a temporary quick fix, so be sure to give hotbar's a few seconds to load in if you haven't customized them yet.

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