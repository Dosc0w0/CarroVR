/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegmentMover : MonoBehaviour
{
    public float baseSpeed = 10f;        // Minimum speed
    public float maxSpeed = 30f;         // Fastest speed when holding the accelerator
    public float accelTime = 2f;         // Time (seconds) to reach max speed
    private float currentSpeed;

    void Start()
    {
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        // Input.GetKey(KeyCode.W) for keyboard 'W' to accelerate
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed, maxSpeed, (maxSpeed - baseSpeed) / accelTime * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed, baseSpeed, (maxSpeed - baseSpeed) / accelTime * Time.deltaTime);
        }

        // Optional: left/right strafe input modifies lateral motion
        float strafe = Input.GetAxis("Horizontal") * currentSpeed * 0.5f * Time.deltaTime;
        if (strafe != 0f) transform.Translate(Vector3.right * strafe, Space.Self);

        // Move the road backward to simulate forward motion
        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime, Space.World);
    }
}

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegmentMover : MonoBehaviour
{
    public float baseSpeed = 10f;        // Default forward speed
    public float maxSpeed = 30f;         // Maximum forward speed
    public float reverseSpeed = 8f;      // Reverse (backward) speed when braking
    public float accelTime = 2f;         // Time to reach max forward speed
    public float decelTime = 1f;         // Time to reach reverse speed when braking
    private float currentSpeed;

    void Start()
    {
        currentSpeed = 0f;  // Start stationary, or baseSpeed if preferred
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            // Accelerate forward from currentSpeed to maxSpeed
            float forwardAccel = (maxSpeed - baseSpeed) / accelTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, forwardAccel * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            // Decelerate into reverse
            float reverseAccel = (baseSpeed + reverseSpeed) / decelTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, -reverseSpeed, reverseAccel * Time.deltaTime);
        }
        else
        {
            // Gradually return to baseSpeed when no input
            float returnRate = (maxSpeed - baseSpeed) / accelTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, baseSpeed, returnRate * Time.deltaTime);
        }

        // Optional: Lateral movement using currentSpeed for momentum
        float strafe = Input.GetAxis("Horizontal") * Mathf.Abs(currentSpeed) * 0.5f * Time.deltaTime;
        if (strafe != 0f)
            transform.Translate(Vector3.right * strafe, Space.Self);

        // Core: Move the road to emulate forward/backward car movement
        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime, Space.World);
    }
}
