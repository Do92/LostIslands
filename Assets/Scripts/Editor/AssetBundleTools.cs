using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class AssetBundleTools : EditorWindow
    {
        private static string assetBundleName = "LogoIntroBundle"; // First usage as placeholder example
        private static string assetName = "Logo intro.mp4"; // First usage as placeholder example
        private static bool isPathEditable;
        private static string rootFolder = "Assets";
        private static string destinationPath = "StreamingAssets";

        [MenuItem("Custom tools/Asset Bundle: Build and Export")]
        private static void OpenBuildWindow()
        {
            AssetBundleTools buildWindow = GetWindow<AssetBundleTools>("Build AssetBundle");
            buildWindow.minSize = new Vector2(260.0f, 150.0f);
        }

        private static void BuildAssetBundle()
        {
            // Create the array of bundle build details.
            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];

            // Initial values for asset bundle and the asset itself
            buildMap[0].assetBundleName = assetBundleName;
            buildMap[0].assetNames = new string[] { rootFolder + '/' + destinationPath + '/' + assetName };

            // Building of the AssetBundle files
            BuildPipeline.BuildAssetBundles(rootFolder + '/' + destinationPath, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

            // Getting rid of redundant output
            FileUtil.DeleteFileOrDirectory(rootFolder + '/' + destinationPath + '/' + assetBundleName.ToLower() + ".manifest");
            FileUtil.DeleteFileOrDirectory(rootFolder + '/' + destinationPath + '/' + assetBundleName.ToLower() + ".manifest.meta");
            FileUtil.DeleteFileOrDirectory(rootFolder + '/' + destinationPath + '/' + destinationPath);
            FileUtil.DeleteFileOrDirectory(rootFolder + '/' + destinationPath + '/' + destinationPath + ".meta");
            FileUtil.DeleteFileOrDirectory(rootFolder + '/' + destinationPath + '/' + destinationPath + ".manifest");
            FileUtil.DeleteFileOrDirectory(rootFolder + '/' + destinationPath + '/' + destinationPath + ".manifest.meta");

            // Here we set the correct asset bundle name since the output becomes completely lowercase for some reason
            AssetDatabase.RenameAsset(rootFolder + '/' + destinationPath + '/' + assetBundleName.ToLower(), assetBundleName);

            // To prevent switching applications to wake Unity up, we refresh the project window ourself
            AssetDatabase.Refresh();
        }

        // EditorWindow input/output handling
        private void OnGUI()
        {
            // Base settings
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            assetBundleName = EditorGUILayout.TextField("Asset Bundle Name", assetBundleName);
            GUILayout.FlexibleSpace();
            assetName = EditorGUILayout.TextField("Asset Name", assetName);
            GUILayout.FlexibleSpace();

            // Developmental settings
            isPathEditable = EditorGUILayout.BeginToggleGroup("Developmental Settings", isPathEditable);
            GUILayout.FlexibleSpace();
            rootFolder = EditorGUILayout.TextField("Root Folder", rootFolder);
            destinationPath = EditorGUILayout.TextField("Destination Folder", destinationPath);
            EditorGUILayout.EndToggleGroup();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Export output to: Assets/" + destinationPath))
                BuildAssetBundle();
        }
    }
}