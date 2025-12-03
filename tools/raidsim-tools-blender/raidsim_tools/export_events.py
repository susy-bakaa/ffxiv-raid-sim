import bpy
import bpy_extras
import xml.etree.ElementTree as ET

def save(operator, context, filepath=""):
    # Build XML structure from actions + markers
    eScene = ET.Element("scene", {
        "version":"%i" % 1,
        "fps":"%i" % context.scene.render.fps
    })
    eTimeline = ET.SubElement(eScene, "timeline")
    eMarkers = ET.SubElement(eTimeline, "markers")
    for marker in context.scene.timeline_markers:
        ET.SubElement(eMarkers, "marker", {"name":marker.name, "frame":"%i" % marker.frame}) 
    
    eActions = ET.SubElement(eScene, "actions")
    for action in bpy.data.actions:
        eAction = ET.SubElement(eActions, "action", {"name":action.name})
        eMarkers = ET.SubElement(eAction, "markers")
        for marker in action.pose_markers:
            ET.SubElement(eMarkers, "marker", {"name":marker.name, "frame":"%i" % marker.frame}) 
 
    if False:
        # wrap it in an ElementTree instance, and save as XML
        doc = ET.ElementTree(eScene)
        doc.write(filepath)
    else:
        import xml.dom.minidom

        # parse it into minidom and pretty print...
        minidom = xml.dom.minidom.parseString(ET.tostring(eScene, encoding='utf8', method='xml'))
        f = open(filepath, "wt")
        f.write(minidom.toprettyxml())
        f.close()
        
    return {'FINISHED'}

def main(*argv):
    import os
    if len(argv) > 0:
        outpath = os.path.abspath(argv[0])
    else:
        outpath = os.path.abspath("default.xml")
    save(None, bpy.context, outpath)
        
if __name__ == '__main__':
    import sys
    try:
        idx = sys.argv.index("--")
        args = sys.argv[idx+1:] 
    except ValueError:
        args = []
    main(*args)
    

