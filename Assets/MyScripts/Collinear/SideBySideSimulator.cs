using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// *********************************************************
// 実機で3台のtoioを横一列に並べるプログラム(simulator version)
// *********************************************************

public class SideBySideSimulator : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType;

    int phase = 0;
    int check = 0;

    Vector2 pos_slope = new Vector2(0, 0);
    Vector2 pos_cube = new Vector2(0, 0);
    Vector2 pos_press = new Vector2(0, 0);

    int angle_slope = 0;

    int L_cube=50, L_press=100;

    public int connectNum = 3;
    
    async void Start()
    {
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);
    }

    void Update()
    {
        if (cm.synced){
            foreach(var navigator in cm.syncNavigators){
                if(check == 0){
                    if(navigator.cube.localName == "Cube2" && navigator.cube.x != 0 && navigator.cube.y != 0){
                        pos_slope = new Vector2(navigator.cube.x, navigator.cube.y);
                        angle_slope = navigator.cube.angle;
                        check += 1;
                        pos_cube = CalculateNewPosition(pos_slope, angle_slope+90, L_cube);
                        pos_press = CalculateNewPosition(pos_slope, angle_slope+90, L_press);
                        Debug.Log("pos_cube: " + pos_cube.x + ", " + pos_cube.y);
                        Debug.Log("pos_press: " + pos_press.x + ", " + pos_press.y);
                    }
                }
                else{
                    if(phase == 0){
                        if(navigator.cube.localName == "Cube0"){
                            var mv = navigator.Navi2Target(pos_cube.x, pos_cube.y, maxSpd:50).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase0");
                        }
                    }
                    if(phase == 1){
                        if(navigator.cube.localName == "Cube1"){
                            var mv = navigator.Navi2Target(pos_press.x, pos_press.y, maxSpd:50).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase1");
                        }
                    }
                    else if(phase == 2){
                        if(navigator.cube.localName == "Cube0"){
                            Movement mv = navigator.handle.Rotate2Deg(angle_slope).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase2");
                        }
                    }
                    else if(phase == 3){
                        if(navigator.cube.localName == "Cube1"){
                            Movement mv = navigator.handle.Rotate2Deg(angle_slope).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase3");
                        }
                    }
                }
            }

            string text = "";
            foreach (var cube in cm.syncCubes){
                if(cube.localName == "Cube0") text += "Cube0: ";
                else if(cube.localName == "Cube1") text += "Cube1: ";
                else if(cube.localName == "Cube2") text += "Cube2: ";

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
