using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks; // 追加

public class ConnectionCounter : MonoBehaviour
{
    public Text connectionCounterLabel;
    public int desiredConnectionNumber = 5;

    CubeManager cm;
    public ConnectType connectType;

    async void Start()
    {
        cm = new CubeManager(connectType);

        // キューブの複数台接続
        await ConnectToioCubes();
    }

    async Task ConnectToioCubes()
    {
        while (cm.syncCubes.Count < desiredConnectionNumber)
        {
            await cm.MultiConnect(1);
        }
    }

    void Update()
    {
        if (cm.synced)
        {
            // 接続台数をコンソールに表示する
            Debug.Log(cm.syncCubes.Count);
        }
    }
}
