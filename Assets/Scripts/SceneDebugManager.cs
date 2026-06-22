using System.Collections.Generic;
using UnityEngine;

public class SceneDebugManager : MonoBehaviour
{
    [Header("Comunicação com o Spawner")]
    [Tooltip("A tag exata do objeto que possui o script RoadSpawner")]
    public string roadSpawnerTag = "RoadSpawner";

    [Header("Elementos de UI / Debug")]
    [Tooltip("Arraste os Canvas ou GameObjects (ex: painéis de telemetria) que deseja mostrar/esconder")]
    public List<GameObject> debugObjectsToToggle = new List<GameObject>();
    
    // Controle de estado
    private bool areObjectsVisible = true;
    
    // Mantém o rastro de qual pista estamos (0 = Tutorial, 1 = Oficial)
    private int currentTrackPhase = 0; 

    void Update()
    {
/* The commented code block is checking for a specific input event using the Oculus VR Input system. It
is looking for a button press on the left index trigger (Gatilho Indicador Esquerdo) of the Oculus
Touch controller (LTouch). When the button press is detected, the `GoToOfficialTrack()` method is
called, which advances the game to the official track phase by interacting with the `RoadSpawner`
object in the scene. */
        // // 1. AVANÇAR PARA PISTA OFICIAL (Gatilho Indicador Esquerdo)
        // if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        // {
        //     GoToOfficialTrack();
        // }

        // // 2. RESETAR PISTA ATUAL / BOTÃO DE PÂNICO (Grip / Gatilho do Dedo Médio Esquerdo)
        // if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        // {
        //     ResetCurrentTrack();
        // }

        // 3. MOSTRAR/ESCONDER UI (Pressionar o Analógico/Joystick Esquerdo para baixo)
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
        {
            ToggleDebugObjects();
        }
    }

    // private void GoToOfficialTrack()
    // {
    //     RoadSpawner spawner = FindRoadSpawner();
    //     if (spawner != null)
    //     {
    //         currentTrackPhase = 1; // 1 é o índice do Prefab Oficial
    //         spawner.SwitchTrack(currentTrackPhase);
    //         Debug.Log("<color=cyan>[DebugManager] Avançando para Pista Oficial!</color>");
    //     }
    // }

    // private void ResetCurrentTrack()
    // {
    //     RoadSpawner spawner = FindRoadSpawner();
    //     if (spawner != null)
    //     {
    //         // Se o usuário bater o carro no tutorial, reseta o tutorial. 
    //         // Se bater na oficial, reseta a oficial.
    //         spawner.SwitchTrack(currentTrackPhase);
    //         Debug.Log($"<color=yellow>[DebugManager] Resetando a pista atual (Fase {currentTrackPhase})</color>");
    //     }
    // }

    // Função auxiliar que vasculha a cena procurando o construtor de estradas
    private RoadSpawner FindRoadSpawner()
    {
        GameObject spawnerObj = GameObject.FindGameObjectWithTag(roadSpawnerTag);
        if (spawnerObj != null)
        {
            return spawnerObj.GetComponent<RoadSpawner>();
        }
        
        Debug.LogError($"[DebugManager] ERRO: Nenhum objeto com a tag '{roadSpawnerTag}' foi encontrado na cena!");
        return null;
    }

    private void ToggleDebugObjects()
    {
        areObjectsVisible = !areObjectsVisible;

        // Percorre a lista e ativa/desativa cada objeto de UI
        foreach (GameObject obj in debugObjectsToToggle)
        {
            if (obj != null)
            {
                obj.SetActive(areObjectsVisible);
            }
        }
        
        Debug.Log($"<color=yellow>[DebugManager] UI visível: {areObjectsVisible}</color>");
    }
}