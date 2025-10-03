This is a **small** but **important** update focused on restoring controller input support and patching the new Unity engine **vulnerability**. **WebGL** controller input should now work on most browsers as well if you're using a controller.

## Changelog

### Fixes & Updates

-   Fixed some **controller input issues** and controller support is now working correctly again. It should also function in **WebGL** builds on most **Chromium**-based browsers now.  
    <b>NOTE:</b> There might still be compatibility issues with **Firefox** when using controller input. Try a different browser if you run into any.
-   Upgraded the Unity engine version to patch a **security vulnerability**.
    <b>NOTE:</b> More information about this can be found [here](https://flatt.tech/research/posts/arbitrary-code-execution-in-unity-runtime/).

---

### Known Issues

-   **Windowed mode** behaves a bit erratically on **Linux**, sometimes resizing on it's own or shifting unexpectedly while loading scenes.
-   **Gap closers** always move you to the edge of a boss’s hitbox, even if you're already inside it.

---

Short and sweet, but important. As always, let us know if you spot any problems through [GitHub](https://github.com/susy-bakaa/ffxiv-raid-sim/issues) or send a message on the [official Discord server](https://discord.gg/wepQtPfC6D)!