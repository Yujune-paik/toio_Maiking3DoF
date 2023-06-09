using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks; 

// *************************************
// 上のtoioが隣のtoioへ移動するプログラム
// *************************************

public class MovingOnToio : MonoBehaviour
{
    public Text label;
    public ConnectType connectType;
    CubeManager cm;

    // Cubeの接続台数
    int connectingCubeNum = 8;

    // 上に乗っているtoioの番号
    int onToioNum = 0;

    // onToioの下にあるtoioの番号
    int underToioNum = -1;

    // underToio0の隣にあるtoioの番号
    int nextToioNum = -1;

    // onToioを移動させたいtoioの番号
    int targetToioNum = 1;

    // toioの移動距離
    int distance = 30;

    // onToioがnextToioへ移動する角度
    int angle_NextToio = 0;

    // 全体のStepの管理
    int step = 0;

    // onToioがnextToioへ移動する段階を管理
    int phase_Move2NextToio = 0;

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>(); // Cubeの番号とIDの対応付け
    Dictionary<int, Vector2> toio_pos = new Dictionary<int, Vector2>(); // Cubeの番号と座標の対応付け

    // underToioの隣にあるtoioの番号と角度を保存する辞書
    Dictionary<int, int> adjacentCubesInfo = new Dictionary<int, int>();

    async void Start()
    {
        // Cubeの番号とIDの対応付け, Cubeの番号と座標の対応付け
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
        // キューブの複数台接続
        await ConnectToioCubes();
    }

    async Task ConnectToioCubes()
    {
        int count = 0;
        while (cm.syncCubes.Count < connectingCubeNum)
        {
            await cm.MultiConnect(connectingCubeNum);
        }

        // 接続台数をコンソールに表示する
        Debug.Log("ConnectToioCubes():" + cm.syncCubes.Count + "台接続しました");
    }

    int f=0;

    void Update()
    {   
        // labelの初期化
        string labelText = "";

        // 接続が完了していない場合は何もしない
        if (cm.syncCubes.Count < connectingCubeNum)
            return;

        // foreach(var handle in cm.handles){
        //     if(f==0){
        //         handle.cube.Move(20,20,600);
        //         f=1;
        //     }
        // }

        foreach(var handle in cm.syncHandles)
        {   
            if(step == 0 && handle.cube.id == toio_dict[onToioNum])
            {
                // onToioの真下のtoioの番号(underToioNum)を更新
                underToioNum = GetUnderToio(handle.cube);
                if(underToioNum == -1) return;
                step ++;
                Debug.Log("step.1");
                Debug.Log("underToioNum: " + underToioNum);
            }
            else if(step == 1 && underToioNum != -1 && handle.cube.id == toio_dict[underToioNum])
            {
                // underToioの隣にあるtoioの番号(nextToioNum)と角度(angle_NextToio)を更新
                adjacentCubesInfo = GetAdjacentCubesInfo(handle.cube, cm.syncCubes);
                foreach (KeyValuePair<int, int> entry in adjacentCubesInfo)
                {
                    nextToioNum = entry.Key;
                    angle_NextToio = entry.Value;
                }
                angle_NextToio = 180;
                Debug.Log("nextToioNum: " + nextToioNum);
                Debug.Log("angle_NextToio: " + angle_NextToio);
                step ++;
                Debug.Log("step.2");
            }
            else if(step == 2 && nextToioNum != targetToioNum && handle.cube.id == toio_dict[onToioNum])
            {
                // onToioをangle_NextToioの角度に一致するように回転させる
                if(phase_Move2NextToio == 0)
                {
                    handle.Rotate2Deg(angle_NextToio,10).Exec();
                    phase_Move2NextToio += 1;
                    Debug.Log("onToioをangle_NextToioの角度に一致するように回転させた");
                }
                else if(phase_Move2NextToio == 1)
                {
                    // onToioをnextToioへ移動させる
                    handle.cube.Move(20,20,600);
                    phase_Move2NextToio += 1;
                    Debug.Log("onToioを相対距離30(=distance)移動させた");
                }
                step ++;
                Debug.Log("step.3");
            }
        }

        // GetUnderToio()の使い方
        // foreach(var cube in cm.syncCubes)
        // {
        //     // onToioの真下のtoioの番号を更新
        //     if(cube.id == toio_dict[onToioNum])
        //     {
        //         labelText += $"\ntoio[{onToioNum}]の真下にあるtoioはtoio[{GetUnderToio(cube)}]だよ";
        //     }
        // }

        // // underToio0の隣にあるtoioの番号と角度を更新
        // foreach(var cube in cm.syncCubes)
        // {
        //     if(cube.id == refCube.id) continue;

        //     float dist = Vector2.Distance(new Vector2(refCube.x, refCube.y), new Vector2(cube.x, cube.y));

        //     if(dist <= distance)
        //     {
        //         angle_NextToio = CalculateAngle(refCube, cube);
        //         underToioNum1 = GetCubeId(cube.id);
        //         labelText += $"toio[{GetCubeId(cube.id)}]が{angle}°の位置でくっついているよー\n";
        //     }
        // }

        // foreach(var handle in cm.syncHandles)
        // {
        //     if(handle.cube.id == toio_dict[onToioNum])
        //     {                   
        //         if(phase_Move2NextToio == 0)
        //         {
        //             // onToioをangle_NextToioの角度に一致するように回転させる
        //             Movement mv = handle.Rotate2Deg(angle_NextToio).Exec();
        //             if(mv.reach) phase_Move2NextToio += 1;
        //             Debug.Log("onToioをangle_NextToioの角度に一致するように回転させた");
        //         }
        //         else if(phase_Move2NextToio == 1)
        //         {
        //             // onToioを相対距離30(=distance)移動させる
        //             handle.TranslateByDist(dist: distance).Exec();
        //             phase_Move2NextToio += 1;
        //             Debug.Log("onToioを相対距離30(=distance)移動させた");
        //         }
        //     }
        // }



        // // GetAdjacentCubesInfo()の使い方
        // Cube refCube = null;
        // foreach (var cube in cm.syncCubes)
        // {
        //     if (cube.id == toio_dict[0])
        //     {
        //         refCube = cube;
        //         break;
        //     }
        // }

        // if(refCube == null) return;

        // Dictionary<int, int> adjacentCubesInfo = GetAdjacentCubesInfo(refCube, cm.syncCubes);
        
        // foreach (KeyValuePair<int, int> entry in adjacentCubesInfo)
        // {
        //     labelText += $"toio[{entry.Key}]が{entry.Value}°の位置でくっついているよー\n";
        // }

        // Display the number of the underToio1 on the UI.Text
        labelText += $"onToio: {onToioNum}\n";
        labelText += $"underToio: {underToioNum}\n";

        foreach (KeyValuePair<int, int> entry in adjacentCubesInfo)
        {
            labelText += $"toio[{entry.Key}]が{entry.Value}°の位置でくっついているよー\n";
        }
        this.label.text = labelText;
    }

    // 上に乗っているtoioの真下のtoioの番号を更新する関数
    int GetUnderToio(Cube onToio)
    {
        int underToioNum = -1;
        foreach(var pos in toio_pos)
        {
            if(Mathf.Abs(onToio.x - pos.Value.x) < 10 && Mathf.Abs(onToio.y - pos.Value.y) < 10)
            {
                underToioNum = pos.Key;
            }
        }

        return underToioNum;
    }

    // あるtoioに対して、もう一方のtoioがどの角度にくっついているか計算する関数
    int CalculateNextAngle(Cube refCube, Cube targetCube)
    {
        int dx = targetCube.x - refCube.x;
        int dy = targetCube.y - refCube.y;
        int angle = (int)(Mathf.Atan2(dy, dx) * Mathf.Rad2Deg) - refCube.angle;
        angle = (angle + 360) % 360;

        return (angle / 90) * 90; // 0°、90°、180°、270°、360°(=0°)に近似する
    }

    // 基準となるtoioにくっついているtoioの番号をキー、くっついている角度をバリューとした辞書を返す関数
    Dictionary<int, int> GetAdjacentCubesInfo(Cube refCube, List<Cube> syncCubes)
    {
        Dictionary<int, int> adjacentCubesInfo = new Dictionary<int, int>();

        foreach(var cube in syncCubes)
        {
            if(cube.id == refCube.id) continue;

            float dist = Vector2.Distance(new Vector2(refCube.x, refCube.y), new Vector2(cube.x, cube.y));
            if(dist <= 30)
            {
                int angle = CalculateNextAngle(refCube, cube);
                int cubeId = GetCubeId(cube.id);
                adjacentCubesInfo[cubeId] = angle;
            }
        }

        return adjacentCubesInfo;
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
