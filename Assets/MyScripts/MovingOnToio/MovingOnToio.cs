using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;

public class MovingOnToio : MonoBehaviour
{
    CubeManager cm;
    public ConnectType connectType;

    Dictionary<int, string> toio_dict = new Dictionary<int, string>();
    Dictionary<int, Vector2> toio_pos = new Dictionary<int, Vector2>();

    public int top_toio_num = 0;  // 上のtoioの番号
    public int under_toio0_num = 1;  // 下のtoioの番号
    public int under_toio1_num = 2;  // 隣のtoioの番号

    public int distance_threshold = 30;
    public int rotate_threshold = 30;
    public int translate_speed = 100;
    public int rotate_speed = 100;

    async void Start()
    {
        cm = new CubeManager(connectType);
        await cm.MultiConnect(3);

        using (var sr = new StreamReader("Assets/toio_number.csv"))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var values = line.Split(',');
                toio_dict.Add(int.Parse(values[0]), values[1]);
                toio_pos.Add(int.Parse(values[0]), new Vector2(int.Parse(values[2]), int.Parse(values[3])));
            }
        }
    }

    void Update()
    {
        if (cm.synced)
        {
            Cube top_toio = GetCubeByID(toio_dict[top_toio_num]);
            Cube under_toio0 = GetCubeByID(toio_dict[under_toio0_num]);
            Cube under_toio1 = GetCubeByID(toio_dict[under_toio1_num]);

            if (top_toio != null && under_toio0 != null && under_toio1 != null)
            {
                float distance = CalculateDistance(under_toio0, under_toio1);
                if (distance <= distance_threshold)
                {
                    float orientation = CalculateOrientation(under_toio0, under_toio1);
                    orientation = (orientation == 360) ? 0 : orientation;

                    // Rotate top_toio to orientation angle
                    top_toio.RotateTo(toio_dict[top_toio_num], (int)orientation, rotate_speed, 0);

                    // Move top_toio forward by distance_threshold
                    top_toio.Move(distance_threshold, 0, translate_speed);

                    // Log toio under top_toio
                    foreach (var item in toio_dict)
                    {
                        Cube cube = GetCubeByID(item.Value);
                        if (cube != null)
                        {
                            if (CalculateDistance(top_toio, cube) <= distance_threshold)
                            {
                                Debug.Log("Top toio is on toio number " + item.Key);
                            }
                        }
                    }
                }
            }
        }
    }

    float CalculateDistance(Cube cube1, Cube cube2)
    {
        int dx = cube1.x - cube2.x;
        int dy = cube1.y - cube2.y;
        return Mathf.Sqrt(dx*dx + dy*dy);
    }

    float CalculateOrientation(Cube referenceCube, Cube neighboringCube)
    {
        float dx = neighboringCube.x - referenceCube.x;
        float dy = neighboringCube.y - referenceCube.y;

        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360;

        angle = Mathf.Floor(angle/10)*10;  // round to nearest 10
        return angle;
    }

    Cube GetCubeByID(string id)
    {
        foreach (Cube cube in cm.syncedCubes)
        {
            if (cube.id == id)
                return cube;
        }
        return null;
    }
}
