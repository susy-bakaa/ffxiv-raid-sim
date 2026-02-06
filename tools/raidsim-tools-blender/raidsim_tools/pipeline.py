import bpy
import sys
import os
import json
import re


def clean_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)


def parse_model_paths(json_or_path: str) -> list[str]:
    """
    Accepts:
      - JSON array of paths: ["path1","path2",...]
      - or a plain single path string
    Returns a list of paths.
    """
    if not json_or_path:
        return []

    try:
        data = json.loads(json_or_path)
    except Exception:
        # Not valid JSON, so treat as single path
        return [json_or_path]

    if isinstance(data, str):
        return [data]
    if isinstance(data, list):
        return [str(x) for x in data]

    raise RuntimeError("Invalid base_models_json; expected JSON list or string.")


def import_base_models(model_paths: list[str]) -> bpy.types.Object:
    """
    Import one or more base model FBXs.

    - If there is only one model (monster):
        * import it
        * pick its armature as master
        * cleanup empties
    - If there are multiple models (demihuman):
        * import all of them
        * pick the first armature as master
        * for each other armature:
            - re-target its meshes to use the master armature
            - join src into master using Blender's join (Ctrl+J equivalent)
            - remove duplicate .001 bones
        * cleanup empties

    Returns the master armature.
    """
    if not model_paths:
        raise RuntimeError("No base models provided.")

    imported_armatures: list[bpy.types.Object] = []
    arm_to_meshes: dict[bpy.types.Object, list[bpy.types.Object]] = {}

    for idx, path in enumerate(model_paths):
        abs_path = os.path.normpath(path)
        print(f"[auto] Importing base model {idx}: {abs_path}")

        pre_objs = set(bpy.data.objects)
        bpy.ops.import_scene.fbx(filepath=abs_path)
        post_objs = set(bpy.data.objects)
        new_objs = [obj for obj in (post_objs - pre_objs)]

        new_arms = [o for o in new_objs if o.type == 'ARMATURE']
        new_meshes = [o for o in new_objs if o.type == 'MESH']

        for arm in new_arms:
            imported_armatures.append(arm)
            arm_to_meshes.setdefault(arm, []).extend(new_meshes)

    if not imported_armatures:
        raise RuntimeError("No armature detected in any base models.")

    master = imported_armatures[0]
    others = imported_armatures[1:]

    # Ensure we're in object mode
    if bpy.context.object and bpy.context.object.mode != 'OBJECT':
        bpy.ops.object.mode_set(mode='OBJECT')

    # Multiple models (Demihuman): merge other armatures into master via join
    for src in others:
        print(f"[merge] Processing source armature '{src.name}'...")

        # Re-target meshes using src to master, preserving world transform
        for mesh in arm_to_meshes.get(src, []):
            # If mesh is directly parented to src armature, reparent to master
            if mesh.parent == src:
                mw = mesh.matrix_world.copy()
                mesh.parent = master
                mesh.matrix_world = mw

            # Update armature modifiers to point at master instead of src
            for mod in mesh.modifiers:
                if mod.type == 'ARMATURE' and mod.object == src:
                    mod.object = master

        # Join src into master using Blender's join
        bpy.ops.object.select_all(action='DESELECT')
        master.select_set(True)
        src.select_set(True)
        bpy.context.view_layer.objects.active = master

        print(f"[merge] Joining '{src.name}' into '{master.name}' via bpy.ops.object.join()")
        bpy.ops.object.join()

        # After join, 'src' object is gone and master now has all bones.
        remove_duplicate_bones(master)

    # In both cases (single or merged), clean empties and reparent meshes
    cleanup_empties_and_reparent(master)

    return master


def cleanup_empties_and_reparent(master: bpy.types.Object):
    """
    - Unparent master from any empties (keeping its world transform).
    - Reparent meshes that were under empties to the master (keeping world transform).
    - Delete all empties in the scene.

    Works for both:
      - demihumans (multiple imported parts)
      - monsters (single imported model)
    """
    print("[cleanup] Cleaning empties and fixing parenting...")

    # Unparent master from empties (if any), keep world transform
    if master.parent is not None and master.parent.type == 'EMPTY':
        mw = master.matrix_world.copy()
        print(f"[cleanup] Unparenting master armature from empty '{master.parent.name}'")
        master.parent = None
        master.matrix_world = mw

    # Reparent meshes from empties to master
    for obj in bpy.context.scene.objects:
        if obj.type != 'MESH':
            continue

        if obj.parent is not None and obj.parent.type == 'EMPTY':
            mw = obj.matrix_world.copy()
            print(f"[cleanup] Reparenting mesh '{obj.name}' from empty '{obj.parent.name}' to master.")
            obj.parent = master
            obj.matrix_world = mw

    # Delete all empty objects
    empties = [o for o in bpy.context.scene.objects if o.type == 'EMPTY']
    for empty in empties:
        print(f"[cleanup] Removing empty '{empty.name}'")
        bpy.data.objects.remove(empty, do_unlink=True)

    print("[cleanup] Done. Scene should now only have armature + meshes (and any cameras/lights you had).")


def remove_duplicate_bones(master: bpy.types.Object):
    """
    After joining armatures, Blender will rename duplicate bones to 'BoneName.001'.
    We assume:
      - the original BoneName should be kept,
      - the .001 copies are redundant and unused.
    So we remove bones with '.001' when their base name also exists.
    """
    print("[merge] Removing duplicate .001 bones if base name exists...")

    bpy.context.view_layer.objects.active = master
    bpy.ops.object.mode_set(mode='EDIT')
    ebones = master.data.edit_bones

    all_names = {b.name for b in ebones}
    to_delete = []

    for b in ebones:
        if b.name.endswith(".001"):
            base = b.name[:-4]  # strips '.001'
            if base in all_names:
                to_delete.append(b)

    for b in to_delete:
        print(f"[merge] Deleting duplicate bone '{b.name}'")
        ebones.remove(b)

    bpy.ops.object.mode_set(mode='OBJECT')
    print(f"[merge] Duplicate cleanup done ({len(to_delete)} removed).")


def select_armature(armature):
    bpy.ops.object.select_all(action='DESELECT')
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature


def run_raidsim_import_operator(anim_folder: str):
    scene = bpy.context.scene

    if not hasattr(scene, "import_anim_folder"):
        raise RuntimeError(
            "Raidsim Tools addon is not registered in this Blender session.\n"
            "Make sure it is installed and enabled."
        )

    scene.import_anim_folder = anim_folder
    scene.import_create_nla_strips = True

    print(f"[auto] Running raidsim.import_and_link_animations on folder: {anim_folder}")
    res = bpy.ops.raidsim.import_and_link_animations()
    print(f"[auto] Operator result: {res}")


def parse_loop_map_from_json(loop_json: str | None) -> dict:
    if not loop_json:
        print("[auto] No loop JSON provided; skipping loop markers.")
        return {}

    try:
        data = json.loads(loop_json)
    except Exception as e:
        print(f"[auto] Failed to parse loop JSON: {e}")
        return {}

    if not isinstance(data, dict):
        print("[auto] Loop JSON was not an object; skipping.")
        return {}

    loop_map: dict[str, bool] = {}
    for k, v in data.items():
        key = str(k)
        if isinstance(v, bool):
            loop_map[key] = v
        elif isinstance(v, (int, float)):
            loop_map[key] = bool(v)

    print(f"[auto] Loaded {len(loop_map)} loop entries from JSON.")
    return loop_map


def extract_motion_name_from_action(action_name: str) -> str | None:
    """
    Try to derive the FFXIV motion name (e.g. 'cbbm_sp01') from possibly a more complex
    Blender Action name like 'mon_sp001_cbbm_sp01'.
    """
    parts = action_name.split("_")
    if len(parts) >= 2:
        candidate = "_".join(parts[-2:])
        if candidate.startswith("c") and "_" in candidate:
            return candidate

    # Fallback regex which grabs 'cXXXXX_YYY' at the end
    m = re.search(r"(c\w+_.+)$", action_name)
    if m:
        return m.group(1)

    # Fallback when it's already exactly the motion name
    if action_name.startswith("c") and "_" in action_name:
        return action_name

    return None


def ensure_loop_marker(action: bpy.types.Action):
    """Add or update a 'UnityLoop|1' pose marker at the first frame of this action."""
    frame_start = int(action.frame_range[0])

    existing = None
    for m in action.pose_markers:
        if m.name == "UnityLoop|1":
            existing = m
            break

    if existing:
        existing.frame = frame_start
    else:
        marker = action.pose_markers.new("UnityLoop|1")
        marker.frame = frame_start


def apply_loop_markers(loop_map: dict[str, bool]):
    if not loop_map:
        print("[auto] No loop map; skipping loop marker application.")
        return

    print("[auto] Applying loop markers to actions...")
    applied = 0

    for action in bpy.data.actions:
        motion_name = extract_motion_name_from_action(action.name)
        if not motion_name:
            continue

        if loop_map.get(motion_name, False):
            ensure_loop_marker(action)
            applied += 1

    print(f"[auto] Loop markers applied to {applied} actions.")


def save_blend(blend_path: str):
    print(f"[auto] Saving .blend: {blend_path}")
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)


def export_final_fbx(fbx_path: str):
    print(f"[auto] Exporting final FBX: {fbx_path}")
    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=False,
        apply_unit_scale=True,
        bake_anim=True,
        add_leaf_bones=False,            # Armature > Add Leaf Bones off
        bake_anim_simplify_factor=0.0,   # Animation > Simplify = 0
        path_mode='AUTO',
    )


def export_events_xml(events_path: str):
    print(f"[auto] Exporting animation events XML: {events_path}")
    res = bpy.ops.raidsim.export_events_xml(filepath=events_path)
    print(f"[auto] Events export result: {res}")


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