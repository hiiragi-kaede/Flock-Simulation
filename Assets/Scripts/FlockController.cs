using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject BackGround;
    [Tooltip("群れの一体のプレハブ")]
    public GameObject Bird;
    [Tooltip("群れの頭数")]
    public int FlockSize = 16;
    [Tooltip("分離の重み付け係数")]
    public float Sep = 1f;
    [Tooltip("整列の重み付け係数")]
    public float Align = 1f;
    [Tooltip("結合の重み付け係数")]
    public float Coh = 1f;
    [Tooltip("速度上限")]
    public float MaxSpeed = 5f;
    [Tooltip("中心に近づく強さ。小さいほど引き寄せられる")]
    public float CenterPullFactor = 300;
    [Tooltip("近づきすぎると離れるときの距離のしきい値")]
    public float DistThreshold = 10f;


    //群れの初期位置を設定するときに範囲指定で使用する
    private float BackGroundSize;
    private GameObject[] Flocks;
    private Vector3[] FlocksVelocitys;
    private Vector3[] OldPos;
    private Vector3[] OldVelo;


    void Start()
    {
        if (BackGround is null) return;
        if (Bird is null) return;

        BackGroundSize = BackGround.transform.localScale.x/2;
        //Debug.Log(BackGroundSize);

        List<GameObject> BirdList = new List<GameObject>();
        List<Vector3> VeloList = new List<Vector3>();
        List<Vector3> oldpos = new List<Vector3>();
        List<Vector3> oldvelo = new List<Vector3>();
        for(int i=0; i<FlockSize; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-1 * BackGroundSize, BackGroundSize),
                                      Random.Range(-1 * BackGroundSize, BackGroundSize),
                                      0);

            Vector3 dir = new Vector3(Random.Range(-0.1f, 0.1f),
                                      Random.Range(-0.1f, 0.1f),
                                      0);

            GameObject obj = Instantiate(Bird, pos, Quaternion.identity);
            BirdList.Add(obj);
            VeloList.Add(dir);
            oldpos.Add(pos);
            oldvelo.Add(dir);
        }
        Flocks = BirdList.ToArray();
        FlocksVelocitys = VeloList.ToArray();
        OldPos = oldpos.ToArray();
        OldVelo = oldvelo.ToArray();

    }

    /// <summary>
    /// 仲間との距離が遠いときは緩やかに、近づきすぎると急旋回して避けるようにする
    /// </summary>
    /// <param name="idx">何番目の鳥についてか</param>
    /// <returns></returns>
    Vector3 Separation(int idx)
    {
        Vector3 vec = Vector3.zero;
        for(int i=0; i<Flocks.Length; i++)
        {
            if (i == idx) continue;
            Vector3 diff = OldPos[idx] - OldPos[i];
            if (diff.magnitude < DistThreshold)
            {
                vec += diff;
            }
                
        }
        return vec / (Flocks.Length - 1);
    }

    /// <summary>
    /// 群れ全体の速度ベクトルの平均に合わせるようにする
    /// </summary>
    /// <param name="idx">何番目の鳥についてか</param>
    /// <returns></returns>
    Vector3 Alignment(int idx)
    {
        Vector3 vel = Vector3.zero;
        for (int i = 0; i < Flocks.Length; i++)
        {
            if (i == idx) continue;
            vel += OldVelo[i];
        }
        vel /= (Flocks.Length - 1);
        return (vel - OldVelo[idx])/2;
    }

    /// <summary>
    /// 群れの中心に移動しようとする。群れの重心位置へ向かうベクトルを返す
    /// </summary>
    /// <param name="idx">何番目の鳥についてか</param>
    /// <returns></returns>
    Vector3 Cohension(int idx)
    {
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < Flocks.Length; i++)
        {
            if (i == idx) continue;
            pos += OldPos[i];
        }
        pos /= (Flocks.Length - 1);
        return (pos - OldPos[idx])/CenterPullFactor;
    }
    
    // Update is called once per frame
    void Update()
    {
        for(int i=0; i<Flocks.Length; i++)
        {
            Vector3 acceralation = Sep * Separation(i) + Align * Alignment(i) + Coh * Cohension(i);
            //Debug.Log(Separation(i));
            //Debug.Log(Alignment(i));
            //Debug.Log(Cohension(i));

            //Debug.Log("Acc:" + acceralation);
            FlocksVelocitys[i] += acceralation * Time.deltaTime;
            FlocksVelocitys[i].x = Mathf.Clamp(FlocksVelocitys[i].x, -1 * MaxSpeed, MaxSpeed);
            FlocksVelocitys[i].y = Mathf.Clamp(FlocksVelocitys[i].y, -1 * MaxSpeed, MaxSpeed);
            Vector3 tmp = Flocks[i].transform.position;
            //Debug.Log(FlocksVelocitys[i]);
            tmp += FlocksVelocitys[i];
            float x = Mathf.Clamp(tmp.x, -1 * BackGroundSize, BackGroundSize);
            float y = Mathf.Clamp(tmp.y, -1 * BackGroundSize, BackGroundSize);
            Flocks[i].transform.position = new Vector3(x,y,0);

            OldPos[i] = Flocks[i].transform.position;
            OldVelo[i] = FlocksVelocitys[i];
        }

        for(int i=0; i<Flocks.Length; i++)
        {
            
        }
    }
}
