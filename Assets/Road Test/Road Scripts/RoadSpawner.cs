using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class RoadSpawner : MonoBehaviour
{
    public GameObject segmentPrefab;
    public int segmentCount = 5;
    public float segmentLength = 20f;
    private List<GameObject> segments = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 pos = Vector3.forward * i * segmentLength;
            var seg = Instantiate(segmentPrefab, this.transform.position, Quaternion.identity);
            segments.Add(seg);
        } 
    }

    public void SpawnNext()
    {
        // Spawn new segment at the end
        var last = segments[segments.Count - 1];
        Vector3 newPos = last.transform.position + Vector3.forward * segmentLength;
        var seg = Instantiate(segmentPrefab, this.transform.position, Quaternion.identity);
        segments.Add(seg);

        // Destroy the oldest behind the camera
        var old = segments[0];
        segments.RemoveAt(0);
        Destroy(old);
    }
}
