using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Editor
{
    /// <summary>
    /// Animation clip target renamer.
    /// This script allows animation curves to be moved from one target to another.
    /// 
    /// Usage:
    ///     1) Open the Animation Clip Target Renamer from the Window menu in the Unity UI.
    ///     2) Select the animation clip whose curves you wish to move.
    ///     3) Change the names in the text boxes on the right side of the window to the names of the objects you wish to move the animations to.
    ///     4) Press Apply.
    /// </summary>
    public class AnimationClipTools : EditorWindow
    {

        public AnimationClip SelectedClip;


        /// <summary>
        /// The curve data for the animation.
        /// </summary>
        private AnimationClipCurveData[] curveDataCollection;

        /// <summary>
        /// The names of the original GameObjects.
        /// </summary>
        private List<string> originalObjectPaths;


        /// <summary>
        /// The names of the target GameObjects.
        /// </summary>
        private List<string> targetObjectPaths;

        private bool isInitialized;

        [MenuItem("Custom tools/Animation Clip: Target Renamer")]
        public static void OpenWindow()
        {
            AnimationClipTools renamer = GetWindow<AnimationClipTools>("Target Renamer");
            renamer.Clear();
        }

        private void Initialize()
        {
            // Because there is a newer system to doing this but we don't care right now
#pragma warning disable 0618
            curveDataCollection = AnimationUtility.GetAllCurves(SelectedClip, true);
#pragma warning restore 0618

            originalObjectPaths = new List<string>();
            targetObjectPaths = new List<string>();
            foreach (AnimationClipCurveData curveData in curveDataCollection)
            {
                if (curveData.path != "" && !originalObjectPaths.Contains(curveData.path))
                {
                    originalObjectPaths.Add(curveData.path);
                    targetObjectPaths.Add(curveData.path);
                }
            }
            isInitialized = true;
        }

        private void Clear()
        {
            curveDataCollection = null;
            originalObjectPaths = null;
            targetObjectPaths = null;
            isInitialized = false;
        }

        private void RenameTargets()
        {
            // Set the curve data to the new values. 
            for (int i = 0; i < targetObjectPaths.Count; i++)
            {
                string oldName = originalObjectPaths[i];
                string newName = targetObjectPaths[i];

                if (oldName != newName)
                    foreach (AnimationClipCurveData curveData in curveDataCollection)
                        if (curveData.path == oldName)
                            curveData.path = newName;
            }

            // Set up the curves based on the new names.
            SelectedClip.ClearCurves();
            foreach (AnimationClipCurveData curveData in curveDataCollection)
                SelectedClip.SetCurve(curveData.path, curveData.type, curveData.propertyName, curveData.curve);

            Clear();
            Initialize();
        }

        private void OnGuiShowTargetsList()
        {
            // If we got here, we have all the data we need to work with,
            // So we should be able to build the UI.

            // Build the list of text boxes for renaming.
            if (targetObjectPaths != null)
            {
                EditorGUILayout.Space();
                EditorGUIUtility.labelWidth = 250;

                for (int i = 0; i < targetObjectPaths.Count; i++)
                {
                    string newName = EditorGUILayout.TextField(originalObjectPaths[i], targetObjectPaths[i]);

                    targetObjectPaths[i] = newName;
                }
            }
        }

        private void OnGUI()
        {
            AnimationClip previous = SelectedClip;
            SelectedClip = EditorGUILayout.ObjectField("Animation Clip", SelectedClip, typeof(AnimationClip), true) as AnimationClip;

            if (SelectedClip != previous)
                Clear();

            if (SelectedClip != null)
            {
                if (!isInitialized)
                    Initialize();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh"))
                {
                    Clear();
                    Initialize();
                }
                EditorGUILayout.EndHorizontal();

                OnGuiShowTargetsList();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Apply"))
                    RenameTargets();

                EditorGUILayout.EndHorizontal();
            }
        }

    }
}