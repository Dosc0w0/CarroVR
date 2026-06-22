using System;

[Serializable]
public class CarTelemetryFrame
{
    public float TimeSinceStart; // O tempo decorrido desde o início da gravação
    
    // ==========================================
    // NOVA COLUNA: FASE DA PISTA
    // 0 = Tutorial / Aquecimento
    // 1 = Pista Oficial / Teste a valer
    // ==========================================
    public int TrackPhase; 
    
    // Coordenadas Espaciais do Carro
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    
    // Rotação (Ângulos de Euler)
    public float RotationX;
    public float RotationY;
    public float RotationZ;
    
    // Dados Dinâmicos
    public float CurrentSpeed;
    public float SteeringWheelAngle; // Rotação do volante
    
    // Pedais separados
    public float AccPedal;   // Valor de 0.0 a 1.0
    public float BrakePedal; // Valor de 0.0 a 1.0
}