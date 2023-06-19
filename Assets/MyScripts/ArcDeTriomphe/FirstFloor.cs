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
    bool s = false;

    bool isCoroutineRunning = false;

    int L = 50; // Cube同士の接続に用いる距離
    int L_Slope = 70; // Slopeの前にCubeが配置するときに用いる距離

    int connectNum = 3;

    // 0と1がくっつく
    int FirstLeft = 1; // くっつかれるほう
    int FirstRight = 0; // くっつきに行くほう

    // 1と2がくっつく
    int SecondLeft = 1; // くっつかれるほう
    int SecondRight = 2; // くっつきに行くほう

    // toioの座標
    Vector2 PosCube0 = new Vector2(0, 0);
    Vector2 PosCube1 = new Vector2(0, 0);
    Vector2 PosCube2 = new Vector2(0, 0);
    Vector2 PosCube3 = new Vector2(0, 0);
    Vector2 PosCube7 = new Vector2(0, 0);

    // toioの角度
    int AngleCube0 = 0;
    int AngleCube1 = 0;
    int AngleCube2 = 0;
    int AngleCube3 = 0;
    int AngleCube7 = 0;

    // Slopeの上り切った平らなところの座標
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

            foreach(var navigator in cm.syncNavigators)
            {
                // Step1. Cube0(構成要素)とCube1(足場)をくっつける
                if(check == 0)
                {   
                    // 1. Cube0の移動後の座標と角度を計算
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[FirstLeft] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            PosCube1 = new Vector2(navigator.cube.x, navigator.cube.y);
                            AngleCube1 = navigator.cube.angle;
                            PosCube0 = CalculateNewPosition(PosCube1, AngleCube1 + 90, L);
                            AngleCube0 = AngleCube1 - 90;
                            Debug.Log("PosCube0_First: " + PosCube0.x + ", " + PosCube0.y + ", " + AngleCube0);
                            phase += 1;
                        }
                    }
                
                    // 2. Cube0を1で求めた座標へ移動
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[FirstRight])
                        {
                            var mv = navigator.Navi2Target(PosCube0.x, PosCube0.y, maxSpd:5).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }

                    // 3. Cube0を1で求めた角度まで回転
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[FirstRight])
                        {
                            int angle_diff = AngleCube0 - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 10, 20);
                            }
                            else
                            {
                                navigator.handle.Move(0, -10, 20);
                            }
                        }
                    }

                    // 4. Cube0をCube1にくっつける
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[FirstRight])
                        {
                            float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosCube1);
                            if(distance < 28)
                            {
                                phase += 1;
                                Debug.Log("phase3");
                            }
                            else
                            {
                                navigator.handle.Move(30, 0, 40);
                            }
                        }
                    }

                    // 5. checkをインクリメントし, phaseを初期化
                    else if(phase > 3)
                    {
                        check += 1;
                        phase = 0;
                        Debug.Log("check: " + check);
                    }
                }

                // Step2. Cube2(構成要素)とCube1(足場)をくっつける
                else if(check == 1)
                {
                    // 1. Cube2の移動後の座標と角度を計算
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[SecondLeft] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            PosCube1 = new Vector2(navigator.cube.x, navigator.cube.y);
                            AngleCube1 = navigator.cube.angle;
                            PosCube2 = CalculateNewPosition(PosCube1, AngleCube1 - 90, L);
                            AngleCube2 = AngleCube1 - 90;
                            Debug.Log("PosCube2_Second: " + PosCube2.x + ", " + PosCube2.y + ", " + AngleCube2);
                            phase += 1;
                        }
                    }
                
                    // 2. Cube2を1で求めた座標へ移動
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[SecondRight])
                        {
                            var mv = navigator.Navi2Target(PosCube2.x, PosCube2.y, maxSpd:5).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }

                    // 3. Cube2を1で求めた角度まで回転
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[SecondRight])
                        {
                            int angle_diff = AngleCube2 - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 10, 20);
                            }
                            else
                            {
                                navigator.handle.Move(0, -10, 20);
                            }
                        }
                    }

                    // 4. Cube2をCube1にくっつける
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[SecondRight])
                        {
                            float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosCube1);
                            if(distance < 28)
                            {
                                phase += 1;
                                Debug.Log("phase3");
                            }
                            else
                            {
                                navigator.handle.Move(-30, 0, 40);
                            }
                        }
                    }

                    // 5. checkをインクリメントし, phaseを初期化
                    else if(phase > 3)
                    {
                        check += 1;
                        phase = 0;
                        Debug.Log("check: " + check);
                        Debug.Log("手順1：PCを変えて，toio_dict[0]の座標&角度を入力してください");
                    }
                }

                // Step3. Cube3をCube0の上に移動させる
                else if(check == 3 && StartClicked)
                {
                    // 1. Cube3の移動後の座標と角度を計算
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[3] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            // InputFieldから座標と角度を取得し，PosCube7とAngleCube7に代入
                            float x = float.Parse(InputFieldX.text);
                            float y = float.Parse(InputFieldY.text);
                            int angle = int.Parse(InputFieldAngle.text);
                            PosCube7 = new Vector2(x, y);
                            AngleCube7 = angle;

                            PosCube3 = CalculateNewPosition(new Vector2(x, y), angle, L_Slope);
                            AngleCube3 = angle;
                            Debug.Log("Pos_toio_dict[3]: " + PosCube3.x + ", " + PosCube3.y + ", " + AngleCube3);
                            phase += 1;
                        }
                    }

                    // 2. Cube3を1で求めた座標へ移動
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

                    // 3. Cube3を1で求めた角度まで回転s
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

                    // 4. Cube3をCube7の坂に乗せる
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[3])
                        {
                            float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosCube7);
                            if(distance < L_Slope - 3)
                            {
                                phase += 1;
                                Debug.Log("phase3");
                            }
                            else
                            {
                                navigator.handle.Move(-50, 0, 100);
                            }
                        }
                    }

                    // 4. Cube3がPosFlat付近(distance < 5)までバックする
                    else if(phase == 4)
                    {
                        if(navigator.cube.id == toio_dict[3])
                        {
                            float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosFlat);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase4");
                            }
                            else
                            {
                                navigator.handle.Move(-30, 0, 50);
                            }
                        }
                    }

                    // 5. Cube3をCube0の上に移動させる(= Cube3をtoio_pos[0]との距離が5以下になるまでMove(10,0,20)を実行する)
                    else if(phase == 5)
                    {   
                        if(navigator.cube.id == toio_dict[3])
                        {
                            float distance = Vector2.Distance(navigator.cube.pos, toio_pos[0]);

                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase5");
                            }
                            else
                            {
                                navigator.handle.Move(10, 0, 20);
                            }
                        }
                    }

                    // checkをインクリメントし, phaseを初期化
                    else if(phase > 5)
                    {
                        check += 1;
                        phase = 0;
                        Debug.Log("check: " + check);
                        Debug.Log("手順3：PCを変えて，矢印キー(下)を押してください");
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
                if(cube.id == toio_dict[0]) text += "Cube0：(" + cube.x + "," + cube.y + "," + cube.angle + ")\n";                
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

    // Buttonが押されたら，StartClickedをtrueにする
    void StartButtonClicked()
    {
        StartClicked = true;
    }
}
