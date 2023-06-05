using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

public class OnSlope : MonoBehaviour
{  
    public Text label;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Real;

    int phase = 0;

    Vector2 pos_slope = new Vector2(0, 0);
    Vector2 pos_cube = new Vector2(0, 0);
    Vector2 pos_press = new Vector2(0, 0);

    int connectNum = 2;

    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

    int num_cube = 1;
    int num_slope = 1;
    int num_press = 3;

    bool OnSlope_flag = false;

    // Slopeの登り切った平らなところの座標
    Vector2 pos_flat = new Vector2(242, 342);

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


    void Update()
    {
        if (cm.synced)
        {
            foreach (var navigator in cm.syncNavigators)
            {
                navigator.handle.Move(-50,0,100);
                // if (phase == 0)
                // {
                //     if (navigator.cube.id == toio_dict[num_cube])
                //     {
                //         navigator.handle.Move(-50, 0, 100);
                //     }
                //     if (navigator.cube.id == toio_dict[num_press])
                //     {
                //         navigator.handle.Move(-50, 0, 100);
                //     }

                //     if(OnSlope_flag)
                //     {
                //         phase += 1;
                //         Debug.Log("phase0");
                //     }
                // } 
                // else if (phase == 1)
                // {
                //     if (navigator.cube.id == toio_dict[num_cube])
                //     {
                //         //角度270度を維持しながらpos_flat付近に到着するまでMove(-10, 0, 10)とRotate2Deg(270, rotateTime: 1000, tolerance: 0.1)を繰り返す
                //         while (navigator.cube.x < pos_flat.x - 10 || navigator.cube.x > pos_flat.x + 10 || navigator.cube.y < pos_flat.y - 10 || navigator.cube.y > pos_flat.y + 10)
                //         {
                //             navigator.handle.Move(-30, 0, 10);
                //         }
                //     }

                //     phase += 1;
                //     Debug.Log("phase1");
                // }
            }
        }

        string text = "";
           
        foreach (var cube in cm.syncCubes)
        {
            if (cube.id == toio_dict[num_cube]) text += "Cube" + num_cube + ": ";
            else if (cube.id == toio_dict[num_press]) text += "Cube" + num_press + ": ";
            else if (cube.id == toio_dict[num_slope]) text += "Cube" + num_slope + ": ";

            text += "(" + cube.x + "," + cube.y + "," + cube.angle + ")\n";
        }
        if (text != "") this.label.text = text;
        
    }

    Vector2 CalculateNewPosition(Vector2 pos, int angle, int distance)
    {
        float angleRadians = angle * Mathf.Deg2Rad;
        float x = pos.x + distance * Mathf.Cos(angleRadians);
        float y = pos.y + distance * Mathf.Sin(angleRadians);

        return new Vector2((int)x, (int)y);
    }

    private void OnSlopeDetector(Cube cube)
    {
        OnSlope_flag = true;
    }
}
