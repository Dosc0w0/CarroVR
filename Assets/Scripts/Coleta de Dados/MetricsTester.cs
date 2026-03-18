using UnityEngine;

public class MetricsTester : MonoBehaviour
{
    [Header("Referência para ler o JSON final")]
    public SessionCollector sessionCollector;

    void Update()
    {
        // Aperte 'S' para Iniciar a Sessão
        if (Input.GetKeyDown(KeyCode.S))
        {
            string fakeID = "TesteConsole_" + System.DateTime.Now.ToString("HHmmss");
            SimulationEvents.TriggerSessionStarted(fakeID);
            Debug.Log("===> TECLADO: Sessão Iniciada!");
        }

        // Aperte 'I' para fingir que pegou um Raio (Item)
        if (Input.GetKeyDown(KeyCode.I))
        {
            SimulationEvents.TriggerItemCollected();
            Debug.Log("===> TECLADO: Raio Coletado!");
        }

        // Aperte 'F' para fingir que pisou no Freio
        if (Input.GetKeyDown(KeyCode.F))
        {
            SimulationEvents.TriggerBrakeApplied(1.0f);
            Debug.Log("===> TECLADO: Freio Acionado!");
        }

        // Aperte 'E' para Encerrar a Sessão e imprimir os resultados
        if (Input.GetKeyDown(KeyCode.E))
        {
            SimulationEvents.TriggerSessionEnded();
            Debug.Log("===> TECLADO: Sessão Encerrada!");
            
            // Vamos imprimir o Resumo da Sessão no Console
            if (sessionCollector != null)
            {
                SessionData dadosFinais = sessionCollector.GetCurrentSessionData();
                string jsonPrint = JsonUtility.ToJson(dadosFinais, true);
                Debug.Log("<color=green><b>RESUMO DA SESSÃO:</b></color>\n" + jsonPrint);
            }
        }
    }
}