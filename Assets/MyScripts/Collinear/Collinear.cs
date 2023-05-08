using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// *********************************************************
// 3台のtoioを一列に並べるプログラム(実機 version)
// *********************************************************

public class Collinear : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType;

    public int slope_num = 0;
    public int cube_num = 1;
    public int press_num = 2;

    int phase = 0;
    int check = 0;

    Vector2 pos_slope = new Vector2(0, 0);
    Vector2 pos_cube = new Vector2(0, 0);
    Vector2 pos_press = new Vector2(0, 0);

    int angle_slope = 0;

    // L : 実際の距離[mm] = 1 : 1.4mm
    int L_cube=60, L_press=110;

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

    // Cube接続台数
    public int connectNum = 3;

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
        if (cm.synced){
            foreach(var navigator in cm.syncNavigators){
                if(check == 0){
                    if(navigator.cube.id == toio_dict[slope_num] && navigator.cube.x != 0 && navigator.cube.y != 0){
                        pos_slope = new Vector2(navigator.cube.x, navigator.cube.y);
                        angle_slope = navigator.cube.angle;
                        check += 1;

                        pos_cube = CalculateNewPosition(pos_slope, angle_slope, L_cube);
                        pos_press = CalculateNewPosition(pos_slope, angle_slope, L_press);
                        Debug.Log("pos_cube: " + pos_cube.x + ", " + pos_cube.y);
                        Debug.Log("pos_press: " + pos_press.x + ", " + pos_press.y);
                    }
                }
                else{
                    if(phase == 0){
                        if(navigator.cube.id == toio_dict[cube_num]){
                            var mv = navigator.Navi2Target(pos_cube.x, pos_cube.y, maxSpd:50).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase0");
                        }
                    }
                    if(phase == 1){
                        if(navigator.cube.id == toio_dict[press_num]){
                            var mv = navigator.Navi2Target(pos_press.x, pos_press.y, maxSpd:50).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase1");
                        }
                    }
                    else if(phase == 2){
                        if(navigator.cube.id == toio_dict[cube_num]){
                            Movement mv = navigator.handle.Rotate2Deg(angle_slope).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase2");
                        }
                    }
                    else if(phase == 3){
                        if(navigator.cube.id == toio_dict[press_num]){
                            Movement mv = navigator.handle.Rotate2Deg(angle_slope).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase3");
                        }
                    }
                }
            }

            string text = "";
            foreach (var cube in cm.syncCubes){
                if(cube.id == toio_dict[slope_num]) text += "slope：";
                else if(cube.id == toio_dict[cube_num]) text += "cube：";
                else if(cube.id == toio_dict[press_num]) text += "press：";
                
                text += "(" + cube.x + "," + cube.y + "," + cube.angle + ")\n";
            }
            if (text != "") this.label.text = text;
        }
    }

    Vector2 CalculateNewPosition(Vector2 pos, int angle, int distance)
    {
        float angleRadians = angle * Mathf.Deg2Rad;
        float x = pos.x + distance * Mathf.Cos(angleRadians);
        float y = pos.y + distance * Mathf.Sin(angleRadians);

        return new Vector2((int)x, (int)y);
    }
}
