## Tools

This folder contains all the internal **tools and utilities** used to build and maintain the **FFXIV Raidsim**, including custom asset processing tools and the updater system.

These tools are not most likely required for regular users of the simulator, but are useful for development, asset conversion and maintenance of the project.

---

## Contents

### ffxiv-raid-sim-updater

A standalone binary used by the main simulator to apply updates after they are downloaded.  
It handles file replacement and integrity checks for installed builds.

### librsim

A standalone Linux library used by the main simulator program to extend default behavior of the Unity runtime.

### xivAnim

A specialized C# console app for extracting and processing **FFXIV models and animations** from game data into a format used by the simulator.  
It automates the workflow of extracting and setting up singular FBX models with all of the original model's respective animations included.

### raidsim-tools

Blender addon that adds useful tools for working on raidsim models. 
Allows importing and linking of multiple animations from different source files into one model armature.

---

## Tool-Specific Docs

Each tool has its own README with usage information and requirements.
