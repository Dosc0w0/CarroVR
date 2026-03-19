using UnityEngine;

public class SmoothCanvasFollower : MonoBehaviour
{
    public Camera referenceCamera;
    public Transform canvasTransform;

    [Header("Movement")]
    public float smoothness = 4f;
    public float distanceFromCamera = 2f;

    [Header("View Limits")]
    public float maxViewAngle = 35f;

    void LateUpdate()
    {
        if (referenceCamera == null || canvasTransform == null) return;

        Transform cam = referenceCamera.transform;

        Vector3 dirToCanvas = (canvasTransform.position - cam.position).normalized;

        float angle = Vector3.Angle(cam.forward, dirToCanvas);

        // Se o canvas estiver fora do campo de visão
        if (angle > maxViewAngle)
        {
            Vector3 targetPosition = cam.position + cam.forward * distanceFromCamera;

            canvasTransform.position = Vector3.Lerp(
                canvasTransform.position,
                targetPosition,
                Time.deltaTime * smoothness
            );
        }

        // sempre olha para o usuário
        canvasTransform.rotation = Quaternion.LookRotation(
            canvasTransform.position - cam.position
        );
    }
}