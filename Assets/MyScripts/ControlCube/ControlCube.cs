using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class ControlCube : MonoBehaviour
{
    public Text label;
    public ConnectType connectType; // 接続種別

    CubeManager cubeManager; // キューブマネージャ

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

    // キューブ複数台接続
    public int connectNum = 8; // 接続数

    async void Start()
    {
        using (var sr = new StreamReader("Assets/toio_number.csv"))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var values = line.Split(',');
                toio_dict.Add(int.Parse(values[0]), values[1]);
            }
        }

        // キューブの複数台接続
        cubeManager = new CubeManager(connectType);
        await cubeManager.MultiConnect(connectNum);
    }

    // フレーム毎に呼ばれる
    void Update()
    {
        foreach (var cube in cubeManager.syncCubes)
        {   
            if(cube.id == toio_dict[0]){
                if (Input.GetKey(KeyCode.LeftArrow)) {
                    cube.Move(-20, 20, 50);
                } else if (Input.GetKey(KeyCode.RightArrow)) {
                    cube.Move(20, -20, 50);
                } else if (Input.GetKey(KeyCode.UpArrow)) {
                    cube.Move(30, 30, 50);
                } else if (Input.GetKey(KeyCode.DownArrow)) {
                    cube.Move(-30, -30, 50);
                }
            }
        }

        // キューブのXY座標表示
        string text = "";
        foreach (var cube in cubeManager.syncCubes)
        {
            text += "(" + cube.x + "," + cube.y + ")\n";
        }
        if (text != "") this.label.text = text;
    }
}
