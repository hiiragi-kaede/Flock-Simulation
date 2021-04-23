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
        FlockSize.text = "size:" + fk.FlockSize.ToString();
        Sep.text = "separation:" + fk.Sep.ToString();
        Ali.text = "alignment:" + fk.Align.ToString();
        Coh.text = "cohension:" + fk.Coh.ToString();
        MaxSpeed.text = "maxSpeed:" + fk.MaxSpeed.ToString();
        DistThreshold.text = "distance:" + fk.DistThreshold.ToString();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
