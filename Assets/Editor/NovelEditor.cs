using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
[CustomEditor(typeof(NovelSetCompiler))]
public class NovelEditor : Editor {
    //スクリプトのクエリごとに保存する。
    List<Scription> scriptions = new List<Scription>();
    int line_counts = 30;

    //ラベルの名前と行数を格納するディクショナリ
    Dictionary<string, int> label_line = new Dictionary<string, int>();
    Dictionary<string, string> label_filename = new Dictionary<string, string>();

    //色情報を作る関数
    //アニメーションの値を変換
    ANIMATION_TYPE Animation_Type_Define(int temp)
    {
        switch (temp)
        {
            case 0:
                return ANIMATION_TYPE.ANIMATION_SYNCHRO; 
            case 1:
                return ANIMATION_TYPE.ANIMATION_NO_TIME;
            case 10:
                return ANIMATION_TYPE.FADE_IN;
            default:
                return ANIMATION_TYPE.MASKRULE;
        }
        return ANIMATION_TYPE.ANIMATION_NO_TIME;
    }

    //会話文中の命令
    string AddBreak(string str)
    {
        int[] index = new int[str.Length];
        int source_length = str.Length;
        int count = 0;
        bool start = false;
        Debug.Log("AddBreak Check");
        int addition = 0;
        for(int i = 0; i < str.Length; i++)
        {
            if(str[i] == '(')
            {
                start = true;
                count--;
                index[i] = count;
                count++;
            }else if(str[i]=='！' || str[i] =='。' || str[i] == '」' || str[i] == '？')
            {
                count--;
                index[i] = count;
                count++;
            }
            else
            {
                index[i] = count;
            }
            //会話文内のコマンドに関しては後で追加
            //ただし、方法としては、コマンドの範囲内まですべてcount -1で埋めてiの値をそこまでスキップする。
            if(str[i] == ')')
            {
                start = false;
                count += addition;
                addition = 0;
            }
            if (!start)
            {
                count++;
            }
            else
            {
                addition++;
            }
        }
        for(count = line_counts;count < source_length ;count += line_counts )
        {
            str = str.Insert(index[count], "\n");
            count += 1;
            for (int j = count; j < source_length; j++)
            {
                index[j]++;
            }
        }
        return str;
    }
    //Special_script_parserの実際のコマンドを解析する関数
    void Special_script_parser_sub(string temp)
    {
        switch (temp[0])
        {
            case '!':
                {
                    COMMAND_TYPE t = COMMAND_TYPE.WAIT_B;
                    if (temp[1] == 'w') t = COMMAND_TYPE.WAIT_W;
                    string timer = temp.Substring(2, temp.Length - 2);
                    scriptions.Add(new WAIT_scription(Int32.Parse(timer), true, temp, t));
                }
                break;
            default:
                if (temp[0] == 'b' && temp[1] == 'g' && temp[2] == ' ')
                {
                    string[] a = temp.Split(' ');
                    string[] b = a[1].Split(',');
                    int time = 0;

                    //アニメーション情報の指定                    
                    ANIMATION_TYPE type = Animation_Type_Define(Int32.Parse(b[1]));
                    if (type != ANIMATION_TYPE.ANIMATION_NO_TIME)
                    {
                        time = Int32.Parse(b[2]);
                    }
                    if (b[0].Equals("white"))
                    {
                        scriptions.Add(new COLOR_scription(Scripter.Color_Parser("#FFFFFF"), b[1], COMMAND_TYPE.BACKGROUND_COLOR));
                    }
                    else if (b[0].Equals("black"))
                    {
                        scriptions.Add(new COLOR_scription(Scripter.Color_Parser("#000000"), b[1], COMMAND_TYPE.BACKGROUND_COLOR));
                    }
                    else if (b[0][0] == '#')
                    {
                        scriptions.Add(new COLOR_scription(Scripter.Color_Parser(b[0]), b[1], COMMAND_TYPE.BACKGROUND_COLOR));
                    }
                    else
                    {
                        string[] path = b[0].Split('\"');

                        scriptions.Add(new IMAGE_scription(null, type, b[1], time,path[1], COMMAND_TYPE.BACKGROUND_IMAGE));
                    }
                    break;
                }

                if (temp[0] == 'l' && temp[1] == 'd')
                {
                    string[] a = temp.Split(' ');
                    string[] b = a[1].Split(',');
                    int time = 0;
                    int place = 1;
                    COMMAND_TYPE command_tmp = COMMAND_TYPE.STAND_IMAGE;
                    switch (b[0][0])
                    {
                        case 'c':
                            place = 2;
                            //command_tmp = COMMAND_TYPE.STAND_IMAGE_CENTER;
                            break;
                        case 'l':
                            place = 4;
                            //command_tmp = COMMAND_TYPE.STAND_IMAGE_RIGHT;
                            break;
                    }
                    string[] path = b[1].Split('\"');

                    //アニメーション情報の指定                    
                    ANIMATION_TYPE type = Animation_Type_Define(Int32.Parse(b[2]));

                    //エフェクト番号0番、1番は時間を取得しない
                    if (type != ANIMATION_TYPE.ANIMATION_NO_TIME && type != ANIMATION_TYPE.ANIMATION_SYNCHRO)
                    {
                        time = Int32.Parse(b[3]);
                    }
                    scriptions.Add(new STAND_IMAGE_scription(place, null, type, b[1], time, path[1], command_tmp));
                    break;
                }
                Debug.Log((temp));
                scriptions.Add(new Text_scription("", false, AddRuby(AddBreak(temp)), COMMAND_TYPE.NORMAL_TEXT_CONTINUE));
                break;
        }

    }
    bool CheckBreak(int counts)
    {
        if (counts > line_counts) return true;
        return false;
    }
    //ルビ情報の追加
    public float rubycorrect_sp = 3.5f;
    public float rubycorrect_int = 3;
    string AddRuby(string str)
    {
        string[] temp = str.Split('(', ')');
        string return_str = "";
        for (int i = 0; i < temp.Length; i++)
        {
            if (temp[i].Contains("/"))
            {
                string[] temp2 = temp[i].Split('/');
                float main_text = temp2[0].Length;
                float tag = temp2[1].Length;
                if (tag != 1)
                {
                    return_str += temp2[0] + 
                        string.Format("\\tag{3}<space=-{0}em><voffset=1em><size=50%><cspace={1}em>{2} </size></cspace></voffset>\\tag",
                        main_text * rubycorrect_sp, (main_text * rubycorrect_int - tag) / (tag - 1), temp2[1].Substring(0, temp2[1].Length), main_text);
                }
                else
                {
                    return_str += temp2[0] +
                        string.Format("\\tag{2}<space=-{0}em><voffset=1em><size=50%>{1} </size></voffset><space={0}>\\tag", 
                        main_text * 1.5 + 0.75, temp2[1].Substring(0, temp2[1].Length), main_text);
                }
            }
            else
            {
                return_str += temp[i];
            }
        }

        return return_str;
    }
    //表示画面内のコマンドに対する処理
    void Special_script_parser(string temp)
    {
        Debug.Log("Special_script_parser");
        string[] a = temp.Split(' ');
        string[] tmp;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i].Equals("ld"))
            {
                a[i] += " " + a[i + 1];
                a[i + 1] = " ";
            }
        }
        Queue<string> list = new Queue<string>();
        for (int i = 0; i < a.Length; i++)
        {
            tmp = a[i].Split('@');
            for (int j = 0; j < tmp.Length; j++)
            {
                if (tmp[j] != " " && tmp[j].Length > 0) list.Enqueue(tmp[j]);
                else if (tmp[j].Length == 0 && j % 2 == 0) list.Enqueue(" ");
            }
        }
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            tmp = list.Dequeue().Split('!');
            for (int j = 0; j < tmp.Length; j++)
            {
                list.Enqueue(tmp[j]);
            }
        }
        string first = list.Dequeue();
        switch (first[0])
        {
            case '[':
                {
                    string[] s = first.Split(']');
                    string[] t = s[0].Split(',');
                    string character_name = t[0].Substring(1, t[0].Length - 1);
                    if (t.Length > 1)
                    {
                        scriptions.Add(new AUDIO_scription(null, "",t[1].Split('\"')[1], COMMAND_TYPE.AUDIO_VOICE));

                        scriptions.Add(new Text_scription(t[0].Substring(1, t[0].Length - 1), true, AddRuby(AddBreak(s[1])), COMMAND_TYPE.NORMAL_TEXT));
                    }
                    else
                    {
                        AddRuby(s[1]);
                        scriptions.Add(new Text_scription(character_name, false, AddRuby(AddBreak(s[1])), COMMAND_TYPE.NORMAL_TEXT));
                    }
                }
                break;
            default:
                AddRuby(first);
                scriptions.Add(new Text_scription("", false, AddRuby(AddBreak(first)), COMMAND_TYPE.NORMAL_TEXT));
                break;
        }
        while (list.Count > 0)
        {
            Special_script_parser_sub(list.Dequeue());
        }
    }
    //スクリプトを構文解析する関数
    void Script_parser(string temp)
    {
        Debug.Log("Script_parser");
        if (temp.Length <= 0) return;
        switch (temp[0])
        {
            //コメント
            case ';':
                break;
            case '!':
                {
                    COMMAND_TYPE t = COMMAND_TYPE.WAIT_B;
                    if (temp[1] == 'w') t = COMMAND_TYPE.WAIT_W;
                    if (temp[1] == 's') t = COMMAND_TYPE.WAIT_S;
                    string timer = temp.Substring(2, temp.Length - 2);
                    bool skipflag = true;
                    scriptions.Add(new WAIT_scription(Int32.Parse(timer), skipflag, temp, t));
                }
                break;
            case '[':
                {
                    Special_script_parser(temp);
                }
                break;
            case '#':
                scriptions.Add(new COLOR_scription(Scripter.Color_Parser(temp), "", COMMAND_TYPE.COLOR));
                break;
            case '*':
                Debug.Log(temp);
                label_line.Add(temp, scriptions.Count);
                label_filename.Add(temp, "");
                break;
            default:
                string[] a = temp.Split(' ');           //命令コマンドと引数を分離
                //bg命令
                if (a[0] == "bg")
                {
                    //string[] a = temp.Split(' ');   //命令コマンドと引数を分離
                    string[] b = a[1].Split(',');   //引数をカンマで各取得
                    int time = 0;

                    //アニメーション情報の指定                    
                    ANIMATION_TYPE type = Animation_Type_Define(Int32.Parse(b[1]));
                    if (type != ANIMATION_TYPE.ANIMATION_NO_TIME && type != ANIMATION_TYPE.ANIMATION_SYNCHRO)
                    {
                        time = Int32.Parse(b[2]);
                    }

                    //命令第一引数で、背景の指定を変化（色or画像）
                    if (b[0].Equals("white"))
                    {
                        scriptions.Add(new COLOR_scription(Scripter.Color_Parser("#FFFFFF"), b[1], time, type, COMMAND_TYPE.BACKGROUND_COLOR));
                    }
                    else if (b[0].Equals("black"))
                    {
                        scriptions.Add(new COLOR_scription(Scripter.Color_Parser("#000000"), b[1], time, type, COMMAND_TYPE.BACKGROUND_COLOR));
                    }
                    else if (b[0][0] == '#')
                    {
                        scriptions.Add(new COLOR_scription(Scripter.Color_Parser(b[0]), b[1], time, type, COMMAND_TYPE.BACKGROUND_COLOR));
                    }
                    else
                    {
                        string[] path = b[0].Split('\"');
                        scriptions.Add(new IMAGE_scription(null, type, b[1], time,path[1], COMMAND_TYPE.BACKGROUND_IMAGE));
                    }

                    break;
                }
                //bgm命令
                else if (a[0] == "bgm")
                {
                    string[] b = a[1].Split('\"');
                    scriptions.Add(new AUDIO_scription(Resources.Load<AudioClip>(b[1]), "",b[1], COMMAND_TYPE.AUDIO_BGM));
                    break;
                }
                //bgmstop命令
                else if (a[0] == "bgmstop")
                {
                    scriptions.Add(new Scription("bgmstop", COMMAND_TYPE.AUDIO_BGM_STOP));
                    break;
                }

                //cl命令
                if (a[0] == "cl")
                {
                    //string[] a = temp.Split(' ');
                    string[] b = a[1].Split(',');
                    int place = 1;  //r 001
                    int time = 0;
                    switch (b[0][0])
                    {
                        case 'c':   //010
                            place = 2;
                            break;
                        case 'l':   //100
                            place = 4;
                            break;
                        case 'a':   //111
                            place = 7;
                            break;
                    }
                    //アニメーション情報の指定                    
                    ANIMATION_TYPE type = Animation_Type_Define(Int32.Parse(b[1]));
                    if (type != ANIMATION_TYPE.ANIMATION_NO_TIME && type != ANIMATION_TYPE.ANIMATION_SYNCHRO)
                    {
                        time = Int32.Parse(b[2]);
                    }

                    scriptions.Add(new IMAGE_clear_scription(place, type, time, b[1], COMMAND_TYPE.STAND_IMAGE_CLEAR));
                    break;
                }
                //partyadd命令
                // partyadd scobjname,level  レベルは無くてもよい
                if(a[0] == "partyadd")
                {
                    string[] b = a[1].Split(',');
                    string scobjname = b[0];
                    int level = 0;
                    //levelがかかれていた場合
                    if(b.Length >= 2)
                    {
                        level = int.Parse(b[1]);
                    }

                    scriptions.Add(new PARTY_scription(scobjname, level,"PARTY ADD", COMMAND_TYPE.PARTY_ADD));
                    break;
                }
                //partyremove命令
                // partyremove scobjname
                if (a[0] == "partyremove")
                {
                    string scobjname = a[1];
                    scriptions.Add(new PARTY_scription(scobjname, 0, "PARTY REMOVE", COMMAND_TYPE.PARTY_REMOVE));
                    break;
                }
                //delay命令
                if (a[0] == "delay")
                {
                    //string[] a = temp.Split(' ');
                    scriptions.Add(new WAIT_scription(Int32.Parse(a[1]), true, "", COMMAND_TYPE.WAIT));
                    break;
                }
                //jump命令
                if (a[0] == "jump")
                {
                    //string[] a = temp.Split(' ');
                    scriptions.Add(new JUMP_scription(a[1], 0, "", "", COMMAND_TYPE.JUMP));
                    break;
                }
                //ld命令
                if (a[0] == "ld")
                {
                    //string[] a = temp.Split(' ');
                    string[] b = a[1].Split(',');
                    int time = 0;
                    int place = 1;  // r  001
                    COMMAND_TYPE command_tmp = COMMAND_TYPE.STAND_IMAGE;
                    switch (b[0][0])
                    {
                        case 'c':
                            place = 2; //010
                            //command_tmp = COMMAND_TYPE.STAND_IMAGE_CENTER;
                            break;
                        case 'l':
                            place = 4; //100
                            //command_tmp = COMMAND_TYPE.STAND_IMAGE_RIGHT;
                            break;
                    }
                    string[] path = b[1].Split('\"');

                    //アニメーション情報の指定                    
                    ANIMATION_TYPE type = Animation_Type_Define(Int32.Parse(b[2]));
                    if (type != ANIMATION_TYPE.ANIMATION_NO_TIME && type != ANIMATION_TYPE.ANIMATION_SYNCHRO)
                    {
                        time = Int32.Parse(b[3]);
                    }

                    scriptions.Add(new STAND_IMAGE_scription(place, null, type, b[2], time, path[1], command_tmp));
                    break;
                }
                //monocro命令 
                //monocro #COLOR で色調マスクをON、色変更
                //monocro offでマスクを解除
                if(a[0] == "monocro")
                {
                    //string[] a = temp.Split(' ');   //命令コマンドと引数を分離
                    if (a[1] == "off")
                        scriptions.Add(new Scription("monocro off", COMMAND_TYPE.MONOCRO_OFF));
                    else
                        scriptions.Add(new COLOR_scription(Scripter.Color_Parser(a[1]), "monocro on", COMMAND_TYPE.MONOCRO_ON));
                    break;
                }
                //return命令
                if (a[0] == "return")
                {
                    scriptions.Add(new Scription("finish", COMMAND_TYPE.RETURN));
                    break;
                }
                //sestop命令
                if (a[0] == "sestop")
                {
                    scriptions.Add(new Scription("sestop", COMMAND_TYPE.AUDIO_SE_STOP));
                    break;
                }
                //setwindow命令
                else if(a[0] == "setwindow")
                {
                    //string[] a = temp.Split(' ');
                    string[] b = a[1].Split(',');

                    string imgsource = b[0].Split('\"')[1];
                    float x = float.Parse(b[1]);
                    float y = float.Parse(b[2]);

                    scriptions.Add(new UI_IMAGE_scription(x, y, imgsource, COMMAND_TYPE.SETWINDOW));

                    break;
                }
                //se命令
                else if (a[0] == "seloop")
                {
                    //string[] a = temp.Split('\"');
                    string[] b = a[1].Split('\"');
                    scriptions.Add(new AUDIO_scription(Resources.Load<AudioClip>(b[1]), "loop", b[1], COMMAND_TYPE.AUDIO_SE));
                    break;
                }
                //se命令
                else if (a[0] == "se")
                {
                    //string[] a = temp.Split('\"');
                    string[] b = a[1].Split('\"');
                    scriptions.Add(new AUDIO_scription(Resources.Load<AudioClip>(b[1]), "", b[1], COMMAND_TYPE.AUDIO_SE));
                    break;
                }
                //texton命令
                if (a[0] == "texton")
                {
                    scriptions.Add(new Scription("texton", COMMAND_TYPE.TEXTON));
                    break;
                }
                //textoff命令
                else if (a[0] == "textoff")
                {
                    scriptions.Add(new Scription("textoff", COMMAND_TYPE.TEXTOFF));
                    break;
                }

                //voicestop命令
                if (a[0] == "voicestop")
                {
                    scriptions.Add(new Scription("voicestop", COMMAND_TYPE.AUDIO_VOICE_STOP));
                    break;
                }
                //voice命令
                else if (a[0] == "voice")
                {
                    //string[] a = temp.Split('\"');
                    string[] b = a[1].Split('\"');
                    scriptions.Add(new AUDIO_scription(Resources.Load<AudioClip>(b[1]), "voice", b[1], COMMAND_TYPE.AUDIO_VOICE));
                    break;
                }

                //wait命令
                if (a[0] == "wait")
                {
                    //string[] a = temp.Split(' ');
                    scriptions.Add(new WAIT_scription(Int32.Parse(a[1]), false, "", COMMAND_TYPE.WAIT));
                    break;
                }
                Special_script_parser(temp);
                break;
        }
    }

    public override void OnInspectorGUI()
    {
        NovelSetCompiler test_data = target as NovelSetCompiler;
        line_counts = EditorGUILayout.IntField("一行に表示する文字数", line_counts);
        rubycorrect_int = EditorGUILayout.FloatField("ルビ補正（ルビ文字間隔）", rubycorrect_int);
        rubycorrect_sp = EditorGUILayout.FloatField("ルビ補正（ルビ文字配置位置）", rubycorrect_sp);
        GUILayout.BeginHorizontal();
        test_data.data = EditorGUILayout.TextField("ファイル名", test_data.data);
        scriptions.Clear();
        FileStream fs;
        StreamReader sr;
        if (GUILayout.Button("Apply"))
        {
            string data = test_data.data;
            try
            {
                fs = new FileStream(Application.streamingAssetsPath + "/" + data, FileMode.Open, FileAccess.Read);

                sr = new StreamReader(fs);
                string[] type_temp = data.Split('.');

                if (type_temp[type_temp.Length - 1].Equals("csv"))
                {
                    Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
                    string plain = "";
                    while (sr.Peek() != -1)
                    {
                        plain += ec.EncryptionSystem(sr.ReadLine(),true) + '\n';
                    }
                    sr.Close();
                    fs = new FileStream(Application.streamingAssetsPath + "/compiled/" + data, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(plain);
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                    fs = new FileStream(Application.streamingAssetsPath + "/" + data, FileMode.Open, FileAccess.Read);
                    sr = new StreamReader(fs);
                    plain ="";
                    while(sr.Peek() != -1)
                    {
                        plain += sr.ReadLine()+"\n";
                    }
                    sr.Close();
                    fs = new FileStream(Application.streamingAssetsPath + "/debug/" + data, FileMode.Create, FileAccess.Write);
                    sw = new StreamWriter(fs);
                    sw.WriteLine(plain);
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                }
                else
                {
                    label_line = new Dictionary<string, int>();
                    label_filename = new Dictionary<string, string>();
                    string temp;
                    while (sr.Peek() >= 0)
                    {
                        temp = sr.ReadLine();
                        if (temp.Length == 0) continue;
                        while (temp[temp.Length - 1] == '/')
                        {
                            temp = temp.Split('/')[0];
                            temp += sr.ReadLine();
                        }
                        Script_parser(temp);

                    }
                    sr.Close();
                    fs.Close();
                    fs = new FileStream(Application.streamingAssetsPath + "/compiled/" + data, FileMode.Create, FileAccess.Write);
                    if (fs == null)
                    {
                        Debug.Log("失敗しました");
                    }
                    StreamWriter sw = new StreamWriter(fs);
                    //**注意**--------------------------------------------------------------------------------------------------------------------------------------------------------------
                    Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
                    Debug.Log(ec.cipher_code);
                    for (int k = 0; k < scriptions.Count; k++)
                    {
                        sw.WriteLine(ec.EncryptionSystem(JsonUtility.ToJson(scriptions[k]), true));
                    }
                    //**--------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                    fs = new FileStream(Application.streamingAssetsPath + "/debug/" + data, FileMode.Create, FileAccess.Write);
                    if (fs == null)
                    {
                        Debug.Log("失敗しました");
                    }
                    sw = new StreamWriter(fs);
                    for (int k = 0; k < scriptions.Count; k++)
                    {
                        sw.WriteLine(JsonUtility.ToJson(scriptions[k]));
                    }
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                    //*********完成版用のコンパイラ--------------------------------------------------------------------------------------------
                    Debug.Log("開始");
                    if (System.IO.File.Exists(Application.streamingAssetsPath + "/compiled/" + "Label_Table.db"))
                    {
                        fs = new FileStream(Application.streamingAssetsPath + "/compiled/" + "Label_Table.db", FileMode.Open);
                        sr = new StreamReader(fs);
                        string temp_label;
                        while (!sr.EndOfStream)
                        {
                            temp_label = ec.DecryptionSystem(sr.ReadLine(), true);
                            Debug.Log(temp_label);
                            string[] label = temp_label.Split(',');
                            Debug.Log(label[0] + label[1] + label[2]);
                            if (label_line.ContainsKey(label[0])) continue;
                            label_line.Add(label[0], int.Parse(label[1]));
                            label_filename.Add(label[0], label[2]);
                        }
                        sr.Close();
                        fs.Close();
                    }
                    Debug.Log("更新");
                    fs = new FileStream(Application.streamingAssetsPath + "/compiled/" + "Label_Table.db", FileMode.Create);
                    sw = new StreamWriter(fs);
                    string name = "";
                    foreach (string label in label_line.Keys)
                    {
                        name = label_filename[label];
                        if (name == "")
                        {
                            name = data;
                        }
                        sw.WriteLine(ec.EncryptionSystem(string.Format("{0},{1},{2}", label, label_line[label], name), true));
                    }
                    sw.Flush();
                    fs.Flush();
                    sw.Close();
                    fs.Close();
                    //****************************************************************************************************************************
                    //**デバッグ用のコンパイラ----------------------------------------------------------------------------------------------------
                    Debug.Log("開始");
                    if (System.IO.File.Exists(Application.streamingAssetsPath + "/debug/" + "Label_Table.db"))
                    {
                        fs = new FileStream(Application.streamingAssetsPath + "/debug/" + "Label_Table.db", FileMode.Open);
                        sr = new StreamReader(fs);
                        string temp_label;
                        while (!sr.EndOfStream)
                        {
                            temp_label = sr.ReadLine();
                            Debug.Log(temp_label);
                            string[] label = temp_label.Split(',');
                            Debug.Log(label[0] + label[1] + label[2]);
                            if (label_line.ContainsKey(label[0])) continue;
                            label_line.Add(label[0], int.Parse(label[1]));
                            label_filename.Add(label[0], label[2]);
                        }
                        sr.Close();
                        fs.Close();
                    }
                    Debug.Log("更新");
                    fs = new FileStream(Application.streamingAssetsPath + "/debug/" + "Label_Table.db", FileMode.Create);
                    sw = new StreamWriter(fs);
                    name = "";
                    foreach (string label in label_line.Keys)
                    {
                        name = label_filename[label];
                        if (name == "")
                        {
                            name = data;
                        }
                        sw.WriteLine(string.Format("{0},{1},{2}", label, label_line[label], name));
                    }
                    sw.Flush();
                    fs.Flush();
                    sw.Close();
                    fs.Close();
                    //****************************************************************************************************************************
                }
                EditorUtility.DisplayDialog("", "コンパイル完了しました", "ok");
            }
            catch (IOException e)
            {

                Debug.Log(e);
            }
        }
        GUILayout.EndHorizontal();
    }
    bool CommandChecker(string str)
    {
        //コメント除外
        if (str[0] == ';')return true;
        //bgm,bgコマンド除外
        if(str[0] == 'b' && str[1] == 'g')return true;
        //カラーコード除外
        if (str[0] == '#')return true;
        //ldコマンド除外
        if (str[0] == 'l') return true;
        return false;
    }
}
