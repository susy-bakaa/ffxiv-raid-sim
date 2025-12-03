import bpy
import os
from bpy.props import (
    StringProperty,
    BoolProperty,
    EnumProperty,
    CollectionProperty,
    IntProperty,
)
from bpy_extras.io_utils import ExportHelper

bl_info = {
    "name": "Raidsim Tools",
    "author": "susy_baka",
    "version": (1, 0, 0),
    "blender": (4, 3, 0),
    "location": "View3D > Sidebar > Raidsim Tools; Dopesheet > Markers; File > Export",
    "description": "Tools for preparing models and animation events for the FFXIV Raidsim Unity project.",
    "warning": "",
    "wiki_url": "",
    "tracker_url": "",
    "support": 'COMMUNITY',
    "category": "Animation",
}

# ------------------------------------------------------------------------
#  Raidsim: import & link animations for armature
# ------------------------------------------------------------------------

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

        for file_name in sorted(os.listdir(abs_dir)):
            if not file_name.lower().endswith(".fbx"):
                continue

            file_path = os.path.join(abs_dir, file_name)
            anim_name = os.path.splitext(file_name)[0]

            if anim_name in bpy.data.actions.keys():
                self.report({'INFO'}, f"Animation '{anim_name}' already exists, skipping.")
                skipped_count += 1
                continue

            pre_import_objs = set(bpy.data.objects)

            try:
                bpy.ops.import_scene.fbx(filepath=file_path)
            except Exception as ex:
                self.report({'ERROR'}, f"Failed to import {file_name}: {ex}")
                continue

            post_import_objs = set(bpy.data.objects)
            new_objects = [obj for obj in (post_import_objs - pre_import_objs)]

            if not new_objects:
                self.report({'ERROR'}, f"No new objects found after importing {file_name}")
                continue

            imported_object = None
            source_action = None
            for obj in new_objects:
                if obj.animation_data and obj.animation_data.action:
                    imported_object = obj
                    source_action = obj.animation_data.action
                    break

            if imported_object is None or source_action is None:
                self.report({'ERROR'}, f"No animation data found in imported file {file_name}")
                for obj in new_objects:
                    bpy.data.objects.remove(obj, do_unlink=True)
                continue

            new_action = source_action.copy()
            new_action.name = anim_name

            main_model.animation_data_create()
            main_model.animation_data.action = new_action

            if create_nla_strips:
                nla_tracks = main_model.animation_data.nla_tracks
                if nla_tracks.find(anim_name) == -1:
                    nla_track = nla_tracks.new()
                    nla_track.name = anim_name
                    nla_strip = nla_track.strips.new(anim_name, 0, new_action)
                    nla_strip.action_frame_start = 0
                    nla_strip.action_frame_end = new_action.frame_range[1]

            for obj in new_objects:
                bpy.data.objects.remove(obj, do_unlink=True)

            added_count += 1

        self.report({'INFO'}, f"Animations processed: {added_count} added, {skipped_count} skipped.")
        return {'FINISHED'}


class RAIDSIM_OT_clear_all_actions(bpy.types.Operator):
    """Remove all animation data and unused actions for the selected armature"""
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
            while main_model.animation_data.nla_tracks:
                nla_track = main_model.animation_data.nla_tracks[0]
                main_model.animation_data.nla_tracks.remove(nla_track)

            if main_model.animation_data.action:
                main_model.animation_data.action = None

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

# ------------------------------------------------------------------------
#  Unity loop & animation event tools (Dopesheet)
# ------------------------------------------------------------------------

class UnityAnimationEventPreset(bpy.types.PropertyGroup):
    name: StringProperty(name="Name", description="Event name")

    type: EnumProperty(
        name="Type",
        description="Parameter type",
        items=[
            ('s', "String", "String parameter"),
            ('i', "Integer", "Integer parameter"),
            ('f', "Float", "Float parameter"),
        ],
        default='i',
    )
    value: StringProperty(name="Value", description="Event value")

    def as_display_name(self):
        return f"{self.name} ({self.value})"


class UNITY_UL_EventPresets(bpy.types.UIList):
    def draw_item(self, context, layout, data, item, icon, active_data, active_property, index):
        if item:
            layout.label(text=f"{item.name} ({item.value})")


class AddUnityLoopMarkerOperator(bpy.types.Operator):
    bl_idname = "action.add_unity_loop_marker"
    bl_label = "Add Unity Loop Marker"
    bl_description = "Add a Unity-compatible loop marker to the current action"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        obj = bpy.context.object
        if not obj or not obj.animation_data:
            self.report({'ERROR'}, "No object with animation data selected")
            return {'CANCELLED'}

        action = obj.animation_data.action
        start_frame = bpy.context.scene.frame_start
        is_loop_pose = context.scene.unity_loop_pose_checkbox

        if not action:
            self.report({'ERROR'}, "No action is currently active")
            return {'CANCELLED'}

        marker_type = 1 if is_loop_pose else 0
        marker_name = f"UnityLoop|{marker_type}"
        existing_marker = next((m for m in action.pose_markers if m.name == marker_name), None)

        if existing_marker:
            self.report({'INFO'}, f"Marker '{marker_name}' already exists")
            return {'CANCELLED'}

        new_marker = action.pose_markers.new(name=marker_name)
        new_marker.frame = start_frame
        self.report({'INFO'}, f"Added marker '{marker_name}' at frame {start_frame}")
        return {'FINISHED'}


class RemoveUnityLoopMarkerOperator(bpy.types.Operator):
    bl_idname = "action.remove_unity_loop_marker"
    bl_label = "Remove Unity Loop Marker"
    bl_description = "Remove any Unity loop marker from the current action"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        obj = bpy.context.object
        if not obj or not obj.animation_data:
            self.report({'ERROR'}, "No object with animation data selected")
            return {'CANCELLED'}

        action = obj.animation_data.action
        if not action:
            self.report({'ERROR'}, "No action is currently active")
            return {'CANCELLED'}

        markers_to_remove = [m for m in action.pose_markers if m.name.startswith("UnityLoop|")]
        for marker in markers_to_remove:
            action.pose_markers.remove(marker)

        if markers_to_remove:
            self.report({'INFO'}, f"Removed {len(markers_to_remove)} Unity loop marker(s)")
        else:
            self.report({'INFO'}, "No Unity loop markers found")

        return {'FINISHED'}


class AddUnityAnimationEventOperator(bpy.types.Operator):
    bl_idname = "action.add_unity_animation_event"
    bl_label = "Add Unity Animation Event"
    bl_description = "Add a Unity-compatible animation event marker to the current frame"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        obj = bpy.context.object
        if not obj or not obj.animation_data:
            self.report({'ERROR'}, "No object with animation data selected")
            return {'CANCELLED'}

        action = obj.animation_data.action
        current_frame = bpy.context.scene.frame_current

        event_name = context.scene.unity_event_name
        event_type = context.scene.unity_event_type
        event_value = context.scene.unity_event_value

        if not action:
            self.report({'ERROR'}, "No action is currently active")
            return {'CANCELLED'}
        if not event_name.strip():
            self.report({'ERROR'}, "Event name cannot be empty")
            return {'CANCELLED'}

        marker_name = f"{event_name}|{event_type}|{event_value}"
        existing_marker = next((m for m in action.pose_markers if m.frame == current_frame), None)

        if existing_marker:
            self.report({'INFO'}, f"Marker already exists at frame {current_frame}")
            return {'CANCELLED'}

        new_marker = action.pose_markers.new(name=marker_name)
        new_marker.frame = current_frame
        self.report({'INFO'}, f"Added marker '{marker_name}' at frame {current_frame}")
        return {'FINISHED'}


class RemoveUnityAnimationEventOperator(bpy.types.Operator):
    bl_idname = "action.remove_unity_animation_event"
    bl_label = "Remove Unity Animation Event"
    bl_description = "Remove Unity animation event markers from the selected frame"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        obj = bpy.context.object
        if not obj or not obj.animation_data:
            self.report({'ERROR'}, "No object with animation data selected")
            return {'CANCELLED'}

        action = obj.animation_data.action
        current_frame = bpy.context.scene.frame_current

        if not action:
            self.report({'ERROR'}, "No action is currently active")
            return {'CANCELLED'}

        markers_to_remove = [m for m in action.pose_markers if m.frame == current_frame]
        for marker in markers_to_remove:
            action.pose_markers.remove(marker)

        if markers_to_remove:
            self.report({'INFO'}, f"Removed {len(markers_to_remove)} marker(s) at frame {current_frame}")
        else:
            self.report({'INFO'}, f"No markers found at frame {current_frame}")

        return {'FINISHED'}


class SaveUnityAnimationEventPresetOperator(bpy.types.Operator):
    bl_idname = "scene.save_unity_event_preset"
    bl_label = "Save Preset"
    bl_description = "Save the current Unity animation event as a preset"

    def execute(self, context):
        scene = context.scene
        new_preset = scene.unity_event_presets.add()
        new_preset.name = scene.unity_event_name
        new_preset.type = scene.unity_event_type
        new_preset.value = scene.unity_event_value

        self.report({'INFO'}, f"Preset '{new_preset.name}' saved")
        return {'FINISHED'}


class DeleteUnityAnimationEventPresetOperator(bpy.types.Operator):
    bl_idname = "scene.delete_unity_event_preset"
    bl_label = "Delete Preset"
    bl_description = "Delete the selected Unity animation event preset"

    def execute(self, context):
        scene = context.scene
        presets = scene.unity_event_presets
        index = scene.unity_event_presets_index

        if 0 <= index < len(presets):
            self.report({'INFO'}, f"Preset '{presets[index].name}' deleted")
            presets.remove(index)
        else:
            self.report({'ERROR'}, "Invalid preset selection")

        return {'FINISHED'}


class ApplyUnityAnimationEventPresetOperator(bpy.types.Operator):
    bl_idname = "scene.apply_unity_event_preset"
    bl_label = "Apply Preset"
    bl_description = "Apply the selected preset to the animation event fields"

    def execute(self, context):
        scene = context.scene
        presets = scene.unity_event_presets
        index = scene.unity_event_presets_index

        if 0 <= index < len(presets):
            preset = presets[index]
            scene.unity_event_name = preset.name
            scene.unity_event_type = preset.type
            scene.unity_event_value = preset.value
            self.report({'INFO'}, f"Preset '{preset.name}' applied")
        else:
            self.report({'ERROR'}, "Invalid preset selection")

        return {'FINISHED'}


class UnityLoopMarkerPanel(bpy.types.Panel):
    bl_label = "Unity Loop Marker"
    bl_idname = "ACTION_PT_unity_loop_marker"
    bl_space_type = 'DOPESHEET_EDITOR'
    bl_region_type = 'UI'
    bl_category = "Markers"
    bl_context = "action"

    def draw(self, context):
        layout = self.layout
        scene = context.scene

        layout.prop(scene, "unity_loop_pose_checkbox", text="Loop Pose")
        layout.operator(AddUnityLoopMarkerOperator.bl_idname)
        layout.separator()
        layout.operator(RemoveUnityLoopMarkerOperator.bl_idname)


class UnityAnimationEventPanel(bpy.types.Panel):
    bl_label = "Unity Animation Event"
    bl_idname = "ACTION_PT_unity_animation_event"
    bl_space_type = 'DOPESHEET_EDITOR'
    bl_region_type = 'UI'
    bl_category = "Markers"
    bl_context = "action"

    def draw(self, context):
        layout = self.layout
        scene = context.scene

        layout.label(text="Event Name:")
        layout.prop(scene, "unity_event_name", text="")

        layout.label(text="Event Type:")
        layout.prop(scene, "unity_event_type", text="")

        layout.label(text="Event Value:")
        layout.prop(scene, "unity_event_value", text="")

        layout.operator(AddUnityAnimationEventOperator.bl_idname)
        layout.operator(RemoveUnityAnimationEventOperator.bl_idname)

        layout.separator()
        layout.label(text="Presets:")
        row = layout.row()
        row.template_list(
            "UNITY_UL_EventPresets", "", scene, "unity_event_presets",
            scene, "unity_event_presets_index"
        )
        col = row.column(align=True)
        col.operator(SaveUnityAnimationEventPresetOperator.bl_idname, icon='ADD', text="")
        col.operator(DeleteUnityAnimationEventPresetOperator.bl_idname, icon='REMOVE', text="")
        layout.operator(ApplyUnityAnimationEventPresetOperator.bl_idname)

# ------------------------------------------------------------------------
#  XML export for animation events
# ------------------------------------------------------------------------

class RAIDSIM_OT_export_anim_events(bpy.types.Operator, ExportHelper):
    """Export Animation Events (actions + pose markers) to XML"""
    bl_idname = "raidsim.export_events_xml"
    bl_label = "Export Events XML"
    bl_options = {'PRESET'}

    filename_ext = ".xml"
    filter_glob: StringProperty(
        default="*.xml",
        options={'HIDDEN'},
    )

    def execute(self, context):
        from . import export_events
        filepath = self.filepath
        return export_events.save(self, context, filepath=filepath)


def menu_func_export(self, context):
    self.layout.operator(RAIDSIM_OT_export_anim_events.bl_idname, text="Animation Events (.xml)")

# ------------------------------------------------------------------------
#  Registration
# ------------------------------------------------------------------------

classes = (
    RAIDSIM_OT_import_and_link_animations,
    RAIDSIM_OT_clear_all_actions,
    RAIDSIM_PT_import_and_link_animations,

    UnityAnimationEventPreset,
    UNITY_UL_EventPresets,
    AddUnityLoopMarkerOperator,
    RemoveUnityLoopMarkerOperator,
    AddUnityAnimationEventOperator,
    RemoveUnityAnimationEventOperator,
    SaveUnityAnimationEventPresetOperator,
    DeleteUnityAnimationEventPresetOperator,
    ApplyUnityAnimationEventPresetOperator,
    UnityLoopMarkerPanel,
    UnityAnimationEventPanel,

    RAIDSIM_OT_export_anim_events,
)


def register():
    from bpy.utils import register_class
    for cls in classes:
        register_class(cls)

    bpy.types.Scene.import_anim_folder = StringProperty(
        name="Animation Folder",
        description="Folder containing animation FBX files",
        subtype='DIR_PATH',
        default="//",
    )
    bpy.types.Scene.import_create_nla_strips = BoolProperty(
        name="Create NLA Strips",
        description="Automatically add actions as NLA strips",
        default=True,
    )

    bpy.types.Scene.unity_loop_pose_checkbox = BoolProperty(
        name="Unity Loop Pose",
        description="Include Loop Pose flag (1 for loop pose, 0 otherwise)",
        default=False,
    )
    bpy.types.Scene.unity_event_name = StringProperty(
        name="Event Name",
        description="Name of the Unity animation event",
    )
    bpy.types.Scene.unity_event_type = EnumProperty(
        name="Event Type",
        description="Type of the Unity animation event parameter",
        items=[
            ('s', "String", "String parameter"),
            ('i', "Integer", "Integer parameter"),
            ('f', "Float", "Float parameter"),
        ],
        default='i',
    )
    bpy.types.Scene.unity_event_value = StringProperty(
        name="Event Value",
        description="Value of the Unity animation event parameter",
    )
    bpy.types.Scene.unity_event_presets = CollectionProperty(type=UnityAnimationEventPreset)
    bpy.types.Scene.unity_event_presets_index = IntProperty(default=0)

    bpy.types.TOPBAR_MT_file_export.append(menu_func_export)


def unregister():
    from bpy.utils import unregister_class
    bpy.types.TOPBAR_MT_file_export.remove(menu_func_export)

    del bpy.types.Scene.import_anim_folder
    del bpy.types.Scene.import_create_nla_strips

    del bpy.types.Scene.unity_loop_pose_checkbox
    del bpy.types.Scene.unity_event_name
    del bpy.types.Scene.unity_event_type
    del bpy.types.Scene.unity_event_value
    del bpy.types.Scene.unity_event_presets
    del bpy.types.Scene.unity_event_presets_index

    for cls in reversed(classes):
        unregister_class(cls)


if __name__ == "__main__":
    register()
