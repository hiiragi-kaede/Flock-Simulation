using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockController : MonoBehaviour
{
    // Start is called before the first frame update
    [Tooltip("群れが動く範囲設定に用いるGameObject。高速化のためにサイズは整数にしてほしい")]
    public GameObject BackGround;
    [Tooltip("群れの一体のプレハブ")]
    public GameObject Bird;
    [Tooltip("群れの頭数")]
    [Range(10,4000)]
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

    private List<Flock> Flocks;

    private class Flock
    {
        public GameObject Bird { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 OldPos { get; set; }
        public Vector3 OldVelo { get; set; }

        public Flock(GameObject bird, Vector3 velo, Vector3 oldpos, Vector3 oldvelo)
        {
            Bird = bird;
            Velocity = velo;
            OldPos = oldpos;
            OldVelo = oldvelo;
        }
    }

    private List<List<List<Flock>>> FlockArea;

    private List<(int, int)>[] boxs = new List<(int, int)>[8]{
        new List<(int,int)>{(-1,0),(-1,1),(0,1),(1,1),(1,0),(-1,2),(0,2),(1,2)},//up
        new List<(int,int)>{(0,2),(0,1),(1,2),(1,1),(1,0),(2,1),(2,0),(2,2)},//upright
        new List<(int,int)>{(0,1),(1,1),(1,0),(1,-1),(0,-1),(2,1),(2,0),(2,-1)},//right
        new List<(int,int)>{(0,-1),(0,-2),(1,0),(1,-1),(1,-2),(2,0),(2,-1),(2,-2)},//downright
        new List<(int,int)>{(-1,0),(-1,-1),(0,-1),(1,-1),(1,0),(-1,-2),(0,-2),(1,-2)},//down
        new List<(int,int)>{(-2,0),(-2,-1),(-1,0),(-1,-1),(-1,-2),(0,-1),(0,-2),(-2,-2)},//downleft
        new List<(int,int)>{(-1,0),(-1,1),(0,1),(-1,-1),(0,-1),(-2,1),(-2,0),(-2,-1)},//left
        new List<(int,int)>{(-2,1),(-2,0),(-1,2),(-1,1),(-1,0),(0,2),(0,1),(-2,2)}//upleft
    };

    void Start()
    {
        if (BackGround is null) return;
        if (Bird is null) return;

        BackGroundSize = (BackGround.transform.localScale.x - 1) / 2;
        //Debug.Log(BackGroundSize);

        Flocks = new List<Flock>();

        for(int i=0; i<FlockSize; i++)
        {
            IncrementFlock();
        }

        int size = Mathf.FloorToInt(BackGroundSize * 2);
        FlockArea = new List<List<List<Flock>>>();
        for(int i=0; i<size; i++)
        {
            List<List<Flock>> tmp_i = new List<List<Flock>>();
            for(int j=0; j<size; j++)
            {
                tmp_i.Add(new List<Flock>());
            }
            FlockArea.Add(tmp_i);
        }

        UpdateFlockArea();

        //for(int i=0; i<size; i++)
        //{
        //    for (int j = 0; j < size; j++)
        //    {
        //        Debug.Log("flock[" + i + "][" + j + "]:" + FlockArea[i][j].Count);
        //    }
        //}
        //Debug.Log("end of start()");

    }

    (int,int) GetIdxs(GameObject bird)
    {
        (int, int) t = (Mathf.FloorToInt(bird.transform.position.x + BackGroundSize) ,
                    Mathf.FloorToInt(bird.transform.position.y + BackGroundSize));
        return t;
    }

    void UpdateFlockArea()
    {
        foreach (Flock flock in Flocks)
        {
            (int,int) Idxs = GetIdxs(flock.Bird);

            FlockArea[Idxs.Item2][Idxs.Item1].Add(flock);
        }
    }

    void ClearFlockArea()
    {
        for(int i=0; i<FlockArea.Count; i++)
        {
            for(int j=0; j<FlockArea[i].Count; j++)
            {
                FlockArea[i][j].Clear();
            }
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

        Flocks.Add(new Flock(obj, dir, pos, dir));
    }

    void DecrementFlock()
    {
        GameObject.Destroy(Flocks[Flocks.Count - 1].Bird);
        Flocks.RemoveAt(Flocks.Count - 1);
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
            Vector3 diff = Flocks[idx].OldPos - Flocks[i].OldPos;
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

    Vector3 Separation(int idx,List<Flock> fellows)
    {
        Vector3 vec = Vector3.zero;
        for (int i = 0; i < fellows.Count; i++)
        {
            Vector3 diff = Flocks[idx].OldPos - fellows[i].OldPos;
            if (diff.sqrMagnitude < Mathf.Pow(DistThreshold, 2))
            {
                if (diff.sqrMagnitude != 0)
                {
                    vec += diff / diff.sqrMagnitude;
                }

            }

        }
        return vec / (fellows.Count - 1);
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
            Vector3 diff = Flocks[idx].OldPos - Flocks[i].OldPos;
            if (diff.sqrMagnitude < Mathf.Pow(DistThreshold, 2))
            {
                vel += Flocks[i].OldVelo;
                cnt++;
            }
                
        }
        if(cnt!=0)
            vel /= cnt;
        return (vel - Flocks[idx].OldVelo);
    }

    Vector3 Alignment(int idx,List<Flock> fellows)
    {
        Vector3 vel = Vector3.zero;
        int cnt = 0;

        for (int i = 0; i < fellows.Count; i++)
        {
            Vector3 diff = Flocks[idx].OldPos - fellows[i].OldPos;
            if (diff.sqrMagnitude < Mathf.Pow(DistThreshold, 2))
            {
                vel += fellows[i].OldVelo;
                cnt++;
            }

        }
        if (cnt != 0)
            vel /= cnt;
        return (vel - Flocks[idx].OldVelo);
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
            Vector3 diff = Flocks[idx].OldPos - Flocks[i].OldPos;
            if (diff.sqrMagnitude < Mathf.Pow(DistThreshold, 2))
            {
                pos += Flocks[i].OldPos;
                cnt++;
            }
                
        }
        if(cnt!=0)
            pos /= cnt;
        return (pos - Flocks[idx].OldPos);
    }

    Vector3 Cohension(int idx,List<Flock> fellows)
    {
        Vector3 pos = Vector3.zero;
        int cnt = 0;

        for (int i = 0; i < fellows.Count; i++)
        {
            Vector3 diff = Flocks[idx].OldPos - fellows[i].OldPos;
            if (diff.sqrMagnitude < Mathf.Pow(DistThreshold, 2))
            {
                pos += fellows[i].OldPos;
                cnt++;
            }

        }
        if (cnt != 0)
            pos /= cnt;
        return (pos - Flocks[idx].OldPos);
    }

    enum Dir
    {
        Up=0,
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

        //for (int i=0; i<Flocks.Count; i++)
        //{
        //    //Debug.Log("Bird" + i);
        //    Vector3 acceralation = Sep * Separation(i) + Align * Alignment(i) + Coh * Cohension(i);

        //    Vector3 noise = new Vector3(Random.Range(-1f, 1f),
        //                              Random.Range(-1f, 1f),
        //                              0);

        //    Flocks[i].Velocity += (acceralation + noise) * Time.deltaTime;

        //    Vector3 dest = Flocks[i].Bird.transform.position + Flocks[i].Velocity * Time.deltaTime;
        //    //移動先がマップ外なら、右もしくは左に直角に曲がる
        //    if (dest.x < -1 * BackGroundSize || dest.x > BackGroundSize ||
        //        dest.y < -1 * BackGroundSize || dest.y > BackGroundSize)
        //    {
        //        Vector3 swap = new Vector3(Flocks[i].Velocity.y, Flocks[i].Velocity.x, 0);
        //        Flocks[i].Velocity = swap;

        //        float rnd = Random.Range(0, 1);
        //        if (rnd > 0.5f) Flocks[i].Velocity = new Vector3(Flocks[i].Velocity.x * (-1), Flocks[i].Velocity.y, 0);
        //        else Flocks[i].Velocity = new Vector3(Flocks[i].Velocity.x, Flocks[i].Velocity.y * (-1), 0);
        //    }

        //    if (Flocks[i].Velocity.magnitude > MaxSpeed)
        //    {
        //        Flocks[i].Velocity /= Flocks[i].Velocity.magnitude;
        //        Flocks[i].Velocity *= MaxSpeed;
        //    }

        //    Vector3 tmp = Flocks[i].Bird.transform.position + Flocks[i].Velocity;
        //    float x = Mathf.Clamp(tmp.x, -1 * BackGroundSize, BackGroundSize);
        //    float y = Mathf.Clamp(tmp.y, -1 * BackGroundSize, BackGroundSize);

        //    //Flocks[i].transform.LookAt(new Vector3(x, y, 0), Vector3.up);
        //    Flocks[i].Bird.transform.position = new Vector3(x,y,0);

        //    float cos = Flocks[i].Velocity.x * 1000 / Flocks[i].Velocity.magnitude * 1000;
        //    cos = Mathf.Clamp(cos, -1f, 1f);
        //    float rotateArg = (Flocks[i].Velocity.x != 0) ?
        //                        Mathf.Acos(cos) :
        //                        0;
        //    if (Mathf.Abs(rotateArg) > 0.01)
        //        Flocks[i].Bird.transform.rotation *= Quaternion.Euler(0, 0, rotateArg);
        //}

        for (int i = 0; i < Flocks.Count; i++)
        {
            (int, int) idxs = GetIdxs(Flocks[i].Bird);
            int X = idxs.Item1;
            int Y = idxs.Item2;

            List<Flock> fellows = new List<Flock>();

            Dir dir = GetDir(Flocks[i].OldVelo);

            foreach((int,int)box in boxs[(int)dir])
            {
                int dx = box.Item1, dy = box.Item2;
                if(X+dx<0 || X + dx >= (int)BackGroundSize*2
                    || Y+dy<0 || Y+dy >= (int)BackGroundSize * 2)
                {
                    //配列外参照
                    continue;
                }

                //そのエリアに仲間がいれば
                if (FlockArea[Y + dy][X + dx].Count != 0)
                {
                    //仲間として認識するリストに追加
                    fellows.AddRange(FlockArea[Y + dy][X + dx]);
                }
            }

            if (fellows.Count != 0)
            {
                Vector3 randDir = new Vector3(Random.Range(-1f, 1f),
                                      Random.Range(-1f, 1f),
                                      0);

                Flocks[i].Velocity += (randDir) * Time.deltaTime;
                continue;
            }

            Vector3 acceralation = Sep * Separation(i,fellows) + Align * Alignment(i,fellows) + Coh * Cohension(i,fellows);

            Vector3 noise = new Vector3(Random.Range(-1f, 1f),
                                      Random.Range(-1f, 1f),
                                      0);

            Flocks[i].Velocity += (acceralation + noise) * Time.deltaTime;


            //Debug.Log("velo before:" + Flocks[i].Velocity);
            Vector3 dest = Flocks[i].Bird.transform.position + Flocks[i].Velocity * Time.deltaTime;
            //移動先がマップ外なら、右もしくは左に直角に曲がる
            if (dest.x < -1 * BackGroundSize || dest.x > BackGroundSize ||
                dest.y < -1 * BackGroundSize || dest.y > BackGroundSize)
            {
                Vector3 swap = new Vector3(Flocks[i].Velocity.y, Flocks[i].Velocity.x, 0);
                Flocks[i].Velocity = swap;

                float rnd = Random.Range(0, 1);
                if (rnd > 0.5f) Flocks[i].Velocity = new Vector3(Flocks[i].Velocity.x * (-1), Flocks[i].Velocity.y, 0);
                else Flocks[i].Velocity = new Vector3(Flocks[i].Velocity.x, Flocks[i].Velocity.y * (-1), 0);
            }
            //Debug.Log("velo after:" + Flocks[i].Velocity);

            if (Flocks[i].Velocity.magnitude > MaxSpeed)
            {
                Flocks[i].Velocity /= Flocks[i].Velocity.magnitude;
                Flocks[i].Velocity *= MaxSpeed;
            }

            
            Vector3 tmp = Flocks[i].Bird.transform.position + Flocks[i].Velocity;

            float x = Mathf.Clamp(tmp.x, -1 * BackGroundSize, BackGroundSize);
            float y = Mathf.Clamp(tmp.y, -1 * BackGroundSize, BackGroundSize);

            if(System.Single.IsNaN(x) || System.Single.IsNaN(y))
            {
                Debug.Log("error at " + i);
                Debug.Log("x,y=" + x + "," + y);
                continue;
            }
            //Debug.Log("x,y=" + x + "," + y);
            //Flocks[i].transform.LookAt(new Vector3(x, y, 0), Vector3.up);
            Flocks[i].Bird.transform.position = new Vector3(x, y, 0);
            //Flocks[i].Bird.transform.SetPositionAndRotation(new Vector3(x, y, 0),
              //                                              Flocks[i].Bird.transform.rotation);

            float cos = Flocks[i].Velocity.x * 1000 / Flocks[i].Velocity.magnitude * 1000;
            cos = Mathf.Clamp(cos, -1f, 1f);
            float rotateArg = (Flocks[i].Velocity.x != 0) ?
                                Mathf.Acos(cos) :
                                0;
            if (Mathf.Abs(rotateArg) > 0.01)
                Flocks[i].Bird.transform.rotation *= Quaternion.Euler(0, 0, rotateArg);
        }

        for (int i=0; i<Flocks.Count; i++)
        {
            Flocks[i].OldPos = Flocks[i].Bird.transform.position;
            Flocks[i].OldVelo = Flocks[i].Velocity;
        }

        ClearFlockArea();
    }
}
