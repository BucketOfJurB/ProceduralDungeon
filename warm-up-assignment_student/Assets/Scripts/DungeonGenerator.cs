using UnityEngine;
using System.Collections.Generic;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public RectInt room = new RectInt(0,0,100,50);
    float duration = 0;
    bool depthTest = false;
    float height = 0.01f;

    private void Update()
    {
        AlgorithmsUtils.DebugRectInt(room, Color.green, duration, depthTest, height);

    }
}
