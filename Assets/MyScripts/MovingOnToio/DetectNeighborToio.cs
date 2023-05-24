using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;

// *******************************************************************************************
// 基準となるtoioに対して隣のtoioがどのような角度にくっついているのか検知するプログラム(実機 version)
// *******************************************************************************************

public class DetectNeighborToio : MonoBehaviour
{
    CubeManager cm;
    public ConnectType connectType;

    // Cube ID of the reference Toio
    public int reference_toio_id = 1;

    // Distance threshold
    public int distance_threshold = 30;

    // CSV dictionary
    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

    int connectNum = 5; // Connect to 5 cubes

    async void Start()
    {
        // Cube Manager Connection
        cm = new CubeManager(connectType);
        await cm.MultiConnect(connectNum);

        // Cube ID and name mapping
        using (var sr = new StreamReader("Assets/toio_number.csv"))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var values = line.Split(',');
                toio_dict.Add(int.Parse(values[0]), values[1]);
            }
        }
    }

    void Update()
    {
        if (cm.synced)
        {
            Cube referenceCube = null;
            foreach (var cube in cm.syncCubes)
            {
                if (cube.id == toio_dict[reference_toio_id])
                {
                    referenceCube = cube;
                    break;
                }
            }

            if (referenceCube != null)
            {
                foreach (var cube in cm.syncCubes)
                {
                    if (cube.id != toio_dict[reference_toio_id])
                    {
                        float distance = CalculateDistance(referenceCube, cube);
                        if (distance <= distance_threshold)
                        {
                            float orientation = CalculateOrientation(referenceCube, cube);
                            orientation = (orientation == 360) ? 0 : orientation;  // Handle 360 degrees as 0 degrees

                            int cubeNumber = GetCubeNumberFromID(cube.id);
                            Debug.Log("隣のtoioは、番号が" + "toio_dict[" + cubeNumber + "]" + "で、" + orientation + "°の位置にいます");
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

        // Calculate the angle in degrees
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360;

        // Adjust by the reference cube's own orientation
        angle = (angle - referenceCube.angle + 360) % 360;

        // Round to the nearest 90 degrees
        angle = Mathf.Round(angle / 90) * 90;

        return angle;
    }

    int GetCubeNumberFromID(string id)
    {
        foreach(var item in toio_dict)
        {
            if (item.Value == id)
                return item.Key;
        }

        return -1; // Return -1 if id not found in dictionary
    }
}
