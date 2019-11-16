using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


/********************************************/
/*SaveManager savemanager セーブ画面の管理  */
/*  セーブ、ロードに関する画面の制御を行う  */
/*  クラス                                  */
/********************************************/
public class SaveManager : MonoBehaviour
{
    public int save_load_page = 10;
    public int save_load = 5;
    public Image save_BackGround;
    public Text save_menu_text;
    public Text load_menu_text;
    public Text page_number;
    public Button nextpagebutton;
    public Button prevpagebutton;
    public GameObject save_box;
    public Canvas canvas;
    public int now_page = 0;
    public GameObject[][] save_object;
    public Savedata[][] sd;
    //改ページする関数(次)
    public void NextPageButton()
    {
        now_page++;
        now_page = now_page % save_load_page;
        page_number.text = string.Format("{0}/{1}", now_page + 1, save_load_page);
    }
    //改ページする関数(前)
    public void PrevPageButton()
    {
        now_page--;
        if (now_page < 0) now_page += save_load_page;
        now_page = now_page % save_load_page;
        page_number.text = string.Format("{0}/{1}", now_page + 1, save_load_page);
    }
    //Save画面を描画する関数
    public void SaveWindowOpen()
    {
        page_number.gameObject.SetActive(true);
        nextpagebutton.gameObject.SetActive(true);
        prevpagebutton.gameObject.SetActive(true);
        save_BackGround.gameObject.SetActive(true);
        save_menu_text.gameObject.SetActive(true);
        page_number.text = string.Format("{0}/{1}", now_page + 1, save_load_page);
        FileStream fs;
        StreamReader sr;
        string filename = "";
        save_object = new GameObject[save_load_page][];
        sd = new Savedata[save_load_page][];
        float y = save_box.GetComponent<RectTransform>().sizeDelta.y + 5;
        for (int page = 0; page < save_load_page; page++)
        {
            save_object[page] = new GameObject[save_load];
            sd[page] = new Savedata[save_load];
            for (int i = 1; i <= save_load; i++)
            {
                filename = Application.streamingAssetsPath + string.Format("/SaveData/{0}{1}.json", page, i);
                save_object[page][i - 1] = Instantiate<GameObject>(save_box, Vector3.zero, Quaternion.identity,canvas.transform);
                save_object[page][i - 1].name = string.Format("{0}{1}", page, i);
                save_object[page][i - 1].transform.position = new Vector3(100, canvas.GetComponent<RectTransform>().sizeDelta.y - 5 - y * (i - 1) - 120, 0);
                if (!System.IO.File.Exists(filename))
                {
                    save_object[page][i - 1].GetComponentInChildren<Text>().text = "　ＮＯ　ＤＡＴＡ";
                    save_object[page][i - 1].gameObject.SetActive(false);
                    continue;
                }
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(fs);

                //**注意**------------------------------------------------------------------------------------------------------------------------------------------------------
                Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
               
                //sd[page][i - 1] = JsonUtility.FromJson<Savedata>(ec.DecryptionSystem(sr.ReadToEnd(),false));
                sd[page][i - 1] = JsonUtility.FromJson<Savedata>(ec.DecryptionSystem(sr.ReadToEnd(),true));  //debug
                FileStream filestream = new FileStream(Application.streamingAssetsPath + string.Format("/SaveData/{0}{1}.png", page, i), FileMode.Open, FileAccess.Read);

                //**------------------------------------------------------------------------------------------------------------------------------------------------------------
                BinaryReader bin = new BinaryReader(filestream);
                byte[] readBinary = bin.ReadBytes((int)bin.BaseStream.Length);
                bin.Close();
                filestream.Dispose();
                filestream = null;
                Rect rect = save_object[page][i - 1].GetComponent<Image>().GetComponent<RectTransform>().rect;
                Texture2D texture = new Texture2D((int)rect.width, (int)rect.height);
                if (readBinary != null)
                {
                    //横サイズ
                    int pos = 16;
                    int width = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        width = width * 256 + readBinary[pos++];
                    }
                    //縦サイズ
                    int height = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        height = height * 256 + readBinary[pos++];
                    }
                    //byteからTexture2D作成
                    texture = new Texture2D(width, height);
                    texture.LoadImage(readBinary);
                }
                readBinary = null;
                save_object[page][i - 1].GetComponentInChildren<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                save_object[page][i - 1].GetComponentInChildren<Text>().text = sd[page][i - 1].current_message;
                save_object[page][i - 1].gameObject.SetActive(false);
                sr.Close();
                fs.Close();
            }
        }
    }
    //Load画面を描画する関数
    public void LoadWindowOpen()
    {
        page_number.gameObject.SetActive(true);
        nextpagebutton.gameObject.SetActive(true);
        prevpagebutton.gameObject.SetActive(true);
        now_page = 0;
        page_number.text = string.Format("{0}/{1}", now_page + 1, save_load_page);
        save_BackGround.gameObject.SetActive(true);
        save_menu_text.gameObject.SetActive(false);
        load_menu_text.gameObject.SetActive(true);
        FileStream fs;
        StreamReader sr;
        string filename = "";
        float y = save_box.GetComponent<RectTransform>().sizeDelta.y + 5;
        sd = new Savedata[save_load_page][];
        save_object = new GameObject[save_load_page][];
        for (int page = 0; page < save_load_page; page++)
        {
            save_object[page] = new GameObject[save_load];
            sd[page] = new Savedata[save_load];
            for (int i = 1; i <= save_load; i++)
            {
                filename = Application.streamingAssetsPath + string.Format("/SaveData/{0}{1}.json", page, i);
                save_object[page][i - 1] = Instantiate<GameObject>(save_box, Vector3.zero, Quaternion.identity, canvas.transform);
                save_object[page][i - 1].name = string.Format("{0}{1}", page, i);
                save_object[page][i - 1].transform.position = new Vector3(100, canvas.GetComponent<RectTransform>().sizeDelta.y - 5 - y * (i - 1) - 120, 0);
                if (!System.IO.File.Exists(filename))
                {
                    sd[page][i - 1] = new Savedata();
                    save_object[page][i - 1].GetComponentInChildren<Text>().text = "　ＮＯ　ＤＡＴＡ";
                    save_object[page][i - 1].gameObject.SetActive(false);
                    continue;
                }
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(fs);
                //**注意**--------------------------------------------------------------------------------------------------------------------------------------------------------------
                Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
                //string temp_text = ec.DecryptionSystem(sr.ReadLine(),false);
                string temp_text = ec.DecryptionSystem(sr.ReadLine(),true);  //debug

                //**--------------------------------------------------------------------------------------------------------------------------------------------------------------------
                Debug.Log(temp_text);
                sd[page][i-1] = JsonUtility.FromJson<Savedata>(temp_text);
                FileStream filestream = new FileStream(Application.streamingAssetsPath + string.Format("/SaveData/{0}{1}.png", page, i), FileMode.Open, FileAccess.Read);
                BinaryReader bin = new BinaryReader(filestream);
                byte[] readBinary = bin.ReadBytes((int)bin.BaseStream.Length);
                bin.Close();
                filestream.Dispose();
                filestream = null;
                Rect rect = save_object[page][i - 1].GetComponent<Image>().GetComponent<RectTransform>().rect;
                Texture2D texture = new Texture2D((int)rect.width, (int)rect.height);
                if (readBinary != null)
                {
                    //横サイズ
                    int pos = 16;
                    int width = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        width = width * 256 + readBinary[pos++];
                    }
                    //縦サイズ
                    int height = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        height = height * 256 + readBinary[pos++];
                    }
                    //byteからTexture2D作成
                    texture = new Texture2D(width, height);
                    texture.LoadImage(readBinary);
                }
                readBinary = null;
                save_object[page][i - 1].GetComponentInChildren<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                save_object[page][i - 1].GetComponentInChildren<Text>().text = sd[page][i - 1].current_message;
                save_object[page][i - 1].gameObject.SetActive(false);
                sr.Close();
                fs.Close();
            }
        }
    }
}
