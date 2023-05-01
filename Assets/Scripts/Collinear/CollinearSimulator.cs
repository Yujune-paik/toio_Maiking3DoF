using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// *******************************
// simulatorで3台のtoioを一列に並べるプログラム
// *******************************

public class CollinearSimulator : MonoBehaviour
{
    public Text label;

    CubeManager cm; // キューブマネージャ
    public ConnectType connectType; // 接続種別

    int phase = 0; // フェーズ
    int check = 0; // slopeの座標を取得したかどうか

    Vector2 pos_slope = new Vector2(0, 0);
    Vector2 pos_cube = new Vector2(0, 0);
    Vector2 pos_press = new Vector2(0, 0);

    int angle_slope = 0;

    int L_cube=10,L_press=60;

    int connecting_num0 = 0;
    int connecting_num1 = 1;
    int connecting_num2 = 2;

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

    // キューブ複数台接続
    public int connectNum = 3; // 接続数

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
        // キューブの接続
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);
    }

    void Update()
    {
        // // 処理の内容
        // // 1. slopeの位置を特定
        // // 2. slopeの位置を基準にcubeとpressが向かうべき座標を計算
        // // 3. cubeとpressをそれぞれ計算した座標に移動
        // // 4. cubeとpressをslopeの角度に回転

        // // シミュレータではこれを使えば良い
        foreach(var handle in cm.syncHandles){
            if(check == 0){
                if(handle.cube.localName == "Cube2"){
                    // pos_slopeにCube0の座標を代入
                    pos_slope = new Vector2(handle.cube.x, handle.cube.y);
                    angle_slope = handle.cube.angle;
                    check += 1;
                    pos_cube = CalculateNewPosition(L_cube);
                    pos_press = CalculateNewPosition(L_press);
                    Debug.Log("pos_cube: " + pos_cube.x + ", " + pos_cube.y);
                    Debug.Log("pos_press: " + pos_press.x + ", " + pos_press.y);
                }
            }else{
                if(phase == 0){
                    if(handle.cube.localName == "Cube0"){
                        Movement mv = handle.Move2Target(pos_cube.x, pos_cube.y).Exec();
                        if(mv.reached) phase += 1; // 到着
                        Debug.Log("phase0");
                    }
                }else if(phase == 1){
                    if(handle.cube.localName == "Cube1"){
                        Movement mv = handle.Move2Target(pos_press.x, pos_press.y).Exec();
                        if(mv.reached) phase += 1; // 到着
                        Debug.Log("phase1");
                    }
                }else if(phase == 2){
                    if(handle.cube.localName == "Cube0"){
                        Movement mv = handle.Rotate2Deg(angle_slope).Exec();
                        if(mv.reached) phase += 1; // 到着
                        Debug.Log("phase2");
                    }
                }else if(phase == 3){
                    if(handle.cube.localName == "Cube1"){
                        Movement mv = handle.Rotate2Deg(angle_slope).Exec();
                        if(mv.reached) phase += 1; // 到着
                        Debug.Log("phase3");
                    }
                }
            }
        }

        string text = "";
        foreach (var handle in cm.syncHandles){
            text += "(" + handle.cube.x + "," + handle.cube.y + "," + handle.cube.angle + ")\n";
        }
        if (text != "") this.label.text = text;
    }

    Vector2 CalculateNewPosition(int distance)
    {
        float angleRadians = angle_slope * Mathf.Deg2Rad;
        float x = pos_slope.x + distance * Mathf.Cos(angleRadians);
        float y = pos_slope.y + distance * Mathf.Sin(angleRadians);

        return new Vector2((int)x, (int)y);
    }
}

