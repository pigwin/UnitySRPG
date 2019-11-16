using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MapEditor : EditorWindow
{

	[MenuItem("Window/Example")]
    static void Open()
    {
        GetWindow<MapEditor>();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Example Labelv");
    }
}
#endif
