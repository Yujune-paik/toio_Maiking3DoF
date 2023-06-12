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
    public InputField InputFieldX;
    public InputField InputFieldY;
    public InputField InputFieldAngle;
    public Button StartButton;

    bool StartClicked = false;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Simulator;

    int phase = 0;
    int check = 0;

    bool isCoroutineRunning = false;

    Vector2 PosCubeLeft = new Vector2(0, 0);
    Vector2 PosCubeRight = new Vector2(0, 0);

    int AngleCubeLeft = 0;

    int L = 50; // Cube同士の接続に用いる距離
    int L_Slope = 70; // Slopeの前にCubeが配置するときに用いる距離

    int connectNum = 4;

    // 0と1がくっつく
    int FirstConnectionLeft = 1; // くっつかれるほう
    int FirstConnectionRight = 0; // くっつきに行くほう

    // 1と2がくっつく
    int SecondConnectionLeft = 1; // くっつかれるほう
    int SecondConnectionRight = 2; // くっつきに行くほう

    // toio_dict[3]の位置
    Vector2 PosCube3 = new Vector2(0, 0);
    int AngleCube3 = 0;

    // Slopeの上り切った平らなところの座標
    Vector2 PosFlat = new Vector2(242, 342);

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

        StartButton.onClick.AddListener(StartButtonClicked);
    }

    async Task ConnectToioCubes()
    {
        while (cm.syncCubes.Count < connectNum)
        {
            await cm.MultiConnect(connectNum);
        }
        // 接続台数をコンソールに表示する
        Debug.Log(cm.syncCubes.Count);

        // 接続したCubeの名前を表示する
        string connectedCubes = "";
        foreach (var cube in cm.syncCubes)
        {
            connectedCubes += "toio_dict[" + GetCubeId(cube.id) +"]";
        }
        Debug.Log(connectedCubes.Trim() + "と接続した");
    }

    void StartButtonClicked()
    {
        StartClicked = true;
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
                    // 移動後のtoio_dict[0]の座標を計算
                    if(navigator.cube.id == toio_dict[FirstConnectionLeft] && navigator.cube.x != 0 && navigator.cube.y != 0)
                    {
                        PosCubeLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                        AngleCubeLeft = navigator.cube.angle;
                        check += 1;
                        PosCubeRight = CalculateNewPosition(PosCubeLeft, AngleCubeLeft+90, L);
                        Debug.Log("PosCubeRight_First: " + PosCubeRight.x + ", " + PosCubeRight.y);
                    }
                }
                else if(check == 1)
                {
                    // PosCubeRightの座標へ移動
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

                    // 指定された角度へ回転
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[FirstConnectionRight])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(AngleCubeLeft-90, rotateTime:2500, tolerance:0.1).Exec();
                            
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
                            StartCoroutine(WaitAndIncrementPhase(1.0f));
                            Debug.Log("phase2");
                        }
                    }
                    else if(phase > 2)
                    {
                        check ++;
                        phase = 0;
                    }
                }

                // toio_dict[1](足場)とtoio_dict[2](新しい構成要素)をくっつける
                else if(check == 2)
                {
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[SecondConnectionLeft]&& navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            PosCubeLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                            AngleCubeLeft = navigator.cube.angle;
                            PosCubeRight = CalculateNewPosition(PosCubeLeft, AngleCubeLeft-90, L);
                            Debug.Log("PosCubeRight_Second: " + PosCubeRight.x + ", " + PosCubeRight.y);
                            phase += 1;
                        }
                    }
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[SecondConnectionRight])
                        {
                            var mv = navigator.Navi2Target(PosCubeRight.x, PosCubeRight.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
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
                            Movement mv = navigator.handle.Rotate2Deg(AngleCubeLeft-90, rotateTime:2500, tolerance:0.1).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                        }
                    }
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[SecondConnectionRight])
                        {
                            navigator.handle.Move(-15, 0, 100);
                        }

                        if (!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(1.0f));
                            Debug.Log("phase3");
                        }
                    }

                    else if(phase > 3)
                    {
                        phase = 0;
                        check ++;
                    }
                }

                // toio_dict[3]の操作
                else if(check == 3 && StartClicked)
                { 
                    if(phase == 0)
                        {
                        int x = int.Parse(InputFieldX.text);
                        int y = int.Parse(InputFieldY.text);
                        int angle = int.Parse(InputFieldAngle.text);
                        if(navigator.cube.id == toio_dict[3] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            PosCube3 = CalculateNewPosition(new Vector2(x, y), angle, L_Slope);
                            AngleCube3 = angle;
                            Debug.Log("Pos_toio_dict[3]: " + PosCube3.x + ", " + PosCube3.y + ", " + AngleCube3);
                            phase += 1;
                        }
                    }
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[3])
                        {
                            var mv = navigator.Navi2Target(PosCube3.x, PosCube3.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[3])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(AngleCube3, rotateTime:2500, tolerance:0.1).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                        }
                    }
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[3])
                        {
                            navigator.handle.Move(-50, 0, 100);
                        }

                        if (!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(2.5f));
                            Debug.Log("phase3");
                        }
                    }
                    else if(phase == 4)
                    {
                        if(navigator.cube.id == toio_dict[3])
                        {
                            float distanceToTarget = Vector2.Distance(navigator.cube.pos, PosCube3);

                            // PosFlat付近(>5)に到達するまでMove(-30,0,10)を実行する
                            if(distanceToTarget > 5)
                            {
                                navigator.handle.Move(-30, 0, 50);
                            }
                            else
                            {
                                    phase += 1;
                                    Debug.Log("phase4");
                            }
                        }
                    }
                    else if(phase == 5)
                    {   
                        // Cube3なら，toio_pos[0]との距離が5以下になるまでMove(15,0,100)を実行する

                        if(navigator.cube.id == toio_dict[3])
                        {
                            float distanceToTarget = Vector2.Distance(navigator.cube.pos, toio_pos[0]);

                            if(distanceToTarget > 5)
                            {
                                navigator.handle.Move(15, 0, 100);
                            }
                            else
                            {
                                phase += 1;
                                Debug.Log("phase5");
                            }
                        }
                    }
                }


                // 1段目の真ん中のtoioを抜く
                // 矢印キー(上)を押されたら3秒前進する
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    if(navigator.cube.id == toio_dict[1])
                    {
                        navigator.handle.Move(50, 0, 100);
                    }
                }
            }

            string text = "";
            foreach (var cube in cm.syncCubes)
            {
                if(cube.id == toio_dict[0]) text += "toio_dict[0]：(" + cube.x + "," + cube.y + "," + cube.angle + ")\n";                
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
