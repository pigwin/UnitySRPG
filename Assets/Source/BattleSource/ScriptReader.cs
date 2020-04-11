using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;

public enum MapdataList
{
    MAPORIGINAL,    //生データ
    MAPHEIGHT,      //高さ情報(1の位から10の位)
    MAPTEXTURE,      //マップのテクスチャ情報(100の位から10000の位)
    MAPUNIT,         //ユニットの位置(100000の位から1000000の位)
    MAPDIRECT,       //ユニットの向き(10000000の位) (0:指定なし、1:上、2:下、3:右、4:左)
    MAPUNITSET,       //ユニット配置可能箇所(100000000の位) (0:配置不可、1:配置可能)
    PARTY_CHECK,     //パーティメンバーか否かの判定をする
    MAX_NUM
}

/*******************************************************/
/*csvファイルからマップのデータを構築するためのクラス  */
/*******************************************************/
public class ScriptReader{
	// csvファイルからマップデータを構成
    public static void  MapReader(string csvname)
    {
        //BattleValの初期化
        BattleVal.mapdata = new List<List<List<int>>>();
        BattleVal.unitlist = new List<Unitdata>();
        CharaSetup.partyForce = new Dictionary<int, bool>();

        if (Debug.isDebugBuild)
        {
            csvname = "/debug/" + csvname;
        }
        else
        {
            csvname = "/compiled/" + csvname;
        }

        StreamReader sr = new StreamReader(Application.streamingAssetsPath + csvname, Encoding.GetEncoding("utf-8")); //読み込んだ文字列の格納
        string loading;

        //生データを取得
        BattleVal.mapdata.Add(new List<List<int>>());
        //読み込みループ
        Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
        int count = 0;
        while (sr.Peek() != -1)
        {
            loading = sr.ReadLine();
            //**注意**---------------------------------------------------------------------
            loading = ec.DecryptionSystem(loading,false);
            count++;
            //**---------------------------------------------------------------------------
            //1行読み込み分のテンポラリ
            List<int> tmpfield = new List<int>();
            //区切り文字
            string[] str = loading.Split(',');
            int num = 0;
            bool check = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (int.TryParse(str[i], out num))
                {
                    check = true;
                    tmpfield.Add(num);
                }
            }
            if(check)BattleVal.mapdata[(int)MapdataList.MAPORIGINAL].Add(tmpfield);
        }

        //csv生データからMAPHEIGHT情報、MAPTEXTURE情報を取得
        //MAPHEIGHT: マスの高さ情報（0は選択不可マス）10の位,1の位（0～99）
        //MAPTEXTURE：マスに張るmaterialの配列番号10000の位,100の位(0～999）
        //MAPUNIT：マスに最初にいるユニットの配列番号+1（0はキャラ不在）1000000の位,100000の位(0～99)

        //マップサイズ取得
        int mapy = BattleVal.mapdata[(int)MapdataList.MAPORIGINAL].Count;
        int mapx = BattleVal.mapdata[(int)MapdataList.MAPORIGINAL][0].Count;

        foreach (MapdataList listnum in System.Enum.GetValues(typeof(MapdataList)))
        {
            if (listnum != MapdataList.MAPORIGINAL)
            {
                BattleVal.mapdata.Add(new List<List<int>>());
                //result[(int)listnum] = new List<List<int>>(mapy);
            }
        }


        for (int i = 0; i < mapy; i++)
        {
            foreach (MapdataList listnum in System.Enum.GetValues(typeof(MapdataList)))
            {
                if (listnum != MapdataList.MAPORIGINAL)
                {
                    BattleVal.mapdata[(int)listnum].Add(new List<int>());
                    //result[(int)listnum][i] = new List<int>(mapx);
                }
            }

            for (int j = 0; j < mapx; j++)
            {
                int temp = BattleVal.mapdata[(int)MapdataList.MAPORIGINAL][i][j];
                bool partymemberflag = false;
                if(temp < 0)
                {
                    BattleVal.mapdata[(int)MapdataList.PARTY_CHECK][i].Add(1);
                    temp *= -1;
                    partymemberflag = true;
                }
                else
                {
                    BattleVal.mapdata[(int)MapdataList.PARTY_CHECK][i].Add(0);
                }
                //10の位、1の位を取得
                BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][i].Add(temp % 100);
                temp = temp / 100;
                //100,1000,10000の位以上を保存
                BattleVal.mapdata[(int)MapdataList.MAPTEXTURE][i].Add(temp % 1000);
                temp = temp / 1000;
                //100000,1000000の位を保存
                BattleVal.mapdata[(int)MapdataList.MAPUNIT][i].Add(temp % 100);
                temp = temp / 100;
                //10000000の位を保存
                BattleVal.mapdata[(int)MapdataList.MAPDIRECT][i].Add(temp%10);
                temp = temp / 10;
                //100000000の位を保存
                BattleVal.mapdata[(int)MapdataList.MAPUNITSET][i].Add(temp % 10);
                temp = temp / 10;
                //1000000000の位を保存
                if(partymemberflag)
                    if(temp != 0)
                        CharaSetup.partyForce.Add(BattleVal.mapdata[(int)MapdataList.MAPUNIT][i][j], true);
                    else
                        CharaSetup.partyForce.Add(BattleVal.mapdata[(int)MapdataList.MAPUNIT][i][j], false);
            }
        }
    }

    //セーブ用Mapdataの生成
    public static List<int> CreateMapSaveData(List<List<List<int>>> mapdata)
    {
        List<int> savedata = new List<int>();

        for(int y = 0; y < Mapclass.mapynum; y++)
        {
            for (int x = 0; x < Mapclass.mapxnum; x++)
            {
                int tmp = mapdata[(int)MapdataList.MAPHEIGHT][y][x]
                    + 100 * mapdata[(int)MapdataList.MAPTEXTURE][y][x]
                    + 100000 * mapdata[(int)MapdataList.MAPUNIT][y][x]
                    + 10000000 * mapdata[(int)MapdataList.MAPDIRECT][y][x]
                    + 100000000 * mapdata[(int)MapdataList.MAPUNITSET][y][x];

                savedata.Add(tmp);
            }
        }

        return savedata;
    }

    //Mapdataをロード用データからmapdata生成
    public static List<List<List<int>>> LoadMapSaveData(List<int> savedata)
    {
        List<List<List<int>>> loaddata = new List<List<List<int>>>();
        for(int i = 0; i < 6; i++)
        {
            loaddata.Add(new List<List<int>>());
            for(int j = 0; j < Mapclass.mapynum; j++)
            {
                loaddata[i].Add(new List<int>());
                for(int k = 0; k < Mapclass.mapxnum; k++)
                {
                    loaddata[i][j].Add(0);
                }
            }
        }

        //initialize
        int temp = 0;
        int y = -1;
        for (int i = 0; i<savedata.Count; i++)
        {
            //x,yの算出
            if (y < i / Mapclass.mapxnum)
            {
                y++;
            }
            int x = (i - y * Mapclass.mapxnum) % Mapclass.mapxnum;

            loaddata[(int)MapdataList.MAPORIGINAL][y][x] = savedata[i];
            //10の位、1の位を取得
            loaddata[(int)MapdataList.MAPHEIGHT][y][x] = savedata[i] % 100;
            temp = savedata[i] / 100;
            //100,1000,10000の位以上を保存
            loaddata[(int)MapdataList.MAPTEXTURE][y][x] = temp % 1000;
            temp  /= 1000;
            //100000,1000000の位を保存
            loaddata[(int)MapdataList.MAPUNIT][y][x] = temp % 100;
            temp /= 100;
            //10000000の位を保存
            loaddata[(int)MapdataList.MAPDIRECT][y][x] = temp % 10;
            temp /= 10;
            //100000000の位を保存
            loaddata[(int)MapdataList.MAPUNITSET][y][x] = temp % 10;
        }

        return loaddata;
    }
}
