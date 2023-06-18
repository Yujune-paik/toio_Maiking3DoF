using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

public class NewConnecting : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType;

    int check = 0;
    int phase = 0;
    bool s = false;

    Vector2 PosLeft = new Vector2(0, 0);
    Vector2 PosRight = new Vector2(0, 0);
    Vector2 PosCube = new Vector2(0, 0);

    int CubeLeft = 6;
    int CubeRight = 4;
    int Cube = 3;

    int AngleLeft = 0;
    int AngleRight = 0;
    int AngleCube = 0;

    int L = 50;
    
    int connectNum = 3;

    float timeSinceLastOrder = 0f;
    float orderInterval = 0.1f;

    Dictionary<int, string> toio_dict = new Dictionary<int, string>();
    
    async void Start()
    {
        using (var sr = new StreamReader("Assets/toio_number.csv"))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var values = line.Split(',');
                toio_dict.Add(int.Parse(values[0]), values[1]);
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
    }

    void Update()
    {
        timeSinceLastOrder += Time.deltaTime;

        if (cm.synced && timeSinceLastOrder >= orderInterval && s)
        {
            timeSinceLastOrder = 0f; 

            foreach(var navigator in cm.syncNavigators)
            {      
                // CubeLeftにCubeRightがくっつく
                // CubeRightの(位置,向き) = (CubeLeftの位置+90°の位置, CubeLeftの向き-90°)
                if(check == 0)
                {
                    // 1. まずはCubeRightの移動後の位置と角度を計算する
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[CubeLeft] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            PosLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                            AngleLeft = navigator.cube.angle;
                            PosRight = CalculateNewPosition(PosLeft, AngleLeft + 90, L);
                            AngleRight = AngleLeft - 90;
                            Debug.Log("PosRight: " + PosRight.x + ", " + PosRight.y);
                            phase += 1;
                        }
                    }

                    // 2. CubeRightの座標へ移動させる
                    else if(phase == 1) 
                    {
                        if(navigator.cube.id == toio_dict[CubeRight])
                        {
                            var mv = navigator.Navi2Target(PosRight.x, PosRight.y, maxSpd:5).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }

                    // 3-1. 指定した角度まで回転
                    else if(phase == 2) 
                    {
                        if(navigator.cube.id == toio_dict[CubeRight])
                        {
                            int angle_diff = AngleRight - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase += 1;
                                Debug.Log("phase2");
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

                    // 3-2. 指定した距離まで移動
                    else if(phase == 3) 
                    {
                        if(navigator.cube.id == toio_dict[CubeRight])
                        {
                            float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosLeft);
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
                    else if(phase > 3)
                    {
                        check += 1;
                        phase = 0;
                    }
                }

                // CubeLeftにCubeがくっつく
                // Cubeの(位置,向き) = (CubeLeftの位置-90°の位置, CubeLeftの向き-90°)
                // if(check == 1)
                // {
                //     // 1. まずはCubeRightの移動後の位置と角度を計算する
                //     if(phase == 0)
                //     {
                //         if(navigator.cube.id == toio_dict[CubeLeft] && navigator.cube.x != 0 && navigator.cube.y != 0)
                //         {
                //             PosLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                //             AngleLeft = navigator.cube.angle;
                //             PosCube = CalculateNewPosition(PosLeft, AngleLeft - 90, L);
                //             AngleCube = AngleLeft - 90;
                //             Debug.Log("PosCube: " +PosCube.x + ", " +PosCube.y);
                //             phase += 1;
                //         }
                //     }

                //     // 2. CubeRightの座標へ移動させる
                //     else if(phase == 1) 
                //     {
                //         if(navigator.cube.id == toio_dict[Cube])
                //         {
                //             var mv = navigator.Navi2Target(PosCube.x, PosCube.y, maxSpd:50).Exec();
                //             if(mv.reached)
                //             {
                //                 phase += 1;
                //                 Debug.Log("phase1");
                //             }
                //         }
                //     }

                //     // 3-1. 指定した角度まで回転
                //     else if(phase == 2) 
                //     {
                //         if(navigator.cube.id == toio_dict[Cube])
                //         {
                //             int angle_diff = AngleCube - navigator.cube.angle;
                //             if(Math.Abs(angle_diff) < 5)
                //             {
                //                 phase += 1;
                //                 Debug.Log("phase2");
                //             }
                //             else if(angle_diff > 0)
                //             {
                //                 navigator.handle.Move(0, 10, 20);
                //             }
                //             else
                //             {
                //                 navigator.handle.Move(0, -10, 20);
                //             }
                //         }
                //     }

                //     // 3-2. 指定した距離まで移動
                //     else if(phase == 3) 
                //     {
                //         if(navigator.cube.id == toio_dict[Cube])
                //         {
                //             float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosLeft);
                //             if(distance < 25)
                //             {
                //                 phase += 1;
                //                 Debug.Log("phase3");
                //             }
                //             else
                //             {
                //                 navigator.handle.Move(10, 0, 20);
                //             }
                //         }
                //     }
                //     else if(phase > 3)
                //     {
                //         check += 1;
                //         phase = 0;
                //     }
                // }
            }

            string text = "";
            foreach (var cube in cm.syncCubes)
            {
                if(cube.id == toio_dict[CubeRight]) text += "CubeRight: ";
                else if(cube.id == toio_dict[CubeLeft]) text += "CubeLeft: ";

                text += "(" + cube.x + "," + cube.y + "," + cube.angle + ")\n";
            }
            if (text != "") this.label.text = text;
        }
    }

    Vector2 CalculateNewPosition(Vector2 pos, int angle, int distance)
    {
        float angleRadians = angle * Mathf.Deg2Rad;
        float x = pos.x + distance * Mathf.Cos(angleRadians);
        float y = pos.y + distance * Mathf.Sin(angleRadians);

        return new Vector2((int)x, (int)y);
    }
}
