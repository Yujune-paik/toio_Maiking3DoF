using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

public class ThreeStepSlope : MonoBehaviour
{
        public Text label;
    public InputField InputFieldX;
    public InputField InputFieldY;
    public InputField InputFieldAngle;
    public Button StartButton;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Simulator;

    int phase = 0;
    int check = 0;
    bool s = false;

    // bool isCoroutineRunning = false;

    int L = 45; // Cube同士の接続に用いる距離
    int L_Cube = 30; // Cubeの大きさ

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

    // toioの角度
    int AngleCube0 = 0;
    int AngleCube1 = 0;
    int AngleCube2 = 0;

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

    // Slopeの上り切った平らなところの座標
    Vector2 PosFlat = new Vector2(242, 342);

    float timeSinceLastOrder = 0f;
    float orderInterval = 0.1f;

    // CSVファイルの読み込み
    Dictionary<int, string> toio_dict = new Dictionary<int, string>(); // Cubeの番号とIDの対応付け
    Dictionary<int, Vector2> toio_pos = new Dictionary<int, Vector2>(); // Cubeの番号と座標の対応付け
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
