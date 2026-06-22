using UnityEngine;
using System.Collections.Generic;

public class VisualizationSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject ghostCarPrefab;
    public GameObject routeTrailPrefab;

    [Header("Organização da Cena")]
    public Transform spawnContainer; 

    private Stack<List<GameObject>> batchStack = new Stack<List<GameObject>>();
    public List<TelemetryPlaybackEngine> ActiveEngines { get; private set; } = new List<TelemetryPlaybackEngine>();

    private Color GetUniqueColor(int index)
    {
        float goldenRatioConjugate = 0.618033988749895f;
        float hue = (index * goldenRatioConjugate) % 1f;
        return Color.HSVToRGB(hue, 0.8f, 0.9f); 
    }

    // A assinatura da função agora recebe o TrailMode
    public void SpawnLayer(List<ParticipantRecord> participants, string condition, TrailVisualizer.ColorMode colorMode, TrailVisualizer.TrailMode trailMode, bool useGroupColors, bool showCars, bool showTrails)
    {
        List<GameObject> currentBatch = new List<GameObject>();

        for (int i = 0; i < participants.Count; i++)
        {
            ParticipantRecord p = participants[i];
            
            string actualSessionKey = condition; 
            if (condition == "Primeira Rodada") actualSessionKey = p.InitialExperience; 
            else if (condition == "Segunda Rodada") actualSessionKey = (p.InitialExperience == "MR") ? "VR" : "MR";

            if (!p.Sessions.ContainsKey(actualSessionKey)) continue;
            
            string rawCsv = p.Sessions[actualSessionKey].CarTelemetryCSV;
            if (string.IsNullOrEmpty(rawCsv)) continue;

            GameObject parentObj = new GameObject($"[Telemetria] {p.Email} ({actualSessionKey})");
            if (spawnContainer != null) parentObj.transform.SetParent(spawnContainer);
            
            TelemetryPlaybackEngine engine = parentObj.AddComponent<TelemetryPlaybackEngine>();
            GameObject carObj = Instantiate(ghostCarPrefab, parentObj.transform);
            GameObject trailObj = Instantiate(routeTrailPrefab, parentObj.transform);

            carObj.SetActive(showCars);
            trailObj.SetActive(showTrails);

            GhostCarVisualizer carVis = carObj.GetComponent<GhostCarVisualizer>();
            if (carVis != null) carVis.ConnectToEngine(engine);

            TrailVisualizer trailVis = trailObj.GetComponent<TrailVisualizer>();
            if (trailVis != null) 
            {
                trailVis.ConnectToEngine(engine);
                trailVis.colorMode = colorMode; 
                trailVis.drawMode = trailMode; // <--- NOVO: Aplica o modo de desenho da linha!
                
                if (colorMode == TrailVisualizer.ColorMode.Solid)
                {
                    if (useGroupColors)
                        trailVis.solidColor = p.Gender.Equals("Feminino", System.StringComparison.OrdinalIgnoreCase) ? new Color(1f, 0.4f, 0.7f) : new Color(0.2f, 0.6f, 1f); 
                    else
                        trailVis.solidColor = GetUniqueColor(ActiveEngines.Count + currentBatch.Count);
                }
            }

            engine.LoadFromCSV(rawCsv);
            currentBatch.Add(parentObj);
            ActiveEngines.Add(engine); 
        }

        if (currentBatch.Count > 0)
        {
            batchStack.Push(currentBatch);
            Debug.Log($"<color=green>[Fábrica]</color> Camada adicionada! Total: {batchStack.Count}");
        }
    }

    public void UndoLastBatch()
    {
        if (batchStack.Count == 0) return; 

        List<GameObject> lastBatch = batchStack.Pop();
        foreach (GameObject obj in lastBatch)
        {
            if (obj != null)
            {
                TelemetryPlaybackEngine engine = obj.GetComponent<TelemetryPlaybackEngine>();
                if (engine != null) ActiveEngines.Remove(engine);
                Destroy(obj);
            }
        }
        Debug.Log($"<color=yellow>[Fábrica]</color> Última ação desfeita. Restantes: {batchStack.Count}");
    }

    public void ClearAll()
    {
        foreach (var batch in batchStack)
            foreach (var obj in batch)
                if (obj != null) Destroy(obj);
        
        batchStack.Clear();
        ActiveEngines.Clear();
        Debug.Log("<color=red>[Fábrica]</color> Pista limpa.");
    }
}