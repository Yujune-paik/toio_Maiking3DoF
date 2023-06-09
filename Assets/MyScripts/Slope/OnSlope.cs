using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

public class OnSlope : MonoBehaviour
{  
    public Text label;

    CubeManager cm;
    public ConnectType connectType = ConnectType.Real;

    int phase = 0;

    int connectNum = 8;

    Dictionary<int, string> toio_dict = new Dictionary<int, string>();

    int num_cube = 0;
    int num_press = 2;

    bool isCoroutineRunning = false;

    // Slopeの登り切った平らなところの座標
    Vector2 pos_flat = new Vector2(242, 342);

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
            foreach (var navigator in cm.syncNavigators)
            {
                if (phase == 0)
                {
                    if (navigator.cube.id == toio_dict[num_cube] || navigator.cube.id == toio_dict[num_press])
                    {
                        navigator.handle.Move(-50, 0, 100);
                    }
                    
                    if (!isCoroutineRunning)
                    {
                        StartCoroutine(WaitAndIncrementPhase(2.5f));
                    }
                } 
                else if (phase == 1)
                {
                    if (navigator.cube.id == toio_dict[num_cube])
                    {
                        // The distance from the current position to the target position.
                        float distanceToTarget = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), pos_flat);
                        
                        //角度270度を維持しながらpos_flat付近に到着するまでMove(-30, 0, 10)を実行する
                        if (distanceToTarget > 5)
                        {
                            navigator.handle.Move(-30, 0, 50);
                        }
                        else
                        {
                            phase += 1;
                            Debug.Log("phase1");
                        }
                    }
                    else if(navigator.cube.id == toio_dict[num_press])
                    {
                        navigator.handle.Move(20,0,1000);
                    }
                }
                else if (phase == 2)
                {
                    if (navigator.cube.id == toio_dict[num_press])
                    {
                        navigator.handle.Move(50, 0, 1000);
                    }
                }
            }
        }

        string text = "";
           
        foreach (var cube in cm.syncCubes)
        {
            if (cube.id == toio_dict[num_cube]) text += "Cube" + num_cube + ": ";
            else if (cube.id == toio_dict[num_press]) text += "Cube" + num_press + ": ";

            text += "(" + cube.x + "," + cube.y + "," + cube.angle + ")\n";
        }
        if (text != "") this.label.text = text;
        
    }

    IEnumerator WaitAndIncrementPhase(float waitTime)
    {
        isCoroutineRunning = true;
        yield return new WaitForSeconds(waitTime);
        phase++;
        Debug.Log("phase" + phase);
        isCoroutineRunning = false;
    }
}
