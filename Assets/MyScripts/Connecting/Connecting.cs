using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class Connecting : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Real;

    int phase = 0;
    int check = 0;

    Vector2 pos_slope = new Vector2(0, 0);
    Vector2 pos_cube = new Vector2(0, 0);

    int angle_slope = 0;

    int L_cube=50;

    public int connectNum = 2;

    int Cube_right=1;
    int Cube_left=0;

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
        await cm.MultiConnect(connectNum);
    }

    void Update()
    {
        if (cm.synced){
            foreach(var navigator in cm.syncNavigators){
                if(check == 0){
                    if(navigator.cube.id == toio_dict[Cube_left] && navigator.cube.x != 0 && navigator.cube.y != 0){
                        pos_slope = new Vector2(navigator.cube.x, navigator.cube.y);
                        angle_slope = navigator.cube.angle;
                        check += 1;
                        pos_cube = CalculateNewPosition(pos_slope, angle_slope, L_cube);
                        Debug.Log("pos_cube: " + pos_cube.x + ", " + pos_cube.y);
                    }
                }
                else{
                    if(phase == 0){
                        if(navigator.cube.id == toio_dict[Cube_right]){
                            var mv = navigator.Navi2Target(pos_cube.x, pos_cube.y, maxSpd:20, rotateTime:1000,tolerance:10).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase0");
                        }
                    }
                    else if(phase == 1){
                        if(navigator.cube.id == toio_dict[Cube_right]){
                            Movement mv = navigator.handle.Rotate2Deg(angle_slope, rotateTime:2500, tolerance:0.05).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase1");
                        }
                    }
                    else if(phase == 2){
                        if(navigator.cube.id == toio_dict[Cube_right]){
                            navigator.handle.Move(-15, 0, 1000);
                            phase += 1;
                            Debug.Log("phase2");
                        }
                        // Debug.Log("phase2 : Cube_Angle: " + navigator.cube.angle);
                    }
                }
            }

            string text = "";
            foreach (var cube in cm.syncCubes){
                if(cube.id == toio_dict[2]) text += "Cube_right: ";
                else if(cube.id == toio_dict[1]) text += "Cube_left: ";

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
