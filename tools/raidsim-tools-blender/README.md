# raidsim-tools

`raidsim-tools` is a Blender addon designed to improve the workflow and speed of working and processing models for the **FFXIV Raidsim**.

---

## Features

- Import animations from multiple different source FBX files from dedicated folder and link them to an existing model armature.
- Clear all animations, actions and NLA strips for selected armature.

---

## Output

The addon does the following:

- Combines and adds multiple animations into one armature.

---

## Requirements

- Blender 4.3.0 minimum
- A 3D model
- Bunch of animations for the same armature but in separate files

---

## How to Use

This section contains simple instructions on how to use this addon. If you want more in-depth steps with pictures included check out the raidsim docs.

### 1. Setup

First you need to manually install this as a blender addon. I recommend you download the release .zip for this addon from the releases section, otherwise you can create a manual zip from the "raidsim_tools" folder. Install and enable it like any other addon.

### 2. Usage

You need an empty scene, then just import a FBX or other model and select the armature. Find the N-Panel called "Raidsim Tools" and pick the folder your separate animation files are stored in. Then just press "Import and Link Animations". Your model is now ready and you can export it if you want to. Existing animations can be cleared with the "Clear All Actions on Armature" button if you want to redo the animations or fix a model that breaks somehow.

### 3. Automation

This addon is required and supports being ran automatically from the `xivAnim` program. It automatically does the steps described above and outputs a .blend file and exported final .fbx file.