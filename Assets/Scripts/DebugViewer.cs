using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugViewer : MonoBehaviour
{
    public FlockController fk;
    public Text FlockSize;
    public Text Sep;
    public Text Ali;
    public Text Coh;
    public Text MaxSpeed;
    public Text DistThreshold;

    // Start is called before the first frame update
    void Start()
    {
        FlockSize.text = "群れの大きさ:" + fk.FlockSize.ToString();
        Sep.text = "分離度:" + fk.Sep.ToString();
        Ali.text = "整列度:" + fk.Align.ToString();
        Coh.text = "結合度:" + fk.Coh.ToString();
        MaxSpeed.text = "速度上限:" + fk.MaxSpeed.ToString();
        DistThreshold.text = "感知距離:" + fk.DistThreshold.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
