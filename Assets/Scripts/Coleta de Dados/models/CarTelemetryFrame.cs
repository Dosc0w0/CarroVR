using System;

[Serializable]
public class CarTelemetryFrame
{
    public float TimeSinceStart; // O tempo decorrido desde o início da corrida
    
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
    public float PedalInput;         // Útil para saber se estava acelerando ou solto
}