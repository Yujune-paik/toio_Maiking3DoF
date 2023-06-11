using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

// 1層目を作るプログラム
// 接続するtoio： 0, 1, 2, 3

public class FirstFloor : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Real;

    int phase = 0;
    int check = 0;

    bool isCoroutineRunning = false;

    Vector2 PosCubeLeft = new Vector2(0, 0);
    Vector2 PosCubeRight = new Vector2(0, 0);

    int AngleCubeLeft = 0;

    int L = 50;

    int connectNum = 2;

    // 0と1がくっつく
    int FirstConnectionLeft = 1; // くっつかれるほう
    int FirstConnectionRight = 0; // くっつきに行くほう

    // 1と2がくっつく
    int SecondConnectionLeft = 1; // くっつかれるほう
    int SecondConnectionRight = 2; // くっつきに行くほう

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>(); // Cubeの番号とIDの対応付け
    Dictionary<int, Vector2> toio_pos = new Dictionary<int, Vector2>(); // Cubeの番号と座標の対応付け
    
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
        while (cm.syncCubes.Count < connectNum)
        {
            await cm.MultiConnect(connectNum);
        }
        // 接続台数をコンソールに表示する
        Debug.Log(cm.syncCubes.Count);
    }

    void Update()
    {
        if (cm.synced)
        {
            foreach(var navigator in cm.syncNavigators)
            {
                // toio_dict[0](構成要素)とtoio_dict[1](足場)をくっつける
                if(check == 0)
                {
                    if(navigator.cube.id == toio_dict[FirstConnectionLeft] && navigator.cube.x != 0 && navigator.cube.y != 0)
                    {
                        PosCubeLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                        AngleCubeLeft = navigator.cube.angle;
                        check += 1;
                        PosCubeRight = CalculateNewPosition(PosCubeLeft, AngleCubeLeft+90, L);
                        Debug.Log("PosCubeRight: " + PosCubeRight.x + ", " + PosCubeRight.y);
                    }
                }
                else if(check == 1)
                {
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[FirstConnectionRight])
                        {
                            var mv = navigator.Navi2Target(PosCubeRight.x, PosCubeRight.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase0");
                            }
                        }
                    }
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[FirstConnectionRight])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(AngleCubeLeft+270, rotateTime:2500, tolerance:0.1).Exec();
                            
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[FirstConnectionRight])
                        {
                            navigator.handle.Move(15, 0, 100);
                        }

                        if (!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(1.2f));
                            Debug.Log("phase2");

                            check += 1;
                            phase = 0;
                        }
                    }
                }

                // toio_dict[1](足場)とtoio_dict[2](構成要素)をくっつける
                else if(check == 2)
                {
                    if(navigator.cube.id == toio_dict[SecondConnectionLeft] && navigator.cube.x != 0 && navigator.cube.y != 0)
                    {
                        PosCubeLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                        AngleCubeLeft = navigator.cube.angle;
                        check += 1;
                        PosCubeRight = CalculateNewPosition(PosCubeLeft, AngleCubeLeft-90, L);
                        Debug.Log("PosCubeRight: " + PosCubeRight.x + ", " + PosCubeRight.y);
                    }
                }
                else if(check == 3)
                {
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[SecondConnectionRight])
                        {
                            var mv = navigator.Navi2Target(PosCubeRight.x, PosCubeRight.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase0");
                            }
                        }
                    }
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[SecondConnectionRight])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(AngleCubeLeft+270, rotateTime:2500, tolerance:0.1).Exec();
                            
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[SecondConnectionRight])
                        {
                            navigator.handle.Move(15, 0, 100);
                        }

                        if (!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(1.2f));
                            Debug.Log("phase2");
                        }
                    }

                    check += 1;
                }
    
                // 1段目の真ん中のtoioを抜く
                // 矢印キー(上)を押されたら3秒前進する
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    if(navigator.cube.id == toio_dict[1])
                    {
                        navigator.handle.Move(50, 0, 1500);
                    }
                }
            }

            string text = "";
            foreach (var cube in cm.syncCubes)
            {
                if(cube.id == toio_dict[FirstConnectionLeft]) text += "toio_dict[" + FirstConnectionLeft +"]：(" + cube.x + "," + cube.y + "," + cube.angle + ")\n";
                else if(cube.id == toio_dict[FirstConnectionRight]) text += "toio_dict[" + FirstConnectionRight + "]：(" + cube.x + "," + cube.y + "," + cube.angle + ")\n";

                // cube.posとPosCubeLeftの距離を計算する
                if(cube.id == toio_dict[FirstConnectionRight]) text += "Distance：" + Vector2.Distance(cube.pos, PosCubeLeft) + "\n";
            }
            if (text != "") this.label.text = text;
        }
    }

    // 新しい座標を計算する
    Vector2 CalculateNewPosition(Vector2 pos, int angle, int distance)
    {
        float angleRadians = angle * Mathf.Deg2Rad;
        float x = pos.x + distance * Mathf.Cos(angleRadians);
        float y = pos.y + distance * Mathf.Sin(angleRadians);

        return new Vector2((int)x, (int)y);
    }

    // 一定時間ごとにphaseをインクリメントする
    // 使用するときは，StartCoroutine(WaitAndIncrementPhase(秒数(s)f));
    IEnumerator WaitAndIncrementPhase(float waitTime)
    {
        isCoroutineRunning = true;
        yield return new WaitForSeconds(waitTime);
        phase++;
        isCoroutineRunning = false;
    }
}
