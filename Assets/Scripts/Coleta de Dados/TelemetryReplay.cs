using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TelemetryReplayer : MonoBehaviour
{
    [Header("Objetos para Animar (Mock)")]
    public Transform carTransform;
    public Transform headTransform;

    [Header("Arquivos de Dados (Arraste os CSVs aqui)")]
    public TextAsset carCSVFile; // <- MUDANÇA AQUI
    public TextAsset headCSVFile; // <- MUDANÇA AQUI

    [Header("Configurações do Player")]
    public bool playOnStart = false;
    public bool loop = false;
    [Tooltip("Multiplicador de velocidade ao avançar")]
    public float fastForwardSpeed = 3f;
    [Tooltip("Multiplicador de velocidade ao retornar")]
    public float rewindSpeed = -3f;

    [Header("Mapeamento de Controles")]
    public KeyCode playPauseKey = KeyCode.Space;
    public KeyCode restartKey = KeyCode.R;
    public KeyCode forwardKey = KeyCode.RightArrow;
    public KeyCode rewindKey = KeyCode.LeftArrow;

    // Listas de dados
    private List<CarTelemetryFrame> carFrames = new List<CarTelemetryFrame>();
    private List<GazeTelemetryFrame> headFrames = new List<GazeTelemetryFrame>();

    // Variáveis de Estado do Player
    private bool isDataLoaded = false;
    private bool isPlaying = false;
    private float currentReplayTime = 0f;
    private float maxReplayTime = 0f;
    
    // Índices de leitura
    private int currentCarIndex = 0;
    private int currentHeadIndex = 0;

    void Start()
    {
        if (playOnStart) 
        {
            LoadAndPlay();
        }
    }

    void Update()
    {
        // 1. LER CONTROLES DO USUÁRIO
        HandleInputs();

        if (!isDataLoaded) return;

        // 2. CALCULAR A VELOCIDADE DO TEMPO NESTE FRAME
        float currentSpeed = 0f;
        
        if (Input.GetKey(forwardKey))
        {
            currentSpeed = fastForwardSpeed; 
        }
        else if (Input.GetKey(rewindKey))
        {
            currentSpeed = rewindSpeed; 
        }
        else if (isPlaying)
        {
            currentSpeed = 1f; 
        }

        // 3. ATUALIZAR A LINHA DO TEMPO VIRTUAL
        if (currentSpeed != 0f)
        {
            currentReplayTime += Time.deltaTime * currentSpeed;

            if (currentReplayTime >= maxReplayTime)
            {
                if (loop)
                {
                    currentReplayTime = 0f; 
                }
                else
                {
                    currentReplayTime = maxReplayTime;
                    isPlaying = false; 
                }
            }
            else if (currentReplayTime < 0f)
            {
                currentReplayTime = 0f;
                if (!isPlaying) currentSpeed = 0f; 
            }
        }

        // 4. APLICAR AS TRANSFORMAÇÕES AOS OBJETOS
        UpdateCarTransform();
        UpdateHeadTransform();
    }

    private void HandleInputs()
    {
        if (Input.GetKeyDown(restartKey))
        {
            LoadAndPlay();
        }

        if (isDataLoaded && Input.GetKeyDown(playPauseKey))
        {
            if (!isPlaying && currentReplayTime >= maxReplayTime)
            {
                currentReplayTime = 0f;
            }
            isPlaying = !isPlaying;
            Debug.Log(isPlaying ? "[Player] ▶ Play" : "[Player] ⏸ Pause");
        }
    }

    public void LoadAndPlay()
    {
        ParseData();
        if (carFrames.Count > 0 || headFrames.Count > 0)
        {
            isDataLoaded = true;
            isPlaying = true;
            currentReplayTime = 0f;
            currentCarIndex = 0;
            currentHeadIndex = 0;
            Debug.Log($"<color=green>[Player] Vídeo Carregado! Duração: {maxReplayTime:F2} segundos.</color>");
        }
        else
        {
            Debug.LogWarning("[Player] Erro: Não há dados válidos nos arquivos CSV.");
        }
    }

    private void UpdateCarTransform()
    {
        if (carFrames.Count == 0 || carTransform == null) return;

        while (currentCarIndex > 0 && carFrames[currentCarIndex].TimeSinceStart > currentReplayTime) currentCarIndex--;
        while (currentCarIndex < carFrames.Count - 1 && carFrames[currentCarIndex + 1].TimeSinceStart <= currentReplayTime) currentCarIndex++;

        var frame = carFrames[currentCarIndex];

        if (currentCarIndex < carFrames.Count - 1)
        {
            var nextFrame = carFrames[currentCarIndex + 1];
            float t = (currentReplayTime - frame.TimeSinceStart) / (nextFrame.TimeSinceStart - frame.TimeSinceStart);
            
            carTransform.position = Vector3.Lerp(
                new Vector3(frame.PositionX, frame.PositionY, frame.PositionZ),
                new Vector3(nextFrame.PositionX, nextFrame.PositionY, nextFrame.PositionZ), t);
                
            carTransform.rotation = Quaternion.Slerp(
                Quaternion.Euler(frame.RotationX, frame.RotationY, frame.RotationZ),
                Quaternion.Euler(nextFrame.RotationX, nextFrame.RotationY, nextFrame.RotationZ), t);
        }
        else
        {
            carTransform.position = new Vector3(frame.PositionX, frame.PositionY, frame.PositionZ);
            carTransform.rotation = Quaternion.Euler(frame.RotationX, frame.RotationY, frame.RotationZ);
        }
    }

    private void UpdateHeadTransform()
    {
        if (headFrames.Count == 0 || headTransform == null) return;

        while (currentHeadIndex > 0 && headFrames[currentHeadIndex].TimeSinceStart > currentReplayTime) currentHeadIndex--;
        while (currentHeadIndex < headFrames.Count - 1 && headFrames[currentHeadIndex + 1].TimeSinceStart <= currentReplayTime) currentHeadIndex++;

        var frame = headFrames[currentHeadIndex];

        if (currentHeadIndex < headFrames.Count - 1)
        {
            var nextFrame = headFrames[currentHeadIndex + 1];
            float t = (currentReplayTime - frame.TimeSinceStart) / (nextFrame.TimeSinceStart - frame.TimeSinceStart);
            
            headTransform.position = Vector3.Lerp(
                new Vector3(frame.HeadPositionX, frame.HeadPositionY, frame.HeadPositionZ),
                new Vector3(nextFrame.HeadPositionX, nextFrame.HeadPositionY, nextFrame.HeadPositionZ), t);
                
            headTransform.rotation = Quaternion.Slerp(
                Quaternion.Euler(frame.HeadRotationX, frame.HeadRotationY, frame.HeadRotationZ),
                Quaternion.Euler(nextFrame.HeadRotationX, nextFrame.HeadRotationY, nextFrame.HeadRotationZ), t);
        }
        else
        {
            headTransform.position = new Vector3(frame.HeadPositionX, frame.HeadPositionY, frame.HeadPositionZ);
            headTransform.rotation = Quaternion.Euler(frame.HeadRotationX, frame.HeadRotationY, frame.HeadRotationZ);
        }
    }

    private void ParseData()
    {
        carFrames.Clear();
        headFrames.Clear();
        maxReplayTime = 0f;

        // LÊ O ARQUIVO FÍSICO DO CARRO
        if (carCSVFile != null && !string.IsNullOrWhiteSpace(carCSVFile.text))
        {
            string[] lines = carCSVFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++) 
            {
                string[] cols = lines[i].Split(',');
                if (cols.Length >= 7)
                {
                    CarTelemetryFrame frame = new CarTelemetryFrame
                    {
                        TimeSinceStart = float.Parse(cols[0], CultureInfo.InvariantCulture),
                        PositionX = float.Parse(cols[1], CultureInfo.InvariantCulture),
                        PositionY = float.Parse(cols[2], CultureInfo.InvariantCulture),
                        PositionZ = float.Parse(cols[3], CultureInfo.InvariantCulture),
                        RotationX = float.Parse(cols[4], CultureInfo.InvariantCulture),
                        RotationY = float.Parse(cols[5], CultureInfo.InvariantCulture),
                        RotationZ = float.Parse(cols[6], CultureInfo.InvariantCulture)
                    };
                    carFrames.Add(frame);
                }
            }
            if (carFrames.Count > 0) maxReplayTime = Mathf.Max(maxReplayTime, carFrames[carFrames.Count - 1].TimeSinceStart);
        }

        // LÊ O ARQUIVO FÍSICO DA CABEÇA
        if (headCSVFile != null && !string.IsNullOrWhiteSpace(headCSVFile.text))
        {
            string[] lines = headCSVFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] cols = lines[i].Split(',');
                if (cols.Length >= 7)
                {
                    GazeTelemetryFrame frame = new GazeTelemetryFrame
                    {
                        TimeSinceStart = float.Parse(cols[0], CultureInfo.InvariantCulture),
                        HeadPositionX = float.Parse(cols[1], CultureInfo.InvariantCulture),
                        HeadPositionY = float.Parse(cols[2], CultureInfo.InvariantCulture),
                        HeadPositionZ = float.Parse(cols[3], CultureInfo.InvariantCulture),
                        HeadRotationX = float.Parse(cols[4], CultureInfo.InvariantCulture),
                        HeadRotationY = float.Parse(cols[5], CultureInfo.InvariantCulture),
                        HeadRotationZ = float.Parse(cols[6], CultureInfo.InvariantCulture)
                    };
                    headFrames.Add(frame);
                }
            }
            if (headFrames.Count > 0) maxReplayTime = Mathf.Max(maxReplayTime, headFrames[headFrames.Count - 1].TimeSinceStart);
        }
    }
}