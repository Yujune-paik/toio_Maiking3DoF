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
public class ThirdFloor : MonoBehaviour
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
    int check = 0;
    bool s = false;

    bool isCoroutineRunning = false;

    int L = 60; // Cube同士の接続に用いる距離
    int L_Cube = 30; //Cubeの幅
    int L_Slope = 60; // Slopeの前にCubeが配置するときに用いる距離

    int connectNum = 4;

    // toio_dict[0]の位置と角度
    Vector2 PosCube0 = new Vector2(0, 0);
    int AngleCube0 = 0;

    // toio_dict[1]の位置と角度
    Vector2 PosCube1 = new Vector2(0, 0);
    int AngleCube1 = 0;

    // toio_dict[2]の位置と角度
    Vector2 PosCube2 = new Vector2(0, 0);
    int AngleCube2 = 0;

    // toio_dict[3]の位置と角度
    Vector2 PosCube3 = new Vector2(0, 0);
    int AngleCube3 = 0;

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
    Vector2 PosSlopeStart = new Vector2(255, 155);

    // Slopeの登り切った平らなところの座標
    Vector2 PosFlat = new Vector2(255, 185);

    float timeSinceLastOrder = 0f;
    float orderInterval = 0.1f;

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>(); // Cubeの番号とIDの対応付け
    Dictionary<int, Vector2> toio_pos = new Dictionary<int, Vector2>(); // Cubeの番号と座標の対応付け

    // Start is called before the first frame update
    void Start()
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
                if(phase == 3)
                {
                    
                }
            }
        }
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
