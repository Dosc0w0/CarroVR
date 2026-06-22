using UnityEngine;

public class SessionCollector : MonoBehaviour
{
    private SessionData currentSession;
    
    // As duas travas de segurança (Os Dois Portões)
    private bool isRecording = false;      // Portão 1: Gravação Geral (Ativada pelo botão X)
    private bool isOfficialPhase = false;  // Portão 2: Fase Oficial (Ativada pelo item de largada)

    // Variáveis auxiliares para calcular a Velocidade Média sem encher a memória
    private float speedSum = 0f;
    private int speedSamplesCount = 0;

    private void OnEnable()
    {
        SimulationEvents.OnSessionStarted += HandleSessionStarted;
        SimulationEvents.OnSessionEnded += HandleSessionEnded;
        
        // --- NOVA ASSINATURA: Escuta o início da fase oficial ---
        SimulationEvents.OnOfficialTrackStarted += HandleOfficialTrackStarted;
        
        SimulationEvents.OnItemCollected += HandleItemCollected;
        SimulationEvents.OnTrackExit += HandleTrackExit;
        SimulationEvents.OnBrakeApplied += HandleBrakeApplied;
    }

    private void OnDisable()
    {
        SimulationEvents.OnSessionStarted -= HandleSessionStarted;
        SimulationEvents.OnSessionEnded -= HandleSessionEnded;
        
        SimulationEvents.OnOfficialTrackStarted -= HandleOfficialTrackStarted;
        
        SimulationEvents.OnItemCollected -= HandleItemCollected;
        SimulationEvents.OnTrackExit -= HandleTrackExit;
        SimulationEvents.OnBrakeApplied -= HandleBrakeApplied;
    }

    private void HandleSessionStarted(string sessionID)
    {
        currentSession = new SessionData();
        currentSession.SessionID = sessionID;
        currentSession.StartTime = Time.time; // Abre o Portão 1 (Tempo Total)
        
        speedSum = 0f;
        speedSamplesCount = 0;
        
        isRecording = true;
        isOfficialPhase = false; // Garante que o Portão 2 está fechado no início
        
        Debug.Log($"[SessionCollector] Gravador Geral ligado. Sessão: {sessionID}. Aguardando linha de partida...");
    }

    // ==========================================
    // NOVA FUNÇÃO: O PORTÃO 2 É ABERTO
    // ==========================================
    private void HandleOfficialTrackStarted()
    {
        if (!isRecording || isOfficialPhase) return;

        isOfficialPhase = true;
        currentSession.OfficialStartTime = Time.time; // Inicia o relógio do experimento
        
        Debug.Log("<color=yellow>[SessionCollector] Fase Oficial Iniciada! A recolher métricas de desempenho.</color>");
    }

    private void HandleSessionEnded()
    {
        if (!isRecording) return;

        currentSession.EndTime = Time.time;
        
        // 1. Calcula o Tempo Total (Desde o botão X)
        currentSession.TotalTimeElapsed = currentSession.EndTime - currentSession.StartTime;
        
        // 2. Calcula o Tempo Oficial (Apenas se chegou a passar na largada)
        if (isOfficialPhase)
        {
            currentSession.OfficialTimeElapsed = currentSession.EndTime - currentSession.OfficialStartTime;
        }
        else
        {
            currentSession.OfficialTimeElapsed = 0f; // Caso tenha abortado no tutorial
        }
        
        // Calcula a média exata baseada nas amostras da fase oficial
        if (speedSamplesCount > 0)
        {
            currentSession.AverageSpeed = speedSum / speedSamplesCount;
        }

        isRecording = false;
        isOfficialPhase = false;
        Debug.Log($"[SessionCollector] Coleta encerrada. Tempo Oficial: {currentSession.OfficialTimeElapsed:F2}s | Vel. Média: {currentSession.AverageSpeed:F2}");
    }

    // ==========================================
    // MUDANÇA NAS MÉTRICAS: AGORA USAM isOfficialPhase
    // ==========================================
    private void HandleItemCollected()
    {
        if (isOfficialPhase) currentSession.TotalItemsCollected++;
    }

    private void HandleTrackExit()
    {
        if (isOfficialPhase) currentSession.TrackExitsCount++;
    }

    private void HandleBrakeApplied(float intensity)
    {
        if (isOfficialPhase) currentSession.BrakeCount++;
    }

    public void UpdateDynamics(float currentSpeed, float currentAcceleration)
    {
        // Se ainda estiver no tutorial, ignora as velocidades e acelerações
        if (!isOfficialPhase) return;

        // 1. Checa os Recordes Máximos
        if (currentSpeed > currentSession.MaxSpeed)
        {
            currentSession.MaxSpeed = currentSpeed;
        }

        if (currentAcceleration > currentSession.MaxAcceleration)
        {
            currentSession.MaxAcceleration = currentAcceleration;
        }

        // 2. Alimenta a Velocidade Média
        speedSum += currentSpeed;
        speedSamplesCount++;
    }
    
    public SessionData GetCurrentSessionData()
    {
        return currentSession;
    }
}