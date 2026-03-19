using System;

[Serializable]
public struct HandTelemetryFrame
{
    public float TimeSinceStart;
    
    // --- MÃO ESQUERDA ---
    public bool IsLeftTracked; // true = Câmera está vendo a mão; false = Mão sumiu
    public float LeftPosX;
    public float LeftPosY;
    public float LeftPosZ;
    public float LeftRotX;
    public float LeftRotY;
    public float LeftRotZ;

    // --- MÃO DIREITA ---
    public bool IsRightTracked; // true = Câmera está vendo a mão; false = Mão sumiu
    public float RightPosX;
    public float RightPosY;
    public float RightPosZ;
    public float RightRotX;
    public float RightRotY;
    public float RightRotZ;
}