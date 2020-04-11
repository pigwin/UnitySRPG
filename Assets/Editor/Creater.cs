using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public class Creater
{
    [MenuItem("Assets/Create/BattleCreate")]
    private static void CopyBattleScene()
    {
        string path = "Assets/Scenes/BaseScene/BattleBase.unity";
        string path2 = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/Battle1.unity");
        EditorSceneManager.OpenScene(path);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path2);
        Debug.Log(EditorBuildSettings.scenes.Length);
        List<EditorBuildSettingsScene> scenelist = new List<EditorBuildSettingsScene>();
        foreach(EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            scenelist.Add(s);
        }
        scenelist.Add(new EditorBuildSettingsScene(path2, true));
        EditorBuildSettings.scenes = scenelist.ToArray();
    }

    [MenuItem("Assets/Create/NovelCreate")]
    private static void CopyNovelScene()
    {
        string path = "Assets/Scenes/BaseScene/NovelBase.unity";
        string path2 = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/Novel1.unity");
        EditorSceneManager.OpenScene(path);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path2);
        List<EditorBuildSettingsScene> scenelist = new List<EditorBuildSettingsScene>();
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            scenelist.Add(s);
        }
        scenelist.Add(new EditorBuildSettingsScene(path2, true));
        EditorBuildSettings.scenes = scenelist.ToArray();
    }

    static string copy_path ="";
    [MenuItem("Assets/Copy", false)]
    private static void Copy()
    {
        int instanceID = Selection.activeInstanceID;
        string path = AssetDatabase.GetAssetPath(instanceID);
        copy_path = System.IO.Path.GetFullPath(path);
    }

    [MenuItem("Assets/Paste", false)]
    private static void Paste()
    {
        if (copy_path.Length == 0) return;
        string full_path = System.IO.Path.GetFullPath(copy_path);
        int paste_instanceID = Selection.activeInstanceID;
        string paste_path = AssetDatabase.GetAssetPath(paste_instanceID);
        string[] temp = copy_path.Split('\\');
        Debug.Log(temp[temp.Length - 1]);
        paste_path += "/" + temp[temp.Length - 1];
        Debug.Log(paste_path);
        paste_path = AssetDatabase.GenerateUniqueAssetPath(paste_path);
        Debug.Log(full_path);
        Debug.Log(paste_path);
        System.IO.File.Copy(full_path, paste_path);
        AssetDatabase.Refresh();
    }
}
