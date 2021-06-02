using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlockController : MonoBehaviour
{
    // Start is called before the first frame update
    [Tooltip("群れが動く範囲設定に用いるGameObject。高速化のためにサイズは整数にしてほしい")]
    public GameObject BackGround;
    [Tooltip("群れの一体のプレハブ")]
    public GameObject Bird;
    [Tooltip("群れの頭数")]
    [Range(10,400)]
    public int FlockSize = 16;
    [Tooltip("近すぎる仲間を避ける重み付け係数")]
    public float Sep = 1f;
    [Tooltip("仲間に速度を合わせる重み付け係数")]
    public float Align = 1f;
    [Tooltip("群れの中心に移動しようとする重み付け係数")]
    public float Coh = 1f;
    [Tooltip("速度上限")]
    public float MaxSpeed = 5f;
    [Tooltip("近づきすぎると離れるときの距離のしきい値")]
    public float DistThreshold = 10f;

    //群れの初期位置を設定するときに範囲指定で使用する
    private float BackGroundSize;
    private List<GameObject> Flocks;
    private List<Vector3> FlocksVelocitys;
    private List<Vector3> OldPos;
    private List<Vector3> OldVelo;


    void Start()
    {
        if (BackGround is null) return;
        if (Bird is null) return;

        BackGroundSize = (BackGround.transform.localScale.x - 1) / 2;
        //Debug.Log(BackGroundSize);

        Flocks = new List<GameObject>();
        FlocksVelocitys = new List<Vector3>();
        OldPos = new List<Vector3>();
        OldVelo = new List<Vector3>();

        for(int i=0; i<FlockSize; i++)
        {
            IncrementFlock();
        }
    }

    void IncrementFlock()
    {
        Vector3 pos = new Vector3(Random.Range(-1 * BackGroundSize, BackGroundSize),
                                      Random.Range(-1 * BackGroundSize, BackGroundSize),
                                      0);

        Vector3 dir = new Vector3(Random.Range(-0.1f, 0.1f),
                                  Random.Range(-0.1f, 0.1f),
                                  0);

        GameObject obj = Instantiate(Bird, pos, Quaternion.identity);

        Flocks.Add(obj);
        FlocksVelocitys.Add(dir);
        OldPos.Add(pos);
        OldVelo.Add(dir);
    }

    void DecrementFlock()
    {
        GameObject.Destroy(Flocks[Flocks.Count - 1]);
        Flocks.RemoveAt(Flocks.Count - 1);
        FlocksVelocitys.RemoveAt(FlocksVelocitys.Count - 1);
        OldPos.RemoveAt(OldPos.Count - 1);
        OldVelo.RemoveAt(OldVelo.Count - 1);
    }

    /// <summary>
    /// 仲間との距離が遠いときは緩やかに、近づきすぎると急旋回して避けるようにする
    /// </summary>
    /// <param name="idx">何番目の鳥についてか</param>
    /// <returns></returns>
    Vector3 Separation(int idx)
    {
        Vector3 vec = Vector3.zero;
        for(int i=0; i<Flocks.Count; i++)
        {
            if (i == idx) continue;
            Vector3 diff = OldPos[idx] - OldPos[i];
            if (diff.sqrMagnitude < Mathf.Pow( DistThreshold ,2) )
            {
                if(diff.sqrMagnitude != 0)
                {
                    vec += diff / diff.sqrMagnitude;
                }

            }
                
        }
        return vec / (Flocks.Count - 1);
    }

    /// <summary>
    /// 群れ全体の速度ベクトルの平均に合わせるようにする
    /// </summary>
    /// <param name="idx">何番目の鳥についてか</param>
    /// <returns></returns>
    Vector3 Alignment(int idx)
    {
        Vector3 vel = Vector3.zero;
        int cnt = 0;

        for (int i = 0; i < Flocks.Count; i++)
        {
            if (i == idx) continue;
            Vector3 diff = OldPos[idx] - OldPos[i];
            if (diff.sqrMagnitude < Mathf.Pow(DistThreshold, 2))
            {
                vel += OldVelo[i];
                cnt++;
            }
                
        }
        if(cnt!=0)
            vel /= cnt;
        return (vel - OldVelo[idx]);
    }

    /// <summary>
    /// 群れの中心に移動しようとする。群れの重心位置へ向かうベクトルを返す
    /// </summary>
    /// <param name="idx">何番目の鳥についてか</param>
    /// <returns></returns>
    Vector3 Cohension(int idx)
    {
        Vector3 pos = Vector3.zero;
        int cnt = 0;

        for (int i = 0; i < Flocks.Count; i++)
        {
            if (i == idx) continue;
            Vector3 diff = OldPos[idx] - OldPos[i];
            if (diff.sqrMagnitude < Mathf.Pow(DistThreshold, 2))
            {
                pos += OldPos[i];
                cnt++;
            }
                
        }
        if(cnt!=0)
            pos /= cnt;
        return (pos - OldPos[idx]);
    }
    
    enum Dir
    {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
    }

    Dir GetDir(Vector3 velo)
    {
        float norm = velo.magnitude;
        float Rad = Mathf.Acos(Mathf.Clamp(velo.x / norm, -1f, 1f));
        float sin = Mathf.Clamp(velo.y / norm, -1f, 1f);

        if (sin < 0) Rad *= -1;

        float pi = Mathf.PI;

        if (Rad >= -pi / 8 && Rad < pi / 8) return Dir.Right;
        else if (Rad >= pi / 8 && Rad < 3 * pi / 8) return Dir.UpRight;
        else if (Rad >= 3 * pi / 8 && Rad < 5 * pi / 8) return Dir.Up;
        else if (Rad >= 5 * pi / 8 && Rad < 7 * pi / 8) return Dir.UpLeft;
        else if (Rad < -pi / 8 && Rad >= -3 * pi / 8) return Dir.DownRight;
        else if (Rad < -3 * pi / 8 && Rad >= -5 * pi / 8) return Dir.Down;
        else if (Rad < -5 * pi / 8 && Rad >= -7 * pi / 8) return Dir.DownLeft;
        else return Dir.Left;

    }

    // Update is called once per frame
    void Update()
    {
        if(Flocks.Count<FlockSize) //群れのサイズに到達していなければ群れを増やす
        {
            IncrementFlock();
        }
        else if (Flocks.Count > FlockSize)
        {
            DecrementFlock();
        }

        for (int i=0; i<Flocks.Count; i++)
        {
            //Debug.Log("Bird" + i);
            Vector3 acceralation = Sep * Separation(i) + Align * Alignment(i) + Coh * Cohension(i);

            Vector3 noise = new Vector3(Random.Range(-1f, 1f),
                                      Random.Range(-1f, 1f),
                                      0);

            FlocksVelocitys[i] += (acceralation + noise) * Time.deltaTime;

            Vector3 dest = Flocks[i].transform.position + FlocksVelocitys[i] * Time.deltaTime;
            //移動先がマップ外なら、右もしくは左に直角に曲がる
            if (dest.x < -1 * BackGroundSize || dest.x > BackGroundSize ||
                dest.y < -1 * BackGroundSize || dest.y > BackGroundSize)
            {
                Vector3 swap = new Vector3(FlocksVelocitys[i].y, FlocksVelocitys[i].x, 0);
                FlocksVelocitys[i] = swap;

                float rnd = Random.Range(0, 1);
                if (rnd > 0.5f) FlocksVelocitys[i] = new Vector3(FlocksVelocitys[i].x * (-1), FlocksVelocitys[i].y, 0);
                else FlocksVelocitys[i] = new Vector3(FlocksVelocitys[i].x, FlocksVelocitys[i].y * (-1), 0);
            }

            if (FlocksVelocitys[i].magnitude > MaxSpeed)
            {
                FlocksVelocitys[i] /= FlocksVelocitys[i].magnitude;
                FlocksVelocitys[i] *= MaxSpeed;
            }

            Vector3 tmp = Flocks[i].transform.position + FlocksVelocitys[i];
            float x = Mathf.Clamp(tmp.x, -1 * BackGroundSize, BackGroundSize);
            float y = Mathf.Clamp(tmp.y, -1 * BackGroundSize, BackGroundSize);

            //Flocks[i].transform.LookAt(new Vector3(x, y, 0), Vector3.up);
            Flocks[i].transform.position = new Vector3(x,y,0);

            float cos = FlocksVelocitys[i].x * 1000 / FlocksVelocitys[i].magnitude * 1000;
            cos = Mathf.Clamp(cos, -1f, 1f);
            float rotateArg = (FlocksVelocitys[i].x != 0) ?
                                Mathf.Acos(cos) :
                                0;
            if (Mathf.Abs(rotateArg) > 0.01)
                Flocks[i].transform.rotation *= Quaternion.Euler(0, 0, rotateArg);

        }

        for (int i=0; i<Flocks.Count; i++)
        {
            OldPos[i] = Flocks[i].transform.position;
            OldVelo[i] = FlocksVelocitys[i];
        }
    }
}
