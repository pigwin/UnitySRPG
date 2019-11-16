using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System;

public class CreateScriptableObject : MonoBehaviour
{
    [MenuItem("Assets/Create/CharaData")]
    public static void CreateCharaData()
    {
        CreateAsset<CharaData>("BattleChara");
    }

    public static void CreateAsset<Type>(string foldername) where Type : ScriptableObject
    {
        Type item = ScriptableObject.CreateInstance<Type>();

        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/"+ foldername +"/" + typeof(Type) + ".asset");

        AssetDatabase.CreateAsset(item, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }

    [MenuItem("Assets/Create/SkillData")]
    public static void CreateSkillData()
    {
        CreateAsset<Skill>("SkillData");
    }

    [MenuItem("Assets/Create/PartySettings")]
    public static void CreatePartySettings()
    {
        CreateAsset<PartySettings>("PartySettings");
    }

}

public class CreateNovelScript : MonoBehaviour
{
    [MenuItem("Assets/Create/NovelScript")]
    public static void CreateTextFile()
    {
        var path = Application.streamingAssetsPath;

        var fileName = "newtext.txt";
        path += "/" + fileName;
        int cnt = 0;
        while (File.Exists(path))
        {
            if (path.Contains(fileName))
            {
                cnt++;
                var newFileName = "new text " + cnt + ".txt";
                path = path.Replace(fileName, newFileName);
                fileName = newFileName;
            }
            else
            {
                Debug.LogError("path dont contain " + fileName);
                break;
            }
        }

        // 空のテキストを書き込む.
        File.WriteAllText(path, "UTF-8で編集してください", System.Text.Encoding.UTF8);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 指定パスがフォルダかどうかチェックします
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsFolder(string path)
    {
        try
        {
            return File.GetAttributes(path).Equals(FileAttributes.Directory);
        }
        catch (Exception ex)
        {
            if (ex.GetType() == typeof(FileNotFoundException))
            {
                return false;
            }
            throw ex;
        }
    }


}

public class Create_MapData : MonoBehaviour
{
    [MenuItem("Assets/Create/MapData")]
    public static void CreateTextFile()
    {
        var path = Application.streamingAssetsPath;

        var fileName = "newmap.csv";
        path += "/" + fileName;
        int cnt = 0;
        while (File.Exists(path))
        {
            if (path.Contains(fileName))
            {
                cnt++;
                var newFileName = "new map " + cnt + ".csv";
                path = path.Replace(fileName, newFileName);
                fileName = newFileName;
            }
            else
            {
                Debug.LogError("path dont contain " + fileName);
                break;
            }
        }

        // 空のテキストを書き込む.
        File.WriteAllText(path, "", System.Text.Encoding.UTF8);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 指定パスがフォルダかどうかチェックします
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsFolder(string path)
    {
        try
        {
            return File.GetAttributes(path).Equals(FileAttributes.Directory);
        }
        catch (Exception ex)
        {
            if (ex.GetType() == typeof(FileNotFoundException))
            {
                return false;
            }
            throw ex;
        }
    }
}
#endif
