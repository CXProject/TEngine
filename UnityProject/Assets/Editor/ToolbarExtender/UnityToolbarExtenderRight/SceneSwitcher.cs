#if !UNITY_6000_3_OR_NEWER

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TEngine.Editor;

namespace TEngine
{
    class SceneItem : AdvancedDropdownItem
    {
        public string Path { get; }

        public SceneItem(string name, string path) : base(name)
        {
            Path = path;
        }
    }

    class SceneDropdown : AdvancedDropdown
    {
        private Action<string> _onSelect;
        private Dictionary<string, List<string>> _sceneItems;

        public SceneDropdown(AdvancedDropdownState state, Dictionary<string, List<string>> sceneItems, Action<string> onSelect) : base(state)
        {
            _onSelect = onSelect;
            _sceneItems = sceneItems;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Root");
            _sceneItems ??= new Dictionary<string, List<string>>();
            foreach (var sp in _sceneItems)
            {
                var category = new AdvancedDropdownItem(sp.Key);
                foreach (var path in sp.Value)
                {
                    var item = new SceneItem(Path.GetFileNameWithoutExtension(path), path);
                    category.AddChild(item);
                }
                root.AddChild(category);
            }
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            if (item is SceneItem sceneItem)
                _onSelect?.Invoke(sceneItem.Path);
        }
    }
    
    /// <summary>
    /// SceneSwitcher
    /// </summary>
    public partial class UnityToolbarExtenderRight
    {
        private static Dictionary<string, List<string>> m_Scenes;

        private static string initScenePath = "Assets/Scenes";
        private static string defaultScenePath = "Assets/AssetRaw/Scenes";
        
        private static SceneDropdown _sceneDropdown;
        
        static void UpdateScenes()
        {
            // 获取初始化场景和默认场景
            var initScenes = SceneSwitcher.GetScenesInPath(initScenePath);
            var defaultScenes = SceneSwitcher.GetScenesInPath(defaultScenePath);

            // 获取所有场景路径
            List<(string sceneName, string scenePath)> allScenes = SceneSwitcher.GetAllScenes();

            // 排除初始化场景和默认场景，获得其他场景
            var otherScenes = new List<(string sceneName, string scenePath)>(allScenes);
            otherScenes.RemoveAll(scene =>
                initScenes.Exists(init => init.scenePath == scene.scenePath) ||
                defaultScenes.Exists(defaultScene => defaultScene.scenePath == scene.scenePath)
            );
            
            m_Scenes ??= new Dictionary<string, List<string>>();
            m_Scenes.Clear();
            AddScene("正式场景", initScenes);
            AddScene("默认场景", defaultScenes);
            AddScene("其他场景", otherScenes);
            _sceneDropdown = new SceneDropdown(new AdvancedDropdownState(), m_Scenes, SwitchScene);
        }
        
        static void AddScene(string category, List<(string sceneName, string scenePath)> scenes)
        {
            if(scenes.Count == 0) return;
            var list = new List<string>();
            foreach (var ss in scenes)
            {
                list.Add((ss.scenePath));
            }
            m_Scenes.Add(category, list);
        }

        static void OnToolbarGUI_SceneSwitch()
        {
            // 如果没有场景，直接返回
            if (m_Scenes.Count == 0)
                return;

            // 获取当前场景名称
            string currentSceneName = SceneManager.GetActiveScene().name;
            EditorGUILayout.LabelField("当前场景:", GUILayout.Width(52));

            // 使用 GUI.skin.button.CalcSize 计算文本的精确宽度
            GUIContent content = new GUIContent(currentSceneName);
            Vector2 textSize = GUI.skin.button.CalcSize(content);

            // 设置按钮宽度为文本的宽度，并限制最大值
            float buttonWidth = textSize.x;
            // 自定义GUIStyle
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft // 左对齐
            };

            // 在工具栏中显示菜单
            var rect = GUILayoutUtility.GetRect(content, buttonStyle, GUILayout.Width(buttonWidth));
            if (GUI.Button(rect, content, buttonStyle))
            {
                
                _sceneDropdown.Show(rect, 150,200);
            }
        }

        private static void AddScenesToMenu(List<(string sceneName, string scenePath)> scenes, string category, GenericMenu menu)
        {
            if (scenes.Count > 0)
            {
                foreach (var scene in scenes)
                {
                    menu.AddItem(new GUIContent($"{category}/{scene.sceneName}"), false, () => SwitchScene(scene.scenePath));
                }
            }
        }

        static void SwitchScene(string scenePath)
        {
            // 保证场景是否保存
            if (SceneSwitcher.PromptSaveCurrentScene())
            {
                // 保存场景后切换到新场景
                EditorSceneManager.OpenScene(scenePath);
            }
        }
    }

    static class SceneSwitcher
    {
        public static bool PromptSaveCurrentScene()
        {
            // 检查当前场景是否有未保存的更改
            if (SceneManager.GetActiveScene().isDirty)
            {
                // 弹出保存对话框
                bool saveScene = EditorUtility.DisplayDialog(
                    "是否保存当前场景",
                    "当前场景有未保存的更改. 你是否想保存?",
                    "保存",
                    "取消"
                );

                // 如果选择保存，保存场景
                if (saveScene)
                {
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                }
                else
                {
                    // 如果选择取消，跳转到目标场景
                    return false; // 表示取消
                }

                return true;
            }

            return true; // 如果场景没有更改，直接返回 true
        }

        public static List<(string sceneName, string scenePath)> GetScenesInPath(string path)
        {
            var scenes = new List<(string sceneName, string scenePath)>();

            // 查找指定路径下的所有场景文件
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { path });
            foreach (var guid in guids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(guid);
                var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                scenes.Add((sceneName, scenePath));
            }

            return scenes;
        }

        // 获取项目中所有场景
        public static List<(string sceneName, string scenePath)> GetAllScenes()
        {
            var allScenes = new List<(string sceneName, string scenePath)>();

            // 查找项目中所有场景文件
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (var guid in guids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(guid);
                var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                allScenes.Add((sceneName, scenePath));
            }

            return allScenes;
        }
    }
}
#endif

#endif