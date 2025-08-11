using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F1x_Pos1t1on : MonoBehaviour
{
    public class LockPosition : MonoBehaviour
    {
        // Edit these values in the Inspector or set them in code
        public Vector3 fixedPosition = new Vector3(-0.008596933f, 0.009879794f, -0.01552708f);

        void Update()
        {
            transform.position = fixedPosition;
        }
    } // end
}
