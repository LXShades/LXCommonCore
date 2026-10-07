using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LX.Common.Core.Editor
{
    public class RecentAssetsEditorWindow : EditorWindow
    {
        ScrollView scrollView;
        Toggle includeSceneSelectionsToggle;

        public void CreateGUI()
        {
            includeSceneSelectionsToggle = new UnityEngine.UIElements.Toggle("Include in-scene selections (not just assets)");
            includeSceneSelectionsToggle.RegisterValueChangedCallback(x => Refresh());
            scrollView = new ScrollView();
            scrollView.style.flexGrow = new StyleFloat(1);
            scrollView.style.flexShrink = new StyleFloat(1);
            scrollView.contentContainer.style.flexDirection = FlexDirection.Row;
            scrollView.contentContainer.style.flexWrap = Wrap.Wrap;

            Refresh();

            rootVisualElement.Clear();
            rootVisualElement.Add(includeSceneSelectionsToggle);
            rootVisualElement.Add(scrollView);
        }

        public void Refresh()
        {
            if (scrollView == null)
            {
                CreateGUI();
                // CreateGUI calls Refresh itself
                return;
            }

            scrollView.Clear();

            IReadOnlyList<string> recents = includeSceneSelectionsToggle.value ? EditorSelectionHistory.SelectionHistory : EditorSelectionHistory.AssetHistory;
            for (int i = recents.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object obj = EditorSelectionHistory.StringToObject(recents[i]);
                GameObject go = obj as GameObject;

                if (go && !string.IsNullOrEmpty(go.scene.name) && includeSceneSelectionsToggle.value)
                    continue;

                if (obj)
                {
                    Button button = new Button();
                    var icon = button.iconImage;
                    button.style.width = 128;
                    button.style.height = 128;
                    if (go && !string.IsNullOrEmpty(go.scene.path))
                        button.text = $"{obj.name}\n(in {go.scene.name})";
                    else
                        button.text = obj.name;
                    icon.texture = AssetPreview.GetAssetPreview(obj);
                    if (icon.texture == null)
                        icon.texture = AssetPreview.GetMiniThumbnail(obj);
                    button.iconImage = icon;
                    button.style.flexDirection = FlexDirection.Column;
                    button.clicked += () => {
                        /*if ((AssetDatabase.IsMainAsset(obj) || AssetDatabase.IsSubAsset(obj)) && AssetDatabase.CanOpenAssetInEditor(obj.GetEntityId()))
                            AssetDatabase.OpenAsset(obj);
                        else*/
                            Selection.activeObject = obj;

                        scrollView.verticalScroller.value = 0f;
                    };

                    scrollView.Add(button);
                }
            }
        }
    }
}
