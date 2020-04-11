using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
/*******************************************************************/
/*マップ全体に対する処理を行う関数　　　　　　　　　　　　　　　　 */
/*******************************************************************/
public class Mapclass : MonoBehaviour
{
    //マップ用box テクスチャも事前に貼っておく可能性あり
    public GameObject[] MapBox;

    public static GameObject[] MapBoxList;
    [Header("スポット参戦を含む最大出撃人数")]
    public int maxpartyunitnum = 1;

    [System.Serializable]
    public class UnitStrLv
    {
        [UnitAttribute]
        public string unitname;
        public int level;
    }

    [Header("敵ユニット・スポット参戦ユニットのID対応")]
    public UnitStrLv[] spotunitandlv;

    [Header("固有パーティメンバーの出撃可否設定")]
    public List<IdAndBool> idAndBools_PartyForbidden; //Inspector入力用

    [Header("味方ユニットを示すタイル")]
    public GameObject prefabPlayerTile;
    [Header("敵ユニットを示すタイル")]
    public GameObject prefabEnemyTile;

    public static float MapBoxHeight = 0.3f;

    public static float maporiginx;
    public static float maporiginy;

    public static int mapxnum;
    public static int mapynum;

    [Header("味方を通過可能か")]
    public bool is_through_teammate = true;
    [Header("高さの高いマスを透過するかどうか")]
    public bool is_fade_highermap = true;
    [Header("高さ閾値（これ以上の高さのマスは透過される）")]
    public int hight_highermap = 6;
    [Header("高いマスの透過度")]
    [Range(0,1)]
    public float highermap_alpha = 0.7f;
    private static bool _is_through_teammate;

    private void Start()
    {
        CharaSetup.maxpartyunitnum = maxpartyunitnum;
        _is_through_teammate = is_through_teammate;
        MapBoxList = MapBox;

        CharaSetup.partyForbidden = new Dictionary<int, bool>();
        foreach (IdAndBool check in idAndBools_PartyForbidden)
        {
            CharaSetup.partyForbidden.Add(check.id, check.flag);
        }
    }

    // Use this for initialization
    void Update()
    {
        if (BattleVal.status == STATUS.DRAW_STAGE)
        {
            maporiginy = (float)(BattleVal.mapdata[(int)MapdataList.MAPORIGINAL].Count / 2);
            maporiginx = (float)(-BattleVal.mapdata[(int)MapdataList.MAPORIGINAL][0].Count / 2);
            mapxnum = BattleVal.mapdata[(int)MapdataList.MAPORIGINAL][0].Count;
            mapynum = BattleVal.mapdata[(int)MapdataList.MAPORIGINAL].Count;
            BattleVal.mapgobj = new Dictionary<string, GameObject>();
            //読み込んだmapdataに基づいて、Mapを描画する
            for (int y = 0; y < mapynum; y++)
            {
                for (int x = 0; x < mapxnum; x++)
                {
                    GameObject tmpbox;
                    if (BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][y][x] == 0)
                    {
                        Vector3 mappos = new Vector3(maporiginx + x, 0, maporiginy - y);
                        tmpbox = Instantiate(MapBox[0], mappos, Quaternion.identity);
                        tmpbox.transform.localScale = new Vector3(1.0f, MapBoxHeight, 1.0f);
                        BattleVal.mapgobj.Add(string.Format("{0},{1}", x, y), tmpbox);
                    }
                    else
                    {
                        Vector3 mappos = new Vector3(maporiginx + x, (float)(BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][y][x] - 1) * MapBoxHeight / 2.0f, maporiginy - y);
                        tmpbox = Instantiate(MapBox[BattleVal.mapdata[(int)MapdataList.MAPTEXTURE][y][x]], mappos, Quaternion.identity);
                        tmpbox.transform.localScale = new Vector3(1.0f, (float)BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][y][x] * MapBoxHeight, 1.0f);
                        BattleVal.mapgobj.Add(string.Format("{0},{1}", x, y), tmpbox);
                    }
                    //tmpbox.GetComponent<Renderer>().material = MapTexture[mapdata[(int)MapdataList.MAPTEXTURE][y][x]];


                }
            }
            DrawUnitInit();
            //初期描画終了
            BattleVal.status = STATUS.DRAW_TITLE;
            //BattleVal.status = STATUS.TURNCHANGE;
        }
        
        //ロード時の画面描写
        if(BattleVal.status == STATUS.DRAW_STAGE_FROM_LOADDATA)
        {
            //セーブデータの呼び出し
            //Windows
            //FileStream fs = new FileStream(Application.streamingAssetsPath + Operation.loadtemp, FileMode.Open, FileAccess.Read);
            //Mac
            FileStream fs = new FileStream(Application.persistentDataPath + Operation.loadtemp, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            BattleSave sd = JsonUtility.FromJson<BattleSave>(sr.ReadLine());
            
            sd.Load();
            //読み込んだmapdataに基づいて、Mapを描画する
            BattleVal.mapgobj = new Dictionary<string, GameObject>();
            for (int y = 0; y < mapynum; y++)
            {
                for (int x = 0; x < mapxnum; x++)
                {
                    GameObject tmpbox;
                    if (BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][y][x] == 0)
                    {
                        Vector3 mappos = new Vector3(maporiginx + x, 0, maporiginy - y);
                        tmpbox = Instantiate(MapBox[0], mappos, Quaternion.identity);
                        tmpbox.transform.localScale = new Vector3(1.0f, MapBoxHeight, 1.0f);
                        BattleVal.mapgobj.Add(string.Format("{0},{1}", x, y), tmpbox);
                    }
                    else
                    {
                        Vector3 mappos = new Vector3(maporiginx + x, (float)(BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][y][x] - 1) * MapBoxHeight / 2.0f, maporiginy - y);
                        tmpbox = Instantiate(MapBox[BattleVal.mapdata[(int)MapdataList.MAPTEXTURE][y][x]], mappos, Quaternion.identity);
                        tmpbox.transform.localScale = new Vector3(1.0f, (float)BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][y][x] * MapBoxHeight, 1.0f);
                        BattleVal.mapgobj.Add(string.Format("{0},{1}", x, y), tmpbox);
                    }
                    //tmpbox.GetComponent<Renderer>().material = MapTexture[mapdata[(int)MapdataList.MAPTEXTURE][y][x]];


                }
            }

            //キャラの配置
            foreach(Unitdata unit in BattleVal.unitlist)
            {
                unit.gobj = Instantiate(unit.gobj_prefab, new Vector3(), Quaternion.identity);
                DrawCharacter(unit.gobj, unit.x, unit.y);
                unit.gobj.transform.forward = sd.keyValuePairsUnitdirection[unit];
                //足元にチーム識別タイルを
                GameObject unittile;
                if (unit.team == 0)
                {
                    unittile = Instantiate(prefabPlayerTile, unit.gobj.transform);
                    unittile.transform.localScale = new Vector3(unittile.transform.localScale.x / unittile.transform.lossyScale.x,
                                                                0.01f,
                                                                unittile.transform.localScale.z / unittile.transform.lossyScale.z);
                    DrawCharacter(unittile, unit.x, unit.y);
                }
                else if (unit.team == 1)
                {
                    unittile = Instantiate(prefabEnemyTile, unit.gobj.transform);
                    unittile.transform.localScale = new Vector3(unittile.transform.localScale.x / unittile.transform.lossyScale.x,
                                                                0.01f,
                                                                unittile.transform.localScale.z / unittile.transform.lossyScale.z);
                    DrawCharacter(unittile, unit.x, unit.y);
                }

                foreach(Condition condition in unit.status.conditions)
                {
                    condition.InstantiateEffect(unit);
                }

                unit.gobj.layer = 8;
            }

            //tmpファイルの削除
            sr.Close();
            fs.Close();
            //Windows
            //System.IO.File.Delete(Application.streamingAssetsPath + Operation.loadtemp);
            //Mac
            System.IO.File.Delete(Application.persistentDataPath + Operation.loadtemp);
            //セーブデータからの再描画終了
            BattleVal.status = STATUS.TAKE_SCREENSHOT_FROM_LOADDATA;

        }

        //TODO
        //マップの透過度設定
        //選択マスよりも10以上高さが高いマスは透過度を高くする。（裏にいるキャラが見れるようになる。）
        //この機能を使用する場合、床のMapBoxはRendering Mode をOpaqueに、壁のMapBoxはFadeにするとよいです。
        if((BattleVal.status == STATUS.PLAYER_UNIT_SELECT || BattleVal.status == STATUS.ENEMY_UNIT_SELECT) && BattleVal.selectX != -1 && is_fade_highermap)
        {
            //選択マスの高さ
            int selectheight = BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][BattleVal.selectY][BattleVal.selectX];

            foreach (KeyValuePair<string,GameObject> mapbox in BattleVal.mapgobj)
            {
                string[] tmp = mapbox.Key.Split(',');
                int x = int.Parse(tmp[0]);
                int y = int.Parse(tmp[1]);

                int mapboxheight = BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][y][x];

                //透明マスは飛ばす
                if (mapboxheight == 0) continue;

                //高さ判定
                if(Mathf.Abs(mapboxheight - selectheight) >= hight_highermap)
                {
                    Color tmpcolor = mapbox.Value.GetComponent<Renderer>().material.color;
                    tmpcolor.a = highermap_alpha;
                    mapbox.Value.GetComponent<Renderer>().material.color = tmpcolor;
                }
                else
                {
                    Color tmpcolor = mapbox.Value.GetComponent<Renderer>().material.color;
                    tmpcolor.a = 1.0f;
                    mapbox.Value.GetComponent<Renderer>().material.color = tmpcolor;
                }
            }
        }
    }
    //実座標からマップ内座標（配列の添え字）への変換
    //引数：position(Vector3) 与えられた座標
    //      mapx (ref int) マップ配列の添え字に相当する x （戻り値）
    //      mapy (ref int) マップ配列の添え字に相当する y （戻り値）
    public static void TranslatePositionToMapCoord(Vector3 position, ref int mapx, ref int mapy)
    {
        mapx = (int)Math.Round(position.x - maporiginx);
        mapy = -(int)Math.Round(position.z - maporiginy);
    }
    //マップ内座標から実座標（配列の添え字）への変換
    //引数：position(Vector3) 実座標（戻り値）
    //      mapx マップ内座標
    //      mapy マップ内座標
    public static void TranslateMapCoordToPosition(ref Vector3 position, int mapx, int mapy, bool yflag = true)
    {
        float posx = maporiginx + (float)mapx;
        float posy = 0.0f;
        if (yflag)
        {
            posy = ((float)BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][mapy][mapx] * MapBoxHeight) / 2.0f;
            posy += ((float)BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][mapy][mapx] - 1.0f) * MapBoxHeight / 2.0f;
        }
        float posz = maporiginy - (float)mapy;

        position = new Vector3(posx, posy, posz);
    }

    //実座標がマップ内にあるか？
    //引数：position(Vector3) 与えられた座標
    //      mapin (ref bool) 与えられた座標がマップ内にあるか？（戻り値）
    public bool IsPositionInMap(Vector3 position)
    {
        //マップ内にあるか？
        if (position.x >= maporiginx - 0.5f && position.x <= maporiginx + (float)(mapxnum - 1) + 0.5f && position.y <= maporiginy + 0.5f && position.x >= maporiginy - (float)(mapynum - 1) - 0.5f)
        {
            return true;
        }

        return false;
    }
    //指定されたマップ内座標にGameObjectを表示する
    public static void DrawCharacter(GameObject Character, int mapx, int mapy)
    {
        //実座標を求める
        float posx = maporiginx + (float)mapx;
        //float posy = ((float)BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][mapy][mapx] * MapBoxHeight + (float)Character.transform.localScale.y) / 2.0f;
        float posy = ((float)BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][mapy][mapx] * MapBoxHeight) / 2.0f;
        posy += ((float)BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][mapy][mapx] - 1.0f) * MapBoxHeight / 2.0f;
        if((float)BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][mapy][mapx] == 0) posy = MapBoxHeight / 2.0f;
        float posz = maporiginy - (float)mapy;
        //描画
        Character.transform.position = new Vector3(posx, posy, posz);
    }
    //指定した向きを向かせる関数
    // directx=1, directy=0 -> x方向+1,y方向+0の向き
    public static void LookAtCharacter(Unitdata unit, int directx, int directy)
    {
        //unitのMap上x,yにdirectx,directyを加える
        int mapx = unit.x + directx;
        int mapy = unit.y + directy;

        //(mapx,mapy)の実座標(yを除く)を得る
        Vector3 dcoord = new Vector3();
        TranslateMapCoordToPosition(ref dcoord, mapx, mapy, false);

        //実y座標をunitに合わせる（段差対策）
        Vector3 unitcoord = new Vector3();
        TranslateMapCoordToPosition(ref unitcoord, unit.x, unit.y);
        dcoord.y = unitcoord.y;

        //dcoordの向きをunitに向かせる
        unit.gobj.transform.LookAt(dcoord);
    }

    //ユニット配列からユニットを描画する（キャラインスタンスの作成）
    public void DrawUnitInit()
    {
        for (int i = 0; i < mapynum; i++)
        {
            for (int j = 0; j < mapxnum; j++)
            {
                //キャラが居れば
                if (BattleVal.mapdata[(int)MapdataList.MAPUNIT][i][j] != 0)
                { 
                    int unitid = 0;

                    //GameObject tmpchara = Instantiate(UnitList[unitid], new Vector3(), Quaternion.identity);
                    //読み込んだunitidのUnitdataを作成する
                    Unitdata tmpchara;
                    if (BattleVal.mapdata[(int)MapdataList.PARTY_CHECK][i][j] == 0)
                    {
                        //MAPで指定されたキャラ（敵orスポット参戦）
                        //マップデータ配列の数値が、配列添え字よりも1大きいことに注意する
                        unitid = BattleVal.mapdata[(int)MapdataList.MAPUNIT][i][j] - 1;
                        tmpchara = new Unitdata(spotunitandlv[unitid].unitname, spotunitandlv[unitid].level);
                    }
                    else
                    {
                        //パーティメンバーのキャラ
                        //複製元のUnitSaveDataをロード
                        unitid = BattleVal.mapdata[(int)MapdataList.MAPUNIT][i][j];
                        tmpchara = new Unitdata(GameVal.masterSave.id2unitdata[unitid]);
                        //HPとスキル使用回数の回復
                        tmpchara.hp = tmpchara.status.maxhp;
                        foreach (Skill skill in tmpchara.skills)
                            skill.use = skill.maxuse;
                        tmpchara.partyid = unitid;
                    }
                    tmpchara.x = j;  //初期位置を設定
                    tmpchara.y = i;
                    tmpchara.hp = tmpchara.status.maxhp;
                    tmpchara.gobj.layer = 8;   //レイヤー番号
                    //Addする前のunitlistのサイズが、Addされた末尾のラベルになる
                    int tmpcharalabel = BattleVal.unitlist.Count;
                    //tmpcharaをunitlistに加える
                    BattleVal.unitlist.Add(tmpchara);
                    BattleVal.id2index.Add(string.Format("{0},{1}", j, i), tmpchara);
                    DrawCharacter(BattleVal.unitlist[tmpcharalabel].gobj, j, i);
                    foreach(Condition cond in tmpchara.status.conditions)
                    {
                        cond.InstantiateEffect(tmpchara);
                    }
                    //足元にチーム識別タイルを
                    GameObject unittile;
                    if (tmpchara.team == 0)
                    {
                        unittile = Instantiate(prefabPlayerTile, tmpchara.gobj.transform);
                        unittile.transform.localScale = new Vector3(unittile.transform.localScale.x / unittile.transform.lossyScale.x,
                                                                    0.01f,
                                                                    unittile.transform.localScale.z / unittile.transform.lossyScale.z);
                        DrawCharacter(unittile, tmpchara.x, tmpchara.y);
                    }
                    else if (tmpchara.team == 1)
                    {
                        unittile = Instantiate(prefabEnemyTile, tmpchara.gobj.transform);
                        unittile.transform.localScale = new Vector3(unittile.transform.localScale.x / unittile.transform.lossyScale.x,
                                                                    0.01f,
                                                                    unittile.transform.localScale.z / unittile.transform.lossyScale.z);
                        DrawCharacter(unittile, tmpchara.x, tmpchara.y);
                    }

                    //向き調整
                    switch (BattleVal.mapdata[(int)MapdataList.MAPDIRECT][i][j])
                    {
                        //上
                        case 1:
                            LookAtCharacter(BattleVal.unitlist[tmpcharalabel], 0, -1);
                            break;
                        //下
                        case 2:
                            LookAtCharacter(BattleVal.unitlist[tmpcharalabel], 0, 1);
                            break;
                        //左
                        case 3:
                            LookAtCharacter(BattleVal.unitlist[tmpcharalabel], -1, 0);
                            break;
                        //右
                        case 4:
                            LookAtCharacter(BattleVal.unitlist[tmpcharalabel], 1, 0);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    //マップ範囲内かを判定する
    public static bool is_Inmap(int x, int y)
    {
        return (x < mapxnum && x >= 0 && y < mapynum && y >= 0);
    }

    //移動可能範囲を探索する関数
    public static List<int[]> Dfs(List<List<List<int>>> maps, int startx, int starty, int steps, int jump)
    {
        Queue<int[]> que = new Queue<int[]>();
        List<int[]> ans = new List<int[]>();
        que.Enqueue(new int[] { startx, starty, steps });
        int[] d = new int[3];
        int[,] vector = new int[,] { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
        int x, y;
        bool check = true;

        //ZOC
        Create_ZOCmap();

        while (que.Count != 0)
        {
            d = que.Dequeue();
            if (d[2] <= 0) continue;
            for (int i = 0; i < 4; i++)
            {
                x = d[0] + vector[i, 0];
                y = d[1] + vector[i, 1];
                check = true;
                foreach (int[] t in ans)
                {
                    if(t[0] == x && t[1] == y)
                    {
                        check = false;
                        break;
                    }
                }
                if (!check) continue;
                //if (x < mapxnum && x >= 0 && y < mapynum && y >= 0)
                if (is_Inmap(x,y))
                {
                    //空欄マスでなく、ジャンプ範囲内
                    if (maps[(int)MapdataList.MAPTEXTURE][y][x] != 0
                        && Mathf.Abs((maps[(int)MapdataList.MAPHEIGHT][y][x] - maps[(int)MapdataList.MAPHEIGHT][d[1]][d[0]]))<= jump)
                    {
                        //キャラがいない
                        if(!BattleVal.id2index.ContainsKey(string.Format("{0},{1}", x, y)))
                        {
                            if (!BattleVal.zocmap[x, y]) que.Enqueue(new int[] { x, y, d[2] - 1 });
                            ans.Add(new int[] { x, y });
                        }
                        //味方通過可能なルールの場合
                        else if (_is_through_teammate)
                        {
                            Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", x, y)];
                            //ターンプレイヤー側のマスなら、探索続行
                            if(temp.team == BattleVal.selectedUnit.team && !BattleVal.zocmap[x, y])
                                que.Enqueue(new int[] { x, y, d[2] - 1 });
                        }

                    }
                    
                }
            }
        }
        return ans;
    }

    //任意のマップ内座標2点から、ユニットの移動経路を返す関数
    public static List<int[]> GetPath(List<List<List<int>>> maps, int steps, int startx, int starty, int endx, int endy, int jump)
    {
        Queue<int[]> que = new Queue<int[]>();
        List<int[]> ans = new List<int[]>();
        Dictionary<string, int[]> parent = new Dictionary<string, int[]>();
        que.Enqueue(new int[] { startx, starty, steps });
        int[] d = new int[3];
        int[,] vector = new int[,] { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
        bool[,] checklist = new bool[steps * 2 + 1, steps * 2 + 1];
        //値の初期化
        for (int i = 0; i < steps * 2 + 1; i++)
        {
            for (int j = 0; j < steps * 2 + 1; j++)
            {
                checklist[i, j] = true;
            }
        }
        checklist[steps, steps] = false;
        int x, y;
        while (que.Count != 0)
        {
            d = que.Dequeue();
            //d[2]は残りステップ数
            if (d[2] <= 0) continue;
            if (d[0] == endx && d[1] == endy) break;
            checklist[d[0] - startx + steps, d[1] - starty + steps] = false;
            for (int i = 0; i < 4; i++)
            {
                x = d[0] + vector[i, 0];
                y = d[1] + vector[i, 1];
                //すでに通過した地点であれば排除
                if (!checklist[x - startx + steps, y - starty + steps]) continue;
                if (x < mapxnum && x >= 0 && y < mapynum && y >= 0)
                {
                    //透明マスでなく、ジャンプ範囲内
                    if (maps[(int)MapdataList.MAPTEXTURE][y][x] != 0
                        && Mathf.Abs((maps[(int)MapdataList.MAPHEIGHT][y][x] - maps[(int)MapdataList.MAPHEIGHT][d[1]][d[0]])) <= jump)
                    {
                        //キャラがいない
                        if(!BattleVal.id2index.ContainsKey(string.Format("{0},{1}", x, y)))
                        {
                            //通過地点をたどるためのハッシュの存在有無
                            if (parent.ContainsKey(string.Format("{0},{1}", x, y)))
                            {
                                //残りステップ数が多い方に更新
                                if (parent[string.Format("{0},{1}", x, y)][2] < d[2])
                                {
                                    parent[string.Format("{0},{1}", x, y)] = new int[] { d[0], d[1], d[2] };
                                    //エンキュー
                                    if (!BattleVal.zocmap[x, y]) que.Enqueue(new int[] { x, y, d[2] - 1 });
                                }
                            }
                            else
                            {
                                //存在しなければ追加
                                parent.Add(string.Format("{0},{1}", x, y), new int[] { d[0], d[1], d[2] });
                                //エンキュー
                                if (!BattleVal.zocmap[x, y]) que.Enqueue(new int[] { x, y, d[2] - 1 });
                            }
                        }
                        else if(_is_through_teammate)
                        {
                            Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", x, y)];
                            //ターンプレイヤー側のマスなら、探索続行
                            if (temp.team == BattleVal.selectedUnit.team)
                            {
                                parent[string.Format("{0},{1}", x, y)] = new int[] { d[0], d[1], d[2] };
                                //エンキュー
                                if (!BattleVal.zocmap[x, y]) que.Enqueue(new int[] { x, y, d[2] - 1 });
                            }
                        }
                        

                    }
                }
            }
        }
        //ゴールから探索スタート
        int tempx = endx;
        int tempy = endy;
        //ゴールからルートの作成
        ans.Add(new int[] { tempx, tempy });
        for (; ; )
        {
            if (tempx == startx && tempy == starty) break;
            int[] temp = parent[string.Format("{0},{1}", tempx, tempy)];
            ans.Add(new int[] { temp[0], temp[1] });
            tempx = temp[0];
            tempy = temp[1];

        }
        ans.Reverse();
        return ans;
    }

    //攻撃可能範囲を探索する関数
    public static List<int[]> DfsA(List<List<List<int>>> maps, int startx, int starty, Range range)
    {
        Queue<int[]> que = new Queue<int[]>();
        List<int[]> ans = new List<int[]>();
        que.Enqueue(new int[] { startx, starty, range.step });
        int[] d = new int[3];
        //各マス探索の基本ベクトル
        List<int[]> basevector = new List<int[]>();
        if(range.Isverthori)
        {
            basevector.Add(new int[] { 1, 0 });
            basevector.Add(new int[] { 0, 1 });
            basevector.Add(new int[] { -1, 0 });
            basevector.Add(new int[] { 0, -1 });
        }
        if(range.Iscross)
        {
            basevector.Add(new int[] { 1, 1 });
            basevector.Add(new int[] { 1, -1 });
            basevector.Add(new int[] { -1, -1 });
            basevector.Add(new int[] { -1, 1 });
        }

        int x, y;
        bool check = true;

        ans.Add(new int[] { startx, starty});

        //基本ベクトルを重ねない場合
        int counter_que = 0;

        if(range.Is1D)
        {
            for(int i = 0; i< basevector.Count - 1; i++)
            {
                que.Enqueue(new int[] { startx, starty, range.step });
            }
        }

        while (que.Count != 0)
        {
            d = que.Dequeue();
            if (d[2] <= 0) continue;
            for (int i = 0; i < basevector.Count; i++)
            {
                //基本ベクトルを重ねない場合
                if (range.Is1D && counter_que != i) continue;

                x = d[0] + basevector[i][0];
                y = d[1] + basevector[i][1];
                check = true;
                foreach (int[] t in ans)
                {
                    if (t[0] == x && t[1] == y)
                    {
                        check = false;
                        break;
                    }
                }
                if (!check)
                {
                    if (range.Is1D)
                    {
                        que.Enqueue(new int[] { startx, starty, d[2] - 1 });
                    }
                    continue;
                }
                if (x < mapxnum && x >= 0 && y < mapynum && y >= 0
                    && maps[(int)MapdataList.MAPTEXTURE][y][x] != 0
                    && (maps[(int)MapdataList.MAPHEIGHT][y][x] - maps[(int)MapdataList.MAPHEIGHT][d[1]][d[0]]) <= range.jumpup
                    && (maps[(int)MapdataList.MAPHEIGHT][y][x] - maps[(int)MapdataList.MAPHEIGHT][d[1]][d[0]]) >= -(range.jumpdown))
                {
                    //空欄マスでなく、上下範囲内
                    que.Enqueue(new int[] { x, y, d[2] - 1 });
                    ans.Add(new int[] { x, y });

                }
                else if (range.Is1D)
                {
                    que.Enqueue(new int[] { startx, starty, d[2] - 1 });
                }
            }

            counter_que = (counter_que + 1) % basevector.Count;
        }
        return ans;
    }

    public static void Create_ZOCmap()
    {
        //ZOCmapのクリア
        BattleVal.zocmap = new bool[mapxnum,mapynum];
        
        foreach(Unitdata unit in BattleVal.unitlist)
        {
            //敵でZOCを有するものの場合のみループ
            if(unit.team != BattleVal.selectedUnit.team && unit.zocflag)
            {
                if(unit.x + 1 < mapxnum)
                {
                    if (unit.y + 1 < mapynum && unit.zoclevel==2)
                        BattleVal.zocmap[unit.x + 1,unit.y + 1] = true;
                    if (unit.y - 1 >= 0 && unit.zoclevel == 2)
                        BattleVal.zocmap[unit.x + 1, unit.y - 1] = true;
                    BattleVal.zocmap[unit.x + 1, unit.y] = true;
                }
                if (unit.x - 1 >= 0)
                {
                    if (unit.y + 1 < mapynum && unit.zoclevel == 2)
                        BattleVal.zocmap[unit.x - 1, unit.y + 1] = true;
                    if (unit.y - 1 >= 0 && unit.zoclevel == 2)
                        BattleVal.zocmap[unit.x - 1, unit.y - 1] = true;
                    BattleVal.zocmap[unit.x - 1, unit.y] = true;
                }
                if (unit.y + 1 < mapynum)
                    BattleVal.zocmap[unit.x, unit.y + 1] = true;
                if (unit.y - 1 >= 0)
                    BattleVal.zocmap[unit.x, unit.y - 1] = true;
            }
        }
        
    }

    //2つの座標間のマンハッタン距離を返す
    public static int Calc_Dist(int[] PointA, int[] PointB)
    {
        return Math.Abs(PointA[0] - PointB[0]) + Math.Abs(PointA[1] - PointB[1]);
    }
}
