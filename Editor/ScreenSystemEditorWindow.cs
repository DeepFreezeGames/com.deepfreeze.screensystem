using GameEditorWindow.Editor;
using UnityEngine;

namespace ScreenSystem.Editor
{
    public class ScreenSystemEditorWindow : IGameEditorWindow
    {
        public GUIContent Icon { get; } = new GUIContent("S", "Screen System");
        public int SortOrder { get; } = 1;
        
        public void OnFocused()
        {
            
        }

        public void OnFocusLost()
        {
            
        }

        public void ToolbarLeft()
        {
            
        }

        public void ToolbarRight()
        {
            
        }

        public void MainContent()
        {
            
        }
    }
}
