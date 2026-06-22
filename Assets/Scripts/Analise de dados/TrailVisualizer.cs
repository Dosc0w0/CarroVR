using UnityEngine;
using System.Collections.Generic;
using System;

public class TrailVisualizer : MonoBehaviour
{
    public enum TrailMode { FullRouteAtStart, DynamicWithCar }
    public enum ColorMode { Solid, Heatmap_Speed, Heatmap_Acceleration, Heatmap_Braking }

    [Header("Conexão com o Cérebro")]
    public TelemetryPlaybackEngine playbackEngine;

    [Header("Modo de Apresentação")]
    public TrailMode drawMode = TrailMode.DynamicWithCar;
    public ColorMode colorMode = ColorMode.Heatmap_Braking;

    [Header("Configurações Visuais")]
    public float lineWidth = 0.5f;
    public float heightOffset = 0.2f;
    [Tooltip("Distância mínima (em metros) para desenhar um novo segmento")]
    public float minDistanceBetweenPoints = 0.5f;

    [Header("Cores e Gradientes")]
    public Color solidColor = Color.cyan;
    [Tooltip("O Gradiente do Mapa de Calor (Ex: Verde no início, Vermelho no fim)")]
    public Gradient heatmapGradient;
    [Tooltip("Velocidade máxima esperada (km/h) para calcular o gradiente de velocidade")]
    public float maxSpeedKmH = 60f; 

    // Estado Interno
    private Material sharedMaterial;
    private Vector3 lastPoint;
    private Color lastColor;
    private bool hasStarted = false;

    void Awake()
    {
        // Criamos um material único compatível com as cores dos vértices para poupar RAM
        sharedMaterial = new Material(Shader.Find("Sprites/Default"));
    }

    private void OnEnable()
    {
        if (playbackEngine != null)
        {
            playbackEngine.OnDataLoaded += HandleDataLoaded;
            playbackEngine.OnFrameUpdate += HandleFrameUpdate;
        }
    }

    private void OnDisable()
    {
        if (playbackEngine != null)
        {
            playbackEngine.OnDataLoaded -= HandleDataLoaded;
            playbackEngine.OnFrameUpdate -= HandleFrameUpdate;
        }
    }

    // NOVO MÉTODO: Conecta a linha ao motor recebido via código
    public void ConnectToEngine(TelemetryPlaybackEngine newEngine)
    {
        if (playbackEngine != null)
        {
            playbackEngine.OnDataLoaded -= HandleDataLoaded;
            playbackEngine.OnFrameUpdate -= HandleFrameUpdate;
        }
        
        playbackEngine = newEngine;
        
        if (playbackEngine != null)
        {
            playbackEngine.OnDataLoaded += HandleDataLoaded;
            playbackEngine.OnFrameUpdate += HandleFrameUpdate;
        }
    }

    private void HandleDataLoaded()
    {
        ClearTrail(); // Limpa o rastro anterior caso carregue outro utilizador

        if (drawMode == TrailMode.FullRouteAtStart)
        {
            // Se for o modo completo, processamos todos os frames de uma vez num loop rápido
            List<CarTelemetryFrame> allFrames = playbackEngine.GetAllFrames();
            foreach (var frame in allFrames)
            {
                ProcessFrameForLine(frame);
            }
        }
    }

    private void HandleFrameUpdate(CarTelemetryFrame currentFrame)
    {
        // Se estiver no modo dinâmico, processamos apenas o frame atual do carro
        if (drawMode == TrailMode.DynamicWithCar)
        {
            ProcessFrameForLine(currentFrame);
        }
    }

    /// <summary>
    /// A lógica central que avalia o dado e decide se desenha a linha
    /// </summary>
    private void ProcessFrameForLine(CarTelemetryFrame frame)
    {
        Vector3 pointPosition = new Vector3(
            frame.PositionX, 
            frame.PositionY + heightOffset, 
            frame.PositionZ
        );

        Color targetColor = CalculateColorForFrame(frame);

        if (!hasStarted)
        {
            lastPoint = pointPosition;
            lastColor = targetColor;
            hasStarted = true;
            return;
        }

        // Se andou o suficiente, desenha o segmento!
        if (Vector3.Distance(lastPoint, pointPosition) >= minDistanceBetweenPoints)
        {
            CreateSegment(lastPoint, pointPosition, lastColor, targetColor);
            
            // Atualiza para o próximo passo
            lastPoint = pointPosition;
            lastColor = targetColor;
        }
    }

    /// <summary>
    /// Calcula a cor exata daquele frame de acordo com o filtro escolhido
    /// </summary>
    private Color CalculateColorForFrame(CarTelemetryFrame frame)
    {
        if (colorMode == ColorMode.Solid) return solidColor;

        float normalizedValue = 0f;

        switch (colorMode)
        {
            case ColorMode.Heatmap_Speed:
                // Normalizamos a velocidade (de 0 a 1) baseada no máximo configurado
                normalizedValue = Mathf.Clamp01(frame.CurrentSpeed / maxSpeedKmH);
                break;
            
            case ColorMode.Heatmap_Acceleration:
                // O pedal já vai de 0 a 1 no seu CSV
                normalizedValue = Mathf.Clamp01(frame.AccPedal);
                break;

            case ColorMode.Heatmap_Braking:
                // O travão também já vai de 0 a 1
                normalizedValue = Mathf.Clamp01(frame.BrakePedal);
                normalizedValue = Mathf.Log(9*normalizedValue + 1)/Mathf.Log(10);
                break;
        }

        return heatmapGradient.Evaluate(normalizedValue);
    }

    /// <summary>
    /// Instancia um pequeno objeto com um LineRenderer para fazer a ponte
    /// </summary>
    private void CreateSegment(Vector3 start, Vector3 end, Color colorStart, Color colorEnd)
    {
        GameObject segment = new GameObject("Segment");
        segment.transform.SetParent(this.transform); // Organiza dentro do objeto pai

        LineRenderer lr = segment.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = sharedMaterial;
        lr.startColor = colorStart;
        lr.endColor = colorEnd;
    }

    public void ClearTrail()
    {
        hasStarted = false;
        // Destrói todos os segmentos antigos que são filhos deste objeto
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}