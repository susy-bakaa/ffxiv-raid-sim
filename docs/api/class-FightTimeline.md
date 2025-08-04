# FightTimeline Component Overview

The `FightTimeline` component is the central controller for each fight scene in the raidsim. It coordinates timeline execution, random mechanics, character behaviors, and bot actions. This document explains the core functionality and purpose of the main fields and methods exposed to developers.

---

## Purpose

`FightTimeline` acts as the brain of a fight scene. It drives the timeline, schedules and executes events, and maintains shared references to the scene's characters, bots, mechanics, and UI. Most fight-specific logic is built around configuring the timeline.

---

## Key Responsibilities

### Timeline Control

- **StartTimeline()**: Begins executing the configured `TimelineEvent` list, sequentially triggering fight events with delays.
- **ResetTimeline()**: Resets the fight back to its initial state, including characters, nodes, bots, and effects.
- **playing / paused**: Used to control and monitor whether the fight is actively running or halted.

### Scene References

- **input / player / partyList / enemyList**: Reference to core scene actors like the player, input manager, and party/enemy lists.
- **mechanicParent / charactersParent / enemiesParent**: Containers for spawned mechanics and characters.
- **botNodeParent / botTimelineParent**: Hold AI navigation and bot timeline objects.

### Event Execution

- **TimelineEvent list**: Configurable fight events that define what happens and when (e.g., boss attacks, target switching).
- **TimelineCharacterEvent**: Sub-events per character such as triggering mechanics, performing actions, buffs/debuffs, or movement.

### Randomization System

- **RandomEventPool / RandomEventResult**: Used to inject randomness into fight timelines. These allow certain actions or effects to vary each run.
- **randomEventResults**: Tracks resolved random results so they can be reused consistently or changed between resets.

### Arena State

- **arenaBounds / isCircle**: Defines the playable area shape and size. Can be changed during the fight with `ChangeArena()`.

### UI and Playback Management

- **disableDuringPlayback[]**: UI buttons that are automatically disabled when the fight starts and re-enabled when it ends.
- **onReset / onPlay / onPausedChanged**: Events for UI or logic to hook into timeline state changes.
- **TogglePause() / ResetPauseState()**: Manage pause state through label-based toggling (multiple systems can pause independently).

---

## Automation Features

- **BotTimeline integration**: Bot timelines are started in sync with the timeline if enabled.
- **UseAutomarker toggle**: Tracks whether the current timeline should use automarkers or not.
- **Jon**: A debug toggle (for special voice lines or features).

---

## Editor Utilities

- **WipePartyButton()**: Triggers a simulated full party wipe (used during debugging).
- **OnValidate() / Reset()**: Auto-corrects values and links scene references when modified in the Unity Editor.

---

## Summary

Use `FightTimeline` to:

- Build fight flow with precise event timings
- Configure random behavior or mechanic variants
- Reset and restart the timeline cleanly
- Integrate bots, mechanics, and scripted character actions
- Hook into timeline states for UI or auxiliary systems
