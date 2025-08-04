# Project Architecture

This page explains the core structure of the **FFXIV Raidsim** project. The architecture is built around modular, self-contained scenes, globally shared managers and data, and a consistent layout that allows for simulation of various FFXIV mechanics in a easy and reusable way.

---

## Global Architecture

The project relies on a few foundational components that exist across all scenes:

### Singleton Managers
Several systems in the project are managed via singleton objects (MonoBehaviours marked as DontDestroyOnLoad) that are instantiated in a temporary startup scene. These handle general responsibilities such as:

- Loading asset bundles linked to scenes
- Saving any configuration values to disk or memory
- Checking for program updates
- Automatically downloading updates from GitHub

### GlobalVariables
A static class used to store global values and configuration variables that are accessed across multiple systems. This includes:

- Game version info
- File system paths (e.g., to config folders or asset bundles)
- Other miscellaneous info

### GlobalData
A static container of shared data structures used throughout the progarm. These include:

- `Damage` struct – for encapsulating FFXIV-like damage instances
- `ActionInfo` struct – for simple way to pass actions with their source/target characters included
- `Flag` class – a boolean with multi-source support that can dynamically resolve its final value based on contributors
- Other important miscellaneous data structures and variables

---

## Scene Lifecycle and Flow

The application starts in a lightweight **bootstrap scene**. This scene initializes the global singleton managers and then immediately loads the **Main Menu scene**.

From the **Main Menu**, the user can:

- Enter the first timeline scene, the demo
- Then choose and load a fight timeline scene of their choice from a dropdown menu

Each fight timeline scene is entirely self-contained, with its own assets and logic. These scenes simulate specific mechanics and encounters.

---

## Fight Timeline Scenes

Every timeline scene is structured the same way for consistency and modularity. Some of the key structure elements include:

### Scene-Level Singletons
- **UserInput** – A prefab instance per scene that manages user inputs; values can be overridden per scene.
- **FightTimeline** – A static singleton GameObject that contains the timeline controller. It dictates the main flow of the fight, controls boss behavior, stores randomized event results for later use, and times the execution of major boss actions and mechanics.

### Timeline Hierarchy

- **FightTimeline** (GameObject)
  - Contains attack sequencing and timing logic for the main boss
  - Has a child GameObject that contains the `UserInput` script
  - Hosts child GameObjects for organizing:
    - `BotTimelines`: all player bot timelines
    - `FightMechanics`: all isolated more complex mechanic chains

### Characters and Enemies
Two GameObjects exist at the scene root:

- `Characters`: holds all player characters (real one and the bots)
- `Enemies`: holds the main boss and any other adds or bosses involved in the fight

Each character has:

- A `Model` child: Contains the 3D model for the boss, loaded via `ModelHandler` from the scene's asset bundle
- An `Actions` child: Contains all the weaponskills, spells, and abilities of the character
- A `StatusEffects` child: Contains all the runtime-spawned instances of the effect prefabs
- A `VisualEffects` child: Contains all VFX objects of the character, most using `CharacterEffect` components
- A `Nameplate` prefab: Contains basic setup for all character nameplates
- A `Pivot` child: Empty transform that is used for tethers and other mechanics that need to visually "attach" to the player
- A `TargetNode` prefab: Contains the component for targeting of the character, has values like hitbox size

Most of these objects (such as actions and status effects) are based on prefabs that get instantiated and perform their behavior and visuals but their actual information and configuration is defined via ScriptableObject based data assets:

- **`ActionData`**: Defines the properties and behavior of weaponskills, spells, and abilities.
- **`StatusEffectData`**: Describes buffs, debuffs, and other effects applied to characters.

These ScriptableObjects make it easy to reuse, modify, and extend content without altering the underlying main prefabs.

### User Interface
The UI is stored and handled under a `UserInterface` GameObject, which has the `Canvas` component.

The `Canvas` contains the following elements but is not limited to them:

- A `PartyList`child: Stores all UI elements for all of the player characters (bots included)
- A `EnemyList`child: Stores all enemies present in the timeline as list, works as the enemies' "party list"
- A `Hotbar` child: Stores all of the Actions available to the player and displays them
- A `SettingsMenu` prefab: Stores all of the global simulator settings, same menu as available in the main menu

### Environment
The visual environment for each timeline is stored under an `Environment` GameObject.
This object contains some of the following as it's children:

- Arena model (placeholder by default, runtime arena model is loaded via `EnvironmentHandler`)
- All lights
- All global particles
- Any death wall or out of bounds triggers

### Collision
The physical environment for each timeline is stored under a `Collision` GameObject.

This GameObject usually has one or two children that have the static colliders for the arena.

### Mechanics
Most dynamic prefabs are spawned at runtime under the `Mechanics` GameObject.

This GameObject usually contains any of the following children:

- AOEs
- Tethers
- Towers
- Other mechanics and effects

### AI Nodes
The `AINodes` GameObject stores navigation transforms and reference points for the bots, bosses and mechanics:

Some of the contained objects include:

- Waymarks
- Bot Nodes
- Bot Node Groups
- Mechanic Nodes
- Other miscellaneous objects

---

This structure ensures fully modular, easily expandable, and repeatable configuration for all fight timelines and mechanics. Most of the complexity is encapsulated within scene-specific prefabs or GameObjects and runtime data-driven systems, allowing for easier content creation and maintenance.

## Next Steps

- [Creating Content](creating-content/adding-scriptable-objects.md)
