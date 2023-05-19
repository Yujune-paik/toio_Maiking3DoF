using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

public class Slope : MonoBehaviour
{  
    public Text label;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Real;

    int phase = 0;
    int check = 0;

    Vector2 pos_slope = new Vector2(0, 0);
    Vector2 pos_cube = new Vector2(0, 0);
    Vector2 pos_press = new Vector2(0, 0);

    int angle_slope = 0;

    int L_cube = 70, L_press = 130;

    public int connectNum = 3;

    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

    int num_cube = 1;
    int num_slope = 0;
    int num_press = 2;

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
                if (check == 0)
                {
                    if (navigator.cube.id == toio_dict[num_slope] && navigator.cube.x != 0 && navigator.cube.y != 0)
                    {
                        pos_slope = new Vector2(navigator.cube.x, navigator.cube.y);
                        angle_slope = navigator.cube.angle;
                        check += 1;
                        pos_cube = CalculateNewPosition(pos_slope, angle_slope + 90, L_cube);
                        pos_press = CalculateNewPosition(pos_slope, angle_slope + 90, L_press);
                        Debug.Log("pos_cube: " + pos_cube.x + ", " + pos_cube.y);
                        Debug.Log("pos_press: " + pos_press.x + ", " + pos_press.y);
                    }
                }
                else
                {
                    if (phase == 0)
                    {
                        if (navigator.cube.id == toio_dict[num_cube])
                        {
                            var mv = navigator.Navi2Target(pos_cube.x, pos_cube.y, maxSpd: 50,tolerance: 10).Exec();
                            if (mv.reached) phase += 1;
                            Debug.Log("phase0");
                        }
                    }
                    if (phase == 1)
                    {
                        if (navigator.cube.id == toio_dict[num_press])
                        {
                            var mv = navigator.Navi2Target(pos_press.x, pos_press.y, maxSpd: 50, tolerance: 10).Exec();
                            if (mv.reached) phase += 1;
                            Debug.Log("phase1");
                        }
                    }
                    else if (phase == 2)
                    {
                        if (navigator.cube.id == toio_dict[num_cube])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(angle_slope+90).Exec();
                            if (mv.reached) phase += 1;
                            Debug.Log("phase2");
                        }
                    }
                    else if (phase == 3)
                    {
                        if (navigator.cube.id == toio_dict[num_press])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(angle_slope+90).Exec();
                            if (mv.reached) phase += 1;
                            Debug.Log("phase3");
                        }
                    }
                    else if (phase == 4)
                    {
                        if (navigator.cube.id == toio_dict[num_cube])
                        {
                            navigator.cube.Move(-50, -50, 100);
                        }
                        if (navigator.cube.id == toio_dict[num_press])
                        {
                            navigator.cube.Move(-100, -100, 100);
                        }
                    } 
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
    }

    Vector2 CalculateNewPosition(Vector2 pos, int angle, int distance)
    {
        float angleRadians = angle * Mathf.Deg2Rad;
        float x = pos.x + distance * Mathf.Cos(angleRadians);
        float y = pos.y + distance * Mathf.Sin(angleRadians);

        return new Vector2((int)x, (int)y);
    }
}
