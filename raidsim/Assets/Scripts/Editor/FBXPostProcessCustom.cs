// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
#pragma warning disable CS8632
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using dev.susybaka.raidsim.Core;

// For this thing to work, you need to first edit one of the clips included with the model, no idea why Unity works that way but it just does.
// I will then go through and update the naming and animation events of all the available clips included in the model.
// Source: https://discussions.unity.com/t/editor-script-for-editing-animation-settings-after-import/494941/2
namespace dev.susybaka.Shared.Editor
{
    public class FBXPostProcessCustom : AssetPostprocessor
    {
        private void OnPostprocessModel(GameObject g)
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;

            string modelBasePath = assetPath.Substring(0, assetPath.LastIndexOf("."));
            string modelExt = assetPath.Substring(assetPath.LastIndexOf(".")).ToLower();

            XMLEvents.Scene xmlScene = null;
            AnimationEventData eventData = null;

            // load XML data
            string xmlPath = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, ".."), modelBasePath + ".events.xml"));

            // Check if the loaded XML data exists
            if (File.Exists(xmlPath))
            {
                xmlScene = XMLEvents.Scene.Load(xmlPath);
                string newName = Path.GetFileName(xmlPath);
                xmlScene.name = newName;
            }
            else // In case it doesn't we try to load it without the 'events' part specified and if found we rename it to the correct format.
            {
                xmlPath = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, ".."), modelBasePath + ".xml"));

                if (File.Exists(xmlPath))
                {
                    string newXmlPath = xmlPath.Replace(".xml", ".events.xml");

                    if (File.Exists(newXmlPath))
                    {
                        File.Delete(newXmlPath);
                    }
                    File.Move(xmlPath, newXmlPath);
                    xmlScene = XMLEvents.Scene.Load(newXmlPath);
                    string newName = Path.GetFileName(newXmlPath);
                    xmlScene.name = newName;
                }
            }

            // Null check and warning about timeline markers not being supported yet.
            if (xmlScene != null)
            {
                if (xmlScene.timeline.markers.Count > 0)
                {
                    UnityEngine.Debug.LogWarning($"Found Timeline markers from file {xmlScene.name}. These are not supported yet. This won't cause any problems but they are unnecessary.");
                }

                // convert XML data into event data
                eventData = ScriptableObject.CreateInstance<AnimationEventData>();
                eventData.name = xmlScene.name.ToLower().Replace(".events.xml", "_events");
                foreach (XMLEvents.Action xmlAction in xmlScene.actions)
                {
                    ActionInfo actionInfo = new ActionInfo();
                    actionInfo.name = xmlAction.name;
                    foreach (XMLEvents.Marker xmlMarker in xmlAction.markers)
                    {
                        EventInfo eventInfo = new EventInfo();
                        //Debug.Log($"eventInfo.time {eventInfo.time} | final eventInfo.time {(float)xmlMarker.frame / (float)xmlScene.fps} | xmlMarker.frame {xmlMarker.frame} xmlMarker.fps {xmlScene.fps}");
                        eventInfo.time = (float)xmlMarker.frame / (float)xmlScene.fps;
                        eventData.fps = (float)xmlScene.fps;
                        eventInfo.value = xmlMarker.name;
                        actionInfo.eventList.Add(eventInfo);
                    }
                    eventData.actionlist.Add(actionInfo);
                }
            }
            else
            {
                // In case XML was not found we try one last time to load scriptable object data from asset in case user has that created
                string eventPath = modelBasePath + "_events.asset";

                if (File.Exists(eventPath))
                {
                    eventData = AssetDatabase.LoadAssetAtPath(eventPath, typeof(AnimationEventData)) as AnimationEventData;
                }
                else
                {
                    return;
                }
            }

            if (eventData != null)
            {
                // a simple linear search would probably work as well, but let's do this properly in case
                // someone has a silly amount of actions:
                Dictionary<string, ActionInfo> actionInfoLookup = new Dictionary<string, ActionInfo>();
                foreach (ActionInfo actionInfo in eventData.actionlist)
                {
                    actionInfoLookup[actionInfo.name] = actionInfo;
                }

                ModelImporterClipAnimation[] animationClips = modelImporter.clipAnimations;

                if (animationClips.Length > 0)
                {
                    // Loop through all animations in the imported model
                    foreach (var clip in animationClips)
                    {
                        // Get the animation name
                        string oldName = clip.name;
                        //Debug.Log($"oldName {oldName}");
                        clip.loopTime = false;
                        clip.loop = false;
                        clip.loopPose = false;

                        if (oldName.Contains("|"))
                        {
                            // Split the name using the "|" character and keep the last part
                            string[] nameParts = oldName.Split('|');
                            string newName = nameParts[nameParts.Length - 1];

                            // Set the new name for the animation clip
                            clip.name = newName;
                            //Debug.Log($"newName {newName}");
                        }

                        // check if we have animations for this clip
                        if (actionInfoLookup.ContainsKey(clip.name))
                        {
                            // get the events for this clip
                            ActionInfo actionInfo = actionInfoLookup[clip.name];

                            // get existing animation events if they're defined through mecanim
                            List<AnimationEvent> animationEventList = new List<AnimationEvent>(); //AnimationUtility.GetAnimationEvents(clip)

                            // add events
                            foreach (EventInfo eventInfo in actionInfo.eventList)
                            {
                                if (eventInfo.value.Contains("UnityLoop"))
                                {
                                    string[] parts = eventInfo.value.Split("|");

                                    if (parts.Length == 2)
                                    {
                                        if (int.TryParse(parts[1], out int flag))
                                        {
                                            if (flag < 1)
                                            {
                                                clip.loopTime = true;
                                                clip.loop = true;
                                                clip.loopPose = false;
                                            }
                                            else
                                            {
                                                clip.loopTime = true;
                                                clip.loop = true;
                                                clip.loopPose = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    string[] parts = eventInfo.value.Split("|");

                                    string functionName = string.Empty;
                                    string? functionParamType = null;

                                    string? strParam = null;
                                    int? intParam = null;
                                    float? floatParam = null;

                                    if (eventInfo.value.Contains("|"))
                                    {
                                        functionName = parts[0];
                                        functionParamType = parts[1];
                                    }
                                    else
                                    {
                                        functionName = eventInfo.value;
                                    }

                                    if (parts.Length == 3 && !string.IsNullOrEmpty(functionParamType))
                                    {
                                        if (functionParamType == "s")
                                        {
                                            strParam = parts[2];  // If it's a string, set strParam
                                        }
                                        else if (functionParamType == "i" && int.TryParse(parts[2], out int intValue))
                                        {
                                            intParam = intValue;  // If it's an integer, set intParam
                                            //floatParam = intValue;
                                        }
                                        else if (functionParamType == "f" && float.TryParse(parts[2], out float floatValue))
                                        {
                                            floatParam = floatValue;  // If it's a float, set floatParam
                                            //intParam = Mathf.RoundToInt(floatValue);
                                        }
                                    }

                                    AnimationEvent animationEvent = new AnimationEvent();

                                    //Debug.Log($"clip.firstFrame {clip.firstFrame} clip.lastFrame {clip.lastFrame} eventInfo.time {eventInfo.time}");
                                    animationEvent.time = Utilities.Map(eventInfo.time, clip.firstFrame / eventData.fps, clip.lastFrame / eventData.fps, 0f, 1f);
                                    animationEvent.functionName = functionName;

                                    if (!string.IsNullOrEmpty(strParam))
                                        animationEvent.stringParameter = strParam;
                                    if (intParam != null)
                                        animationEvent.intParameter = (int)intParam;
                                    if (floatParam != null)
                                        animationEvent.floatParameter = (float)floatParam;

                                    animationEventList.Add(animationEvent);
                                }
                            }

                            // store new events in the clip
                            clip.events = animationEventList.ToArray(); //AnimationUtility.SetAnimationEvents(animationClip, animationEventList.ToArray());
                        }
                    }
                }

                modelImporter.clipAnimations = animationClips;
            }
        }
    }
}