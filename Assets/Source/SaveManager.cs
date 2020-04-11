using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;

public class SaveDataOption
{
    public string object_name;
    public string contents;
    public Sprite save_picture;
}
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
    public SaveDataOption[][] save_object;
    public GameObject[] Show_Object;
    public Savedata[][] sd;
    //改ページする関数(次)
    public void NextPageButton()
    {
        now_page++;
        now_page = now_page % save_load_page;
        for (int i = 0; i < save_load; i++)
        {
            Show_Object[i].GetComponentInChildren<Text>().text = save_object[now_page][i].contents;
            Show_Object[i].name = save_object[now_page][i].object_name;
            Show_Object[i].SetActive(true);
            if (save_object[now_page][i].save_picture == null)
            {
                Show_Object[i].GetComponentInChildren<Image>().sprite = null;
                continue;
            }
            Show_Object[i].GetComponentInChildren<Image>().sprite = save_object[now_page][i].save_picture;
        }
        page_number.text = string.Format("{0}/{1}", now_page + 1, save_load_page);
    }
    //改ページする関数(前)
    public void PrevPageButton()
    {
        now_page--;
        if (now_page < 0) now_page += save_load_page;
        now_page = now_page % save_load_page;
        for (int i = 0; i < save_load; i++)
        {
            Show_Object[i].GetComponentInChildren<Text>().text = save_object[now_page][i].contents;
            Show_Object[i].name = save_object[now_page][i].object_name;
            Show_Object[i].SetActive(true);
            if (save_object[now_page][i].save_picture == null)
            {
                Show_Object[i].GetComponentInChildren<Image>().sprite = null;
                continue;
            }
            Show_Object[i].GetComponentInChildren<Image>().sprite = save_object[now_page][i].save_picture;
        }
        page_number.text = string.Format("{0}/{1}", now_page + 1, save_load_page);
    }
    //改ページボタンマウスオーバー時にSelectをNullにする
    public void PageButtonOnMouse(bool nextflag)
    {
        if(nextflag) EventSystem.current.SetSelectedGameObject(nextpagebutton.gameObject);
        else EventSystem.current.SetSelectedGameObject(prevpagebutton.gameObject);
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
        save_object = new SaveDataOption[save_load_page][];
        Show_Object = new GameObject[save_load];
        sd = new Savedata[save_load_page][];
        float y = save_box.GetComponent<RectTransform>().sizeDelta.y + 5;
        for (int page = 0; page < save_load_page; page++)
        {
            save_object[page] = new SaveDataOption[save_load];
            sd[page] = new Savedata[save_load];
            for (int i = 1; i <= save_load; i++)
            {
                save_object[page][i - 1] = new SaveDataOption();
                save_object[page][i - 1].save_picture = null;
                //save_object[page][i - 1] = save_box;
                if (page == 0)
                {
                    Show_Object[i-1] = Instantiate<GameObject>(save_box, Vector3.zero, Quaternion.identity, canvas.transform);
                    //save_object[page][i - 1] = Instantiate<GameObject>(save_box, Vector3.zero, Quaternion.identity, canvas.transform);
                    Show_Object[i - 1].transform.position = new Vector3(100, canvas.GetComponent<RectTransform>().sizeDelta.y - 5 - y * (i - 1) - 120, 0);
                }
                //Windows
                //filename = Application.streamingAssetsPath + string.Format("/SaveData/{0}{1}.json", page, i);
                //Mac
                filename = Application.persistentDataPath + string.Format("/SaveData/{0}{1}.json", page, i);
                save_object[page][i - 1].object_name = string.Format("{0}{1}", page, i);
                if (!System.IO.File.Exists(filename))
                {
                    save_object[page][i-1].contents = "　ＮＯ　ＤＡＴＡ";
                    //save_object[page][i - 1].GetComponentInChildren<Text>().text = "　ＮＯ　ＤＡＴＡ";
                    continue;
                }
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(fs);

                //**注意**------------------------------------------------------------------------------------------------------------------------------------------------------
                Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");

                //sd[page][i - 1] = JsonUtility.FromJson<Savedata>(ec.DecryptionSystem(sr.ReadToEnd(),false));
                sd[page][i - 1] = JsonUtility.FromJson<Savedata>(ec.DecryptionSystem(sr.ReadToEnd(), true));  //debug
                //**------------------------------------------------------------------------------------------------------------------------------------------------------------

                Rect rect = Show_Object[i - 1].GetComponent<Image>().GetComponent<RectTransform>().rect;

                try
                {
                    //Windows
                    //FileStream filestream = new FileStream(Application.streamingAssetsPath + string.Format("/SaveData/{0}{1}.png", page, i), FileMode.Open, FileAccess.Read);
                    //Mac
                    FileStream filestream = new FileStream(Application.persistentDataPath + string.Format("/SaveData/{0}{1}.png", page, i), FileMode.Open, FileAccess.Read);
                    BinaryReader bin = new BinaryReader(filestream);
                    byte[] readBinary = bin.ReadBytes((int)bin.BaseStream.Length);
                    bin.Close();
                    filestream.Dispose();
                    filestream = null;
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
                    save_object[page][i - 1].save_picture = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                }
                catch (System.IO.IsolatedStorage.IsolatedStorageException ex)
                {
                    Texture2D texture = new Texture2D((int)rect.width, (int)rect.height);
                    save_object[page][i - 1].save_picture = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                }

                save_object[page][i - 1].contents = sd[page][i - 1].current_message;
                
                sr.Close();
                fs.Close();
            }
        }
        for(int i = 0; i < save_load; i++)
        {
            Show_Object[i].GetComponentInChildren<Text>().text = save_object[0][i].contents;
            Show_Object[i].name = save_object[0][i].object_name;
            Show_Object[i].SetActive(true);
            if (save_object[0][i].save_picture == null) continue;
            Show_Object[i].GetComponentInChildren<Image>().sprite = save_object[0][i].save_picture;
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
        Show_Object = new GameObject[save_load];
        sd = new Savedata[save_load_page][];
        save_object = new SaveDataOption[save_load_page][];
        for (int page = 0; page < save_load_page; page++)
        {
            save_object[page] = new SaveDataOption[save_load];
            sd[page] = new Savedata[save_load];
            for (int i = 1; i <= save_load; i++)
            {
                save_object[page][i - 1] = new SaveDataOption();
                save_object[page][i - 1].save_picture = null;
                //Windows
                //filename = Application.streamingAssetsPath + string.Format("/SaveData/{0}{1}.json", page, i);
                //Mac
                filename = Application.persistentDataPath + string.Format("/SaveData/{0}{1}.json", page, i);
                if (page == 0)
                {
                    Show_Object[i - 1] = Instantiate<GameObject>(save_box, Vector3.zero, Quaternion.identity, canvas.transform);
                    Show_Object[i - 1].transform.position = new Vector3(100, canvas.GetComponent<RectTransform>().sizeDelta.y - 5 - y * (i - 1) - 120, 0);

                }
                save_object[page][i - 1].object_name = string.Format("{0}{1}", page, i);
                if (!System.IO.File.Exists(filename))
                {
                    sd[page][i - 1] = new Savedata();
                    save_object[page][i - 1].contents = "　ＮＯ　ＤＡＴＡ";
                    continue;
                }
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(fs);
                //**注意**--------------------------------------------------------------------------------------------------------------------------------------------------------------
                Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
                //string temp_text = ec.DecryptionSystem(sr.ReadLine(),false);
                string temp_text = ec.DecryptionSystem(sr.ReadLine(),true);  //debug

                //**--------------------------------------------------------------------------------------------------------------------------------------------------------------------
                //Debug.Log(temp_text);
                sd[page][i-1] = JsonUtility.FromJson<Savedata>(temp_text);

                Rect rect = Show_Object[i - 1].GetComponent<Image>().GetComponent<RectTransform>().rect;

                try
                {
                    //Windows
                    //FileStream filestream = new FileStream(Application.streamingAssetsPath + string.Format("/SaveData/{0}{1}.png", page, i), FileMode.Open, FileAccess.Read);
                    //Mac
                    FileStream filestream = new FileStream(Application.persistentDataPath + string.Format("/SaveData/{0}{1}.png", page, i), FileMode.Open, FileAccess.Read);
                    BinaryReader bin = new BinaryReader(filestream);
                    byte[] readBinary = bin.ReadBytes((int)bin.BaseStream.Length);
                    bin.Close();
                    filestream.Dispose();
                    filestream = null;
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
                    save_object[page][i - 1].save_picture = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                }
                catch (System.IO.IsolatedStorage.IsolatedStorageException ex)
                {
                    Texture2D texture = new Texture2D((int)rect.width, (int)rect.height);
                    save_object[page][i - 1].save_picture = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                }

                save_object[page][i - 1].contents = sd[page][i - 1].current_message;
                sr.Close();
                fs.Close();
            }
        }
        for (int i = 0; i < save_load; i++)
        {
            Show_Object[i].GetComponentInChildren<Text>().text = save_object[0][i].contents;
            Show_Object[i].name = save_object[0][i].object_name;
            Show_Object[i].GetComponentInChildren<Image>().sprite = save_object[0][i].save_picture;
            Show_Object[i].SetActive(true);
        }
    }
}
