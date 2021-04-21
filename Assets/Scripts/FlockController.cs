using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlockController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject BackGround;
    [Tooltip("群れの一体のプレハブ")]
    public GameObject Bird;
    [Tooltip("群れの頭数")]
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
    public Text sepText;
    public Text aliText;
    public Text cohText;


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
            if (diff.sqrMagnitude < Mathf.Pow( DistThreshold ,2) )
            {
                if(diff.sqrMagnitude != 0)
                {
                    vec += diff / diff.sqrMagnitude;
                }
                else
                {
                    vec += new Vector3(Random.Range(-5f,5f),
                                       Random.Range(-5f,5f),
                                       0);
                }
                
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
        return (vel - OldVelo[idx])/8;
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
        return (pos - OldPos[idx]);
    }
    
    // Update is called once per frame
    void Update()
    {
        Vector3 sepAve = Vector3.zero;
        Vector3 aliAve = Vector3.zero;
        Vector3 cohAve = Vector3.zero;

        for(int i=0; i<Flocks.Length; i++)
        {
            //Debug.Log("Bird" + i);
            Vector3 acceralation = Sep * Separation(i) + Align * Alignment(i) + Coh * Cohension(i);
            sepAve += Separation(i);
            aliAve += Alignment(i);
            cohAve += Cohension(i);

            //Debug.Log("sep:"+Separation(i));
            //Debug.Log("ali:"+Alignment(i));
            //Debug.Log("coh:"+Cohension(i));

            //Debug.Log("Acc:" + acceralation);
            FlocksVelocitys[i] += acceralation * Time.deltaTime;
            if(FlocksVelocitys[i].magnitude > MaxSpeed)
            {
                FlocksVelocitys[i] /= FlocksVelocitys[i].magnitude;
                FlocksVelocitys[i] *= MaxSpeed;
            }

            Vector3 dest = Flocks[i].transform.position + FlocksVelocitys[i] * Time.deltaTime;
            //移動先がマップ外なら、右もしくは左に直角に曲がる
            if(dest.x < -1 * BackGroundSize ||  dest.x > BackGroundSize ||
                dest.y < -1 * BackGroundSize || dest.y > BackGroundSize)
            {
                float swap = FlocksVelocitys[i].x;
                FlocksVelocitys[i].x = FlocksVelocitys[i].y;
                FlocksVelocitys[i].y = swap;

                float rnd = Random.Range(0, 1);
                if (rnd > 0.5f) FlocksVelocitys[i].x *= -1;
                else FlocksVelocitys[i].y *= -1;
            }

            Vector3 tmp = Flocks[i].transform.position + FlocksVelocitys[i];
            float x = Mathf.Clamp(tmp.x, -1 * BackGroundSize, BackGroundSize);
            float y = Mathf.Clamp(tmp.y, -1 * BackGroundSize, BackGroundSize);
            Flocks[i].transform.position = new Vector3(x,y,0);

        }

        sepAve *= 1 << 9;
        aliAve *= 1 << 9;
        cohAve *= 1 << 9;
        sepText.text = "sep:" + sepAve.ToString("F3");
        aliText.text = "ali:" + aliAve.ToString("F3");
        cohText.text = "coh:" + cohAve.ToString("F3");

        for (int i=0; i<Flocks.Length; i++)
        {
            OldPos[i] = Flocks[i].transform.position;
            OldVelo[i] = FlocksVelocitys[i];
        }
    }
}
