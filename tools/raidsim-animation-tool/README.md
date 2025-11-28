# xivAnim

`xivAnim` is a utility tool designed to extract and process **boss models** and **animations** from **Final Fantasy XIV** game files and raw files. It converts the assets into a format compatible with **FFXIV Raidsim**, streamlining the process of integrating new enemies into timelines.

---

## Features

- Extracts requested raw skeleton and animation files directly from the game.
- Exports the raw animation data into FBX through MultiAssist.
- Combines the exported animations into a single model FBX ready for Unity import.

---

## Output

The tool generates the following files:

- Raw skeleton and animation files for specified models
- Exported animation clips in FBX format
- Final combined FBX 3D model containing the boss mesh and all requested animations

---

## Requirements

- 64-bit Windows, Linux support currently unknown
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

You need to now configure the tool. Here is an example configuration `config.json` file:
```json
{
    "gamePath": "C:/Games/Steam/steamapps/common/FINAL FANTASY XIV Online/game/sqpack",
    "multiAssistExe": "F:/Tools/MultiAssist/MultiAssist.exe",
    "blenderExe": "C:/Program Files (x86)/Steam/steamapps/common/Blender/blender.exe",
    "outputRoot": "F:/Tools/TexTools/Saved/Mounts",
    "jobs": [
        {
            "name": "d1024e0001_top_v0",
            "modelPath": "3D/d1024e0001_top.fbx",
            "skeletonGamePath": "chara/demihuman/d1024/skeleton/base/b0001/skl_d1024b0001.sklb",
            "papGamePaths": [
                "chara/demihuman/d1024/animation/a0001/bt_common/resident/idle.pap",
                "chara/demihuman/d1024/animation/a0001/bt_common/resident/action.pap",
                "chara/demihuman/d1024/animation/a0001/bt_common/resident/event.pap",
                "chara/demihuman/d1024/animation/a0001/bt_common/resident/demihuman.pap"
            ],
            "appendFileNames": [
                "mon_sp0"
            ]
        }
    ]
}
```
Let's go through this one by one. "gamePath" is your game data path, this should be the 'sqpack' folder inside your game installation directory. "multiAssistExe" is the path to your MultiAssist installation and executable. "blenderExe" is your Blender installation and executable. "outputRoot" is the folder the tool will use as the base folder for all of it's operations, each created file will be placed inside subfolder with the "name" parameter from each of the "jobs" list entries as the folder name. If you used my recommended setup this should match the TexTools folder name that has the `3D` subfolder inside it. "modelPath" is the original model that will be processed and the animations will be added to, this should be the one exported from TexTools or similar program. "skeletonGamePath" is the game data path for the skeleton, that will be extracted from your own game installation, put in the value we obtained from ResLogger2 or other sources. "papGamePaths" is all of the animations you want to extract and stuff into this model, these paths are also game data paths, so use the ResLogger2 paths. "appendFileNames" is an optional list of names, where if the animation pap file name matches this, it will append the pap file name into the final exported animation fbx name, this is a bit more advanced setting and mainly should be used for animations that don't have unique names, such as the mon_sp00X pap files.

### 3. Running the Tool

Next you are ready to just run the tool with your json as the parameter.
```bash
xivAnim.exe config.json
```
This should result in the following folders being created:
- Animations, which contains the raw extracted pap files.
- Exported Animations, which contains all of the animations from every pap file as separate FBX file.
- Skeleton, which contains the extracted raw skeleton file MultiAssist requires.
- The final combined model FBX in the job's respective output folder, named after the original model file, e.g. "d1024e0001_top" if using my config file from above.