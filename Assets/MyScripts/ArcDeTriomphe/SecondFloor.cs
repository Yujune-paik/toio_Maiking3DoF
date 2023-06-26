using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

// 2層目を作るプログラム
// 接続するtoio： 4, 5, 6, 7

public class SecondFloor : MonoBehaviour
{  
    public Text label;
    public InputField InputFieldX;
    public InputField InputFieldY;
    public InputField InputFieldAngle;
    public Button StartButton;

    bool StartClicked = false;
    bool StartCheck3 = false;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Simulator;

    int phase = 0;
    int check = 1;

    bool isCoroutineRunning = false;

    int L = 50; // Cube同士の接続に用いる距離
    int L_Cube = 30; //Cubeの幅
    int L_Slope = 70; // Slopeの前にCubeが配置するときに用いる距離

    int connectNum = 4;

    // toio_dict[4]の位置と角度
    Vector2 PosCube4 = new Vector2(0, 0);
    int AngleCube4 = 0;

    // toio_dict[5]の位置と角度
    Vector2 PosCube5 = new Vector2(0, 0);
    int AngleCube5 = 0;

    // toio_dict[6]の位置と角度
    Vector2 PosCube6 = new Vector2(0, 0);
    int AngleCube6 = 0;

    // toio_dict[7]の位置と角度
    Vector2 PosCube7 = new Vector2(0, 0);
    int AngleCube7 = 0;

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
            connectedCubes += cube.localName + " ";
        }
        Debug.Log(connectedCubes.Trim() + "と接続した");
    }

    // Startボタンが押されたら，StartClickedをtrueにする
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
                // Step3. toio_dict[0](構成要素)とtoio_dict[7](Slope)をくっつける
                if(check == 0 & StartClicked)
                {
                    if(phase == 0)
                        {
                        int x = int.Parse(InputFieldX.text);
                        int y = int.Parse(InputFieldY.text);
                        int angle = int.Parse(InputFieldAngle.text);
                        // if(navigator.cube.id == toio_dict[7] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        // {
                            PosCube7 = CalculateNewPosition(new Vector2(x, y), angle+180, L);
                            AngleCube7 = angle+180;
                            Debug.Log("PosCube7: " + PosCube7.x + ", " + PosCube7.y + ", " + AngleCube7);
                            phase += 1;
                        // }
                    }
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[7])
                        {
                            var mv = navigator.Navi2Target(PosCube7.x, PosCube7.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[7])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(AngleCube7, rotateTime:2500, tolerance:0.1).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                        }
                    }

                    else if(phase > 2)
                    {
                        phase = 0;
                        check ++;
                    }
                }
                
                // Step4. toio_dict[5](構成要素)をtoio_dict[2]の上に運ぶ
                // toio_dict[5](構成要素)とtoio_dict[6](Press)をtoio_dict[7](Slope)の前まで移動
                // toio_dict[5](構成要素)をtoio_dict[7](Slope)の上に移動
                // toio_dict[6](Press)は前進
                else if(check == 1)
                {   
                    // 1. toio_dict[5](構成要素)とtoio_dict[6](Press)の移動先の座標と角度を計算
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[7] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            PosCube5 = CalculateNewPosition(navigator.cube.pos, navigator.cube.angle, L_Slope);
                            AngleCube5 = navigator.cube.angle;
                            PosCube6 = CalculateNewPosition(navigator.cube.pos, navigator.cube.angle, L_Slope + L);
                            AngleCube6 = navigator.cube.angle;
                            Debug.Log("PosCube5: " + PosCube5.x + ", " + PosCube5.y + ", " + AngleCube5);
                            Debug.Log("PosCube6: " + PosCube6.x + ", " + PosCube6.y + ", " + AngleCube6);
                            phase += 1;
                        }
                    }

                    // 2. toio_dict[5](構成要素)を移動(座標)
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[5])
                        {
                            var mv = navigator.Navi2Target(PosCube5.x, PosCube5.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }

                    // 3. toio_dict[5](構成要素)を移動(角度)
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[5])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(AngleCube5, rotateTime:2500, tolerance:0.1).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase3");
                            }
                        }
                    }

                    // toio_dict[6](Press)を移動(座標)
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[6])
                        {
                            var mv = navigator.Navi2Target(PosCube6.x, PosCube6.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                        }
                    }
                    
                    // 

                    // toio_dict[6](Press)を移動(角度)
                    else if(phase == 4)
                    {
                        if(navigator.cube.id == toio_dict[6])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(AngleCube6, rotateTime:2500, tolerance:0.1).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase4");
                            }
                        }
                    }

                    // toio_dict[5](構成要素)が坂の上に乗るまで，toio_dict[5]とtoio_dict[6]をバックさせる
                    // OnSlope.csのphase==0の部分
                    else if(phase == 5)
                    {
                        if(navigator.cube.id == toio_dict[5] || navigator.cube.id == toio_dict[6])
                        {
                            navigator.handle.Move(-50, 0, 100);
                        }
                        
                        if (!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(2.5f));
                        }
                    }                    

                    // toio_dict[5](構成要素)がPosFlatに到達するまで，toio_dict[5](構成要素)にMove(-30,0,50)の命令を出す
                    // toio_dict[6](Press)は2.5秒前進(Move(20,0,100))
                    else if(phase == 6)
                    {
                        if(navigator.cube.id == toio_dict[5])
                        {
                            float distanceToTarget = Vector2.Distance(navigator.cube.pos, PosFlat);

                            if(distanceToTarget > 5)
                            {
                                navigator.handle.Move(-30, 0, 50);
                            }
                            else
                            {
                                phase += 1;
                                Debug.Log("phase6");
                            }
                        }
                        else if(navigator.cube.id == toio_dict[6])
                        {
                            navigator.handle.Move(20, 0, 100);
                        }
                    }
                    
                    // toio_dict[5](構成要素)がtoio_pos[2]の上に乗るまで，toio_dict[5](構成要素)にMove(-15,0,30)の命令を出す
                    else if(phase == 7)
                    {
                        if(navigator.cube.id == toio_dict[5])
                        {
                            float distanceToTarget = Vector2.Distance(navigator.cube.pos, toio_pos[2]);

                            if(distanceToTarget > 5)
                            {
                                navigator.handle.Move(-15, 0, 30);
                            }
                            else
                            {
                                phase += 1;
                                Debug.Log("phase7");
                            }
                        }
                    }
                    
                    else if(phase > 7)
                    {
                        phase = 0;
                        check ++;
                    }
                }

                // Step5. toio_dict[4]をtoio_dict[1]の上に移動
                // toio_dict[4](構成要素)とtoio_dict[6](Press)をtoio_dict[7](Slope)の前まで移動
                // toio_dict[4](構成要素)をtoio_dict[7](Slope)の上に移動
                // toio_dict[6](Press)は前進
                else if(check == 2)
                {
                    // toio_dict[4](構成要素)とtoio_dict[6](Press)の移動先の座標と角度を計算
                    if(navigator.cube.id == toio_dict[7] && navigator.cube.x != 0 && navigator.cube.y != 0)
                    {
                        PosCube4 = CalculateNewPosition(navigator.cube.pos, navigator.cube.angle, L_Slope);
                        AngleCube4 = navigator.cube.angle;
                        PosCube6 = CalculateNewPosition(navigator.cube.pos, navigator.cube.angle, L_Slope + L);
                        AngleCube6 = navigator.cube.angle;
                        Debug.Log("PosCube4: " + PosCube4.x + ", " + PosCube4.y + ", " + AngleCube4);
                        Debug.Log("PosCube6: " + PosCube6.x + ", " + PosCube6.y + ", " + AngleCube6);
                        phase += 1;
                    }

                    // toio_dict[4](構成要素)を移動(座標)
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[4])
                        {
                            var mv = navigator.Navi2Target(PosCube4.x, PosCube4.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }

                    // toio_dict[6](Press)を移動(座標)
                    else if(phase == 2)
                    {
                        if(navigator.cube.id == toio_dict[6])
                        {
                            var mv = navigator.Navi2Target(PosCube6.x, PosCube6.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                        }
                    }
                    
                    // toio_dict[4](構成要素)を移動(角度)
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[4])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(AngleCube4, rotateTime:2500, tolerance:0.1).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase3");
                            }
                        }
                    }

                    // toio_dict[6](Press)を移動(角度)
                    else if(phase == 4)
                    {
                        if(navigator.cube.id == toio_dict[6])
                        {
                            Movement mv = navigator.handle.Rotate2Deg(AngleCube6, rotateTime:2500, tolerance:0.1).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase4");
                            }
                        }
                    }

                    // toio_dict[4](構成要素)が坂の上に乗るまで，toio_dict[4]とtoio_dict[6]をバックさせる
                    // OnSlope.csのphase==0の部分
                    else if(phase == 5)
                    {
                        if(navigator.cube.id == toio_dict[4] || navigator.cube.id == toio_dict[6])
                        {
                            navigator.handle.Move(-50, 0, 100);
                        }
                        
                        if (!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(2.5f));
                        }
                    }                    

                    // toio_dict[4](構成要素)がPosFlatに到達するまで，toio_dict[4](構成要素)にMove(-30,0,50)の命令を出す
                    // toio_dict[6](Press)は2.5秒前進(Move(20,0,100))
                    else if(phase == 6)
                    {
                        if(navigator.cube.id == toio_dict[4])
                        {
                            float distanceToTarget = Vector2.Distance(navigator.cube.pos, PosFlat);

                            if(distanceToTarget > 5)
                            {
                                navigator.handle.Move(-30, 0, 50);
                            }
                            else
                            {
                                phase += 1;
                                Debug.Log("phase6");
                            }
                        }

                        else if(navigator.cube.id == toio_dict[6])
                        {
                            navigator.handle.Move(20, 0, 100);
                        }
                    }
                    
                    // toio_dict[4](構成要素)がtoio_pos[1]の上に乗るまで，toio_dict[4](構成要素)にMove(-15,0,30)の命令を出す
                    else if(phase == 7)
                    {
                        if(navigator.cube.id == toio_dict[4])
                        {
                            float distanceToTarget = Vector2.Distance(navigator.cube.pos, toio_pos[1]);

                            if(distanceToTarget > 5)
                            {
                                navigator.handle.Move(-15, 0, 30);
                            }
                            else
                            {
                                phase += 1;
                                Debug.Log("phase7");
                            }
                        }
                    }
                    
                    else if(phase > 7)
                    {
                        phase = 0;
                        check ++;
                        Debug.Log("手順2：PCを変えて，toio_dict[7]の座標&角度を入力してください");
                    }
                }

                else if(check == 3)
                {
                    // 矢印キー(下)が押されたら，StartCheck3をtrueにする
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        StartCheck3 = true;
                    }

                    // toio_dict[6]を2.5秒バックさせる(Move(-50,0,100))
                    if (phase == 0 && StartCheck3 == true)
                    {
                        if(navigator.cube.id == toio_dict[6])
                        {
                            navigator.handle.Move(-50, 0, 100);
                        }
                        
                        if (!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(2.5f));
                        }
                    }
                    else if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[6])
                        {
                            navigator.handle.Move(20, 0, 100);
                        }

                        if (!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(2.5f));
                        }
                    }
                    else if(phase == 2)
                    {
                        if(navigator.handle.cube.id == toio_dict[7])
                        {
                            navigator.handle.Move(50, 0, 100);
                        }
                        if (!isCoroutineRunning)
                        {
                            StartCoroutine(WaitAndIncrementPhase(2.5f));
                        }
                    }

                    Debug.Log("手順4：PCを変えて，toio_dict[0]の座標&角度を入力してください");
                }
            }

            string text = "";
            foreach (var cube in cm.syncCubes)
            {
                if(cube.id == toio_dict[7]) text += "toio_dict[7]：(" + cube.x + "," + cube.y + "," + cube.angle + ")\n"; 
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
