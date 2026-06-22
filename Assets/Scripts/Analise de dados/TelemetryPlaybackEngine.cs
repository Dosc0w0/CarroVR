using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TelemetryPlaybackEngine : MonoBehaviour
{
    [Header("Configurações de Reprodução")]
    [Tooltip("Se marcado, ignora o tutorial (TrackPhase == 0)")]
    public bool playOnlyOfficialPhase = true;
    
    [Tooltip("Velocidade do tempo (1 = Normal, 2 = 2x mais rápido)")]
    public float playbackSpeed = 1f;
    
    public bool isPlaying = false;

    [Header("Estado Atual (Somente Leitura)")]
    [SerializeField] private float currentTime = 0f;
    [SerializeField] private float maxTime = 0f;
    private int currentFrameIndex = 0;

    // A lista principal guardada na RAM
    private List<CarTelemetryFrame> frames = new List<CarTelemetryFrame>();

    // ==========================================
    // O EVENTO DE TRANSMISSÃO (A "Antena")
    // Qualquer script (Carro Fantasma, UI, Linha) pode "ouvir" isto
    // ==========================================
    public event Action<CarTelemetryFrame> OnFrameUpdate;
    public event Action OnDataLoaded;

    /// <summary>
    /// Recebe o texto gigante do CSV e transforma na lista de frames utilizável.
    /// </summary>
    public void LoadFromCSV(string rawCsv)
    {
        frames.Clear();
        currentTime = 0f;
        currentFrameIndex = 0;

        if (string.IsNullOrEmpty(rawCsv)) return;

        string[] lines = rawCsv.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        // Variável para guardar o "tempo gasto" antes da fase oficial começar
        float timeOffset = -1f; 

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 12) continue; 

            int trackPhase = int.Parse(cols[1], CultureInfo.InvariantCulture);
            
            // FILTRO DE FASE
            if (playOnlyOfficialPhase && trackPhase == 0) continue;

            float originalTime = float.Parse(cols[0], CultureInfo.InvariantCulture);

            // O primeiro frame que passar pelo filtro define o nosso "Novo Zero"
            if (timeOffset < 0f)
            {
                timeOffset = originalTime;
            }

            CarTelemetryFrame frame = new CarTelemetryFrame();
            
            // NORMALIZAÇÃO: Subtraímos o offset para que a corrida comece sempre em 0.0s
            frame.TimeSinceStart = originalTime - timeOffset;
            frame.TrackPhase = trackPhase;
            
            frame.PositionX = float.Parse(cols[2], CultureInfo.InvariantCulture);
            frame.PositionY = float.Parse(cols[3], CultureInfo.InvariantCulture);
            frame.PositionZ = float.Parse(cols[4], CultureInfo.InvariantCulture);
            
            frame.RotationX = float.Parse(cols[5], CultureInfo.InvariantCulture);
            frame.RotationY = float.Parse(cols[6], CultureInfo.InvariantCulture);
            frame.RotationZ = float.Parse(cols[7], CultureInfo.InvariantCulture);
            
            frame.CurrentSpeed = float.Parse(cols[8], CultureInfo.InvariantCulture);
            frame.SteeringWheelAngle = float.Parse(cols[9], CultureInfo.InvariantCulture);
            frame.AccPedal = float.Parse(cols[10], CultureInfo.InvariantCulture);
            frame.BrakePedal = float.Parse(cols[11], CultureInfo.InvariantCulture);

            frames.Add(frame);
        }

        if (frames.Count > 0)
        {
            currentTime = frames[0].TimeSinceStart; // Agora isto será sempre 0
            maxTime = frames[frames.Count - 1].TimeSinceStart;
            
            Debug.Log($"<color=cyan>[Cérebro]</color> Processados {frames.Count} frames. Duração oficial: {maxTime:F2} segundos.");
            
            OnDataLoaded?.Invoke(); 
        }
    }

    private void Update()
    {
        if (!isPlaying || frames.Count == 0) return;

        // Avançamos o relógio interno usando o tempo real da Unity
        currentTime += Time.deltaTime * playbackSpeed;

        if (currentTime > maxTime)
        {
            currentTime = maxTime;
            isPlaying = false; // Pausa automaticamente ao cruzar a linha de chegada
        }

        CalculateAndBroadcastInterpolatedFrame();
    }

    /// <summary>
    /// A mágica que transforma dados de 10Hz num movimento fluido de 60 FPS
    /// </summary>
    private void CalculateAndBroadcastInterpolatedFrame()
    {
        // Encontra em que parte da lista estamos baseados no currentTime
        while (currentFrameIndex < frames.Count - 2 && frames[currentFrameIndex + 1].TimeSinceStart < currentTime)
        {
            currentFrameIndex++;
        }

        CarTelemetryFrame frameA = frames[currentFrameIndex];
        CarTelemetryFrame frameB = currentFrameIndex + 1 < frames.Count ? frames[currentFrameIndex + 1] : frameA;

        // Qual a percentagem (de 0.0 a 1.0) que estamos entre o Frame A e o B?
        float timeDiff = frameB.TimeSinceStart - frameA.TimeSinceStart;
        float t = timeDiff > 0 ? (currentTime - frameA.TimeSinceStart) / timeDiff : 0;

        // Construímos um frame temporário fundindo os valores
        CarTelemetryFrame interpolatedFrame = new CarTelemetryFrame
        {
            TimeSinceStart = currentTime,
            TrackPhase = frameA.TrackPhase,
            
            PositionX = Mathf.Lerp(frameA.PositionX, frameB.PositionX, t),
            PositionY = Mathf.Lerp(frameA.PositionY, frameB.PositionY, t),
            PositionZ = Mathf.Lerp(frameA.PositionZ, frameB.PositionZ, t),
            
            // Usamos LerpAngle nas rotações para o carro não dar um "giro maluco de 360º" 
            // ao passar de 359 para 1 grau.
            RotationX = Mathf.LerpAngle(frameA.RotationX, frameB.RotationX, t),
            RotationY = Mathf.LerpAngle(frameA.RotationY, frameB.RotationY, t),
            RotationZ = Mathf.LerpAngle(frameA.RotationZ, frameB.RotationZ, t),

            CurrentSpeed = Mathf.Lerp(frameA.CurrentSpeed, frameB.CurrentSpeed, t),
            SteeringWheelAngle = Mathf.Lerp(frameA.SteeringWheelAngle, frameB.SteeringWheelAngle, t),
            AccPedal = Mathf.Lerp(frameA.AccPedal, frameB.AccPedal, t),
            BrakePedal = Mathf.Lerp(frameA.BrakePedal, frameB.BrakePedal, t)
        };

        // Grita para todo o sistema: "O CARRO ESTÁ AQUI NESTE MILISSEGUNDO!"
        OnFrameUpdate?.Invoke(interpolatedFrame);
    }

    // ==========================================
    // CONTROLOS PÚBLICOS (Para usar depois com botões de UI)
    // ==========================================
    public void Play() => isPlaying = true;
    public void Pause() => isPlaying = false;
    
    public void SetTime(float newTime) 
    { 
        if (frames.Count == 0) return;
        currentTime = Mathf.Clamp(newTime, frames[0].TimeSinceStart, maxTime);
        currentFrameIndex = 0; // Reinicia a busca
        CalculateAndBroadcastInterpolatedFrame();
    }

    public List<CarTelemetryFrame> GetAllFrames() { return frames; }

    // Permite que a UI saiba qual é o limite de tempo deste utilizador
    public float MaxTime => maxTime;
}