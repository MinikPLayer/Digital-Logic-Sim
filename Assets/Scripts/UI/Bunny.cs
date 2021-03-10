using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Bunny : MonoBehaviour
{
    public TextMeshProUGUI title;

    int[] bs = {273,273,274,274,276,275,276,275,98,97 };

    int pos = 0;
    void DoMagic()
    {
        title.text = "NES circuit simulator";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey((KeyCode)bs[pos]))
        {
            pos++;
            if(pos == bs.Length)
            {
                pos = 0;
                DoMagic();
            }
        }
    }
}
