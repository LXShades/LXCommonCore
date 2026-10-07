using LX.Common.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace LX.Common.Core.Editor
{
    [InitializeOnLoad]
    public static class EditorSelectionHistory
    {
        private const int kMaxSelectionHistoryLength = 30;
        private const int kMaxAssetHistoryLength = 30;
        private static bool hasDoneHistoryActionThisFrame = false;

        private static bool shouldIncludeFolders = false;

        public static IReadOnlyList<string> SelectionHistory => selectionHistory.AsReadOnly();
        public static IReadOnlyList<string> AssetHistory => assetHistory.AsReadOnly();

        private static List<string> selectionHistory = new();
        private static List<string> assetHistory = new();
        private static int currentSelectionHistoryItemIndex;
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

                Object selectedObject = Selection.objects[0];
                string objectAsString = ObjectToString(selectedObject);
                // Asset history is simple, just add latest and ensure there's just one of each
                if (!(selectedObject is GameObject selectedGameObject) || string.IsNullOrEmpty(selectedGameObject.scene.name))
                {
                    assetHistory.Remove(objectAsString);
                    assetHistory.Add(objectAsString);
                }

                // Remove remaining upcoming history items because we are creating a new history
                if (currentSelectionHistoryItemIndex + 1 < selectionHistory.Count)
                    selectionHistory.RemoveRange(currentSelectionHistoryItemIndex + 1, selectionHistory.Count - (currentSelectionHistoryItemIndex + 1));

                selectionHistory.Remove(objectAsString);
                selectionHistory.Add(objectAsString);
                currentSelectionHistoryItemIndex = selectionHistory.Count - 1;
            }

            if (selectionHistory.Count > kMaxSelectionHistoryLength)
                selectionHistory.RemoveRange(0, selectionHistory.Count - kMaxSelectionHistoryLength);

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
            currentSelectionHistoryItemIndex = Mathf.Clamp(currentSelectionHistoryItemIndex + offset, selectionHistory.Count > 0 ? 0 : -1, selectionHistory.Count - 1);

            if (selectionHistory.IsValidIndex(currentSelectionHistoryItemIndex))
            {
                string historyItem = selectionHistory[currentSelectionHistoryItemIndex];
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
            string selectionHistoryString = PlayerPrefs.GetString("UserRecentSelectionHistory");
            if (selectionHistoryString != null)
            {
                selectionHistory.Clear();
                selectionHistory.AddRange(selectionHistoryString.Split(';'));
            }
            string assetHistoryString = PlayerPrefs.GetString("UserRecentAssetHistory");
            if (assetHistoryString != null)
            {
                assetHistory.Clear();
                assetHistory.AddRange(selectionHistoryString.Split(';'));
            }

            currentSelectionHistoryItemIndex = selectionHistory.Count - 1;
        }

        private static void SaveHistory()
        {
            PlayerPrefs.SetString("UserRecentSelectionHistory", string.Join(';', selectionHistory));
            PlayerPrefs.SetString("UserRecentAssetHistory", string.Join(';', assetHistory));
        }
    }
}