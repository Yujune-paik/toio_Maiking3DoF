using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class CollinearSimulator : MonoBehaviour
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

    public int L_cube=10, L_press=60;

    public int connectNum = 3;

    async void Start()
    {
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);
    }

    void Update()
    {
        foreach(var handle in cm.syncHandles){
            if(check == 0){
                if(handle.cube.localName == "Cube2"){
                    pos_slope = new Vector2(handle.cube.x, handle.cube.y);
                    angle_slope = handle.cube.angle;
                    check += 1;
                    pos_cube = CalculateNewPosition(pos_slope, angle_slope, L_cube);
                    pos_press = CalculateNewPosition(pos_slope, angle_slope, L_press);
                    Debug.Log("pos_cube: " + pos_cube.x + ", " + pos_cube.y);
                    Debug.Log("pos_press: " + pos_press.x + ", " + pos_press.y);
                }
            }else{
                if(phase == 0){
                    if(handle.cube.localName == "Cube0"){
                        Movement mv = handle.Move2Target(pos_cube.x, pos_cube.y).Exec();
                        if(mv.reached) phase += 1;
                        Debug.Log("phase0");
                    }
                }
                else if(phase == 1){
                    if(handle.cube.localName == "Cube1"){
                        Movement mv = handle.Move2Target(pos_press.x, pos_press.y).Exec();
                        if(mv.reached) phase += 1;
                        Debug.Log("phase1");
                    }
                }
                else if(phase == 2){
                    if(handle.cube.localName == "Cube0"){
                        Movement mv = handle.Rotate2Deg(angle_slope).Exec();
                        if(mv.reached) phase += 1;
                        Debug.Log("phase2");
                    }
                }
                else if(phase == 3){
                    if(handle.cube.localName == "Cube1"){
                        Movement mv = handle.Rotate2Deg(angle_slope).Exec();
                        if(mv.reached) phase += 1;
                        Debug.Log("phase3");
                    }
                }
            }
        }

        string text = "";
        foreach (var handle in cm.syncHandles){
            text += "(" + handle.cube.x + "," + handle.cube.y + "," + handle.cube.angle + ")\n";
        }
        if (text != "") this.label.text = text;
    }

    Vector2 CalculateNewPosition(Vector2 pos_slope, int angle_slope, int distance)
{
    float angleRadians = angle_slope * Mathf.Deg2Rad;
    float x = pos_slope.x + distance * Mathf.Cos(angleRadians);
    float y = pos_slope.y + distance * Mathf.Sin(angleRadians);

    return new Vector2((int)x, (int)y);
}
}
