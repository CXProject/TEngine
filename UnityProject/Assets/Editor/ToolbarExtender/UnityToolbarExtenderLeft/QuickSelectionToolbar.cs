using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TEngine
{
    public static partial class UnityToolbarExtenderLeft
    {
        private static readonly Dictionary<GUIContent, string> _nameToPathDict = new Dictionary<GUIContent, string>
        {
            {new GUIContent("代码文件夹"), "Assets/GameScripts/"},
            {new GUIContent("UI文件夹"), "Assets/AssetRaw/UI/"},
            {new GUIContent("Actor文件夹"), "Assets/AssetRaw/Actor/"},
            {new GUIContent("Configs文件夹"), "Assets/AssetRaw/Configs/"}
        };
        
        private static void OnQuickSelectionToolbarGUI()
        {
            // 创建下拉菜单按钮
            if (EditorGUILayout.DropdownButton(
                    new GUIContent("快速定位操作", EditorGUIUtility.IconContent("d__Popup").image),
                    FocusType.Passive,
                    EditorStyles.toolbarDropDown, GUILayout.Width(300)))
            {
                var menu = new GenericMenu();
                foreach (var keyValue in _nameToPathDict)
                {
                    menu.AddItem(keyValue.Key, false, () =>
                    {
                        if (keyValue.Value.StartsWith("Assets/"))
                        {
                            //是工程内的文件夹
                            string[] guids = AssetDatabase.FindAssets("", searchInFolders: new[] {keyValue.Value});
                            foreach (var i in guids)
                            {
                                string path = AssetDatabase.GUIDToAssetPath(i);
                                var use = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                                Selection.activeObject = use;
                                EditorGUIUtility.PingObject(use);
                                break;
                            }
                        }
                        else
                        {
                            //是工程外的文件夹
                            string dir = $"{Directory.GetCurrentDirectory()}\\Configs\\";
                            UnityEditor.EditorUtility.RevealInFinder(dir);
                        }
                    });
                }
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("<玩家对象>"), false, () =>
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        Selection.activeGameObject = player;
                        EditorGUIUtility.PingObject(player);
                    }
                });
                
                // 添加菜单项
                menu.AddSeparator("");
                menu.ShowAsContext();
            }
        }
    }
}