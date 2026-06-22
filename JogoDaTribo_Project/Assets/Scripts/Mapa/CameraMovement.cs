using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviour
{
    Transform panel;

    void Start()
    {
        panel = transform.GetChild(0);
    }


    void Update()
    {
        Vector3 pos = panel.localPosition;
        pos.y += Input.mouseScrollDelta.y * 40f;
        panel.localPosition = pos;
        if(panel.localPosition.y > 0)
        {
            pos.y = 0;
            panel.localPosition = pos;
        }
        if(panel.localPosition.y < -1500)
        {
            pos.y = -1500;
            panel.localPosition = pos;
        }
    }
}
