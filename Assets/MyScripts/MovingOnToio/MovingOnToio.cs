using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

// *************************************
// 上のtoioが隣のtoioへ移動するプログラム
// *************************************

public class MovingOnToio : MonoBehaviour
{
    public Text label;
    public ConnectType connectType;
    CubeManager cm;
    Cube cube;

    // Cubeの接続台数
    int cube_num = 5;

    // 上に乗っているtoioの番号
    int onToioNum = 0;

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
        await cm.MultiConnect(cube_num);
    }

    void Update()
    {
        //以下のステップ1~6を、GetToioOnTopText()とCalculateAngle()とGetCubeId()を適宜用いて、実装してください。
        // ステップ1: toioキューブの位置設定 - 初期設定として各toioキューブの位置情報を保持するためのデータ構造を設定します。キューブの番号をキーとし、座標を値とするようなディクショナリ（onToio_pos）を作成します。


        // ステップ2: キューブ位置情報の取得と追跡 - WhereIsToio.csスクリプトを使用して、各キューブの位置を定期的に取得し、その位置情報を更新します。ここでcm.syncCubesの各キューブについてループを回し、座標情報を取得します。

        // ステップ3: 近接キューブの検出 - 取得したキューブの位置情報を初期設定のonToio_posと比較します。あるキューブが特定のtoio_posの位置に近接している（ここで「近接」は、x座標とy座標の差が10未満であることを意味します）場合、そのキューブは該当のonToio_posのキューブの上にあると判断します。

        // ステップ4: onToio_posの回転 - 上記の情報を元に、under_toio0に対するunder_toio1の角度までtop_toioを回転させます。これは、top_toioが正確にunder_toio1に向かって移動できるようにするためです。

        // ステップ5: onToio_posの移動 - onToio_posをunder_toio0からunder_toio1へ移動させます。この操作はonToio_posの上にある物体を適切に移動させるために必要です。

        // ステップ6: 結果の表示 - 最後に、onToio_posがunder_toio1に移動したことを確認し、その結果をUIに表示します。これはユーザーに現在のキューブの位置と状態を視覚的に理解してもらうためです。



        // // 上に乗っているtoioの番号をLabelに表示
        // string labelText = GetToioOnTopText();
        // this.label.text = labelText;
    }

    // 上に乗っているtoioの真下のtoioの番号を更新する関数
    string GetToioOnTopText()
    {
        string labelText = "";

        foreach(var cube in cm.syncCubes)
        {   
            if(cube.id == toio_dict[onToioNum]){
                foreach(var pos in toio_pos)
                {
                    if(Mathf.Abs(cube.x - pos.Value.x) < 10 && Mathf.Abs(cube.y - pos.Value.y) < 10)
                    {
                        labelText = "toio["+ pos.Key +"]の上にあるよ";
                    }
                }
            }
        }

        return labelText;
    }

    // あるtoioに対して、もう一方のtoioがどの角度にくっついているか計算する関数
    int CalculateAngle(Cube refCube, Cube targetCube)
    {
        int dx = targetCube.x - refCube.x;
        int dy = targetCube.y - refCube.y;
        int angle = (int)(Mathf.Atan2(dy, dx) * Mathf.Rad2Deg) - refCube.angle;
        angle = (angle + 360) % 360;

        return (angle / 90) * 90; // 0°、90°、180°、270°、360°(=0°)に近似する
    }

    // 「cebe.id → Cubeの番号」 へ変換する関数
    int GetCubeId(string cubeId)
    {
        foreach(var item in toio_dict)
        {
            if(item.Value == cubeId) return item.Key;
        }
        return -1;
    }
}
