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

    int connectNum = 4; // 接続台数

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>(); // Cubeの番号とIDの対応付け
    Dictionary<int, Vector2> toio_pos = new Dictionary<int, Vector2>(); // Cubeの番号と座標の対応付け
    
    async void Start()
    {
        // Cubeの番号とIDの対応付け, Cubeの番号と座標の対応付け
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

        // キューブの複数台接続
        cubeManager = new CubeManager(connectType);
        await cubeManager.MultiConnect(connectNum);
    }

    // フレーム毎に呼ばれる
    void Update()
    {
        foreach (var cube in cubeManager.syncCubes)
        {   
            if(cube.id == toio_dict[1] || cube.id == toio_dict[2] || cube.id == toio_dict[7]){
                if (Input.GetKey(KeyCode.LeftArrow)) {
                    cube.Move(-20, 20, 50);
                } else if (Input.GetKey(KeyCode.RightArrow)) {
                    cube.Move(20, -20, 50);
                } else if (Input.GetKey(KeyCode.UpArrow)) {
                    cube.Move(100, 100, 100);
                } else if (Input.GetKey(KeyCode.DownArrow)) {
                    cube.Move(-100, -100, 100);
                }
            }
            else if(cube.id == toio_dict[3]){
                if (Input.GetKey(KeyCode.LeftArrow)) {
                    cube.Move(-20, 20, 50);
                } else if (Input.GetKey(KeyCode.RightArrow)) {
                    cube.Move(20, -20, 50);
                } else if (Input.GetKey(KeyCode.UpArrow)) {
                    cube.Move(-100, -100, 100);
                } else if (Input.GetKey(KeyCode.DownArrow)) {
                    cube.Move(100, 100, 100);
                }
            }
        }

        foreach(var navigator in cubeManager.syncNavigators)
        {
            // if(navigator.cube.id == toio_dict[3])
            // {
            //     float distance = Vector2.Distance(navigator.cube.pos, toio_pos[1]);
            //     if(distance < 3)
            //     {
            //         navigator.handle.Stop();
            //         Debug.Log("toio_dict[3]とtoio_pos[1]の距離が3以下になったよ");
            //     }
            //     else
            //     {
            //         navigator.handle.Move(-20, 0, 20);
            //     }
            // }
        }

        // キューブのXY座標表示
        string text = "";
        foreach (var cube in cubeManager.syncCubes)
        {
            text += "toio_dict[3]:(" + cube.x + "," + cube.y + "," + cube.angle + ") \n";

            // toio_dict[3]とtoio_pos[0]の距離を表示する
            if(cube.id == toio_dict[3]){
                float distance = Vector2.Distance(cube.pos, toio_pos[0]);
                text += "distance(toio_pos[0]): " + distance + "\n";
            }

            // toio_dict[3]とtoio_pos[1]の距離を表示する
            if(cube.id == toio_dict[3]){
                float distance = Vector2.Distance(cube.pos, toio_pos[1]);
                text += "distance(toio_pos[1]): " + distance + "\n";
            }

            // toio_dict[3]とtoio_pos[2]の距離を表示する
            if(cube.id == toio_dict[3]){
                float distance = Vector2.Distance(cube.pos, toio_pos[2]);
                text += "distance(toio_pos[2]): " + distance + "\n";
            }

            if(cube.id == toio_dict[1]){
                Debug.Log("toio_dict[1]と接続したよ");
            }

            else if(cube.id == toio_dict[4]){
                Debug.Log("toio_dict[4]と接続したよ");
            }
        }
        if (text != "") this.label.text = text;
    }
}
