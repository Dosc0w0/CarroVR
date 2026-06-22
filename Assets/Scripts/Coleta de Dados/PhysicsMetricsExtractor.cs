using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class PhysicsMetricsExtractor : MonoBehaviour
{
    [Header("Prefabs de Simulação")]
    [Tooltip("Arraste o Prefab do Carro que possui um Rigidbody e Collider")]
    public GameObject physicsCarPrefab;
    
    [Tooltip("Arraste o Prefab Pai de todas as Baterias (da pasta Prefabs)")]
    public GameObject batteryMapPrefab;
    
    [Header("Organização")]
    public Transform spawnContainer;

    // Variáveis de Contagem
    private int tempBatteryCount = 0;

    private void OnEnable() { SimulationEvents.OnItemCollected += CountBattery; }
    private void OnDisable() { SimulationEvents.OnItemCollected -= CountBattery; }
    private void CountBattery() { tempBatteryCount++; }

    /// <summary>
    /// Inicia a corrida fantasma em fast-forward até ao tempo limite escolhido
    /// </summary>
    public void RunExtraction(ParticipantRecord participant, string condition, float targetTime, System.Action<string> onComplete)
    {
        StartCoroutine(ExtractionRoutine(participant, condition, targetTime, onComplete));
    }

    private IEnumerator ExtractionRoutine(ParticipantRecord participant, string condition, float targetTime, System.Action<string> onComplete)
    {
        // 1. Encontra a sessão correta (MR ou VR) com base na rodada selecionada
        string actualSessionKey = condition == "Primeira Rodada" ? participant.InitialExperience : (participant.InitialExperience == "MR" ? "VR" : "MR");
        
        if (!participant.Sessions.ContainsKey(actualSessionKey) || string.IsNullOrEmpty(participant.Sessions[actualSessionKey].CarTelemetryCSV)) 
        {
            onComplete?.Invoke($"[Erro] Faltam dados na sessão {actualSessionKey} para {participant.Email}");
            yield break;
        }

        // 2. Transforma o CSV em Frames
        List<CarTelemetryFrame> frames = ParseCSV(participant.Sessions[actualSessionKey].CarTelemetryCSV);
        if (frames.Count == 0) yield break;

        // 3. Prepara o Cenário Físico
        GameObject batteries = Instantiate(batteryMapPrefab, spawnContainer);
        GameObject car = Instantiate(physicsCarPrefab, spawnContainer);
        
        Rigidbody rb = car.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; // Essencial para podermos teleportar via código

        // Zera os contadores
        tempBatteryCount = 0;
        float totalDistance = 0f;
        float timeOutsideTrack = 0f;
        int trackExits = 0;
        bool wasOutside = false;
        
        float sumSpeed = 0f;
        int speedSamples = 0;

        // 4. A MAGIA: ACELERA O TEMPO DA UNITY EM 20x
        Time.timeScale = 20f;

        Vector3 lastPos = new Vector3(frames[0].PositionX, frames[0].PositionY, frames[0].PositionZ);
        if (rb != null) rb.position = lastPos;

        for (int i = 0; i < frames.Count; i++)
        {
            if (frames[i].TimeSinceStart > targetTime) break; // Para exatamente no corte que você fez no Slider!

            Vector3 newPos = new Vector3(frames[i].PositionX, frames[i].PositionY, frames[i].PositionZ);
            Quaternion newRot = Quaternion.Euler(frames[i].RotationX, frames[i].RotationY, frames[i].RotationZ);

            // Move fisicamente (ativa colisões com as baterias!)
            if (rb != null)
            {
                rb.MovePosition(newPos);
                rb.MoveRotation(newRot);
            }
            else
            {
                car.transform.SetPositionAndRotation(newPos, newRot);
            }

            // Espera a Unity calcular as colisões deste frame
            yield return new WaitForFixedUpdate();

            // SOMA A DISTÂNCIA
            totalDistance += Vector3.Distance(lastPos, newPos);
            lastPos = newPos;

            // SOMA A VELOCIDADE (para a média final)
            sumSpeed += frames[i].CurrentSpeed;
            speedSamples++;

            // O LASER PARA SAÍDA DE PISTA
            // Dispara um raio de 1 metro acima do carro para baixo
            Vector3 rayOrigin = newPos + Vector3.up * 1f; 
            bool isOutside = true;
            
            // Se o laser bater em algo nos próximos 3 metros para baixo...
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 3f))
            {
                // Verifica se bateu no Mesh Collider que adicionámos à rua
                if (hit.collider.CompareTag("Pista")) isOutside = false;
            }

            if (isOutside)
            {
                // Calcula quanto tempo passou na terra (diferença entre este frame e o anterior)
                float dt = (i > 0) ? (frames[i].TimeSinceStart - frames[i-1].TimeSinceStart) : 0.1f;
                timeOutsideTrack += dt;

                if (!wasOutside) 
                {
                    trackExits++; // Conta uma nova saída
                    wasOutside = true;
                }
            }
            else 
            {
                wasOutside = false;
            }
        }

        // 5. Devolve o tempo da Unity ao normal
        Time.timeScale = 1f;
        
        float avgSpeed = speedSamples > 0 ? (sumSpeed / speedSamples) : 0f;

        // Limpa a "Sala de Cirurgia"
        Destroy(car);
        Destroy(batteries);

        // Gera o relatório!
        string result = $"Métricas Processadas:\n" +
                        $"Duração Avaliada: {targetTime:F1} s\n" +
                        $"Distância: {totalDistance:F1} m\n" +
                        $"Vel. Média: {avgSpeed:F1} km/h\n" +
                        $"Baterias: {tempBatteryCount}\n" +
                        $"Tempo Fora: {timeOutsideTrack:F1} s ({trackExits} saídas)";
        
        onComplete?.Invoke(result);
    }

    // Leitor de CSV interno simplificado (idêntico ao seu TelemetryPlaybackEngine)
    private List<CarTelemetryFrame> ParseCSV(string rawCsv)
    {
        List<CarTelemetryFrame> frames = new List<CarTelemetryFrame>();
        string[] lines = rawCsv.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        float timeOffset = -1f; 

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 12) continue; 
            
            int trackPhase = int.Parse(cols[1], CultureInfo.InvariantCulture);
            if (trackPhase == 0) continue; // Ignora tutorial

            float originalTime = float.Parse(cols[0], CultureInfo.InvariantCulture);
            if (timeOffset < 0f) timeOffset = originalTime;

            frames.Add(new CarTelemetryFrame
            {
                TimeSinceStart = originalTime - timeOffset,
                PositionX = float.Parse(cols[2], CultureInfo.InvariantCulture),
                PositionY = float.Parse(cols[3], CultureInfo.InvariantCulture),
                PositionZ = float.Parse(cols[4], CultureInfo.InvariantCulture),
                RotationX = float.Parse(cols[5], CultureInfo.InvariantCulture),
                RotationY = float.Parse(cols[6], CultureInfo.InvariantCulture),
                RotationZ = float.Parse(cols[7], CultureInfo.InvariantCulture),
                CurrentSpeed = float.Parse(cols[8], CultureInfo.InvariantCulture),
            });
        }
        return frames;
    }
}