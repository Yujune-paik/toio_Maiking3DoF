using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// ******************************************
// toioの上で下のtoioの中心へ移動するプログラム
// *****************************************

public class ToCenter : MonoBehaviour
{
    public Text label;
    public ConnectType connectType;
    CubeManager cm;
    Cube cube;

    float elapsedTime = 0.0f; // 経過時間

    int L = 30; // cube's size
    int phase = 0; // phase of movement
    
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

        text += "Position:( "+cube.x+", "+cube.y+")\n";
        text += "Angle:" + cube.angle+" deg";
        
        
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

        // if (Input.GetKey(KeyCode.LeftArrow)) {
        //     cube.Move(-20, 20, 50);
        // } else if (Input.GetKey(KeyCode.RightArrow)) {
        //     cube.Move(20, -20, 50);
        // } else if (Input.GetKey(KeyCode.UpArrow)) {
        //     cube.Move(30, 30, 50);
        // } else if (Input.GetKey(KeyCode.DownArrow)) {
        //     cube.Move(-30, -30, 50);
        // }
        
        // 上のtoioがtoio_dict[0]
        // 下のtoioがtoio_dict[1]
        // 上のtoioが下のtoioの中心へ移動する
        // その後、次のtoioへ移動する
        foreach(var handle in cm.syncHandles){
            if(phase == 0){
                // 上のtoioが次の下のtoioの向きへ回転する
                var mv = handle.Rotate2Deg(180,tolerance:3).Exec();
                if(mv.reached) phase += 1;
                Debug.Log("phase0");
            }
            else if(phase == 1){
                // 上のtoioが下のtoioの中心へ移動する
                var mv = handle.Move2Target(128,504,tolerance:7).Exec();
                if(mv.reached) phase += 1;
                Debug.Log("phase1");
            }else if(phase == 2){
                // 次のtoioの中心へLの長さ移動する
                // 1秒毎に命令実行
                elapsedTime += Time.deltaTime;
                if (1.0f > elapsedTime) return;
                elapsedTime = 0.0f;

                // dist: 相対距離
                // translate: 移動速度
                handle.TranslateByDist(dist:L, translate:40).Exec();

                phase += 1;
                Debug.Log("phase2");
            }
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
