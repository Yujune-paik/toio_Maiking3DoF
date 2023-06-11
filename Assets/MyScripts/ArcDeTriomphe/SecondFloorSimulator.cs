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

public class SecondFloorSimulator : MonoBehaviour
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
    string FirstConnectionLeft = "Cube1"; // くっつかれるほう
    string FirstConnectionRight = "Cube0"; // くっつきに行くほう

    // 1と2がくっつく
    string SecondConnectionLeft = "Cube1"; // くっつかれるほう
    string SecondConnectionRight = "Cube2"; // くっつきに行くほう

    // 3の位置
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
            connectedCubes += cube.localName + " ";
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
                    if(navigator.cube.localName == FirstConnectionLeft && navigator.cube.x != 0 && navigator.cube.y != 0)
                    {
                        PosCubeLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                        AngleCubeLeft = navigator.cube.angle;
                        check += 1;
                        PosCubeRight = CalculateNewPosition(PosCubeLeft, AngleCubeLeft+90, L);
                        Debug.Log("PosCubeRight_First: " + PosCubeRight.x + ", " + PosCubeRight.y);
                    }
                }
                


                // // 1段目の真ん中のtoioを抜く
                // // 矢印キー(上)を押されたら3秒前進する
                // if (Input.GetKey(KeyCode.UpArrow))
                // {
                //     if(navigator.cube.localName == "Cube1")
                //     {
                //         navigator.handle.Move(50, 0, 100);
                //     }
                // }
            }

            string text = "";
            foreach (var cube in cm.syncCubes)
            {
                if(cube.localName == "Cube0") text += "Cube0" + "：(" + cube.x + "," + cube.y + "," + cube.angle + ")\n";
                
                // cube.posとPosCubeLeftの距離を計算する
                if(cube.localName == FirstConnectionRight) text += "Distance：" + Vector2.Distance(cube.pos, PosCubeLeft) + "\n";
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
