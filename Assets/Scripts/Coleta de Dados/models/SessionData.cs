using System;

[Serializable]
public class SessionData
{
    public string SessionID; // Ex: "20260317_153000"
    
    // Tempos
    public float StartTime;
    public float EndTime;
    public float TotalTimeElapsed;
    
    // Métricas de Desempenho do Carro
    public float DistanceTraveled;
    public float AverageSpeed;
    public float MaxSpeed;
    public float MaxAcceleration;
    
    // Contadores de Eventos
    public int BrakeCount;
    public int TrackExitsCount;
    public int TotalItemsCollected;

    // Construtor vazio para inicializar facilmente
    public SessionData() {}
}