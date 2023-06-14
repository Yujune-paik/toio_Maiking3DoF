using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

public class NewConnectingSimulator : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Real;

    int phase = 0;
    int check = 0;

    Vector2 PosCubeLeft = new Vector2(0, 0);
    Vector2 PosCubeRight = new Vector2(0, 0);

    int AngleCubeLeft = 0;
    int AngleCubeRight = 0;

    int L_cube=50;

    public int connectNum = 2;

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
    }

    void Update()
    {
        if (cm.synced)
        {
            foreach(var navigator in cm.syncNavigators)
            {
                if(check == 0)
                {
                    if(navigator.cube.localName == "Cube_left" && navigator.cube.x != 0 && navigator.cube.y != 0)
                    {
                        PosCubeLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                        AngleCubeLeft = navigator.cube.angle;
                        check += 1;
                        PosCubeRight = CalculateNewPosition(PosCubeLeft, AngleCubeLeft, L_cube);
                        Debug.Log("PosCubeRight: " + PosCubeRight.x + ", " + PosCubeRight.y);
                    }
                }
                else
                {
                    if(phase == 0)
                    {
                        if(navigator.cube.localName == "Cube_right")
                        {
                            var mv = navigator.Navi2Target(PosCubeRight.x, PosCubeRight.y, maxSpd:20, rotateTime:1000,tolerance:15).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase0");
                            }
                        }
                    }

                    else if(phase == 1)
                    {
                        if(navigator.cube.localName == "Cube_right")
                        {
                            Movement mv = navigator.handle.Rotate2Deg(AngleCubeLeft, rotateTime:2500, tolerance:0.1).Exec();
                            if(mv.reached)
                            {
                                phase += 1;
                                Debug.Log("phase1");
                            }
                        }
                    }
                    else if(phase == 2)
                    {
                        if(navigator.cube.localName == "Cube_right")
                        {
                            navigator.handle.Move(-15, 0, 1000);
                            phase += 1;
                            Debug.Log("phase2");
                        }
                        // Debug.Log("phase2 : Cube_Angle: " + navigator.cube.angle);
                    }
                    // else if(phase == 1)
                    // {
                    //     if(navigator.cube.localName == "Cube_right")
                    //     {
                    //         var angleDifference = Math.Abs(AngleCubeLeft - navigator.cube.angle);
                    //         if(angleDifference <= 3)
                    //         {
                    //             phase += 1;
                    //             Debug.Log("phase1");
                    //         }
                    //         else
                    //         {
                    //             if(AngleCubeLeft < navigator.cube.angle)
                    //             {
                    //                 navigator.handle.Move(0, -5, 10);
                    //             }
                    //             else
                    //             {
                    //                 navigator.handle.Move(0, 5, 10);
                    //             }
                    //         }
                    //     }
                    // }
                    // else if(phase == 2)
                    // {
                    //     if(navigator.cube.localName == "Cube_right")
                    //     {
                    //         var distance = Vector2.Distance(PosCubeRight, new Vector2(navigator.cube.x, navigator.cube.y));
                    //         if(distance <= 30)
                    //         {
                    //             phase += 1;
                    //             Debug.Log("phase2");
                    //         }
                    //         else
                    //         {
                    //             if(distance > 0)
                    //             {
                    //                 navigator.handle.Move(-5, 0, 10);
                    //             }
                    //             else
                    //             {
                    //                 navigator.handle.Move(5, 0, 10);
                    //             }
                    //         }
                    //     }
                    //     // Debug.Log("phase2 : Cube_Angle: " + navigator.cube.angle);
                    // }

                }
            }

            string text = "";
            foreach (var cube in cm.syncCubes)
            {
                if(cube.localName == "Cube_right")
                {
                    text += "Cube_right： (" + cube.x + "," + cube.y + "," + cube.angle + ")\n";
                    PosCubeRight = cube.pos;
                    AngleCubeRight = cube.angle;
                }
                else if(cube.localName == "Cube_left")
                {
                    text += "Cube_left： (" + cube.x + "," + cube.y + "," + cube.angle + ")\n";
                    PosCubeLeft = cube.pos;
                    
                }

                // CubeRightとCubeLeftの距離と角度の差を計算
                var distance = Vector2.Distance(PosCubeRight, PosCubeLeft);
                var angleDifference = Math.Abs(AngleCubeLeft - AngleCubeRight);
                text += "distance: " + distance + "\n";
                text += "angleDifference: " + angleDifference + "\n";
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
