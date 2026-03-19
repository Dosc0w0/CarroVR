using UnityEngine;

public class SessionCollector : MonoBehaviour
{
    private SessionData currentSession;
    private bool isRecording = false;

    // Variáveis auxiliares para calcular a Velocidade Média sem encher a memória
    private float speedSum = 0f;
    private int speedSamplesCount = 0;

    private void OnEnable()
    {
        SimulationEvents.OnSessionStarted += HandleSessionStarted;
        SimulationEvents.OnSessionEnded += HandleSessionEnded;
        SimulationEvents.OnItemCollected += HandleItemCollected;
        SimulationEvents.OnTrackExit += HandleTrackExit;
        SimulationEvents.OnBrakeApplied += HandleBrakeApplied;
    }

    private void OnDisable()
    {
        SimulationEvents.OnSessionStarted -= HandleSessionStarted;
        SimulationEvents.OnSessionEnded -= HandleSessionEnded;
        SimulationEvents.OnItemCollected -= HandleItemCollected;
        SimulationEvents.OnTrackExit -= HandleTrackExit;
        SimulationEvents.OnBrakeApplied -= HandleBrakeApplied;
    }

    private void HandleSessionStarted(string sessionID)
    {
        currentSession = new SessionData();
        currentSession.SessionID = sessionID;
        currentSession.StartTime = Time.time;
        
        // Zera os contadores de média para a nova sessão
        speedSum = 0f;
        speedSamplesCount = 0;
        
        isRecording = true;
        Debug.Log($"[SessionCollector] Iniciando coleta para a sessão {sessionID}");
    }

    private void HandleSessionEnded()
    {
        if (!isRecording) return;

        currentSession.EndTime = Time.time;
        currentSession.TotalTimeElapsed = currentSession.EndTime - currentSession.StartTime;
        
        // Calcula a média exata baseada em todas as amostras recebidas
        if (speedSamplesCount > 0)
        {
            currentSession.AverageSpeed = speedSum / speedSamplesCount;
        }

        isRecording = false;
        Debug.Log($"[SessionCollector] Coleta encerrada. Vel. Máx: {currentSession.MaxSpeed:F2} | Vel. Média: {currentSession.AverageSpeed:F2}");
    }

    private void HandleItemCollected()
    {
        if (isRecording) currentSession.TotalItemsCollected++;
    }

    private void HandleTrackExit()
    {
        if (isRecording) currentSession.TrackExitsCount++;
    }

    private void HandleBrakeApplied(float intensity)
    {
        if (isRecording) currentSession.BrakeCount++;
    }

    // ==========================================
    // NOVA FUNÇÃO: CHAMADA PELO TELEMETRY RECORDER
    // ==========================================
    public void UpdateDynamics(float currentSpeed, float currentAcceleration)
    {
        if (!isRecording) return;

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