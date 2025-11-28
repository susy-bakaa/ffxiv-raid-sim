import bpy
import os

bl_info = {
    "name": "Raidsim Tools",
    "author": "susy_baka",
    "version": (0, 1, 0),
    "blender": (4, 3, 0),
    "location": "View3D > Sidebar > Raidsim Tools",
    "description": "Tools for preparing models and animations for the FFXIV Raidsim Unity project.",
    "warning": "",
    "wiki_url": "",
    "tracker_url": "",
    "support": 'COMMUNITY',
    "category": "Animation",
}


def get_default_path():
    return "//"


class RAIDSIM_OT_import_and_link_animations(bpy.types.Operator):
    """Import animations from FBX files and link them to the selected main armature"""
    bl_idname = "raidsim.import_and_link_animations"
    bl_label = "Import and Link Animations"
    bl_description = "Import animations from FBX files and link them to the selected main armature"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        scene = context.scene
        main_model = context.object
        directory = scene.import_anim_folder
        create_nla_strips = scene.import_create_nla_strips

        added_count = 0
        skipped_count = 0

        # Validate input
        if not directory:
            self.report({'ERROR'}, "No path specified. Please enter a valid path.")
            return {'CANCELLED'}

        abs_dir = bpy.path.abspath(directory)
        if not os.path.isdir(abs_dir):
            self.report({'ERROR'}, f"Invalid path: {abs_dir}")
            return {'CANCELLED'}

        if main_model is None:
            self.report({'ERROR'}, "Please select the main model with the skeleton (armature).")
            return {'CANCELLED'}

        if main_model.type != 'ARMATURE':
            self.report({'ERROR'}, "Selected object is not an armature.")
            return {'CANCELLED'}

        # Iterate through all FBX files in the selected directory
        for file_name in sorted(os.listdir(abs_dir)):
            if not file_name.lower().endswith(".fbx"):
                continue

            file_path = os.path.join(abs_dir, file_name)
            anim_name = os.path.splitext(file_name)[0]

            # Check for duplicates
            if anim_name in bpy.data.actions.keys():
                self.report({'INFO'}, f"Animation '{anim_name}' already exists, skipping.")
                skipped_count += 1
                continue

            # Track existing objects to find what was imported
            pre_import_objs = set(bpy.data.objects)

            # Import the animation FBX
            try:
                bpy.ops.import_scene.fbx(filepath=file_path)
            except Exception as ex:
                self.report({'ERROR'}, f"Failed to import {file_name}: {ex}")
                continue

            post_import_objs = set(bpy.data.objects)
            new_objects = [obj for obj in post_import_objs - pre_import_objs]

            if not new_objects:
                self.report({'ERROR'}, f"No new objects found after importing {file_name}")
                continue

            # Try to find an imported object that actually has an action
            imported_object = None
            source_action = None
            for obj in new_objects:
                if obj.animation_data and obj.animation_data.action:
                    imported_object = obj
                    source_action = obj.animation_data.action
                    break

            if imported_object is None or source_action is None:
                self.report({'ERROR'}, f"No animation data found in imported file {file_name}")
                # Cleanup imported objects anyway
                for obj in new_objects:
                    bpy.data.objects.remove(obj, do_unlink=True)
                continue

            # Copy the action so we don't depend on the imported object
            new_action = source_action.copy()
            new_action.name = anim_name

            # Link to main armature
            main_model.animation_data_create()
            main_model.animation_data.action = new_action

            # Optionally push the action to NLA as a strip
            if create_nla_strips:
                nla_tracks = main_model.animation_data.nla_tracks
                if nla_tracks.find(anim_name) == -1:
                    nla_track = nla_tracks.new()
                    nla_track.name = anim_name
                    nla_strip = nla_track.strips.new(anim_name, 0, new_action)
                    nla_strip.action_frame_start = 0
                    nla_strip.action_frame_end = new_action.frame_range[1]

            # Remove imported helper objects
            for obj in new_objects:
                bpy.data.objects.remove(obj, do_unlink=True)

            added_count += 1

        self.report({'INFO'}, f"Animations processed: {added_count} added, {skipped_count} skipped.")
        return {'FINISHED'}


class RAIDSIM_OT_clear_all_actions(bpy.types.Operator):
    """Remove all actions and animation data from the selected armature"""
    bl_idname = "raidsim.clear_all_actions"
    bl_label = "Clear All Actions on Armature"
    bl_description = "Remove animation data and unused actions related to the selected armature"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        main_model = context.object

        if main_model is None or main_model.type != 'ARMATURE':
            self.report({'ERROR'}, "Please select an armature object to clear actions from.")
            return {'CANCELLED'}

        removed_actions = 0

        if main_model.animation_data:
            # Remove NLA tracks
            while main_model.animation_data.nla_tracks:
                nla_track = main_model.animation_data.nla_tracks[0]
                main_model.animation_data.nla_tracks.remove(nla_track)

            # Clear active action on this armature
            if main_model.animation_data.action:
                main_model.animation_data.action = None

        # Optionally remove unused actions from the file
        actions_to_remove = [act for act in bpy.data.actions if act.users == 0]
        for action in actions_to_remove:
            bpy.data.actions.remove(action, do_unlink=True)
            removed_actions += 1

        self.report({'INFO'}, f"Cleared animation data; {removed_actions} unused actions removed.")
        return {'FINISHED'}


class RAIDSIM_PT_import_and_link_animations(bpy.types.Panel):
    bl_label = "Raidsim Tools"
    bl_idname = "RAIDSIM_PT_import_and_link_animations"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Raidsim Tools"

    def draw(self, context):
        layout = self.layout
        scene = context.scene

        layout.label(text="Animation Folder:")
        layout.prop(scene, "import_anim_folder", text="")

        layout.prop(scene, "import_create_nla_strips", text="Create NLA Strips")

        layout.separator()
        layout.operator(RAIDSIM_OT_import_and_link_animations.bl_idname, icon='ANIM')

        layout.separator()
        layout.operator(RAIDSIM_OT_clear_all_actions.bl_idname, icon='TRASH')


classes = (
    RAIDSIM_OT_import_and_link_animations,
    RAIDSIM_OT_clear_all_actions,
    RAIDSIM_PT_import_and_link_animations,
)


def register():
    from bpy.utils import register_class
    for cls in classes:
        register_class(cls)

    bpy.types.Scene.import_anim_folder = bpy.props.StringProperty(
        name="Animation Folder",
        description="Folder containing animation FBX files",
        subtype='DIR_PATH',
        default=get_default_path(),
    )
    bpy.types.Scene.import_create_nla_strips = bpy.props.BoolProperty(
        name="Create NLA Strips",
        description="Automatically add actions as NLA strips",
        default=True,
    )


def unregister():
    from bpy.utils import unregister_class
    for cls in reversed(classes):
        unregister_class(cls)

    del bpy.types.Scene.import_anim_folder
    del bpy.types.Scene.import_create_nla_strips


if __name__ == "__main__":
    register()
