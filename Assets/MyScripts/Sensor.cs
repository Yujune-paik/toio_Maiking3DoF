using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// ****************************
// toio 1台のセンサーの値を取得するプログラム
// ****************************

public class Sensor : MonoBehaviour
{
    public Text label;
    public ConnectType connectType;
    CubeManager cm;
    Cube cube;

    public int connectNum = 8;

    async void Start()
    {
        cm = new CubeManager(connectType);
        cube = await cm.SingleConnect();

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

        foreach(var cube in cm.syncCubes)
        {
            text += "Position:( "+cube.x+", "+cube.y+")\n";
            text += "Angle:" + cube.angle+" deg";
        }
        
        if(text != "")
        {
            this.label.text = text;
        }

        Debug.ClearDeveloperConsole();
        
        if(cube.isGrounded)
        {
            Debug.Log("接地している");
        }
        else
        {
            Debug.Log("接地していない");
        }
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
