using System;
using UnityEngine;

public static class SimulationEvents
{
    // ==========================================
    // EVENTOS DE CONTROLO DA SESSÃO
    // ==========================================
    
    // Disparado quando a corrida/teste de facto começa. Passa o ID da sessão gerado.
    public static event Action<string> OnSessionStarted;
    
    // Disparado quando o utilizador cruza a linha de chegada ou o teste é abortado.
    public static event Action OnSessionEnded;

    // --- NOVO EVENTO: INÍCIO DA FASE OFICIAL ---
    // Disparado quando o utilizador passa pelo gatilho que inicia a recolha oficial de tempo e pontos.
    public static event Action OnOfficialTrackStarted;


    // ==========================================
    // EVENTOS DISCRETOS DE GAMEPLAY (MÉTRICAS)
    // ==========================================
    
    // Disparado quando o carro colide com o raio (Gamificação)
    public static event Action OnItemCollected;
    
    // Disparado quando o colisor do pneu sai da área delimitada da pista principal
    public static event Action OnTrackExit;
    
    // Disparado quando o utilizador pisa o travão (passando a intensidade)
    public static event Action<float> OnBrakeApplied;


    // ==========================================
    // MÉTODOS GATILHOS (INVOKERS)
    // ==========================================
    // Usamos métodos para disparar os eventos com segurança, 
    // verificando se há alguém a escutar (?.) para evitar NullReferenceExceptions.

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

    // --- NOVO GATILHO: INÍCIO DA FASE OFICIAL ---
    public static void TriggerOfficialTrackStarted()
    {
        OnOfficialTrackStarted?.Invoke();
        // Um log com cor ajuda muito a identificar o momento exato no terminal durante o teste
        Debug.Log("<color=cyan>[Métricas] Pista Oficial Iniciada! A contar tempo e pontos!</color>");
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