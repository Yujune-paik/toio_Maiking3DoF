using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// *************************************
// 上のtoioが隣のtoioへ移動するプログラム
// *************************************

public class MovingOnToio : MonoBehaviour
{
    public Text label;
    public ConnectType connectType;
    CubeManager cm;
    Cube cube;

    // Cubeの接続台数
    int cube_num = 5;

    // 上に乗っているtoioの番号
    int onToioNum = 0;

    // onToioの下にあるtoioの番号
    int underToioNum0 = 1;

    // underToioの隣にあるtoioの番号
    int underToioNum1 = 2;

    // toioの移動距離
    int distance = 30;

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>(); // Cubeの番号とIDの対応付け
    Dictionary<int, Vector2> toio_pos = new Dictionary<int, Vector2>(); // Cubeの番号と座標の対応付け

    async void Start()
    {
        // CubeのIDと名前の対応付け
        using (var sr = new StreamReader("Assets/toio_number.csv"))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var values = line.Split(',');
                toio_dict.Add(int.Parse(values[0]), values[1]); // Cubeの番号とIDの対応付け
                toio_pos.Add(int.Parse(values[0]), new Vector2(int.Parse(values[2]), int.Parse(values[3]))); // Cubeの番号と座標の対応付け
            }
        }

        cm = new CubeManager(connectType);
        await cm.MultiConnect(cube_num);
    }

    void Update()
    {
        // Calculate the angle between underToio0 and underToio1
        int angle = CalculateAngle(cm.syncCubes[underToio0], cm.syncCubes[underToio1]);

        // Rotate the onToio to the direction of underToio1
        Cube onToioCube = cm.syncCubes[onToioNum];
        // Here we are assuming that cm.handles[onToioNum] will return the handle for the onToio cube
        cm.handles[onToioNum].Update();
        cm.handles[onToioNum].RotateByDeg(angle, 40).Exec();

        // Move the onToio to the direction of underToio1
        // We are not sure about the distance you want to move the onToio cube. Please replace 'distance' with the actual value you want.
        cm.handles[onToioNum].Update();
        cm.handles[onToioNum].TranslateByDist(distance, 40).Exec();

        // Display the number of the underToio1 on the UI.Text
        string labelText = GetToioOnTopText();
        this.label.text = labelText;
    }

    // 上に乗っているtoioの真下のtoioの番号を更新する関数
    string GetToioOnTopText()
    {
        string labelText = "";

        foreach(var cube in cm.syncCubes)
        {   
            if(cube.id == toio_dict[onToioNum]){
                foreach(var pos in toio_pos)
                {
                    if(Mathf.Abs(cube.x - pos.Value.x) < 10 && Mathf.Abs(cube.y - pos.Value.y) < 10)
                    {
                        labelText = "toio["+ pos.Key +"]の上にあるよ";
                    }
                }
            }
        }

        return labelText;
    }

    // あるtoioに対して、もう一方のtoioがどの角度にくっついているか計算する関数
    int CalculateAngle(Cube refCube, Cube targetCube)
    {
        int dx = targetCube.x - refCube.x;
        int dy = targetCube.y - refCube.y;
        int angle = (int)(Mathf.Atan2(dy, dx) * Mathf.Rad2Deg) - refCube.angle;
        angle = (angle + 360) % 360;

        return (angle / 90) * 90; // 0°、90°、180°、270°、360°(=0°)に近似する
    }

    // 「cebe.id → Cubeの番号」 へ変換する関数
    int GetCubeId(string cubeId)
    {
        foreach(var item in toio_dict)
        {
            if(item.Value == cubeId) return item.Key;
        }
        return -1;
    }
}
