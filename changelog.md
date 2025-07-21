This is a **bigger update** with tons of internal improvements, visual upgrades, and exciting new features. Most notably, I am excited to welcome the **first external contributor** to the project **Worst Aqua Player**, who created the new timeline: **UWU P5: Predation**! There's also a new **minimap**, **refined mechanic visuals**, **UI sound effects**, and much more.

## Changelog:

### New Features

-   Added a new timeline: **UWU P5: Predation**, created by **Aqua**, the simulator’s first external contributor and now its **second developer**!
-   Added a **minimap** to all timelines.
-   Added a **custom mouse cursor** and an optional **software cursor toggle** in the settings.
-   Added a global setting to **toggle deaths off** across all timelines.
-   Most **UI elements** now have **sound effects** for better feedback and feel.
-   Added **damage type icons** to **DoT debuff popups** in the damage feed for clearer feedback.
-   Added a **new custom shader** for all **tether mechanics**, improving clarity and visual quality.
-   Added **refined visuals** to many mechanics across **nearly all timelines** for a more polished experience.

### Changes

-   **Improved targeting performance** by offloading heavy logic to a **background thread**. Hopefully this should make things feel snappier across the board.
-   **Reworked asset bundle handling**: Resources are now split into **smaller, shared bundles**, improving load efficiency and reducing redundancy.
-   **Improved WebGL saving**: Settings should now **persist even after closing the tab**, making the web version much more usable.
-   Combined the two **M4S: Sunrise Sabbath** timelines into a single scene, with a new **strat selector** similar to **M8S: Terrestrial Rage**.
-   Updated the **timeline selector** to group timelines in a more logical order.
-   Updated the **action recast text** with a **stronger border** for better readability.
-   Renamed and updated the description of the debuff in **CODC: Grim Embrace** to match the in-game names.

### Bug Fixes

-   Fixed an issue where **HUD input blocking** missed a specific case, causing inconsistent behavior.
-   Fixed various **broken** or **missing** timeline functionalities across multiple fights.
-   Fixed **target info panels** being interactable even when not visible.
-   Fixed **status effects** being interactable for every character instead of just the currently targeted one.
-   Fixed **fade to black transitions** between scenes, as this had been broken for a while but is now working again.
-   Fixed the boss **Omega** being unclickable in both **Helloworld** timelines.
-   Fixed an issue where **self-targeting actions** were not functioning correctly.

---

This update brings a massive round of polish and paves the way for more and easier contributions and improvements in the future. Big thanks to **Aqua** for joining the development effort and making an amazing debut with the new UWU P5 timeline!

As always, report any issues you may come across [here on GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or contact me through DMs on Discord or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D).
