using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class DetectNextToio : MonoBehaviour
{
    public Text label;
    public ConnectType connectType;
    CubeManager cm;

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
        await cm.MultiConnect(toio_dict.Count);
    }

    void Update()
    {
        if (cm.synced)
        {
            // select reference toio (0 is the id of reference toio)
            Cube refCube = null;
            foreach (var cube in cm.syncCubes)
            {
                if (cube.id == toio_dict[0])
                {
                    refCube = cube;
                    break;
                }
            }

            if(refCube == null) return;

            string labelText = "";

            foreach(var cube in cm.syncCubes)
            {
                if(cube.id == refCube.id) continue;

                float dist = Vector2.Distance(new Vector2(refCube.x, refCube.y), new Vector2(cube.x, cube.y));
                if(dist <= 30)
                {
                    int angle = CalculateAngle(refCube, cube);
                    labelText += $"toio[{GetCubeId(cube.id)}]が{angle}°の位置でくっついているよー\n";
                }
            }

            this.label.text = labelText;
        }
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
