using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.IO;
using System;
/***************************************/
/*Encryption encryption 暗号           */
/*  暗号化・復号化を行う関数を所持する */
/*    現状はone time pad を利用        */
/***************************************/
public class Encryption_Config : MonoBehaviour
{
    [Range(0, 1)]
    public byte[] cipher_code;
    public static byte[] key;
    public static byte[] IV;
    public static void GenerateKeys()
    {
        key = new byte[32];
        IV = new byte[16];
        RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        rngCsp.GetBytes(key);
        rngCsp.GetBytes(IV);
    }
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
    /*
    //暗号化関数(推奨)-Rijndael暗号-
    public string EncryptionSystem(string json_data,int count,bool forced_do)
    {
        if (Debug.isDebugBuild && !forced_do) return json_data;
        byte[] keys = key;
        byte[] temp = IV;
        temp[count%temp.Length] = Convert.ToByte(count);
        byte[] IVs = temp;
        byte[] encrypted;
        Rijndael use_rijndael = Rijndael.Create();
        use_rijndael.Key = keys;
        use_rijndael.IV = IVs;
        ICryptoTransform encryptor = use_rijndael.CreateEncryptor(use_rijndael.Key, use_rijndael.IV);
        using (MemoryStream msEncrypt = new MemoryStream())
        {
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(json_data);
                }
                encrypted = msEncrypt.ToArray();
            }
        }
        Console.WriteLine(BitConverter.ToString(encrypted));
        return BitConverter.ToString(encrypted);
    }
    //復号化暗号(推奨)-Rijndael-
    public string DecryptionSystem(string json_data,int count,bool forced_do)
    {
        if (Debug.isDebugBuild && !forced_do) return json_data;
        byte[] keys = key;
        byte[] temp = IV;
        temp[count] = Convert.ToByte(count);
        byte[] IVs = IV;
        string answer = "";
        Rijndael use_rijndael = Rijndael.Create();
        use_rijndael.Key = keys;
        use_rijndael.IV = IVs;
        ICryptoTransform decryptor = use_rijndael.CreateDecryptor(use_rijndael.Key, use_rijndael.IV);
        string[] array = json_data.Split('-');
        byte[] bytes = new byte[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            bytes[i] = Convert.ToByte(array[i], 16);
        }
        Console.WriteLine(BitConverter.ToString(bytes));
        using (MemoryStream msDecrypt = new MemoryStream(bytes))
        {
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader swDecrypt = new StreamReader(csDecrypt))
                {
                    answer = swDecrypt.ReadToEnd();
                }
            }
        }
        return answer;
    }*/
}
