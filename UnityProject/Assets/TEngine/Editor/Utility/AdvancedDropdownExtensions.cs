using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TEngine.Editor
{
    public static class AdvancedDropdownExtensions
    {
        public static void Show(this AdvancedDropdown dropdown, Rect buttonRect, float width, float height)
        {
            dropdown.Show(buttonRect);
            SetMaxHeightForOpenedPopup(buttonRect, width, height);
        }

        private static void SetMaxHeightForOpenedPopup(Rect buttonRect, float width, float height)
        {
            var window = EditorWindow.focusedWindow;

            if(window == null)
            {
                Log.Warning("EditorWindow.focusedWindow was null.");
                return;
            }

            if(!string.Equals(window.GetType().Namespace, typeof(AdvancedDropdown).Namespace))
            {
                Log.Warning("EditorWindow.focusedWindow " + EditorWindow.focusedWindow.GetType().FullName + " was not in expected namespace.");
                return;
            }

            var position = window.position;
            position.width = width > position.width ? width : position.width;
            position.height = height > position.height ? height : position.height;
            window.minSize = position.size;
            window.maxSize = position.size;
            window.position = position;
            window.ShowAsDropDown(GUIUtility.GUIToScreenRect(buttonRect), position.size);
        }
    }
}