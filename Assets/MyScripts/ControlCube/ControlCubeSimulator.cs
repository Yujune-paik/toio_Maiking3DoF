using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class ControlCubeSimulator : MonoBehaviour
{
    public Text label;
    public ConnectType connectType; // 接続種別

    CubeManager cubeManager; // キューブマネージャ

    // キューブ複数台接続
    public int connectNum = 8; // 接続数

    // 停止する座標(187,45)
    Vector2 stopPos = new Vector2(187,45);

    async void Start()
    {
        // キューブの複数台接続
        cubeManager = new CubeManager(connectType);
        await cubeManager.MultiConnect(connectNum);
    }

    // フレーム毎に呼ばれる
    void Update()
    {
        // foreach (var cube in cubeManager.syncCubes)
        // {   
        //     // "CubeX"のX値を変えれば、
        //     // 接続しているCubeごとに処理を分けられる
        //     if(cube.localName == "Cube0"){
        //         if (Input.GetKey(KeyCode.LeftArrow)) {
        //             cube.Move(-20, 20, 50);
        //         } else if (Input.GetKey(KeyCode.RightArrow)) {
        //             cube.Move(20, -20, 50);
        //         } else if (Input.GetKey(KeyCode.UpArrow)) {
        //             cube.Move(50, 50, 50);
        //         } else if (Input.GetKey(KeyCode.DownArrow)) {
        //             cube.Move(-50, -50, 50);
        //         }
        //     }
        // }

        // stopPos付近(誤差 5)に到達するまでcube.Move(50,50,50)
        foreach (var cube in cubeManager.syncCubes)
        {
            if(cube.localName == "Cube0"){
                if (cube.x < stopPos.x - 10 || cube.x > stopPos.x + 10 || cube.y < stopPos.y - 10 || cube.y > stopPos.y + 10) {
                    cube.Move(50, 50, 10);
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
