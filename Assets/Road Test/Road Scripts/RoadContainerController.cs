using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadContainerController : MonoBehaviour
{
    public float turnSpeed = 10f;

    void Update()
    {
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            turnInput = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            turnInput = +1f;

        if (turnInput != 0f)
        {
            float angle = turnInput * turnSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, angle, Space.Self);
        }
    }
}
