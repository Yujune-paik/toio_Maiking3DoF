using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

// 2層目を作るプログラム
// 接続するtoio：3,4,5,6,7

public class ConnectingSlope : MonoBehaviour
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

    int L = 60; // Cube同士の接続に用いる距離
    int L_Cube = 30; //Cubeの幅

    int connectNum = 1;

    // toio_dict[0]の位置と角度
    Vector2 PosCube0 = new Vector2(0, 0);
    int AngleCube0 = 0;

    // toio_dict[7]の位置と角度
    Vector2 PosCube7 = new Vector2(0, 0);
    int AngleCube7 = 0;

    // **************************
    // phase==3で使用する変数
    int x1 = 0;
    int y1 = 0;
    int theta1 = 0;

    int x2 = 0;
    int y2 = 0;
    int theta2 = 0;

    int alpha = 0;
    int beta = 0;
    int gamma = 0;

    Vector2 v = new Vector2(0, 0);
    Vector2 R = new Vector2(0, 0);

    int d = 0;
    // **************************

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
            connectedCubes += cube.localName + " ";
        }
        Debug.Log(connectedCubes.Trim() + "と接続した");
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastOrder += Time.deltaTime;

        if(cm.synced && timeSinceLastOrder >= orderInterval && s)
        {
            timeSinceLastOrder = 0f;

            foreach(var navigator in cm.syncNavigators)
            {   
                // Slope(=toio_dict[7])をCube0とくっつける
                if(check == 0 && StartClicked)
                {
                    // 1. InputFieldに入力された値を取得し，PosCube0とAngleCube0に代入し，PosCube7とAngleCube7を計算する
                    if(phase == 0)
                    {
                        int x = int.Parse(InputFieldX.text);
                        int y = int.Parse(InputFieldY.text);
                        int angle = int.Parse(InputFieldAngle.text);
                        PosCube0 = new Vector2(x, y);
                        AngleCube0 = angle;

                        PosCube7 = CalculateNewPosition(PosCube0, AngleCube0+180, L);
                        AngleCube7 = AngleCube0 + 180;

                        Debug.Log("PosCube7:(" + PosCube7.x + ", " + PosCube7.y + ", " + AngleCube7 + ")");
                        phase += 1;
                    }

                    // 2. Slope(=toio_dict[7])をPosCube7の位置へ移動する
                    if(phase == 1)
                    {
                        if(navigator.cube.id == toio_dict[7])
                        {
                            var mv = navigator.Navi2Target(PosCube7.x, PosCube7.y, maxSpd:10).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }

                    // 3-1. Slope(=toio_dict[7])をAngleCube7の角度へ回転する
                    else if(phase == 2) 
                    {
                        if(navigator.cube.id == toio_dict[7])
                        {
                            int angle_diff = AngleCube7 - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase += 1;
                                Debug.Log("phase2");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 20, 20);
                            }
                            else
                            {
                                navigator.handle.Move(0, -20, 20);
                            }
                        }
                    }

                    // 3-2. Slope(=toio_dict[7])を軌道修正する
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[7])
                        {
                            // 3-2-1. 必要な情報を変数に保存する
                            // 3-2-1-1. Cube0(=toio_dict[0])の位置と角度をそれぞれx1, y1, theta1に保存
                            // 3-2-1-2. Slope(=toio_dict[7])の位置と角度をそれぞれx2, y2, theta2に保存
                            x1 = (int)PosCube0.x;
                            y1 = (int)PosCube0.y;
                            theta1 = AngleCube0;

                            x2 = navigator.cube.x;
                            y2 = navigator.cube.y;
                            theta2 = navigator.cube.angle;

                            // 3-2-1-3. alpha, betaの値は0, 180とする (前にくっつき，後退してくっつく場合)
                            alpha = 0;
                            beta = 180;

                            // 3-2-2. 3-2-1で取得した変数をもとに，計算する
                            // 3-2-2-1. d = numerator/denominatorを計算する
                            // numerator = y1-y2+(L_Cube/2)*sin(theta1+alpha)-tan(theta2+beta)*(x1-x2+(L_Cube/2)*cos(theta1+alpha))
                            // denominator = tan(theta2+beta)*cos(theta1+alpha-90)-sin(theta1+alpha-90)
                            float numerator = y1 - y2 + (L_Cube / 2) * Mathf.Sin((theta1+alpha)*Mathf.Deg2Rad) - Mathf.Tan((theta2+beta)*Mathf.Deg2Rad) * (x1 - x2 + (L_Cube / 2) * Mathf.Cos((theta1+alpha)*Mathf.Deg2Rad));
                            float denominator = Mathf.Tan((theta2+beta)*Mathf.Deg2Rad) * Mathf.Cos((theta1+alpha-90)*Mathf.Deg2Rad) - Mathf.Sin((theta1+alpha-90)*Mathf.Deg2Rad);
                            // dはint型であることに注意してください
                            d = (int)(numerator / denominator);

                            // 3-2-2-2. R=(x2+0.8*|d|*cos(gamma), y2+0.8*|d|*sin(gamma))を計算する
                            if(d<0) gamma = theta1 + alpha - 90;
                            else gamma = theta1 + alpha + 90;

                            // gammaが360以上の場合は360を引く
                            if(gamma >= 360) gamma -= 360;
                            // gammaが0未満の場合は360を足す
                            else if(gamma < 0) gamma += 360;

                            float angleRadians = gamma * Mathf.Deg2Rad;
                            R = new Vector2((int)(x2 + 0.8f * Mathf.Abs(d) * Mathf.Cos(angleRadians)), (int)(y2 + 0.8f * Mathf.Abs(d) * Mathf.Sin(angleRadians)));

                            phase += 1;
                            
                            Debug.Log("d: " + d);
                            Debug.Log("R:( " + R.x + ", " + R.y + ")");
                            Debug.Log("phase3");
                        }
                    }

                    else if(phase == 4)
                    {
                        // 3-2-3. |d|<2なら，次のphaseへ進む．そうでなければ，以下の処理を行う．
                        if(Mathf.Abs(d) < 2)
                        {
                            phase = 8;
                            Debug.Log("phase4");
                        }
                        else
                        {
                            phase += 1;
                        }
                    }
                    else if(phase == 5)
                    {
                        if(navigator.cube.id == toio_dict[7])
                        {            
                            // 3-2-4. Slope(=toio_dict[7])をgammaまで回転させる
                            int angle_diff = gamma - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 5)
                            {
                                phase += 1;
                                Debug.Log("phase5");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 20, 20);
                            }
                            else
                            {
                                navigator.handle.Move(0, -20, 20);
                            }
                        }
                    }
                    else if(phase == 6)
                    {
                        if(navigator.cube.id == toio_dict[7])
                        {
                            // 3-2-5. Slope(=toio_dict[7])をRまで移動させる
                            var distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), R);
                            if(distance < 5)
                            {
                                navigator.handle.Stop();
                                phase += 1;
                                Debug.Log("phase6");
                            }
                            else{
                                navigator.handle.Move(20, 0, 20);
                            }
                        }
                    }

                    else if(phase == 7)
                    {
                        if(navigator.cube.id == toio_dict[7])
                        {
                            // 3-2-6. Slope(=toio_dict[7])をAngleCube7まで回転させる
                            int angle_diff = AngleCube7 - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                navigator.handle.Stop();
                                phase =3;
                                Debug.Log("phase7");
                            }
                            else
                            {
                                if(angle_diff > 0)
                                {
                                    navigator.handle.Move(0, 20, 20);
                                }
                                else
                                {
                                    navigator.handle.Move(0, -20, 20);
                                }
                            }
                        }
                    }

                    // 4. Slope(=toio_dict[7])を指定した距離まで移動
                    else if(phase == 8) 
                    {
                        if(navigator.cube.id == toio_dict[7])
                        {
                            float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosCube0);
                            if(distance < 30)
                            {
                                navigator.handle.Stop();
                                phase += 1;
                                Debug.Log("phase8");
                            }
                            else
                            {
                                navigator.handle.Move(-30, 0, 50);
                            }
                        }
                    }

                    // 5. phaseをリセットし，checkをインクリメントする
                    else if(phase > 8)
                    {
                        phase = 0;
                        Debug.Log("check: " + check);
                        check += 1;
                    }
                }
            }
        }

        string text = "";
        foreach(var cube in cm.syncCubes)
        {
            if(cube.id == toio_dict[7])
            {
                // PosCube0との距離を計算し，表示する
                float distance = Vector2.Distance(cube.pos, PosCube0);
                text += "distance: " + distance + "\n";
            }
        }

        if(text != "") this.label.text = text;
    }    

    // Startボタンが押されたら，StartClickedをtrueにする
    void StartButtonClicked()
    {
        StartClicked = true;
    }
        // 新しい座標を計算する
    Vector2 CalculateNewPosition(Vector2 pos, int angle, int distance)
    {
        float angleRadians = angle * Mathf.Deg2Rad;
        float x = pos.x + distance * Mathf.Cos(angleRadians);
        float y = pos.y + distance * Mathf.Sin(angleRadians);

        return new Vector2((int)x, (int)y);
    }
}
