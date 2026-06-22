using System;
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Wheel target to rotate")]
    public Transform wheel;

    [Header("Rotation axis (local)")]
    public Axis rotationAxis = Axis.X;

    // --- NOVA ABORDAGEM: SmoothDampAngle ---
    [Header("Configuração de Suavização (Mola)")]
    [Tooltip("Tempo de resposta (em segundos) para o volante virtual alcançar o físico. Ex: 0.05 (muito rápido), 0.2 (suave)")]
    public float smoothTime = 0.05f; 
    
    // Variável interna obrigatória para a Unity guardar a "inércia" do movimento
    private float angularVelocity = 0f;

    [Header("Dados do Bluetooth")]
    [Tooltip("Raw data received")]
    public float raw;

    [Tooltip("Angle offset added after bluetooth value (deg)")]
    public float angleOffset = 0f;

    [Tooltip("Current (applied) angle in degrees")]
    public float currentAngle;

    [Tooltip("Latest commanded angle in degrees (set by client)")]
    public float targetAngle;

    [Tooltip("Maximum allowed angle in degrees")]
    public float maxAngle;

    [Tooltip("Minimum allowed angle in degrees")]
    public float mnAngle;

    void Reset() { wheel = transform; }

    private void Start()
    {
        // Mantido exatamente como o seu original
        GameObject[] clients = GameObject.FindGameObjectsWithTag("Client");

        if (clients.Length > 0)
            clients[0].GetComponent<GetFromBluetooth>().wheel = this;
    }

    void Update()
    {
        // 1. Inversão do sinal do hardware (mantido do seu código original)
        targetAngle = (raw * -1);

        if (!wheel) return;
        
        // 2. Aplica o offset
        float tgt = targetAngle + angleOffset;

        // 3. A MÁGICA DA MOLA (SmoothDampAngle)
        if (smoothTime <= 0f)
        {
            currentAngle = tgt; // Se colocar 0 no Inspector, ele teleporta instantaneamente (sem filtro)
        }
        else
        {
            // Calcula a curva suave com base na inércia, sem nunca perder a velocidade de resposta
            currentAngle = Mathf.SmoothDampAngle(currentAngle, tgt, ref angularVelocity, smoothTime);
        }

        // 4. Aplica fisicamente no modelo 3D (preservando o seu X em 25f)
        switch (rotationAxis)
        {
            case Axis.X: wheel.localRotation = Quaternion.Euler(25f, 0f, 0f); break;
            case Axis.Y: wheel.localRotation = Quaternion.Euler(25f, currentAngle, 0f); break;
            case Axis.Z: wheel.localRotation = Quaternion.Euler(25f, 0f, currentAngle); break;
        }
    }
}