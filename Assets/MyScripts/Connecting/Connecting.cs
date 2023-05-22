using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class Connecting : MonoBehaviour
{
    // public Text label;

    CubeManager cm;
    public ConnectType connectType;

    int phase = 0;

    Vector2 pos_cube_left = new Vector2(0, 0);
    Vector2 pos_pre_cube_right = new Vector2(0, 0); // one cube behind final position
    Vector2 pos_cube_right = new Vector2(0, 0); // final position

    int angle_left = 0;

    int L = 30; // cube's size

    public int connectNum = 2;
    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

    int Cube_left=1;
    int Cube_right=0;

    async void Start()
    {
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);
        

        using (var sr = new StreamReader("Assets/toio_number.csv"))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var values = line.Split(',');
                toio_dict.Add(int.Parse(values[0]), values[1]);
            }
        }
    }

    void Update()
    {
        if (cm.synced)
        {
            foreach (var navigator in cm.syncNavigators)
            {
                if (navigator.cube.id == toio_dict[Cube_left] && navigator.cube.x != 0 && navigator.cube.y != 0)
                {
                    pos_cube_left = new Vector2(navigator.cube.x, navigator.cube.y);
                    angle_left = navigator.cube.angle;
                    pos_pre_cube_right = CalculateNewPosition(pos_cube_left, angle_left + 135, L); // one cube behind final position
                    pos_cube_right = CalculateNewPosition(pos_cube_left, angle_left + 90, L); // final position
                    Debug.Log("pos_cube_right: " + pos_cube_right.x + ", " + pos_cube_right.y);
                }
                else
                {
                    if (phase == 0)
                    {
                        if (navigator.cube.id == toio_dict[Cube_right])
                        {
                            var mv = navigator.Navi2Target(pos_pre_cube_right.x, pos_pre_cube_right.y, maxSpd: 50).Exec();
                            if (mv.reached) phase += 1;
                            Debug.Log("phase0");
                        }
                    }
                    else if (phase == 1)
                    {
                        if (navigator.cube.id == toio_dict[Cube_right])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(angle_left).Exec();
                            if (mv.reached) phase += 1;
                            Debug.Log("phase1");
                        }
                    }
                    else if (phase == 2)
                    {
                        if (navigator.cube.id == toio_dict[Cube_right])
                        {
                            // ここをcube.TargetMove()を用いて、pos_cube_rightに移動させる
                            navigator.cube.TargetMove((int)pos_cube_right.x, (int)pos_cube_right.y, 340, angle_left, maxSpd: 50);
                            if(navigator.cube.pos == pos_cube_right) phase += 1;
                            Debug.Log("phase2");
                        }
                    }
                }
            }

            // string text = "";
            // foreach (var cube in cm.syncCubes)
            // {
            //     if (cube.id == toio_dict[1]) text += "Cube_left: ";
            //     else if (cube.id == toio_dict[2]) text += "Cube_right: ";

            //     text += "(" + cube.x + "," + cube.y + "," + cube.angle + ")\n";
            // }
            // if (text != "") this.label.text = text;
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
