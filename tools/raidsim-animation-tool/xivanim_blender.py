import bpy
import sys
import os


def get_args():
    argv = sys.argv
    if "--" not in argv:
        raise RuntimeError("No '--' in Blender args; cannot get script arguments.")
    return argv[argv.index("--") + 1:]


def clean_scene():
    # DO NOT call read_factory_settings() here, it wipes addons.
    # Just delete all objects so we get an empty scene but keep preferences.
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)


def import_base_fbx(base_fbx_path):
    print(f"[auto] Importing base FBX: {base_fbx_path}")
    bpy.ops.import_scene.fbx(filepath=base_fbx_path)

    for obj in bpy.context.scene.objects:
        if obj.type == 'ARMATURE':
            return obj

    raise RuntimeError("No armature found after importing base FBX.")


def select_armature(armature):
    bpy.ops.object.select_all(action='DESELECT')
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature


def run_raidsim_import_operator(anim_folder):
    scene = bpy.context.scene

    # Sanity check: is the addon actually loaded?
    if not hasattr(scene, "import_anim_folder"):
        raise RuntimeError(
            "Raidsim Tools addon is not registered in this Blender session.\n"
            "Make sure it is installed and enabled, and that you are NOT calling "
            "bpy.ops.wm.read_factory_settings() in this script."
        )

    scene.import_anim_folder = anim_folder
    scene.import_create_nla_strips = True  # or False, if you prefer

    print(f"[auto] Running raidsim.import_and_link_animations on folder: {anim_folder}")
    res = bpy.ops.raidsim.import_and_link_animations()
    print(f"[auto] Operator result: {res}")


def save_blend(blend_path):
    print(f"[auto] Saving .blend: {blend_path}")
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)


def export_final_fbx(fbx_path):
    print(f"[auto] Exporting final FBX: {fbx_path}")
    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=False,
        apply_unit_scale=True,
        bake_anim=True,
        add_leaf_bones=False,
        path_mode='AUTO'
    )


def ensure_raidsim_addon():
    module_name = "raidsim_tools"

    if hasattr(bpy.types.Scene, "import_anim_folder"):
        return  # already registered

    print(f"[auto] Raidsim Tools props missing; trying to enable addon '{module_name}'...")
    bpy.ops.preferences.addon_enable(module=module_name)

    # After enabling, verify props exist
    if not hasattr(bpy.types.Scene, "import_anim_folder"):
        raise RuntimeError(
            f"Failed to enable Raidsim Tools addon '{module_name}'. "
            f"Check that it is installed correctly."
        )


def main():
    base_fbx, anim_folder, blend_out, fbx_out = get_args()

    base_fbx = os.path.normpath(base_fbx)
    anim_folder = os.path.normpath(anim_folder)
    blend_out = os.path.normpath(blend_out)
    fbx_out = os.path.normpath(fbx_out)

    # Try to ensure addon is enabled before doing anything
    ensure_raidsim_addon()

    clean_scene()
    armature = import_base_fbx(base_fbx)
    select_armature(armature)
    run_raidsim_import_operator(anim_folder)
    save_blend(blend_out)
    export_final_fbx(fbx_out)


if __name__ == "__main__":
    main()
