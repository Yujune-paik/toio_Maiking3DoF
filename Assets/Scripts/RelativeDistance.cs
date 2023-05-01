using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// ***************************
// 相対距離50によるキューブの移動 → 相対角度90度によるキューブの回転 をするプログラム
// 相対距離:= 現在の位置から目的地までの距離
// ***************************

public class RelativeDistance : MonoBehaviour
{
    public Text label;

    CubeManager cm; // キューブマネージャ
    public ConnectType connectType; // 接続種別

    float elapsedTime = 0.0f; // 経過時間
    int phase = 0; // フェーズ

    // キューブ複数台接続
    public int connectNum = 3; // 接続数

    async void Start()
    {
        // キューブの接続
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);
    }

    void Update()
    {
       // cm.handlesにアクセスする前に、接続しているtoioの個数が2台以上かどうかをチェック
        if (cm.handles.Count < 2) return;

        // 1秒毎に命令実行
        elapsedTime += Time.deltaTime;
        if (1.0f > elapsedTime) return;
        elapsedTime = 0.0f;

        // 非同期の更新
        cm.handles[0].Update();
        cm.handles[1].Update();

        // 相対距離50によるキューブの移動
        if (phase == 0)
        {
            // dist: 相対距離
            // translate: 移動速度
            cm.handles[0].TranslateByDist(dist:50, translate:40).Exec();
            cm.handles[1].TranslateByDist(dist:50, translate:80).Exec();
        }
        // 相対角度90度によるキューブの回転
        else if (phase == 1)
        {
            // ddeg: 相対角度
            // rotate: 回転速度
            cm.handles[0].RotateByDeg(ddeg:90, rotate:40).Exec();
            cm.handles[1].RotateByRad(drad:Mathf.PI/2, rotate:80).Exec();
        }
        phase += 1;

        string text = "";
        foreach (var handle in cm.syncHandles){
            text += "(" + handle.cube.x + "," + handle.cube.y + "," + handle.cube.angle + ")\n";
        }
        if (text != "") this.label.text = text;

    }
}

