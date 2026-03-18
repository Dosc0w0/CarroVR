using System;
using UnityEngine;

public static class SimulationEvents
{
    // ==========================================
    // EVENTOS DE CONTROLE DA SESSÃO
    // ==========================================
    
    // Disparado quando a corrida/teste de fato começa. Passa o ID da sessão gerado.
    public static event Action<string> OnSessionStarted;
    
    // Disparado quando o usuário cruza a linha de chegada ou o teste é abortado.
    public static event Action OnSessionEnded;


    // ==========================================
    // EVENTOS DISCRETOS DE GAMEPLAY (MÉTRICAS)
    // ==========================================
    
    // Disparado quando o carro colide com o raio (Gamificação)
    public static event Action OnItemCollected;
    
    // Disparado quando o colisor do pneu sai da área delimitada da pista principal
    public static event Action OnTrackExit;
    
    // Disparado quando o usuário pisa no freio (podemos passar a intensidade do freio se necessário)
    public static event Action<float> OnBrakeApplied;


    // ==========================================
    // MÉTODOS GATILHOS (INVOKERS)
    // ==========================================
    // Usamos métodos para disparar os eventos com segurança, 
    // verificando se há alguém escutando (?.) para evitar NullReferenceExceptions.

    public static void TriggerSessionStarted(string sessionID)
    {
        OnSessionStarted?.Invoke(sessionID);
        Debug.Log($"[Métricas] Sessão Iniciada: {sessionID}");
    }

    public static void TriggerSessionEnded()
    {
        OnSessionEnded?.Invoke();
        Debug.Log("[Métricas] Sessão Encerrada.");
    }

    public static void TriggerItemCollected()
    {
        OnItemCollected?.Invoke();
    }

    public static void TriggerTrackExit()
    {
        OnTrackExit?.Invoke();
    }

    public static void TriggerBrakeApplied(float brakeIntensity)
    {
        OnBrakeApplied?.Invoke(brakeIntensity);
    }
}