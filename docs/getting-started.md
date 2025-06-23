# Getting Started

Welcome! This guide will help you set up the **FFXIV Raidsim** project on your local machine and download the assets required to run it.

---

## Prerequisites

- [Unity 2022.3 (latest LTS)](https://unity.com/releases/editor/whats-new/2022.3)
- [Git](https://git-scm.com/)
- Optional: Code Editor (e.g., Visual Studio or Rider)
- Optional: [Blender](https://www.blender.org/) (for asset editing)
- Optional: [`gdrive`](https://github.com/prasmussen/gdrive) (for internal contributors using Google Drive storage)

---

## Cloning the Repository

To get started, first clone the repository from GitHub:

```bash
git clone https://github.com/susy-bakaa/ffxiv-raid-sim.git
```

This will give you access to all source files, scripts, and text-based assets.

---

## Downloading Required Assets

The project uses a compressed asset archive to store large files like 3D models and textures. These are not included in the Git repo.

If you're an external contributor, you need to manually download the archive from here:

[Download ffxiv-raid-sim.tar.gz](https://drive.google.com/file/d/1ybYaJ8LGnHwY5jeCv1Zr6B5fT7FHL51i/view?usp=drive_link)

Once downloaded, extract the archive into the following folder path (Basically just match the raidsim folder inside the archive to the repository):

```plaintext
ffxiv-raid-sim\raidsim
```

After extraction, you’re ready to open the project in Unity via:

```plaintext
ffxiv-raid-sim\raidsim
```

---

## Internal Contributors: Using `gdrive_util.bat`

If you are contributing internally and want to **sync your own changes** to the Google Drive archive and are running on Windows:

1. Install [`gdrive`](https://github.com/prasmussen/gdrive) and set it up according to [this guide](https://medium.com/machine-learning-intuition/tutorial-storing-large-a-i-models-with-gdrive-don-t-use-git-lfs-a1aaccdc5b26).
2. Edit the `gdrive_default.txt` inside the `.gdrive` folder with your system-specific values, then save it as `gdrive_config.txt`.
3. Run `gdrive_util.bat` from the root of the repository and follow the command prompt instructions.

This is an explanation of what each line inside the config file has to include:
1. Disk your repository is on
2. Path to your local repository (In a format the windows cd command can read)
3. Name of the archive used with Google Drive
4. File ID of your existing archive on Google Drive (Must be shared to everyone with link)
5. Folder ID of the folder where your archive will sit in (Must be shared to everyone with link)
6. File used for ignored folders and file types (By default `.gdriveignore`)

This tool just allows you to automatically do the following:
- Create archived backups of all binary and large files.
- Upload backups to Google Drive.
- Maintain consistency in archive structure.
- Avoid committing large binaries directly to GitHub.

**Note:** If you don’t have write access to the original Drive archive, just use the manual download method described above. You can also create your own archive backup in Google Drive with gdrive if you want to. If you do any changes to assets that would need to be ported to the main branch of the official simulator let me know through discord (@no00ob).

---

## Building the Project

After opening the project in Unity:

- Let Unity import and compile everything (this may take a few minutes).
- Use any of the provided demo scenes to test features or begin development.

---

## Next Steps

- [Architecture](architecture.md)
- [Creating Content](creating-content/adding-scriptable-objects.md)
