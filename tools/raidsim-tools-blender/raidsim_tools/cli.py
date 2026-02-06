import bpy
import sys
import os
import json
import re


from . import pipeline


def get_args():
    argv = sys.argv
    if "--" not in argv:
        raise RuntimeError("No '--' in Blender args; cannot get script arguments.")
    args = argv[argv.index("--") + 1:]
    if len(args) < 4:
        raise RuntimeError(
            "Expected at least 4 args: base_models_json anim_folder blend_out fbx_out [loop_json]"
        )

    base_models_json, anim_folder, blend_out, fbx_out = args[0:4]
    loop_json = args[4] if len(args) > 4 else None
    # Temporarily print args for debugging
    #print(f"[auto] Script args:\nbase_fbx={base_fbx}\nanim_folder={anim_folder}\nblend_out={blend_out}\nfbx_out={fbx_out}\nloop_json={loop_json}")
    return base_models_json, anim_folder, blend_out, fbx_out, loop_json


def main():
    base_models_json, anim_folder, blend_out, fbx_out, loop_json = get_args()
    model_paths = [os.path.normpath(p) for p in pipeline.parse_model_paths(base_models_json)]
    anim_folder = os.path.normpath(anim_folder)
    blend_out = os.path.normpath(blend_out)
    fbx_out = os.path.normpath(fbx_out)

    loop_map = pipeline.parse_loop_map_from_json(loop_json)

    pipeline.clean_scene()
    master_armature = pipeline.import_base_models(model_paths)
    pipeline.select_armature(master_armature)

    pipeline.run_raidsim_import_operator(anim_folder)
    pipeline.apply_loop_markers(loop_map)

    pipeline.save_blend(blend_out)
    pipeline.export_final_fbx(fbx_out)
    root, _ = os.path.splitext(fbx_out)
    events_xml = root + ".events.xml"
    pipeline.export_events_xml(events_xml)


if __name__ == "__main__":
    main()
