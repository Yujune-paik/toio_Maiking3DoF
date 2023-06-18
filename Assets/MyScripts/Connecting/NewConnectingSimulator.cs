using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class NewConnectingSimulator : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType;

    int check = 0;
    int phase = 0;

    Vector2 PosLeft = new Vector2(0, 0);
    Vector2 PosRight = new Vector2(0, 0);
    Vector2 PosCube = new Vector2(0, 0);

    string CubeLeft = "Cube_left";
    string CubeRight = "Cube_right";
    string Cube = "Cube";

    int AngleLeft = 0;
    int AngleRight = 0;
    int AngleCube = 0;

    int L = 50;
    
    int connectNum = 3;

    float timeSinceLastOrder = 0f;
    float orderInterval = 0.1f;

    async void Start()
    {
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);
    }

    void Update()
    {
        timeSinceLastOrder += Time.deltaTime;

        if (cm.synced && timeSinceLastOrder >= orderInterval)
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
                        if(navigator.cube.localName == CubeLeft && navigator.cube.x != 0 && navigator.cube.y != 0)
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
                        if(navigator.cube.localName == CubeRight)
                        {
                            var mv = navigator.Navi2Target(PosRight.x, PosRight.y, maxSpd:50).Exec();
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
                        if(navigator.cube.localName == CubeRight)
                        {
                            int angle_diff = AngleRight - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 5)
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

                    // 3-2. 指定した距離まで移動
                    else if(phase == 3) 
                    {
                        if(navigator.cube.localName == CubeRight)
                        {
                            float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosLeft);
                            if(distance < 25)
                            {
                                phase += 1;
                                Debug.Log("phase3");
                            }
                            else
                            {
                                navigator.handle.Move(10, 0, 20);
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
                if(check == 1)
                {
                    // 1. Cubeの移動後の位置と角度を計算する
                    if(phase == 0)
                    {
                        if(navigator.cube.localName == CubeLeft && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            PosLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                            AngleLeft = navigator.cube.angle;
                            PosCube = CalculateNewPosition(PosLeft, AngleLeft - 90, L);
                            AngleCube = AngleLeft - 90;
                            Debug.Log("PosCube: " + PosCube.x + ", " + PosCube.y);
                            phase += 1;
                        }
                    }
                    
                    // 2. Cubeの座標へ移動させる
                    else if(phase == 1) 
                    {
                        if(navigator.cube.localName == Cube)
                        {
                            var mv = navigator.Navi2Target(PosCube.x, PosCube.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }

                    // 3-1. 指定した角度まで回転
                    else if(phase == 2) 
                    {
                        if(navigator.cube.localName == Cube)
                        {
                            int angle_diff = AngleCube - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 5)
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

                    // 3-2. 指定した距離まで移動
                    else if(phase == 3) 
                    {
                        if(navigator.cube.localName == Cube)
                        {
                            float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosLeft);
                            if(distance < 25)
                            {
                                phase += 1;
                                Debug.Log("phase3");
                            }
                            else
                            {
                                navigator.handle.Move(-10, 0, 20);
                            }
                        }
                    }
                    else if(phase > 3)
                    {
                        check += 1;
                        phase = 0;
                    }
                }
            }

            string text = "";
            foreach (var cube in cm.syncCubes)
            {
                if(cube.localName == CubeRight) text += "CubeRight: ";
                else if(cube.localName == CubeLeft) text += "CubeLeft: ";

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
