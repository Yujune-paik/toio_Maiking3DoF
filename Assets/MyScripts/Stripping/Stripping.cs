using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks; // 追加

public class Stripping : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Real;

    int phase = 0;
    int check = 0;

    Vector2 pos_cube1 = new Vector2(0, 0);
    Vector2 pos_cube0 = new Vector2(0, 0);
    Vector2 pos_cube2 = new Vector2(0, 0);

    int angle_cube1 = 0;

    int L = 50;

    int connectNum = 4;
    
    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

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

        cm = new CubeManager(connectType);
        // キューブの複数台接続
        await ConnectToioCubes();
    }

    async Task ConnectToioCubes()
    {
        while (cm.syncCubes.Count < connectNum)
        {
            await cm.MultiConnect(connectNum);
        }
        // 接続台数をコンソールに表示する
        Debug.Log(cm.syncCubes.Count);
    }

    float reverseStartTimeCube0 = 0f; // 後退を開始した時刻[s]
    float reverseStartTimeCube2 = 0f; // 後退を開始した時刻[s]
    bool reverseCube0 = false; // Cube0(後方)が後退したかどうか
    bool reverseCube2 = false; // Cube2(前方)が後退したかどうか
    float reverseDuration = 0.5f; // 後退する秒数[s]

    void Update()
    {
        if (cm.synced){
            foreach(var navigator in cm.syncNavigators){
                if(check == 0){
                    if(navigator.cube.id == toio_dict[1] && navigator.cube.x != 0 && navigator.cube.y != 0){
                        pos_cube1 = new Vector2(navigator.cube.x, navigator.cube.y);
                        angle_cube1 = navigator.cube.angle;
                        check += 1;
                        pos_cube0 = CalculateNewPosition(pos_cube1, angle_cube1, -L);
                        pos_cube2 = CalculateNewPosition(pos_cube1, angle_cube1, L);
                        Debug.Log("pos_cube0: " + pos_cube0.x + ", " + pos_cube0.y);
                        Debug.Log("pos_cube2: " + pos_cube2.x + ", " + pos_cube2.y);
                    }
                }
                else{
                    if(phase == 0){
                        if(navigator.cube.id == toio_dict[0]){
                            var mv = navigator.Navi2Target(pos_cube0.x, pos_cube0.y, maxSpd:50).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase0");
                        }
                    }
                    if(phase == 1){
                        if(navigator.cube.id == toio_dict[5]){
                            var mv = navigator.Navi2Target(pos_cube2.x, pos_cube2.y, maxSpd:50).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase1");
                        }
                    }
                    else if(phase == 2){
                        if(navigator.cube.id == toio_dict[0]){
                            Movement                             mv = navigator.handle.Rotate2Deg(angle_cube1 + 180).Exec();
                            if (mv.reached) phase += 1;
                            Debug.Log("phase2");
                        }
                    }
                    else if (phase == 3)
                    {
                        if (navigator.cube.id == toio_dict[5])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(angle_cube1).Exec();
                            if (mv.reached)
                            {
                                phase += 1;
                                reverseStartTimeCube0 = Time.time;
                                reverseStartTimeCube2 = Time.time;
                            }
                            Debug.Log("phase3");
                        }
                    }
                    else if (phase == 4)
                    {
                        if (navigator.cube.id == toio_dict[0] && !reverseCube0)
                        {
                            navigator.cube.Move(-50, -50, 50);
                            if (Time.time - reverseStartTimeCube0 >= reverseDuration)
                            {
                                navigator.handle.Stop();
                                reverseCube0 = true;
                            }
                            Debug.Log("phase4 Cube0");
                        }
                        if (navigator.cube.id == toio_dict[5] && !reverseCube2)
                        {
                            navigator.cube.Move(-50, -50, 50);
                            if (Time.time - reverseStartTimeCube2 >= reverseDuration)
                            {
                                navigator.handle.Stop();
                                reverseCube2 = true;
                            }
                            Debug.Log("phase4 Cube2");
                        }
                    }
                    else if(phase == 5)
                    {
                        // toio_dict[3]の処理
                        // 10秒間後退させる
                        if (navigator.cube.id == toio_dict[3])
                        {
                            navigator.cube.Move(-50, -50, 50);
                            if (Time.time - reverseStartTimeCube0 >= 10)
                            {
                                navigator.handle.Stop();
                                phase += 1;
                            }
                            Debug.Log("phase5");
                        }
                    }
                }
            }

            string text = "";
            foreach (var cube in cm.syncCubes)
            {
                if (cube.id == toio_dict[0]) text += "Cube0: ";
                else if (cube.id == toio_dict[1]) text += "Cube1: ";
                else if (cube.id == toio_dict[5]) text += "Cube5: ";
                else if (cube.id == toio_dict[3]) text += "Cube3: ";

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

