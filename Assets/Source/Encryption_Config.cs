using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***************************************/
/*Encryption encryption 暗号           */
/*  暗号化・復号化を行う関数を所持する */
/*    現状はone time pad を利用        */
/***************************************/
public class Encryption_Config : MonoBehaviour
{
    [Range(0, 1)]
    public byte[] cipher_code;

    //暗号化関数
    public string EncryptionSystem(string plain_data,bool forced_do)
    {
        if (Debug.isDebugBuild && !forced_do) return plain_data;
        byte[] byte_data = System.Text.Encoding.UTF8.GetBytes(plain_data);
        int count = cipher_code.Length;
        for (int i = 0; i < byte_data.Length; i++)
        {
            byte_data[i] ^= cipher_code[i % count];
        }
        return System.Convert.ToBase64String(byte_data);
    }
    //復号化関数
    public string DecryptionSystem(string json_data,bool forced_do)
    {
        if (Debug.isDebugBuild && !forced_do) return json_data;
        byte[] byte_data = System.Convert.FromBase64String(json_data);
        int count = cipher_code.Length;
        for (int i = 0; i < byte_data.Length; i++)
        {
            byte_data[i] ^= cipher_code[i % count];
        }
        return System.Text.Encoding.UTF8.GetString(byte_data);
    }
}
