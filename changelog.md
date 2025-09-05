This update brings bit better **Linux compatibility**, some minor **WebGL improvements**, and a few **new contributions** from the community! With automatic updates now working on **all platforms** and new **Game8 strats** added for two timelines.

## Changelog

### New Features

-   Added **Game8 strats** to both **M8S: Terrestrial Rage** and **M8S: Beckon Moonlight** timelines. Thanks **khanh-alice** for the contribution!
-   Added a **WebGL server uptime tracker** to the UI, so you can now see the current status of the bundle server at a glance.
-   The **automatic updater now supports Linux**, making update handling seamless on **all platforms**.

	<b>NOTE:</b> For the updater to work properly, you need to ensure the application is located in a folder where your user has full access and permissions. For more info check the updated <b>README</b>.

### Bug Fixes

-   Fixed an issue where **non-interactable** or **disabled UI elements** were still playing **interaction sounds**.
-   Fixed the **settings confirmation popup** not playing any audio in the main menu.
-   Fixed **mouse cursor size** being incorrect on **Linux**.
-   Fixed the **resolution dropdown** not working properly on Linux.
-   Fixed **mouse cursor offset issues** on Linux.
	
	<b>NOTE:</b> With these changes the Linux build should now be fully tested and working almost just as well as the Windows one, minus the one known issue. If you encounter more problems let us know.

### Known Issues

-   **Windowed mode** behaves a bit erratically on **Linux**, sometimes resizing on it's own or shifting unexpectedly while loading scenes.
-   **Gap closers** always move you to the edge of a boss’s hitbox, even if you're already inside it.

---

Thanks again to everyone who helped with testing, reported any bugs, or contributed new strats or timelines. As always, let us know if you spot any problems through [GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D)!