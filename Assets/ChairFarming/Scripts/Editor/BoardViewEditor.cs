#if UNITY_EDITOR
using ChairFarming.Runtime.Board;
using UnityEditor;
using UnityEngine;

namespace ChairFarming.Editor
{
    [CustomEditor(typeof(BoardView))]
    public sealed class BoardViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(8f);

            BoardView boardView = (BoardView)target;

            if (GUILayout.Button("Auto Collect Layout"))
            {
                Undo.RecordObject(boardView, "Auto Collect Board Layout");
                boardView.AutoCollectLayout();
                EditorUtility.SetDirty(boardView);
            }

            if (GUILayout.Button("Rebuild Rows"))
            {
                Undo.RecordObject(boardView, "Rebuild Board Rows");
                boardView.RebuildRows();
                EditorUtility.SetDirty(boardView);
            }
        }
    }
}
#endif
