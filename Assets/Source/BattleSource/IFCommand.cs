using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*************************/
/*条件式を評価するクラス */
/*************************/
public class IFCommand : MonoBehaviour
{
    //Unitdataと変数名の対応
    public static Dictionary<string, Unitdata> value_dictionary = new Dictionary<string, Unitdata>();

    //テスト用
    //初期座標からユニットデータを引っ張ってくる
    //名前からユニットデータを引っ張ってくる ---> Unitdataから引っ張ってこれば可能
    //敵キャラ集合としての変数の用意         ---> 敵キャラの判定となっているキャラクターを選択する。変数名teki
    //操作キャラ集合としての変数の用意(可能性あり？)変数名player
    //初期座標からユニットデータを引っ張ってくる場合
    public List<string>value = new List<string>();

    //敵キャラ集合としての変数
    public static List<Unitdata> enemy_data_value = new List<Unitdata>();

    public static List<Unitdata> player_data_value = new List<Unitdata>();
    //Inspectorで変数名とユニット名の対応を登録（ユニット名が重複しないものを想定）
    public void Initialization()
    {
        foreach(string temp_name in value)
        {
            //名前で取得[name,value_name]
            if (temp_name[0] == '[')
            {
                string[] temp_1 = temp_name.Split('[');
                string[] temp_2 = temp_1[1].Split(']');
                string[] unit_val = temp_2[0].Split(',');
                foreach (Unitdata unit in BattleVal.unitlist)
                {
                    if (unit_val[0].Equals(unit.charaname))
                    {
                        if (value_dictionary.ContainsKey(unit_val[1]))
                        {
                            value_dictionary[unit_val[1]] = unit;
                        }
                        else
                        {
                            value_dictionary.Add(unit_val[1], unit);
                        }
                        break;
                    }
                }
            }
            //現状この機能は保留--------------------------------------------------------------------------
            /*
            //座標で取得(x,y,value_name)
            if(temp_name[0] == '(')
            {
                string[] temp_1 = temp_name.Split('(');
                string[] temp_2 = temp_1[1].Split(')');
                string[] unit_val = temp_2[0].Split(',');
                if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", unit_val[0], unit_val[1])))
                {
                    value_dictionary.Add(unit_val[2], BattleVal.id2index[string.Format("{0},{1}", unit_val[0], unit_val[1])]);
                }
            }
            ----------------------------------------------------------------------------------------------*/
        }
        foreach(Unitdata unit in BattleVal.unitlist)
        {
            if(unit.team != 0)
            {
                enemy_data_value.Add(unit);
            }
            else
            {
                player_data_value.Add(unit);
            }
        }
    }
    //数字かどうかの判定
    static bool Num_check(string src)
    {
        int test = 0;
        return int.TryParse(src, out test);
    }
    //文字列か演算子か区別を行う関数
    static int Status_Check(string src)
    {
        if (Num_check(src))
        {
            return 1;
        }
        else if ('a' <= src[0] && src[0] <= 'z')
        {
            return 1;
        }
        else if ('A' <= src[0] && src[0] <= 'Z')
        {
            return 1;
        }
        else
        {
            if (src.Equals("+"))
            {
                return 2;
            }
            else if (src.Equals("-"))
            {
                return 2;
            }
            else if (src.Equals("*"))
            {
                return 2;
            }
            else if (src.Equals("/"))
            {
                return 2;
            }
            else if (src.Equals("<="))
            {
                return 3;
            }
            else if (src.Equals(">="))
            {
                return 3;
            }
            else if (src.Equals("<"))
            {
                return 3;
            }
            else if (src.Equals(">"))
            {
                return 3;
            }
            else if (src.Equals("=="))
            {
                return 3;
            }
            else if (src.Equals("!="))
            {
                return 3;
            }
            else if (src.Equals("&"))
            {
                return 4;
            }
            else if (src.Equals("|"))
            {
                return 4;
            }
            else
            {
                return -1;
            }
        }
    }
    int state = 0;

    private void Update()
    {/*
        if(BattleVal.status == STATUS.IFCMD_DIC_SET)
        {
            for (int i = 0; i < value_name.Count; i++)
            {
                foreach (Unitdata tempunit in BattleVal.unitlist)
                {
                    if (tempunit.charaname == unitname[i])
                        value_dictionary.Add(value_name[i], tempunit);
                }
            }
            BattleVal.status = STATUS.DRAW_TITLE;
        }*/
    }
    //数値の評価に変数の値を入れ込む関数
    int Value_Insert(string temp_a,string temp_b,ref int a,ref int b,ref List<int> a_list,ref List<int> b_list)
    {
        int ret = 0;
        if (!Num_check(temp_a))
        {
            //変数名.position.x
            string[] check_value_contents = temp_a.Split('.');
            string value_name = check_value_contents[0];
            string[] split_temp = temp_a.Split(':');
            if (value_name.Equals("turn"))
            {
                a = BattleVal.turn;
            }else if (split_temp.Length >1 && value_name.Split(':')[1].Equals("ENEMY"))
            {
                if (check_value_contents[1].Equals("position"))
                {
                    if (check_value_contents[2].Equals("x"))
                    {
                        for (int i = 0; i < enemy_data_value.Count; i++)
                        {
                            a_list.Add(enemy_data_value[i].x);
                        }
                    } else if (check_value_contents[2].Equals("y"))
                    {
                        for (int i = 0; i < enemy_data_value.Count; i++)
                        {
                            a_list.Add(enemy_data_value[i].y);
                        }
                    }
                } else if (check_value_contents[1].Equals("hp"))
                {
                    for (int i = 0; i < enemy_data_value.Count; i++)
                    {
                        a_list.Add(enemy_data_value[i].hp);
                    }
                }
                ret += 1;
                if (value_name.Split(':')[0].Equals("ANY"))
                {
                    ret += 3;
                }
            }else if (split_temp.Length > 1 && value_name.Split(':')[1].Equals("PLAYER"))
            {
                if (check_value_contents[1].Equals("position"))
                {
                    if (check_value_contents[2].Equals("x"))
                    {
                        for (int i = 0; i < player_data_value.Count; i++)
                        {
                            a_list.Add(player_data_value[i].x);
                        }
                    }
                    else if (check_value_contents[2].Equals("y"))
                    {
                        for (int i = 0; i < player_data_value.Count; i++)
                        {
                            a_list.Add(player_data_value[i].y);
                        }
                    }
                }
                else if (check_value_contents[1].Equals("hp"))
                {
                    for (int i = 0; i < player_data_value.Count; i++)
                    {
                        a_list.Add(player_data_value[i].hp);
                    }
                }
                else if (check_value_contents[1].Equals("level"))
                {
                    for (int i = 0; i < player_data_value.Count; i++)
                    {
                        a_list.Add(player_data_value[i].status.level);
                    }
                }
                ret += 1;
                if (value_name.Split(':')[1].Equals("ANY"))
                {
                    ret += 3;
                }
            }
            else if (check_value_contents[1].Equals("position"))
            {
                if (check_value_contents[2].Equals("x"))
                {
                    a = value_dictionary[value_name].x;
                }
                else if (check_value_contents[2].Equals("y"))
                {
                    a = value_dictionary[value_name].y;
                }
            }
            else if (check_value_contents[1].Equals("hp"))
            {
                a = value_dictionary[value_name].hp;
            }
            else if (check_value_contents[1].Equals("level"))
            {
                a = value_dictionary[value_name].status.level;

            }
        }
        else
        {
            a = int.Parse(temp_a);
        }
        if (!Num_check(temp_b))
        {
            //変数名.position.x
            string[] check_value_contents = temp_b.Split('.');
            string value_name = check_value_contents[0];
                string[] split_temp = temp_a.Split(':');
            if (value_name.Equals("turn"))
            {
                b = BattleVal.turn;
            }
            else if (split_temp.Length > 1 && value_name.Split(':')[1].Equals("ENEMY"))
            {
                if (check_value_contents[1].Equals("position"))
                {
                    if (check_value_contents[2].Equals("x"))
                    {
                        for (int i = 0; i < enemy_data_value.Count; i++)
                        {
                            b_list.Add(enemy_data_value[i].x);
                        }
                    }
                    else if (check_value_contents[2].Equals("y"))
                    {
                        for (int i = 0; i < enemy_data_value.Count; i++)
                        {
                            b_list.Add(enemy_data_value[i].y);
                        }
                    }
                }
                else if (check_value_contents[1].Equals("hp"))
                {
                    for (int i = 0; i < enemy_data_value.Count; i++)
                    {
                        b_list.Add(enemy_data_value[i].hp);
                    }
                }
                ret += 2;
                if (value_name.Split(':')[0].Equals("ANY"))
                {
                    ret += 6;
                }
            }
            else if (split_temp.Length > 1 && value_name.Split(':')[0].Equals("PLAYER"))
            {
                if (check_value_contents[1].Equals("position"))
                {
                    if (check_value_contents[2].Equals("x"))
                    {
                        for (int i = 0; i < player_data_value.Count; i++)
                        {
                            a_list.Add(player_data_value[i].x);
                        }
                    }
                    else if (check_value_contents[2].Equals("y"))
                    {
                        for (int i = 0; i < player_data_value.Count; i++)
                        {
                            a_list.Add(player_data_value[i].y);
                        }
                    }
                }
                else if (check_value_contents[1].Equals("hp"))
                {
                    for (int i = 0; i < player_data_value.Count; i++)
                    {
                        a_list.Add(player_data_value[i].hp);
                    }
                }
                ret += 2;
                if (value_name.Split(':')[1].Equals("ANY"))
                {
                    ret += 6;
                }
            }
            else if (check_value_contents[1].Equals("position"))
            {
                if (check_value_contents[2].Equals("x"))
                {
                    a = value_dictionary[value_name].x;
                }
                else if (check_value_contents[2].Equals("y"))
                {
                    a = value_dictionary[value_name].y;
                }
            }
            else if (check_value_contents[1].Equals("hp"))
            {
                a = value_dictionary[value_name].hp;
            }
            else if (check_value_contents[1].Equals("position"))
            {
                if (check_value_contents[2].Equals("x"))
                {
                    b = value_dictionary[value_name].x;
                }
                else if (check_value_contents[2].Equals("y"))
                {
                    b = value_dictionary[value_name].y;
                }
            }
            else if (check_value_contents[1].Equals("hp"))
            {
                b = value_dictionary[value_name].hp;
            }
        }
        else
        {
            b = int.Parse(temp_b);
        }
        return ret;
    }
    //中間置記法から後置記法に変換する
    private Stack<string> Analysis(string str)
    {
        Stack<string> p = new Stack<string>();
        Stack<string> s = new Stack<string>();
        Stack<string> temp_stack = new Stack<string>();
        int counter = 0;
        string[] temp = str.Split(' ');
        for (int i = 0; i < temp.Length; i++)
        {
            if(temp[i] == "(")
            {
                counter++;
                string value = "";
                for (i+=1; i < temp.Length; i++)
                {
                    if(temp[i] == "(")
                    {
                        counter++;
                    }
                    if(temp[i] == ")")
                    {
                        counter--;
                    }

                    if(counter == 0)
                    {
                        value = value.Remove(value.Length - 1);
                        temp_stack = Analysis(value);
                        Stack<string> temp_stack2 = new Stack<string>();
                        while(temp_stack.Count > 0)
                        {
                            temp_stack2.Push(temp_stack.Pop());
                        }
                        while(temp_stack2.Count > 0)
                        {
                            p.Push(temp_stack2.Pop());
                        }
                        break;
                    }
                    else
                    {
                        value += temp[i] + " ";
                    }
                }
                
            }else if (Status_Check(temp[i]) == 1)
            {
                if (temp[i].Equals("ALL"))
                {
                    i++;
                    p.Push(temp[i - 1] + ":" + temp[i]);
                }
                else if (temp[i].Equals("ANY"))
                {
                    i++;
                    p.Push(temp[i - 1] + ":" + temp[i]);
                }
                else
                {
                    p.Push(temp[i]);
                }
            }
            else
            {
                if (s.Count > 0)
                {
                    string t = s.Pop();
                    if (Status_Check(t) < Status_Check(temp[i]))
                    {
                        p.Push(t);
                        s.Push(temp[i]);
                    }
                    else
                    {
                        s.Push(t);
                        s.Push(temp[i]);
                    }
                }
                else
                {
                    s.Push(temp[i]);
                }
            }
        }
        while (s.Count > 0)
        {
            p.Push(s.Pop());
        }
        return p;
    }
    //実際に演算する関数
    public bool If_command(string str)
    {
        Stack<string> p = new Stack<string>();
        Stack<string> s = new Stack<string>();
        p = Analysis(str);
        Stack<string> answer = new Stack<string>();
        List<string> p_str = new List<string>();
        while(p.Count != 0)
        {
            p_str.Add(p.Pop());
        }
        for (int i = p_str.Count-1; i >=0; i--)
        {
            if (Status_Check(p_str[i]) >= 2)
            {
                string temp_b = answer.Pop();
                string temp_a = answer.Pop();
                int a = 0;
                int b = 0;
                List<int> a_list = new List<int>();
                List<int> b_list = new List<int>();
                if (p_str[i].Equals("<"))
                {
                    int ret = Value_Insert(temp_a, temp_b, ref a, ref b,ref a_list,ref b_list);
                    if (ret == 0)
                    {
                        if (b - a > 0)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //aの方でall
                    else if(ret == 1)
                    {
                        bool check = true;
                        for(int j = 0; j < a_list.Count; j++)
                        {
                            if(b - a_list[j] <= 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if(ret == 2)
                    {
                        bool check = true;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a <= 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //何か一つ成り立てばok
                    else if (ret == 4)
                    {
                        bool check = false;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j] <= 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 8)
                    {
                        bool check = false;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a <= 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                }
                else if (p_str[i].Equals(">"))
                {
                    int ret = Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    if (ret == 0)
                    {
                        if (b - a < 0)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //aの方でall
                    else if (ret == 1)
                    {
                        bool check = true;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j] >= 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 2)
                    {
                        bool check = true;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a >= 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //何か一つ成り立てばok
                    else if (ret == 4)
                    {
                        bool check = false;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j] >= 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 8)
                    {
                        bool check = false;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a <= 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                }
                else if (p_str[i].Equals("<="))
                {
                    int ret = Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    if (ret == 0)
                    {
                        if (b - a >= 0)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //aの方でall
                    else if (ret == 1)
                    {
                        bool check = true;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j] < 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 2)
                    {
                        bool check = true;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a < 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //何か一つ成り立てばok
                    else if (ret == 4)
                    {
                        bool check = false;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j] < 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 8)
                    {
                        bool check = false;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a < 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                }
                else if (p_str[i].Equals(">="))
                {
                    int ret = Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    if (ret == 0)
                    {
                        if (b - a <= 0)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //aの方でall
                    else if (ret == 1)
                    {
                        bool check = true;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j] <= 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 2)
                    {
                        bool check = true;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a <= 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //何か一つ成り立てばok
                    else if (ret == 4)
                    {
                        bool check = false;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j]  <= 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 8)
                    {
                        bool check = false;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a <= 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                }
                else if (p_str[i].Equals("!="))
                {
                    int ret = Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    if (ret == 0)
                    {
                        if (b - a != 0)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //aの方でall
                    else if (ret == 1)
                    {
                        bool check = true;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j] == 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 2)
                    {
                        bool check = true;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a == 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //何か一つ成り立てばok
                    else if (ret == 4)
                    {
                        bool check = false;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j] == 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 8)
                    {
                        bool check = false;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a == 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                }
                else if (p_str[i].Equals("=="))
                {
                    int ret = Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    if (ret == 0)
                    {
                        if (b - a == 0)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //aの方でall
                    else if (ret == 1)
                    {
                        bool check = true;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j] != 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 2)
                    {
                        bool check = true;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a != 0)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    //何か一つ成り立てばok
                    else if (ret == 4)
                    {
                        bool check = false;
                        for (int j = 0; j < a_list.Count; j++)
                        {
                            if (b - a_list[j] != 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                    else if (ret == 8)
                    {
                        bool check = false;
                        for (int j = 0; j < b_list.Count; j++)
                        {
                            if (b_list[j] - a != 0)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            answer.Push("1");
                        }
                        else
                        {
                            answer.Push("0");
                        }
                    }
                }
                else if (p_str[i].Equals("&"))
                {
                    Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    if (b == a && a == 1)
                    {
                        answer.Push("1");
                    }
                    else
                    {
                        answer.Push("0");
                    }
                }
                else if (p_str[i].Equals("|"))
                {
                    Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    if (b == 1 || a == 1)
                    {
                        answer.Push("1");
                    }
                    else
                    {
                        answer.Push("0");
                    }
                }
                else if (p_str[i].Equals("+"))
                {
                    Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    answer.Push(string.Format("{0}", a + b));
                }
                else if (p_str[i].Equals("-"))
                {
                    Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    answer.Push(string.Format("{0}", a - b));
                }
                else if (p_str[i].Equals("*"))
                {
                    Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    answer.Push(string.Format("{0}", a * b));
                }
                else if (p_str[i].Equals("/"))
                {
                    Value_Insert(temp_a, temp_b, ref a, ref b, ref a_list, ref b_list);
                    answer.Push(string.Format("{0}", a / b));
                }
            }
            else
            {
                answer.Push(p_str[i]);
            }
        }
        if (answer.Pop().Equals("1"))
        {
            return true;
        }
        return false;
    }
}
