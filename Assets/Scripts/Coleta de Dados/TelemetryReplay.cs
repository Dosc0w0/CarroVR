using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TelemetryReplayer : MonoBehaviour
{
    [Header("Objetos para Animar (Mock)")]
    public Transform carTransform;
    public Transform headTransform;
    // --- MUDANÇA 1: Referências para os modelos 3D das mãos do replay ---
    public Transform leftHandTransform;
    public Transform rightHandTransform;

    [Header("Arquivos de Dados (Arraste os CSVs aqui)")]
    public TextAsset carCSVFile; 
    public TextAsset headCSVFile; 
    // --- MUDANÇA 2: Arquivo CSV das mãos ---
    public TextAsset handCSVFile; 

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
    // --- MUDANÇA 3: Lista de dados das mãos ---
    private List<HandTelemetryFrame> handFrames = new List<HandTelemetryFrame>();

    // Variáveis de Estado do Player
    private bool isDataLoaded = false;
    private bool isPlaying = false;
    private float currentReplayTime = 0f;
    private float maxReplayTime = 0f;
    
    // Índices de leitura
    private int currentCarIndex = 0;
    private int currentHeadIndex = 0;
    // --- MUDANÇA 4: Índice das mãos ---
    private int currentHandIndex = 0;

    void Start()
    {
        if (playOnStart) 
        {
            LoadAndPlay();
        }
    }

    void Update()
    {
        HandleInputs();

        if (!isDataLoaded) return;

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

        // --- MUDANÇA 5: Chamada para atualizar as mãos ---
        UpdateCarTransform();
        UpdateHeadTransform();
        UpdateHandTransforms();
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
        if (carFrames.Count > 0 || headFrames.Count > 0 || handFrames.Count > 0)
        {
            isDataLoaded = true;
            isPlaying = true;
            currentReplayTime = 0f;
            currentCarIndex = 0;
            currentHeadIndex = 0;
            currentHandIndex = 0; // Reseta o índice
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

    // --- MUDANÇA 6: O Motor de Animação e Visibilidade das Mãos ---
    private void UpdateHandTransforms()
    {
        if (handFrames.Count == 0) return;

        while (currentHandIndex > 0 && handFrames[currentHandIndex].TimeSinceStart > currentReplayTime) currentHandIndex--;
        while (currentHandIndex < handFrames.Count - 1 && handFrames[currentHandIndex + 1].TimeSinceStart <= currentReplayTime) currentHandIndex++;

        var frame = handFrames[currentHandIndex];

        // Processa a Mão Esquerda
        if (leftHandTransform != null)
        {
            // Se perdeu o rastreio, oculta a mão instantaneamente
            leftHandTransform.gameObject.SetActive(frame.IsLeftTracked);
            
            if (frame.IsLeftTracked)
            {
                if (currentHandIndex < handFrames.Count - 1 && handFrames[currentHandIndex + 1].IsLeftTracked)
                {
                    var nextFrame = handFrames[currentHandIndex + 1];
                    float t = (currentReplayTime - frame.TimeSinceStart) / (nextFrame.TimeSinceStart - frame.TimeSinceStart);
                    
                    leftHandTransform.position = Vector3.Lerp(
                        new Vector3(frame.LeftPosX, frame.LeftPosY, frame.LeftPosZ),
                        new Vector3(nextFrame.LeftPosX, nextFrame.LeftPosY, nextFrame.LeftPosZ), t);
                        
                    leftHandTransform.rotation = Quaternion.Slerp(
                        Quaternion.Euler(frame.LeftRotX, frame.LeftRotY, frame.LeftRotZ),
                        Quaternion.Euler(nextFrame.LeftRotX, nextFrame.LeftRotY, nextFrame.LeftRotZ), t);
                }
                else
                {
                    leftHandTransform.position = new Vector3(frame.LeftPosX, frame.LeftPosY, frame.LeftPosZ);
                    leftHandTransform.rotation = Quaternion.Euler(frame.LeftRotX, frame.LeftRotY, frame.LeftRotZ);
                }
            }
        }

        // Processa a Mão Direita
        if (rightHandTransform != null)
        {
            rightHandTransform.gameObject.SetActive(frame.IsRightTracked);
            
            if (frame.IsRightTracked)
            {
                if (currentHandIndex < handFrames.Count - 1 && handFrames[currentHandIndex + 1].IsRightTracked)
                {
                    var nextFrame = handFrames[currentHandIndex + 1];
                    float t = (currentReplayTime - frame.TimeSinceStart) / (nextFrame.TimeSinceStart - frame.TimeSinceStart);
                    
                    rightHandTransform.position = Vector3.Lerp(
                        new Vector3(frame.RightPosX, frame.RightPosY, frame.RightPosZ),
                        new Vector3(nextFrame.RightPosX, nextFrame.RightPosY, nextFrame.RightPosZ), t);
                        
                    rightHandTransform.rotation = Quaternion.Slerp(
                        Quaternion.Euler(frame.RightRotX, frame.RightRotY, frame.RightRotZ),
                        Quaternion.Euler(nextFrame.RightRotX, nextFrame.RightRotY, nextFrame.RightRotZ), t);
                }
                else
                {
                    rightHandTransform.position = new Vector3(frame.RightPosX, frame.RightPosY, frame.RightPosZ);
                    rightHandTransform.rotation = Quaternion.Euler(frame.RightRotX, frame.RightRotY, frame.RightRotZ);
                }
            }
        }
    }

    private void ParseData()
    {
        carFrames.Clear();
        headFrames.Clear();
        handFrames.Clear(); // Limpa o cache antigo
        maxReplayTime = 0f;

        // LÊ O ARQUIVO FÍSICO DO CARRO
        if (carCSVFile != null && !string.IsNullOrWhiteSpace(carCSVFile.text))
        {
            string[] lines = carCSVFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++) 
            {
                string[] cols = lines[i].Split(',');
                
                if (cols.Length >= 11)
                {
                    CarTelemetryFrame frame = new CarTelemetryFrame
                    {
                        TimeSinceStart = float.Parse(cols[0], CultureInfo.InvariantCulture),
                        PositionX = float.Parse(cols[2], CultureInfo.InvariantCulture),
                        PositionY = float.Parse(cols[3], CultureInfo.InvariantCulture),
                        PositionZ = float.Parse(cols[4], CultureInfo.InvariantCulture),
                        RotationX = float.Parse(cols[5], CultureInfo.InvariantCulture),
                        RotationY = float.Parse(cols[6], CultureInfo.InvariantCulture),
                        RotationZ = float.Parse(cols[7], CultureInfo.InvariantCulture),
                        CurrentSpeed = float.Parse(cols[8], CultureInfo.InvariantCulture),
                        SteeringWheelAngle = float.Parse(cols[9], CultureInfo.InvariantCulture),
                        AccPedal = float.Parse(cols[10], CultureInfo.InvariantCulture),
                        BrakePedal = float.Parse(cols[11], CultureInfo.InvariantCulture)
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

        // --- MUDANÇA 7: LÊ O ARQUIVO FÍSICO DAS MÃOS ---
        if (handCSVFile != null && !string.IsNullOrWhiteSpace(handCSVFile.text))
        {
            string[] lines = handCSVFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] cols = lines[i].Split(',');
                // Exigimos as 15 colunas que criamos no DataPersistenceManager
                if (cols.Length >= 15) 
                {
                    HandTelemetryFrame frame = new HandTelemetryFrame
                    {
                        TimeSinceStart = float.Parse(cols[0], CultureInfo.InvariantCulture),
                        // Transforma os "0" e "1" do CSV de volta para booleanos
                        IsLeftTracked = int.Parse(cols[1], CultureInfo.InvariantCulture) == 1,
                        LeftPosX = float.Parse(cols[2], CultureInfo.InvariantCulture),
                        LeftPosY = float.Parse(cols[3], CultureInfo.InvariantCulture),
                        LeftPosZ = float.Parse(cols[4], CultureInfo.InvariantCulture),
                        LeftRotX = float.Parse(cols[5], CultureInfo.InvariantCulture),
                        LeftRotY = float.Parse(cols[6], CultureInfo.InvariantCulture),
                        LeftRotZ = float.Parse(cols[7], CultureInfo.InvariantCulture),
                        
                        IsRightTracked = int.Parse(cols[8], CultureInfo.InvariantCulture) == 1,
                        RightPosX = float.Parse(cols[9], CultureInfo.InvariantCulture),
                        RightPosY = float.Parse(cols[10], CultureInfo.InvariantCulture),
                        RightPosZ = float.Parse(cols[11], CultureInfo.InvariantCulture),
                        RightRotX = float.Parse(cols[12], CultureInfo.InvariantCulture),
                        RightRotY = float.Parse(cols[13], CultureInfo.InvariantCulture),
                        RightRotZ = float.Parse(cols[14], CultureInfo.InvariantCulture)
                    };
                    handFrames.Add(frame);
                }
            }
            if (handFrames.Count > 0) maxReplayTime = Mathf.Max(maxReplayTime, handFrames[handFrames.Count - 1].TimeSinceStart);
        }
    }
}