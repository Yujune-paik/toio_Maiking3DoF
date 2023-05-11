using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// **************************************************************************************
// toioに坂を登らせるプログラム(simulator version)
// Simulatorのため、3次元的な上り下りはできないため、本プログラムでは、押し出すSimulationをする
// **************************************************************************************

public class SlopeSimulator : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType;

    int phase = 0;
    int check = 0;

    Vector2 pos_slope = new Vector2(0, 0); // slopeの位置
    Vector2 pos_cube = new Vector2(0, 0); // cubeの位置
    Vector2 pos_press = new Vector2(0, 0); // pressの位置

    Vector2 pos_target = new Vector2(0, 0); // cubeがslopeに上る直前の位置

    int angle_slope = 0;

    int L_cube=60, L_press=100,L_target=50;

    public int connectNum = 3;

    async void Start()
    {
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);

        // それぞれのCubeの役割をコンソールに表示する
        Debug.Log("Cube0: Cube");
        Debug.Log("Cube1: Slope");
        Debug.Log("Cube2: Press");
    }

    void Update()
    {
        if (cm.synced){
            foreach(var navigator in cm.syncNavigators){
                
                // ***toioを一列に並べる(start)***
                if(check == 0){
                    if(navigator.cube.localName == "Cube2" && navigator.cube.x != 0 && navigator.cube.y != 0){
                        pos_slope = navigator.cube.pos;
                        angle_slope = navigator.cube.angle;
                        check += 1;
                        pos_cube = CalculateNewPosition(pos_slope, angle_slope, L_cube);
                        pos_press = CalculateNewPosition(pos_slope, angle_slope, L_press);
                        Debug.Log("pos_cube: " + pos_cube.x + ", " + pos_cube.y);
                        Debug.Log("pos_press: " + pos_press.x + ", " + pos_press.y);

                        pos_target = CalculateNewPosition(pos_slope, angle_slope, L_target);
                        Debug.Log("pos_target: " + pos_target.x + ", " + pos_target.y);
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
                // ***toioを一列に並べる(end)***

                
                    // ***CubeをSlopeの直前まで移動させ、坂を登り切らせる(start)***
                    else if(phase == 4){
                        if(navigator.cube.localName == "Cube0"){
                            var mv = navigator.Navi2Target(pos_target.x, pos_target.y, maxSpd:80).Exec();
                            if(mv.reached) phase += 1;
                            Debug.Log("phase4");
                        }
                    }
                    else if (phase == 5)
                    {
                        if (navigator.cube.localName == "Cube2")
                        {
                            var mv = navigator.Navi2Target(pos_cube.x, pos_cube.y, maxSpd: 50).Exec();
                            if (mv.reached) phase += 1;
                            Debug.Log("phase5");
                        }
                    }
                    else if (phase == 6)
                    {
                        if (navigator.cube.localName == "Cube1")
                        {
                            var mv = navigator.Navi2Target(pos_slope.x, pos_slope.y, maxSpd: 50).Exec();
                            if (mv.reached) phase += 1;
                            Debug.Log("phase6");
                        }
                    }

                }
            }

            foreach(var navigator in cm.syncNavigators){
                // CubeがSlopeの直前の位置へ移動する
                if(navigator.cube.localName == "Cube0"){
                    var mv0 = navigator.Navi2Target(pos_target.x, pos_target.y, maxSpd:80).Exec();
                    Debug.Log("phase4");
                }
                // PressはCubeを追いかける
                else if(navigator.cube.localName == "Cube2"){
                    var mv2 = navigator.Navi2Target(pos_cube.x, pos_cube.y, maxSpd:50).Exec();
                    Debug.Log("phase5");
                }


                //編集中
                // CubeがSlopeを登る(Cube&Pressはtoioマットに乗るまで後退する)
                else if(navigator.cube.localName == "Cube1"){
                    var mv1 = navigator.Navi2Target(pos_slope.x, pos_slope.y, maxSpd:50).Exec();
                    Debug.Log("phase6");
                }
            }
            // ***CubeをSlopeの直前まで移動させ、坂を登り切らせる(end)***

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
