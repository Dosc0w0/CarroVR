using UnityEngine;

public class SessionCollector : MonoBehaviour
{
    // A nossa "gaveta" de dados da sessão atual
    private SessionData currentSession;
    
    // Variável de controle para saber se estamos gravando
    private bool isRecording = false;

    private void OnEnable()
    {
        // Inscrevendo-se nos "gritos" do barramento de eventos
        SimulationEvents.OnSessionStarted += HandleSessionStarted;
        SimulationEvents.OnSessionEnded += HandleSessionEnded;
        SimulationEvents.OnItemCollected += HandleItemCollected;
        SimulationEvents.OnTrackExit += HandleTrackExit;
        SimulationEvents.OnBrakeApplied += HandleBrakeApplied;
    }

    private void OnDisable()
    {
        // Cancelando a inscrição (Boas práticas de Clean Architecture)
        SimulationEvents.OnSessionStarted -= HandleSessionStarted;
        SimulationEvents.OnSessionEnded -= HandleSessionEnded;
        SimulationEvents.OnItemCollected -= HandleItemCollected;
        SimulationEvents.OnTrackExit -= HandleTrackExit;
        SimulationEvents.OnBrakeApplied -= HandleBrakeApplied;
    }

    // ==========================================
    // HANDLERS (O QUE FAZER QUANDO OUVIR O EVENTO)
    // ==========================================

    private void HandleSessionStarted(string sessionID)
    {
        // Cria uma nova sessão em branco
        currentSession = new SessionData();
        currentSession.SessionID = sessionID;
        currentSession.StartTime = Time.time;
        
        isRecording = true;
        Debug.Log($"[SessionCollector] Iniciando coleta para a sessão {sessionID}");
    }

    private void HandleSessionEnded()
    {
        if (!isRecording) return;

        currentSession.EndTime = Time.time;
        currentSession.TotalTimeElapsed = currentSession.EndTime - currentSession.StartTime;
        isRecording = false;
        
        // Aqui, futuramente, chamaremos o sistema da Fase 4 para salvar o JSON!
        Debug.Log($"[SessionCollector] Coleta encerrada. Total de itens: {currentSession.TotalItemsCollected}");
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
    
    // Método público caso outro script precise ler os dados atuais
    public SessionData GetCurrentSessionData()
    {
        return currentSession;
    }
}