# xivAnim

`xivAnim` is a utility tool designed to extract and process **boss models** and **animations** from **Final Fantasy XIV** game files and raw files. It converts the assets into a format compatible with **FFXIV Raidsim**, streamlining the process of integrating new enemies into timelines.

---

## Features

- Extracts requested raw skeleton and animation files directly from the game.
- Exports the raw animation data into FBX through MultiAssist.
- Combines the exported animations into a single FBX model with loops marked ready for Unity import.

---

## Output

The tool generates the following files:

- Raw skeleton and animation files for specified models
- Exported animation clips in FBX format
- Final combined FBX 3D model containing the boss mesh and all requested animations

---

## Requirements

- 64-bit Windows, Linux support currently untested but coming in the future!
- .NET 8.0 Runtime
- MultiAssist installation
- Blender installation with my XIV model processing addon
- FFXIV installation or partial installation
- Access to pre-extracted FFXIV FBX model and textures of your choice (via TexTools, SaintCoinach or custom datamine)
- A list of the game data paths for your model's skeleton and all animations you want to export

---

## How to Use

This section contains simple instructions on how to use the tool. If you want more in-depth steps with pictures included check out the raidsim docs.

### 1. Setup

First you need to manually export the boss model of your choice, as this tool does not handle the meshes or textures. I recommend using TexTools for this.

Ensure you have a working folder for your model, I recommend the same folder TexTools extracts it's models into if you are using it already. As an example, I have my export for the 'Omega' model (ID m0515) in the following folder: 
```bash
F:\FFXIV\Tools\TexTools\Saved\Mounts\m0515b0001_v0
```
Inside this folder you should already have a `3D` subfolder with your exported FBX mesh and textures.

Next you have to obtain a list of the internal game data paths for each animation you want and the skeleton of your model. Easiest way to accomplish this is to just run the duty with the boss and log the paths with ResLogger2, this plugin outputs the paths in the exact format we need. Alternative options include using FFXIV Data Explorer or another similar tool and manually writing down each path you want.

### 2. Tool config

First of all you need to set the global configuration values. The easiest way to do this is with the GUI version. Just open up the xivAnim.Gui.exe and press Edit -> Settings... to configure your global settings. Here is an example config created by the tool, the global config lives in `%AppData%/Roaming/xivAnim/config.json`:

```json
{
  "ffxivGamePath": "E:\Modding\FFXIV\Installation\FINAL FANTASY XIV Online\game\sqpack",
  "multiAssistPath": "F:\Modding\FFXIV\Tools\MultiAssist\MultiAssist.exe",
  "blenderPath": "C:\Program Files (x86)\Steam\steamapps\common\Blender\blender.exe",
  "RecentJobs": [
    "F:\Modding\FFXIV\Tools\xivAnim\god_kefka.json",
    "F:\Modding\\FFXIV\\Tools\\xivAnim\kefka.json",
    "F:\Modding\FFXIV\Tools\xivAnim\job.json"
  ],
  "debugMode": false
}
```
Let's go through this one by one. "ffxivGamePath" is your game data path, this should be the 'sqpack' folder inside your game installation directory. This supports partial installations. "multiAssistPath" is the path to your MultiAssist installation and executable. "blenderPath" is your Blender installation and executable. "RecentJobs" is not something you fill out yourself, but the tool adds them as you make and run jobs. "debugMode" is for if you want more verbose logs.

After you have the global config setup, you need to now configure a job the tool can run. Here is an example configuration `kefka.json` file:

```json
{
  "name": "m0462b0001",
  "workingDirectory": "F:\Modding\FFXIV\Tools\TexTools\Saved\Mounts\m0462b0001_v0",
  "exportDirectory": "F:\Modding\FFXIV\Tools\TexTools\Saved\Mounts\m0462b0001_v0",
  "modelPaths": [
    "F:\Modding\FFXIV\Tools\TexTools\Saved\Mounts\m0462b0001_v0\3D\m0462b0001.fbx"
  ],
  "skeletonGamePath": "chara/monster/m0462/skeleton/base/b0001/skl_m0462b0001.sklb",
  "skeletonLocalPath": "",
  "papGamePaths": [
    "chara/monster/m0462/animation/a0001/bt_common/resident/monster.pap",
    "chara/monster/m0462/animation/a0001/bt_common/idle_sp/idle_sp_1.pap",
    "chara/monster/m0462/animation/a0001/bt_common/mon_sp/m0462/mon_sp001.pap",
    "chara/monster/m0462/animation/a0001/bt_common/mon_sp/m0462/mon_sp002.pap",
    "chara/monster/m0462/animation/a0001/bt_common/mon_sp/m0462/mon_sp003.pap",
    "chara/monster/m0462/animation/a0001/bt_common/mon_sp/m0462/mon_sp007.pap",
    "chara/monster/m0462/animation/a0001/bt_common/mon_sp/m0462/mon_sp008.pap",
    "chara/monster/m0462/animation/f0000/resident/face.pap"
  ],
  "papLocalPaths": [],
  "appendFileNamesForPaths": [
    "m0462/mon_sp\\d{3}"
  ]
}
```
Let's go through this as well one by one. "name" is simply the name of this model, if following the official raidsim naming scheme this should be set to the ffxiv model name as shown above. "workingDirectory" is simply the folder that will be used for storing all of the extracted game files, all of them inside their own subfolders of course. I recommend setting this to the TextTools extract folder for each model as that keeps all of the files cleanly together. "exportDirectory" is an optional directory to where the final .blend, .fbx and animation events .xml will be placed after the tool has finished. I recommend just using the same folder as the working directory. "modelPaths" are the original models that will be processed and the animations will be added to, this should be the one exported from TexTools or similar program. The reason this is in plural is because the tool supports demi-humans. To process a monster you just need the one base model but for demi-humans you can add all of their parts here and the tool will combine them into one final mesh. "skeletonGamePath" is the game data path for the skeleton, that will be extracted from your own game installation, put in the value we obtained from ResLogger2 or other sources. "papGamePaths" is all of the animations you want to extract and apply to this model, these paths are also game data paths, so use the ResLogger2 paths. "appendFileNamesForPaths" is an optional list of file paths, where if the animation pap file's GAME PATH matches this, it will append the pap file name into the final exported animation fbx name, this is a bit more advanced setting and mainly should be used for animations that don't have unique names, such as the mon_sp00X pap files, as they can have duplicate animations inside them with the same names.

### 3. Running the Tool

Next you are ready to just run the tool. You can either load your config into the GUI version and press "Run", or you can use your json as the parameter for the CLI version.
```bash
xivAnim.exe -j job.json
```
This should result in the following folders being created:
- Animations, which contains the raw extracted pap files.
- Exported Animations, which contains all of the animations from every pap file as separate FBX files.
- Skeleton, which contains the extracted raw skeleton file MultiAssist requires.
- The final combined model FBX in the "exportDirectory" output folder, named after the job, e.g. "m0462b0001.fbx" if using my config file from above.