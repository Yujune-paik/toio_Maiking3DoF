using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

public class MovingOnSlopeThree : MonoBehaviour
{  
    public Text label;
    // public InputField InputFieldX;
    // public InputField InputFieldY;
    // public InputField InputFieldAngle;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Real;

    int phase = 0;
    int check = 0;
    bool s = false;

    int connectNum = 4;

    // 構成要素の番号
    int NumCube3 = 3;
    int NumCube4 = 4;
    int NumCube5 = 5;

    int NumPress = 6;
    int NumSlope = 7;

    // toioの座標
    Vector2 PosSlope = new Vector2(0, 0);
    Vector2 PosCube = new Vector2(0, 0);
    Vector2 PosPress = new Vector2(317, 287);

    // toioの角度
    int AngleSlope = 0;
    int AngleCube = 0;
    int AnglePress = 0;

    // toio間の距離
    int L_Cube = 70, L_Press = 130;

    bool isCoroutineRunning = false;

    // Slopeの登り始めの座標
    Vector2 PosSlopeStart = new Vector2(242, 313);

    // Slopeの登り切った平らなところの座標
    Vector2 PosFlat = new Vector2(242, 342);

    float timeSinceLastOrder = 0f;
    float orderInterval = 0.1f;

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
        s = true;

        // 接続したCubeの名前を表示する
        string connectedCubes = "";
        foreach (var cube in cm.syncCubes)
        {
            connectedCubes += "toio_dict[" + GetCubeId(cube.id) +"]";
        }
        Debug.Log(connectedCubes.Trim() + "と接続した");
    }

    void Update()
    {
        timeSinceLastOrder += Time.deltaTime;

        if (cm.synced && timeSinceLastOrder >= orderInterval && s)
        {
            timeSinceLastOrder = 0f;

            foreach (var navigator in cm.syncNavigators)
            {   
                if(check == 0)
                {
                    // 1. Cube[NumCube5]とCube[NumPress]の移動後の座標と角度を計算
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[NumSlope] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            // // InputFieldから座標と角度を取得し，PosCube7とAngleCube7に代入
                            // float x = float.Parse(InputFieldX.text);
                            // float y = float.Parse(InputFieldY.text);
                            // int angle = int.Parse(InputFieldAngle.text);

                            // Cube[NumSlope]の座標と角度を取得
                            PosSlope = new Vector2(navigator.cube.x, navigator.cube.y);
                            AngleSlope = navigator.cube.angle;

                            // Cube[NumCube5]の座標と角度を計算
                            PosCube = CalculateNewPosition(PosSlope, AngleSlope, L_Cube);
                            AngleCube = AngleSlope;
                            Debug.Log("PosCube: (" + PosCube.x + ", " + PosCube.y + ")");
                            Debug.Log("AngleCube: " + AngleCube);

                            // Cube[NumPress]の座標と角度を計算
                            PosPress = CalculateNewPosition(PosSlope, AngleSlope, L_Press);
                            AnglePress = AngleSlope;
                            Debug.Log("PosPress: (" + PosPress.x + ", " + PosPress.y + ")");
                            Debug.Log("AnglePress: " + AnglePress);
                            
                            phase += 1;
                        }
                    }

                    // 2-1. Cube[NumCube5]をPosCubeまで移動
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[NumCube5])
                        {
                            var mv = navigator.Navi2Target(PosCube.x, PosCube.y, maxSpd:5).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }

                    // 2-2. Cube[Press]をPosPressまで移動
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[NumPress])
                        {
                            var mv = navigator.Navi2Target(PosPress.x, PosPress.y, maxSpd:5).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                        }
                    }

                    // 3-1. Cube[Cube]をAngleCubeまで回転
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[NumCube5])
                        {
                            int angle_diff = AngleCube - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase += 1;
                                Debug.Log("phase3");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 20, 30);
                            }
                            else
                            {
                                navigator.handle.Move(0, -20, 30);
                            }
                        }
                    }

                    // 3-2. Cube[Press]をAnglePressまで回転
                    else if(phase == 4)
                    {
                        if(navigator.cube.id == toio_dict[NumPress])
                        {
                            int angle_diff = AnglePress - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase += 1;
                                Debug.Log("phase4");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 20, 30);
                            }
                            else
                            {
                                navigator.handle.Move(0, -20, 30);
                            }
                        }
                    }

                    // 4-1. Cube[Cube]を坂の入口(PosSlopeStart)に乗るまでバックする
                    // 4-2. Cube[Press]もCube[Cube]が坂の入口(PosSlopeStart)に乗るまでバックする
                    else if(phase == 5)
                    {
                        if(navigator.cube.id == toio_dict[NumCube5])
                        {
                            Debug.Log("phase5");
                            float distance = Vector2.Distance(navigator.cube.pos, PosSlopeStart);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase5");
                            }
                            else
                            {
                                navigator.handle.Move(-30, 0, 50);
                            }
                        }

                        else if(navigator.cube.id == toio_dict[NumPress])
                        {
                            navigator.handle.Move(-50, 0, 50);
                        }
                    }

                    // 5-1. Cube[Press]は1.0秒間前進する
                    else if(phase == 6)
                    {
                        if(navigator.cube.id == toio_dict[NumPress])
                        {
                            navigator.handle.Move(100, 0, 100);
                        }

                        if(!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(0.5f));
                        }
                    }

                    // 5-2. Cube[Cube]がPosFlat付近(distance < 5)までバックする
                    else if(phase == 7)
                    {
                        if(navigator.cube.id == toio_dict[NumCube5])
                        {
                            float distance = Vector2.Distance(navigator.cube.pos, PosFlat);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase7");
                                Debug.Log("PosPress: (" + PosPress.x + ", " + PosPress.y + ")");
                            }
                            else
                            {
                                navigator.handle.Move(-30, 0, 40);
                            }
                        }
                    }

                    // 6-1. Cube[Cube]がtoio_pos[0]との距離が5以下になるまでバックする
                    else if(phase == 8)
                    {
                        if(navigator.cube.id == toio_dict[NumCube5])
                        {
                            float distance = Vector2.Distance(navigator.cube.pos, toio_pos[0]);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase8");
                            }
                            else
                            {
                                navigator.handle.Move(-20, 0, 20);
                            }
                        }
                    }

                    // 6-2. Cube[Cube]を指定した角度まで回転
                    else if(phase == 9)
                    {
                        if(navigator.cube.id == toio_dict[NumCube5])
                        {
                            int angle_diff = 180 - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 1)
                            {
                                phase += 1;
                                Debug.Log("phase9");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 15, 15);
                            }
                            else
                            {
                                navigator.handle.Move(0, -15, 15);
                            }
                        }
                    }

                    // 7-1. Cube[Cube]がtoio_pos[1]との距離が5以下になるまでバックする
                    else if(phase == 10)
                    {
                        if(navigator.cube.id == toio_dict[NumCube5])
                        {
                            float distance = Vector2.Distance(navigator.cube.pos, toio_pos[1]);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase10");
                            }
                            else
                            {
                                navigator.handle.Move(-20, 0, 20);
                            }
                        }
                    }

                    // 7-2. Cube[Cube]を指定した角度まで回転
                    else if(phase == 11)
                    {
                        if(navigator.cube.id == toio_dict[NumCube5])
                        {
                            int angle_diff = 90 - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 1)
                            {
                                phase += 1;
                                Debug.Log("phase11");

                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 15, 15);
                            }
                            else
                            {
                                navigator.handle.Move(0, -15, 15);
                            }
                        }
                    }

                    // 8-1. Cube[Cube]がtoio_pos[2]との距離が5以下になるまでバックする
                    else if(phase == 12)
                    {
                        if(navigator.cube.id == toio_dict[NumCube5])
                        {
                            float distance = Vector2.Distance(navigator.cube.pos, toio_pos[2]);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase12");
                            }
                            else
                            {
                                navigator.handle.Move(-20, 0, 20);
                            }
                        }
                    }

                    // 8-2. Cube[Cube]を指定した角度まで回転
                    else if(phase == 13)
                    {
                        if(navigator.cube.id == toio_dict[NumCube5])
                        {
                            int angle_diff = 180 - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 1)
                            {
                                phase += 1;
                                Debug.Log("phase13");
                                Debug.Log("手順3：PCを変えて，矢印キー(下)を押してください");

                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 15, 15);
                            }
                            else
                            {
                                navigator.handle.Move(0, -15, 15);
                            }
                        }
                    }

                    // checkをインクリメントし, phaseを初期化
                    else if(phase > 13)
                    {
                        check += 1;
                        phase = 0;
                        Debug.Log("check: " + check);
                        // Debug.Log("手順3：PCを変えて，矢印キー(下)を押してください");
                    }
                }

                else if(check == 1)
                {
                    // 1. Cube[NumCube4]とCube[NumPress]の移動後の座標と角度を計算
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[NumSlope] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            // // InputFieldから座標と角度を取得し，PosCube7とAngleCube7に代入
                            // float x = float.Parse(InputFieldX.text);
                            // float y = float.Parse(InputFieldY.text);
                            // int angle = int.Parse(InputFieldAngle.text);

                            // Cube[NumSlope]の座標と角度を取得
                            PosSlope = new Vector2(navigator.cube.x, navigator.cube.y);
                            AngleSlope = navigator.cube.angle;

                            // Cube[NumCube4]の座標と角度を計算
                            PosCube = CalculateNewPosition(PosSlope, AngleSlope, L_Cube);
                            AngleCube = AngleSlope;
                            Debug.Log("PosCube: (" + PosCube.x + ", " + PosCube.y + ")");
                            Debug.Log("AngleCube: " + AngleCube);

                            // Cube[NumPress]の座標と角度を計算
                            PosPress = CalculateNewPosition(PosSlope, AngleSlope, L_Press);
                            AnglePress = AngleSlope;
                            Debug.Log("PosPress: (" + PosPress.x + ", " + PosPress.y + ")");
                            Debug.Log("AnglePress: " + AnglePress);
                            
                            phase += 1;
                        }
                    }

                    // 2-1. Cube[NumCube4]をPosCubeまで移動
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[NumCube4])
                        {
                            var mv = navigator.Navi2Target(PosCube.x, PosCube.y, maxSpd:5).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }

                    // 2-2. Cube[Press]をPosPressまで移動
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[NumPress])
                        {
                            var mv = navigator.Navi2Target(PosPress.x, PosPress.y, maxSpd:5).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                        }
                    }

                    // 3-1. Cube[Cube]をAngleCubeまで回転
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[NumCube4])
                        {
                            int angle_diff = AngleCube - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase += 1;
                                Debug.Log("phase3");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 20, 30);
                            }
                            else
                            {
                                navigator.handle.Move(0, -20, 30);
                            }
                        }
                    }

                    // 3-2. Cube[Press]をAnglePressまで回転
                    else if(phase == 4)
                    {
                        if(navigator.cube.id == toio_dict[NumPress])
                        {
                            int angle_diff = AnglePress - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase += 1;
                                Debug.Log("phase4");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 20, 30);
                            }
                            else
                            {
                                navigator.handle.Move(0, -20, 30);
                            }
                        }
                    }

                    // 4-1. Cube[Cube]を坂の入口(PosSlopeStart)に乗るまでバックする
                    // 4-2. Cube[Press]もCube[Cube]が坂の入口(PosSlopeStart)に乗るまでバックする
                    else if(phase == 5)
                    {
                        if(navigator.cube.id == toio_dict[NumCube4])
                        {
                            Debug.Log("phase5");
                            float distance = Vector2.Distance(navigator.cube.pos, PosSlopeStart);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase5");
                            }
                            else
                            {
                                navigator.handle.Move(-30, 0, 50);
                            }
                        }

                        else if(navigator.cube.id == toio_dict[NumPress])
                        {
                            navigator.handle.Move(-50, 0, 50);
                        }
                    }

                    // 5-1. Cube[Press]は1.0秒間前進する
                    else if(phase == 6)
                    {
                        if(navigator.cube.id == toio_dict[NumPress])
                        {
                            navigator.handle.Move(100, 0, 100);
                        }

                        if(!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(0.5f));
                        }
                    }

                    // 5-2. Cube[Cube]がPosFlat付近(distance < 5)までバックする
                    else if(phase == 7)
                    {
                        if(navigator.cube.id == toio_dict[NumCube4])
                        {
                            float distance = Vector2.Distance(navigator.cube.pos, PosFlat);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase7");
                                Debug.Log("PosPress: (" + PosPress.x + ", " + PosPress.y + ")");
                            }
                            else
                            {
                                navigator.handle.Move(-30, 0, 40);
                            }
                        }
                    }

                    // 6-1. Cube[Cube]がtoio_pos[0]との距離が5以下になるまでバックする
                    else if(phase == 8)
                    {
                        if(navigator.cube.id == toio_dict[NumCube4])
                        {
                            float distance = Vector2.Distance(navigator.cube.pos, toio_pos[0]);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase8");
                            }
                            else
                            {
                                navigator.handle.Move(-20, 0, 20);
                            }
                        }
                    }

                    // 6-2. Cube[Cube]を指定した角度まで回転
                    else if(phase == 9)
                    {
                        if(navigator.cube.id == toio_dict[NumCube4])
                        {
                            int angle_diff = 180 - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 1)
                            {
                                phase += 1;
                                Debug.Log("phase9");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 15, 15);
                            }
                            else
                            {
                                navigator.handle.Move(0, -15, 15);
                            }
                        }
                    }

                    // 7-1. Cube[Cube]がtoio_pos[1]との距離が5以下になるまでバックする
                    else if(phase == 10)
                    {
                        if(navigator.cube.id == toio_dict[NumCube4])
                        {
                            float distance = Vector2.Distance(navigator.cube.pos, toio_pos[1]);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase10");
                            }
                            else
                            {
                                navigator.handle.Move(-20, 0, 20);
                            }
                        }
                    }

                    // 7-2. Cube[Cube]を指定した角度まで回転
                    else if(phase == 11)
                    {
                        if(navigator.cube.id == toio_dict[NumCube4])
                        {
                            int angle_diff = 90 - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 1)
                            {
                                phase += 1;
                                Debug.Log("phase11");

                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 15, 15);
                            }
                            else
                            {
                                navigator.handle.Move(0, -15, 15);
                            }
                        }
                    }

                    // checkをインクリメントし, phaseを初期化
                    else if(phase > 11)
                    {
                        check += 1;
                        phase = 0;
                        Debug.Log("check: " + check);
                        // Debug.Log("手順3：PCを変えて，矢印キー(下)を押してください");
                    }
                }

                else if(check == 2)
                {
                    // 1. Cube[NumCube3]とCube[NumPress]の移動後の座標と角度を計算
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[NumSlope] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            // // InputFieldから座標と角度を取得し，PosCube7とAngleCube7に代入
                            // float x = float.Parse(InputFieldX.text);
                            // float y = float.Parse(InputFieldY.text);
                            // int angle = int.Parse(InputFieldAngle.text);

                            // Cube[NumSlope]の座標と角度を取得
                            PosSlope = new Vector2(navigator.cube.x, navigator.cube.y);
                            AngleSlope = navigator.cube.angle;

                            // Cube[NumCube3]の座標と角度を計算
                            PosCube = CalculateNewPosition(PosSlope, AngleSlope, L_Cube);
                            AngleCube = AngleSlope;
                            Debug.Log("PosCube: (" + PosCube.x + ", " + PosCube.y + ")");
                            Debug.Log("AngleCube: " + AngleCube);

                            // Cube[NumPress]の座標と角度を計算
                            PosPress = CalculateNewPosition(PosSlope, AngleSlope, L_Press);
                            AnglePress = AngleSlope;
                            Debug.Log("PosPress: (" + PosPress.x + ", " + PosPress.y + ")");
                            Debug.Log("AnglePress: " + AnglePress);
                            
                            phase += 1;
                        }
                    }

                    // 2-1. Cube[NumCube3]をPosCubeまで移動
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[NumCube3])
                        {
                            var mv = navigator.Navi2Target(PosCube.x, PosCube.y, maxSpd:5).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }

                    // 2-2. Cube[Press]をPosPressまで移動
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[NumPress])
                        {
                            var mv = navigator.Navi2Target(PosPress.x, PosPress.y, maxSpd:5).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                        }
                    }

                    // 3-1. Cube[Cube]をAngleCubeまで回転
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[NumCube3])
                        {
                            int angle_diff = AngleCube - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase += 1;
                                Debug.Log("phase3");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 20, 30);
                            }
                            else
                            {
                                navigator.handle.Move(0, -20, 30);
                            }
                        }
                    }

                    // 3-2. Cube[Press]をAnglePressまで回転
                    else if(phase == 4)
                    {
                        if(navigator.cube.id == toio_dict[NumPress])
                        {
                            int angle_diff = AnglePress - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase += 1;
                                Debug.Log("phase4");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 20, 30);
                            }
                            else
                            {
                                navigator.handle.Move(0, -20, 30);
                            }
                        }
                    }

                    // 4-1. Cube[Cube]を坂の入口(PosSlopeStart)に乗るまでバックする
                    // 4-2. Cube[Press]もCube[Cube]が坂の入口(PosSlopeStart)に乗るまでバックする
                    else if(phase == 5)
                    {
                        if(navigator.cube.id == toio_dict[NumCube3])
                        {
                            Debug.Log("phase5");
                            float distance = Vector2.Distance(navigator.cube.pos, PosSlopeStart);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase5");
                            }
                            else
                            {
                                navigator.handle.Move(-30, 0, 50);
                            }
                        }

                        else if(navigator.cube.id == toio_dict[NumPress])
                        {
                            navigator.handle.Move(-50, 0, 50);
                        }
                    }

                    // 5-1. Cube[Press]は1.0秒間前進する
                    else if(phase == 6)
                    {
                        if(navigator.cube.id == toio_dict[NumPress])
                        {
                            navigator.handle.Move(100, 0, 100);
                        }

                        if(!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(0.5f));
                        }
                    }

                    // 5-2. Cube[Cube]がPosFlat付近(distance < 5)までバックする
                    else if(phase == 7)
                    {
                        if(navigator.cube.id == toio_dict[NumCube3])
                        {
                            float distance = Vector2.Distance(navigator.cube.pos, PosFlat);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase7");
                                Debug.Log("PosPress: (" + PosPress.x + ", " + PosPress.y + ")");
                            }
                            else
                            {
                                navigator.handle.Move(-30, 0, 40);
                            }
                        }
                    }

                    // 6-1. Cube[Cube]がtoio_pos[0]との距離が5以下になるまでバックする
                    else if(phase == 8)
                    {
                        if(navigator.cube.id == toio_dict[NumCube3])
                        {
                            float distance = Vector2.Distance(navigator.cube.pos, toio_pos[0]);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase8");
                            }
                            else
                            {
                                navigator.handle.Move(-20, 0, 20);
                            }
                        }
                    }

                    // 6-2. Cube[Cube]を指定した角度まで回転
                    else if(phase == 9)
                    {
                        if(navigator.cube.id == toio_dict[NumCube3])
                        {
                            int angle_diff = 180 - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 1)
                            {
                                phase += 1;
                                Debug.Log("phase9");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 15, 15);
                            }
                            else
                            {
                                navigator.handle.Move(0, -15, 15);
                            }
                        }
                    }

                    // checkをインクリメントし, phaseを初期化
                    else if(phase > 9)
                    {
                        // check += 1;
                        // phase = 0;
                        // Debug.Log("check: " + check);
                        // Debug.Log("手順3：PCを変えて，矢印キー(下)を押してください");
                    }
                }
            }  
        }

        string text = "";      
        foreach (var cube in cm.syncCubes)
        {
            if (cube.id == toio_dict[NumCube5]) 
            {
                text += "Cube[Cube5]：(" + cube.pos.x + ", " + cube.pos.y + ")\n";
                text += "Cube[Cube5]_Angle：(" + cube.angle + ")\n";
            }
            else if(cube.id == toio_dict[NumCube4])
            {
                text += "Cube[Cube4]：(" + cube.pos.x + ", " + cube.pos.y + ")\n";
                text += "Cube[Cube4]_Angle：(" + cube.angle + ")\n";
            }
            else if(cube.id == toio_dict[NumCube3])
            {
                text += "Cube[Cube3]：(" + cube.pos.x + ", " + cube.pos.y + ")\n";
                text += "Cube[Cube3]_Angle：(" + cube.angle + ")\n";
            }
            else if (cube.id == toio_dict[NumPress]) text += "Cube[Press]：(" + cube.pos.x + ", " + cube.pos.y + ")\n";
            else if (cube.id == toio_dict[NumSlope]) text += "Cube[Slope]：(" + cube.pos.x + "," + cube.pos.y + ")\n";
        }
        if (text != "") this.label.text = text;
        
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
