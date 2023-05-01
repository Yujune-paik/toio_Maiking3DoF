using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class Sensor : MonoBehaviour
{
    public Text label;
    public ConnectType connectType;
    CubeManager cubeManager;
    Cube cube;
    int line_angle=90; // 直線の角度
    int angle_diff=5; // 直線との角度の差

    public int connectNum = 8;

    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

    async void Start()
    {
        cubeManager = new CubeManager(connectType);
        cube = await cubeManager.SingleConnect();

        // コールバックの登録
        if (cube != null)
        {
            cube.slopeCallback.AddListener("EventScene", OnSlope);
            cube.doubleTapCallback.AddListener("EventScene", OnDoubleTap);
        }
    }

    void Update()
    {
        if (cube == null) return;

        string text = "";

        //cubeを直線に沿って動かす
        foreach(var cube in cubeManager.syncCubes)
        {
            // if (line_angle - angle_diff < cube.angle && cube.angle < line_angle + angle_diff)
            // {
            //     Debug.Log("後退");
                cube.Move(-50, -50, 50);
            // }
            // else if (cube.angle <= line_angle - angle_diff)
            // {
            //     Debug.Log("右回転");
            //     // cubeを右回転させる
            //     cube.Move(10, -10, 50);
            // }
            // else if (cube.angle >= line_angle + angle_diff)
            // {
            //     Debug.Log("左回転");
            //     // cubeを左回転させる
            //     cube.Move(-10, 10, 50);
            // }
        }

        foreach(var cube in cubeManager.syncCubes)
        {
            text += "Position:( "+cube.x+", "+cube.y+")\n";
            text += "Angle:" + cube.angle+" deg";
        }
        
        if(text != "")
        {
            this.label.text = text;
        }

        Debug.ClearDeveloperConsole();
    }

    private void OnSlope(Cube cube)
    {
        Debug.Log("傾いた");
    }

    private void OnDoubleTap(Cube cube)
    {
        Debug.Log("２回たたかれた");
    }
}
