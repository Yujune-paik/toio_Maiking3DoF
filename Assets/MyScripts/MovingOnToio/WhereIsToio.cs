using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// ********************************************
// toioがどのtoioの上にいるのか検知するプログラム
// ********************************************

public class WhereIsToio : MonoBehaviour
{
    public Text label;
    public ConnectType connectType;
    CubeManager cm;
    Cube cube;

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>(); // Cubeの番号とIDの対応付け
    Dictionary<int, Vector2> toio_pos = new Dictionary<int, Vector2>(); // Cubeの番号と座標の対応付け

    async void Start()
    {
        // CubeのIDと名前の対応付け
        using (var sr = new StreamReader("Assets/toio_number.csv"))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var values = line.Split(',');
                toio_dict.Add(int.Parse(values[0]), values[1]); // Cubeの番号とIDの対応付け
                toio_pos.Add(int.Parse(values[0]), new Vector2(int.Parse(values[2]), int.Parse(values[3]))); // Cubeの番号と座標の対応付け
            }
        }

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

        text += "Position:( "+cube.x+", "+cube.y+")\n";
        text += "Angle:" + cube.angle+" deg";
        
        
        if(text != "")
        {
            this.label.text = text;
        }

        string labelText = "";

        // toioの位置が、toio_posのどこかに近づいたら、そのtoioの番号を表示する
        foreach(var cube in cm.syncCubes)
        {
            foreach(var pos in toio_pos)
            {
                if(Mathf.Abs(cube.x - pos.Value.x) < 10 && Mathf.Abs(cube.y - pos.Value.y) < 10)
                {
                    labelText = "toio["+ pos.Key +"]の上にあるよ";
                }
            }
        }

        this.label.text = labelText;
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
