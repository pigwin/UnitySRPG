using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class Copy
{
    static string copy_path = "";
    [MenuItem("Assets/AssetsCopy")]
    private static void CopyScene()
    {
        int instanceID = Selection.activeInstanceID;
        copy_path = AssetDatabase.GetAssetPath(instanceID);
        Debug.Log(copy_path);
    }

    [MenuItem("Assets/AssetsPaste")]
    private static void PasteScene()
    {
        if (copy_path.Length == 0) return;
        string full_path = System.IO.Path.GetFullPath(copy_path);
        int paste_instanceID = Selection.activeInstanceID;
        string paste_path = AssetDatabase.GetAssetPath(paste_instanceID);
        string[] temp = copy_path.Split('/');
        Debug.Log(temp[temp.Length - 1]);
        paste_path += "/" + temp[temp.Length - 1];
        paste_path = AssetDatabase.GenerateUniqueAssetPath(paste_path);
        EditorSceneManager.OpenScene(full_path);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), paste_path);
    }
}
