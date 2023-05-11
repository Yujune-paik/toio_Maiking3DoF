using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class StrippingSimulator : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType;

    int phase = 0;
    int check = 0;

    Vector2 pos_cube1 = new Vector2(0, 0);
    Vector2 pos_cube0 = new Vector2(0, 0);
    Vector2 pos_cube2 = new Vector2(0, 0);

    int angle_cube1 = 0;

    int L = 50;

    public int connectNum = 3;

    async void Start()
    {
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);
    }

    // 後退開始時刻を保存する変数
    float reverseStartTimeCube0 = 0f;
    float reverseStartTimeCube2 = 0f;

    void Update()
    {
        if (cm.synced){
            foreach(var navigator in cm.syncNavigators){
                if(check == 0){
                    if(navigator.cube.localName == "Cube1" && navigator.cube.x != 0 && navigator.cube.y != 0){
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
                        if(navigator.cube.localName == "Cube0"){
                            var mv = navigator.Navi2Target(pos_cube0.x, pos_cube0.y, maxSpd:50).Exec();
                            if(mv.reached) {
                                phase += 1;
                                reverseStartTimeCube0 = Time.time;
                            }
                            Debug.Log("phase0");
                        }
                    }
                    if(phase == 1){
                        if(navigator.cube.localName == "Cube2"){
                            var mv = navigator.Navi2Target(pos_cube2.x, pos_cube2.y, maxSpd:50).Exec();
                            if(mv.reached) {
                                phase += 1;
                                reverseStartTimeCube2 = Time.time;
                            }
                            Debug.Log("phase1");
                        }
                    }
                    else if(phase == 2){
                        if(navigator.cube.localName == "Cube0"){
                            if (Time.time - reverseStartTimeCube0 >= 10){
                                Movement mv = navigator.handle.Rotate2Deg(angle_cube1+180).Exec();
                                if(mv.reached) phase += 1;
                                Debug.Log("phase2");
                            } else {
                                navigator.cube.TargetMove(pos_cube0.x, pos_cube0.y, angle_cube1, timeOut: 100, maxSpd: 50, targetRotationType: TargetRotationType.AbsoluteClockwise);
                            }
                        }
                    }
                    else if(phase == 3){
                        if(navigator.cube.localName == "Cube2"){
                            if (Time.time - reverseStartTimeCube2 >= 10){
                                Movement mv = navigator.handle.Rotate2Deg(angle_cube1).Exec();
if(mv.reached) phase += 1;
Debug.Log("phase3");
} else {
navigator.cube.TargetMove(pos_cube2.x, pos_cube2.y, angle_cube1, timeOut: 100, maxSpd: 50, targetRotationType: TargetRotationType.AbsoluteClockwise);
}
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
