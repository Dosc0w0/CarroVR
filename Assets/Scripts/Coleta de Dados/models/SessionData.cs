using System;

[Serializable]
public class SessionData
{
    public string SessionID; // Ex: "20260317_153000"
    
    // ==========================================
    // TEMPOS GERAIS (Portão 1 - Botão X)
    // ==========================================
    public float StartTime;
    public float EndTime;
    public float TotalTimeElapsed; // Tempo total com o Quest na cabeça a gravar
    
    // ==========================================
    // TEMPOS OFICIAIS (Portão 2 - Gatilho na Pista)
    // ==========================================
    public float OfficialStartTime;
    public float OfficialTimeElapsed; // Apenas o tempo que demorou a fazer o percurso real
    
    // Métricas de Desempenho do Carro (Contabilizadas apenas na fase oficial)
    public float DistanceTraveled;
    public float AverageSpeed;
    public float MaxSpeed;
    public float MaxAcceleration;
    
    // Contadores de Eventos (Contabilizados apenas na fase oficial)
    public int BrakeCount;
    public int TrackExitsCount;
    public int TotalItemsCollected;

    // Construtor vazio para inicializar facilmente
    public SessionData() {}
}