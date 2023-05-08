using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// ****************************
// toio同士の結合をはがすプログラム
// ****************************

public class Stripping : MonoBehaviour
{
    public Text label;

    CubeManager cm; // キューブマネージャ
    public ConnectType connectType; // 接続種別

    // int connecting_num0 = 0;
    // int connecting_num1 = 1;
    // int connecting_num2 = 2;
    // int connectiong_num3 = 3;

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

    // キューブ複数台接続
    public int connectNum = 4; // 接続数

    async void Start()
    {
        // CubeのIDと名前の対応付け
        using (var sr = new StreamReader("Assets/toio_number.csv"))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var values = line.Split(',');
                toio_dict.Add(int.Parse(values[0]), values[1]);
            }
        }
        
        // キューブの接続
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);
    }

    void Update()
    {
        string text = "";
        foreach (var handle in cm.syncHandles){
            text += "(" + handle.cube.x + "," + handle.cube.y + "," + handle.cube.angle + ")\n";
        }
        if (text != "") this.label.text = text;
    }
}

