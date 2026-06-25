using LX.Common.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EditorSelectionHistory
{
    private const int kMaxHistoryLength = 30;
    private static bool hasDoneHistoryActionThisFrame = false;

    private static List<string> assetHistory = new();
    private static int currentAssetHistoryItemIndex;
    private static bool isExpectingSelectionChangeDueToHistoryAccess;

    [InitializeOnLoadMethod]
    public static void OnLoaded()
    {
        EditorApplication.projectWindowItemOnGUI += OnGui;
        EditorApplication.delayCall += OnNewFrame;

        Selection.selectionChanged += OnSelectionChanged;

        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeScriptReload;

        LoadHistory();
    }

    private static void OnBeforeScriptReload()
    {
        SaveHistory();
    }

    private static void OnSelectionChanged()
    {
        if (isExpectingSelectionChangeDueToHistoryAccess)
        {
            // Occurs if selection changed due to a history action
            isExpectingSelectionChangeDueToHistoryAccess = false;
            return;
        }

        if (Selection.assetGUIDs != null && Selection.assetGUIDs.Length == 1)
        {
            if (currentAssetHistoryItemIndex + 1 < assetHistory.Count)
                assetHistory.RemoveRange(currentAssetHistoryItemIndex + 1, assetHistory.Count - (currentAssetHistoryItemIndex + 1));
            assetHistory.Add(Selection.assetGUIDs[0]);
            currentAssetHistoryItemIndex = assetHistory.Count - 1;

            if (assetHistory.Count > kMaxHistoryLength)
                assetHistory.RemoveRange(0, assetHistory.Count - kMaxHistoryLength);
        }
    }

    private static void OnGui(string guid, Rect selectionRect)
    {
        if (!hasDoneHistoryActionThisFrame)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 3)
                StepAssetHistory(-1);
            else if (Event.current.type == EventType.MouseDown && Event.current.button == 4)
                StepAssetHistory(1);
        }
    }

    private static void StepAssetHistory(int offset)
    {
        currentAssetHistoryItemIndex = Mathf.Clamp(currentAssetHistoryItemIndex + offset, assetHistory.Count > 0 ? 0 : -1, assetHistory.Count - 1);

        if (assetHistory.IsValidIndex(currentAssetHistoryItemIndex))
        {
            string assetGuid = assetHistory[currentAssetHistoryItemIndex];
            var loadedAsset = AssetDatabase.LoadAssetByGUID(new GUID(assetGuid), typeof(UnityEngine.Object));

            if (loadedAsset && loadedAsset != Selection.activeObject)
            {
                Selection.activeObject = loadedAsset;
                isExpectingSelectionChangeDueToHistoryAccess = true;
            }
        }
        hasDoneHistoryActionThisFrame = true;
    }

    private static void OnNewFrame()
    {
        hasDoneHistoryActionThisFrame = false;
        EditorApplication.delayCall += OnNewFrame;
    }

    private static void LoadHistory()
    {
        string assetHistoryString = PlayerPrefs.GetString("UserRecentAssetHistory");
        if (assetHistoryString != null)
        {
            assetHistory.Clear();
            assetHistory.AddRange(assetHistoryString.Split(';'));
        }
    }

    private static void SaveHistory()
    {
        PlayerPrefs.SetString("UserRecentAssetHistory", string.Join(';', assetHistory));
    }
}
