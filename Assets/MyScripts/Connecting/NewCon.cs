using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Threading.Tasks;

public class NewCon : MonoBehaviour
{
    public Text label;

    CubeManager cm;
    public ConnectType connectType;

    int check = 0;
    int phase = 0;
    bool s = false;

    Vector2 PosLeft = new Vector2(246, 268);
    Vector2 PosRight = new Vector2(0, 0);
    Vector2 PosCube = new Vector2(0, 0);

    int CubeLeft = 1;
    int CubeRight = 0;
    int CubeRight2 = 2;

    int AngleLeft = 0;
    int AngleRight = 0;

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

    int L = 50;
    int L_Cube = 30;
    
    int connectNum = 2;

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
                if(check == 0)
                {
                    // CubeLeftにCubeRightがくっつく
                    // CubeRightの(位置,向き) = (CubeLeftの位置+90°の位置, CubeLeftの向き-90°)
                    
                    // 1. まずはCubeRightの移動後の位置と角度を計算する
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[CubeLeft] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            PosLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                            AngleLeft = navigator.cube.angle;
                            PosRight = CalculateNewPosition(PosLeft, AngleLeft + 90, L);
                            AngleRight = AngleLeft - 90;
                            if(AngleRight < 0)
                            {
                                AngleRight += 360;
                            }
                            else if(AngleRight > 360)
                            {
                                AngleRight -= 360;
                            }
                            Debug.Log("PosRight: " + PosRight.x + ", " + PosRight.y);
                            Debug.Log("AngleRight: " + AngleRight);
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

                    // 3-2. CubeRightを軌道修正する
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[CubeRight])
                        {
                            // 3-2-1. 必要な情報を変数に保存する
                            // 3-2-1-1. CubeLeftの位置と角度をそれぞれx1, y1, theta1に保存
                            // 3-2-1-2. CubeRightの位置と角度をそれぞれx2, y2, theta2に保存
                            // CubeLeftの情報はPosLeftとAngleLeftから取得，CubeRightの情報はnavigator.cubeから取得
                            x1 = (int)PosLeft.x;
                            y1 = (int)PosLeft.y;
                            theta1 = AngleLeft;

                            x2 = navigator.cube.x;
                            y2 = navigator.cube.y;
                            theta2 = navigator.cube.angle;

                            // 3-2-1-3. alpha, betaの値は90, 0とする (右にくっつき，前進してくっつく場合)
                            alpha = 90;
                            beta = 0;
                        

                            // 3-2-2. 3-2-1で取得した変数をもとに，計算する

                            // 3-2-2-1. d = numerator/denominatorを計算する
                            // numerator = y1-y2+(L_Cube/2)*sin(theta1+alpha)-tan(theta2+beta)*(x1-x2+(L_Cube/2)*cos(theta1+alpha))
                            // denominator = tan(theta2+beta)*cos(theta1+alpha-90)-sin(theta1+alpha-90)
                            float numerator = y1 - y2 + (L_Cube / 2) * Mathf.Sin((theta1+alpha)*Mathf.Deg2Rad) - Mathf.Tan((theta2+beta)*Mathf.Deg2Rad) * (x1 - x2 + (L_Cube / 2) * Mathf.Cos((theta1+alpha)*Mathf.Deg2Rad));
                            float denominator = Mathf.Tan((theta2+beta)*Mathf.Deg2Rad) * Mathf.Cos((theta1+alpha-90)*Mathf.Deg2Rad) - Mathf.Sin((theta1+alpha-90)*Mathf.Deg2Rad);
                            // dはint型であることに注意してください
                            d = (int)(numerator / denominator);

                            // 3-2-2-2. R=(x2+0.8*|d|*cos(gamma), y2+0.8*|d|*sin(gamma))を計算する
                            if(d<0) gamma = theta1 + alpha - 90;
                            else gamma = theta1 + alpha + 90;

                            // gammaが360以上の場合は360を引く
                            if(gamma >= 360) gamma -= 360;
                            // gammaが0未満の場合は360を足す
                            else if(gamma < 0) gamma += 360;

                            float angleRadians = gamma * Mathf.Deg2Rad;
                            R = new Vector2((int)(x2 + 0.8f * Mathf.Abs(d) * Mathf.Cos(angleRadians)), (int)(y2 + 0.8f * Mathf.Abs(d) * Mathf.Sin(angleRadians)));

                            phase += 1;
                            // theta1, alpha, theta2, beta, x1, x2, y1, y2を表示
                            Debug.Log("theta1: " + theta1);
                            Debug.Log("alpha: " + alpha);
                            Debug.Log("theta2: " + theta2);
                            Debug.Log("beta: " + beta);
                            Debug.Log("x1: " + x1);
                            Debug.Log("x2: " + x2);
                            Debug.Log("y1: " + y1);
                            Debug.Log("y2: " + y2);

                            Debug.Log("分子: " + numerator);
                            Debug.Log("分母: " + denominator);
                            Debug.Log("d: " + d);
                            Debug.Log("R:( " + R.x + ", " + R.y + ")");
                            Debug.Log("phase3");
                        }
                    }

                    else if(phase == 4)
                    {
                        // 3-2-3. |d|<3なら，次のphaseへ進む．そうでなければ，以下の処理を行う．
                        if(Mathf.Abs(d) < 3)
                        {
                            phase = 8;
                            Debug.Log("phase4");
                        }
                        else
                        {
                            phase += 1;
                        }
                    }
                    else if(phase == 5)
                    {
                        if(navigator.cube.id == toio_dict[CubeRight])
                        {            
                            // 3-2-4. CubeRightをgammaまで回転させる
                            int angle_diff = gamma - navigator.cube.angle;
                            Debug.Log("angle_diff: " + angle_diff);
                            if(Math.Abs(angle_diff) < 5)
                            {
                                phase += 1;
                                Debug.Log("phase5");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 20, 20);
                            }
                            else
                            {
                                navigator.handle.Move(0, -20, 20);
                            }
                        }
                    }
                    else if(phase == 6)
                    {
                        if(navigator.cube.id == toio_dict[CubeRight])
                        {
                            // 3-2-5. CubeRightをRまで移動させる
                            var distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), R);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase6");
                            }
                            else{
                                navigator.handle.Move(20, 0, 20);
                            }
                        }
                    }

                    else if(phase == 7)
                    {
                        if(navigator.cube.id == toio_dict[CubeRight])
                        {
                            // 3-2-6. CubeRightをAngleRightまで回転させる
                            int angle_diff = AngleRight - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase =3;
                                Debug.Log("phase7");
                            }
                            else
                            {
                                if(angle_diff > 0)
                                {
                                    navigator.handle.Move(0, 20, 20);
                                }
                                else
                                {
                                    navigator.handle.Move(0, -20, 20);
                                }
                            }
                        }
                    }

                    // 3-3. 指定した距離まで移動
                    else if(phase == 8) 
                    {
                        if(navigator.cube.id == toio_dict[CubeRight])
                        {
                            float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosLeft);
                            if(distance < 28)
                            {
                                phase += 1;
                                Debug.Log("phase8");
                            }
                            else
                            {
                                navigator.handle.Move(30, 0, 50);
                            }
                        }
                    }
                    else if(phase > 8)
                    {
                        phase = 0;
                        Debug.Log("check: " + check);
                        check += 1;
                    }
                }
                
                if(check == 1)
                {
                    // CubeLeftにCubeRightがくっつく
                    // CubeRightの(位置,向き) = (CubeLeftの位置-90°の位置, CubeLeftの向き-90°)
                    //
                    // 1. まずはCubeRightの移動後の位置と角度を計算する
                    if(phase == 0)
                    {
                        if(navigator.cube.id == toio_dict[CubeLeft] && navigator.cube.x != 0 && navigator.cube.y != 0)
                        {
                            PosLeft = new Vector2(navigator.cube.x, navigator.cube.y);
                            AngleLeft = navigator.cube.angle;
                            PosRight = CalculateNewPosition(PosLeft, AngleLeft - 90, L);
                            AngleRight = AngleLeft - 90;
                            if(AngleRight < 0)
                            {
                                AngleRight += 360;
                            }
                            else if(AngleRight > 360)
                            {
                                AngleRight -= 360;
                            }
                            Debug.Log("PosRight: " + PosRight.x + ", " + PosRight.y);
                            Debug.Log("AngleRight: " + AngleRight);
                            phase += 1;
                        }
                    }

                    // 2. CubeRightの座標へ移動させる
                    else if(phase == 1) 
                    {
                        if(navigator.cube.id == toio_dict[CubeRight2])
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
                        if(navigator.cube.id == toio_dict[CubeRight2])
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

                    // 3-2. CubeRightを軌道修正する
                    else if(phase == 3)
                    {
                        if(navigator.cube.id == toio_dict[CubeRight2])
                        {
                            // 3-2-1. 必要な情報を変数に保存する
                            // 3-2-1-1. CubeLeftの位置と角度をそれぞれx1, y1, theta1に保存
                            // 3-2-1-2. CubeRightの位置と角度をそれぞれx2, y2, theta2に保存
                            // CubeLeftの情報はPosLeftとAngleLeftから取得，CubeRightの情報はnavigator.cubeから取得
                            x1 = (int)PosLeft.x;
                            y1 = (int)PosLeft.y;
                            theta1 = AngleLeft;

                            x2 = navigator.cube.x;
                            y2 = navigator.cube.y;
                            theta2 = navigator.cube.angle;

                            // 3-2-1-3. alpha, betaの値は90, 0とする (左にくっつき，後退してくっつく場合)
                            alpha = 270;
                            beta = 180;
                        

                            // 3-2-2. 3-2-1で取得した変数をもとに，計算する
                            // 3-2-2-1. d = numerator/denominatorを計算する
                            // numerator = y1-y2+(L_Cube/2)*sin(theta1+alpha)-tan(theta2+beta)*(x1-x2+(L_Cube/2)*cos(theta1+alpha))
                            // denominator = tan(theta2+beta)*cos(theta1+alpha-90)-sin(theta1+alpha-90)
                            float numerator = y1 - y2 + (L_Cube / 2) * Mathf.Sin((theta1+alpha)*Mathf.Deg2Rad) - Mathf.Tan((theta2+beta)*Mathf.Deg2Rad) * (x1 - x2 + (L_Cube / 2) * Mathf.Cos((theta1+alpha)*Mathf.Deg2Rad));
                            float denominator = Mathf.Tan((theta2+beta)*Mathf.Deg2Rad) * Mathf.Cos((theta1+alpha-90)*Mathf.Deg2Rad) - Mathf.Sin((theta1+alpha-90)*Mathf.Deg2Rad);
                            // dはint型であることに注意してください
                            d = (int)(numerator / denominator);

                            // 3-2-2-2. R=(x2+0.8*|d|*cos(gamma), y2+0.8*|d|*sin(gamma))を計算する
                            if(d<0) gamma = theta1 + alpha - 90;
                            else gamma = theta1 + alpha + 90;

                            // gammaが360以上の場合は360を引く
                            if(gamma >= 360) gamma -= 360;
                            // gammaが0未満の場合は360を足す
                            else if(gamma < 0) gamma += 360;

                            float angleRadians = gamma * Mathf.Deg2Rad;
                            R = new Vector2((int)(x2 + 0.8f * Mathf.Abs(d) * Mathf.Cos(angleRadians)), (int)(y2 + 0.8f * Mathf.Abs(d) * Mathf.Sin(angleRadians)));

                            phase += 1;
                            // theta1, alpha, theta2, beta, x1, x2, y1, y2を表示
                            Debug.Log("theta1: " + theta1);
                            Debug.Log("alpha: " + alpha);
                            Debug.Log("theta2: " + theta2);
                            Debug.Log("beta: " + beta);
                            Debug.Log("x1: " + x1);
                            Debug.Log("x2: " + x2);
                            Debug.Log("y1: " + y1);
                            Debug.Log("y2: " + y2);

                            Debug.Log("分子: " + numerator);
                            Debug.Log("分母: " + denominator);
                            Debug.Log("d: " + d);
                            Debug.Log("R:( " + R.x + ", " + R.y + ")");
                            Debug.Log("phase3");
                        }
                    }

                    else if(phase == 4)
                    {
                        // 3-2-3. |d|<3なら，次のphaseへ進む．そうでなければ，以下の処理を行う．
                        if(Mathf.Abs(d) < 3)
                        {
                            phase = 8;
                            Debug.Log("phase4");
                        }
                        else
                        {
                            phase += 1;
                        }
                    }
                    else if(phase == 5)
                    {
                        if(navigator.cube.id == toio_dict[CubeRight2])
                        {            
                            // 3-2-4. CubeRightをgammaまで回転させる
                            int angle_diff = gamma - navigator.cube.angle;
                            Debug.Log("angle_diff: " + angle_diff);
                            if(Math.Abs(angle_diff) < 5)
                            {
                                phase += 1;
                                Debug.Log("phase5");
                            }
                            else if(angle_diff > 0)
                            {
                                navigator.handle.Move(0, 20, 20);
                            }
                            else
                            {
                                navigator.handle.Move(0, -20, 20);
                            }
                        }
                    }
                    else if(phase == 6)
                    {
                        if(navigator.cube.id == toio_dict[CubeRight2])
                        {
                            // 3-2-5. CubeRightをRまで移動させる
                            var distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), R);
                            if(distance < 5)
                            {
                                phase += 1;
                                Debug.Log("phase6");
                            }
                            else{
                                navigator.handle.Move(20, 0, 20);
                            }
                        }
                    }

                    else if(phase == 7)
                    {
                        if(navigator.cube.id == toio_dict[CubeRight2])
                        {
                            // 3-2-6. CubeRightをAngleRightまで回転させる
                            int angle_diff = AngleRight - navigator.cube.angle;
                            if(Math.Abs(angle_diff) < 3)
                            {
                                phase =3;
                                Debug.Log("phase7");
                            }
                            else
                            {
                                if(angle_diff > 0)
                                {
                                    navigator.handle.Move(0, 20, 20);
                                }
                                else
                                {
                                    navigator.handle.Move(0, -20, 20);
                                }
                            }
                        }
                    }

                    // 3-3. 指定した距離まで移動
                    else if(phase == 8) 
                    {
                        if(navigator.cube.id == toio_dict[CubeRight2])
                        {
                            float distance = Vector2.Distance(new Vector2(navigator.cube.x, navigator.cube.y), PosLeft);
                            if(distance < 28)
                            {
                                phase += 1;
                                Debug.Log("phase8");
                            }
                            else
                            {
                                navigator.handle.Move(-30, 0, 50);
                            }
                        }
                    }
                    else if(phase > 8)
                    {
                        phase = 0;
                        Debug.Log("check: " + check);
                        check += 1;
                    }
                }
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
