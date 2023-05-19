using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class ConnectingSimulator : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType;

    int phase = 0;

    Vector2 pos_slope = new Vector2(0, 0);
    Vector2 pos_cube = new Vector2(0, 0);

    int angle_slope = 0;

    int L_cube=50;

    public int connectNum = 2;

    async void Start()
    {
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);
    }

    void Update()
    {
        if (cm.synced){
            foreach(var navigator in cm.syncNavigators){
                if(navigator.cube.localName == "cube_right" && navigator.cube.x != 0 && navigator.cube.y != 0){
                    pos_slope = new Vector2(navigator.cube.x, navigator.cube.y);
                    angle_slope = navigator.cube.angle;
                    pos_cube = CalculateNewPosition(pos_slope, angle_slope+90, L_cube);
                    Debug.Log("pos_cube: " + pos_cube.x + ", " + pos_cube.y);
                }
                else{
                    if(phase == 0){
                        if(navigator.cube.localName == "cube_left"){
                            var mv = navigator.Navi2Target(pos_cube.x, pos_cube.y, maxSpd:50).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase0");
                        }
                    }
                    else if(phase == 1){
                        if(navigator.cube.localName == "cube_left"){
                            Movement mv = navigator.handle.Rotate2Deg(angle_slope).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase1");
                        }
                    }
                }
            }

            string text = "";
            foreach (var cube in cm.syncCubes){
                if(cube.localName == "cube_left") text += "cube_left: ";
                else if(cube.localName == "cube_right") text += "cube_right: ";

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
