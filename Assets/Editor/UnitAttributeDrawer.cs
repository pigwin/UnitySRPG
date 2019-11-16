using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;


[CustomPropertyDrawer(typeof(UnitAttribute))]
public class UnitAttributeDrawer : PropertyDrawer
{
    private UnitAttribute unitattribute
    {
        get { return (UnitAttribute)attribute; }
    }
    private bool isInitialized = false;
    private int[] unitIds = null;
    private string[] unitLabels = null;
    public static Dictionary<int, string> names = new Dictionary<int, string>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Initialize
        if (!isInitialized)
        {
            Dictionary<int, string> units = GetUnitLabels();
            unitIds = new int[units.Count];
            unitLabels = new string[units.Count];
            units.Keys.CopyTo(unitIds, 0);
            units.Values.CopyTo(unitLabels, 0);

            isInitialized = true;
        }
       
        if (property.propertyType == SerializedPropertyType.String)
        {
            string[] names2 = new string[names.Count];
            names.Values.CopyTo(names2, 0);
            int index = Mathf.Max(ArrayUtility.IndexOf(names2, property.stringValue), 0);
            property.stringValue = names[EditorGUI.Popup(position, label.text, index, names2)];

        }
    }

    public static Dictionary<int, string> GetUnitLabels()
    {
        Dictionary<int, string> result = new Dictionary<int, string>();

        //キャラデータを探す
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", "CharaData"));
        if (guids.Length == 0)
        {
            return result;
        }
        int targetId = 0;
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CharaData chara = AssetDatabase.LoadAssetAtPath<CharaData>(assetPath);

            //フォルダによる絞り込み
            Match match = Regex.Match(assetPath, string.Format("Assets/Resources/{0}/(.*?).asset", "BattleChara"));
            
            foreach (Group g in match.Groups)
            {
                if (g.Index == 0)
                {
                    continue;
                }
                //キャラクタ名を表示
                result[targetId] = string.Format("{0}: {1}", targetId, chara.charaname);
                names[targetId] = assetPath;
                names[targetId] = names[targetId].Substring(string.Format("Assets/Resources/{0}/", "BattleChara").Length);
                names[targetId] = names[targetId].Substring(0, names[targetId].Length - string.Format(".asset").Length);
                targetId++;
            }
        }

        return result;
    }
}