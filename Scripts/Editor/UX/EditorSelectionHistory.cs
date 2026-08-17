using ImGuiNET;
using LX.Common.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class RecentAssetsEditorWindow : EditorWindow
{
    ScrollView scrollView;

    public void CreateGUI()
    {
        scrollView = new ScrollView();
        scrollView.style.flexGrow = new StyleFloat(1);
        scrollView.style.flexShrink = new StyleFloat(1);
        scrollView.contentContainer.style.flexDirection = FlexDirection.Row;
        scrollView.contentContainer.style.flexWrap = Wrap.Wrap;

        Refresh();

        rootVisualElement.Add(scrollView);
    }

    public void Refresh()
    {
        scrollView.Clear();

        var recents = EditorSelectionHistory.AssetHistory;
        for (int i = recents.Count - 1; i >= 0; i--)
        {
            UnityEngine.Object obj = EditorSelectionHistory.StringToObject(recents[i]);
            if (obj)
            {
                Button button = new Button();
                var icon = button.iconImage;
                button.style.width = 128;
                button.style.height = 128;
                button.text = obj.name;
                icon.texture = AssetPreview.GetAssetPreview(obj);
                if (icon.texture == null)
                    icon.texture = AssetPreview.GetMiniThumbnail(obj);
                button.iconImage = icon;
                button.style.flexDirection = FlexDirection.Column;
                button.clicked += () => {
                    if (AssetDatabase.IsMainAsset(obj) || AssetDatabase.IsSubAsset(obj))
                        AssetDatabase.OpenAsset(obj);
                    else
                        Selection.activeObject = obj;
                };

                scrollView.Add(button);
            }
        }
    }
}

[InitializeOnLoad]
public static class EditorSelectionHistory
{
    private const int kMaxHistoryLength = 30;
    private static bool hasDoneHistoryActionThisFrame = false;

    private static bool shouldIncludeFolders = false;

    public static IReadOnlyList<string> AssetHistory => assetHistory.AsReadOnly();

    private static List<string> assetHistory = new();
    private static int currentAssetHistoryItemIndex;
    private static bool isExpectingSelectionChangeDueToHistoryAccess;

    [InitializeOnLoadMethod]
    public static void OnLoaded()
    {
        EditorApplication.projectWindowItemOnGUI += OnGui;
        EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnEntityGui;
        EditorApplication.delayCall += OnNewFrame;

        Selection.selectionChanged += OnSelectionChanged;

        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeScriptReload;

        LoadHistory();
    }

    [MenuItem("LX/Recent Assets")]
    public static void RecentAssets()
    {
        EditorWindow wnd = EditorWindow.GetWindow<RecentAssetsEditorWindow>("Recent Assets");
        wnd.Show();
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
        
        if (Selection.objects != null && Selection.objects.Length == 1)
        {
            if (!shouldIncludeFolders && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.objects[0])))
                return;

            // Remove remaining upcoming history items because we are creating a new history
            if (currentAssetHistoryItemIndex + 1 < assetHistory.Count)
                assetHistory.RemoveRange(currentAssetHistoryItemIndex + 1, assetHistory.Count - (currentAssetHistoryItemIndex + 1));

            string objectString = ObjectToString(Selection.objects[0]);
            assetHistory.Remove(objectString);
            assetHistory.Add(objectString);
            currentAssetHistoryItemIndex = assetHistory.Count - 1;
        }

        if (assetHistory.Count > kMaxHistoryLength)
            assetHistory.RemoveRange(0, assetHistory.Count - kMaxHistoryLength);

        if (EditorWindow.HasOpenInstances<RecentAssetsEditorWindow>())
            EditorWindow.GetWindow<RecentAssetsEditorWindow>("Recent Assets", false).Refresh();
    }

    private static void OnGui(string guid, Rect selectionRect) => PollMouseButtonsAndStepAssetHistory();

    private static void OnEntityGui(EntityId entityId, Rect selectionRect) => PollMouseButtonsAndStepAssetHistory();

    private static void PollMouseButtonsAndStepAssetHistory()
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
            string historyItem = assetHistory[currentAssetHistoryItemIndex];
            UnityEngine.Object loadedObject = StringToObject(historyItem);

            if (loadedObject)
            {
                isExpectingSelectionChangeDueToHistoryAccess = true;
                Selection.activeObject = loadedObject;
            }
        }

        hasDoneHistoryActionThisFrame = true;
    }

    private static string ObjectToString(UnityEngine.Object obj)
    {
        bool isMainAsset = AssetDatabase.IsMainAsset(obj);
        bool isSubAsset = AssetDatabase.IsSubAsset(obj);

        if (isMainAsset || isSubAsset)
            return $"{AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj))}";
        else
        {
            string scene = (obj as GameObject)?.scene.path;
            string sceneGuid = AssetDatabase.AssetPathToGUID(scene);

            if (!string.IsNullOrEmpty(sceneGuid))
                return $"{sceneGuid}.{EntityId.ToULong(obj.GetEntityId())}";
        }

        return "";
    }

    public static UnityEngine.Object StringToObject(string str)
    {
        int dotIdx = str.IndexOf('.');
        string sceneGuid = str.Substring(0, dotIdx > 0 ? dotIdx : str.Length);
        string objInScene = dotIdx >= 0 ? str.Substring(dotIdx + 1) : "";
        ulong objInSceneAsEntityId = 0;

        if (!string.IsNullOrEmpty(objInScene))
            ulong.TryParse(objInScene, out objInSceneAsEntityId);

        UnityEngine.Object sceneAsset = AssetDatabase.LoadAssetByGUID(new GUID(sceneGuid), typeof(UnityEngine.Object));
        if (sceneAsset)
        {
            if (!string.IsNullOrEmpty(objInScene))
            {
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);

                // If the scene/prefab is loaded we can select the object directly
                if ((prefabStage != null && prefabStage.assetPath == scenePath)
                        || EditorSceneManager.GetActiveScene().path == scenePath)
                    return EditorUtility.EntityIdToObject(EntityId.FromULong(objInSceneAsEntityId));
                else
                    return sceneAsset; // Otherwise the scene is the best option we have right now
            }
            else
                return sceneAsset;
        }

        return null;
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
        currentAssetHistoryItemIndex = assetHistory.Count - 1;
    }

    private static void SaveHistory()
    {
        PlayerPrefs.SetString("UserRecentAssetHistory", string.Join(';', assetHistory));
    }
}
