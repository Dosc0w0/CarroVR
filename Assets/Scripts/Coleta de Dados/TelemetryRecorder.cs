using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelemetryRecorder : MonoBehaviour
{
    [Header("Referências da Cena (Carro e Cabeça)")]
    [Tooltip("Será preenchido automaticamente buscando a tag 'Car'")]
    public Transform carTransform;
    [Tooltip("Arraste a câmara do Quest (CenterEyeAnchor) aqui")]
    public Transform headTransform;

    [Header("Referências de Dados do Carro")]
    [Tooltip("Será preenchido automaticamente")]
    public VelocityController velocityController;
    [Tooltip("Será preenchido automaticamente buscando a tag 'SteeringWheel'")]
    public WheelRotator wheelRotator;
    public SessionCollector sessionCollector;

    [Header("Referências de Hand Tracking (Quest)")]
    public OVRHand leftHand;
    public OVRHand rightHand;

    [Header("Configurações de Gravação")]
    public float recordRateHz = 10f; 

    private List<CarTelemetryFrame> carHistory = new List<CarTelemetryFrame>();
    private List<GazeTelemetryFrame> gazeHistory = new List<GazeTelemetryFrame>();
    private List<HandTelemetryFrame> handHistory = new List<HandTelemetryFrame>();

    private bool isRecording = false;
    private float sessionStartTime = 0f;
    
    // --- NOVA VARIÁVEL: FASE ATUAL ---
    private int currentTrackPhase = 0;

    private void OnEnable()
    {
        SimulationEvents.OnSessionStarted += StartRecording;
        SimulationEvents.OnSessionEnded += StopRecording;
        
        // --- NOVA ASSINATURA: Escuta a mudança de fase ---
        SimulationEvents.OnOfficialTrackStarted += HandleOfficialTrackStarted;
    }

    private void OnDisable()
    {
        SimulationEvents.OnSessionStarted -= StartRecording;
        SimulationEvents.OnSessionEnded -= StopRecording;
        
        SimulationEvents.OnOfficialTrackStarted -= HandleOfficialTrackStarted;
    }

    // ==========================================
    // NOVA FUNÇÃO: MUDA A FASE PARA 1
    // ==========================================
    private void HandleOfficialTrackStarted()
    {
        currentTrackPhase = 1;
        Debug.Log("<color=magenta>[Telemetry] Gravador atualizado para Fase Oficial (TrackPhase = 1)</color>");
    }

    private void TryFindCarReferences()
    {
        if (carTransform == null)
        {
            GameObject carObj = GameObject.FindGameObjectWithTag("Car");
            if (carObj != null) carTransform = carObj.transform;
        }

        if (velocityController == null)
        {
            velocityController = FindObjectOfType<VelocityController>();
        }

        if (wheelRotator == null)
        {
            GameObject wheelObj = GameObject.FindGameObjectWithTag("SteeringWheel");
            if (wheelObj != null) wheelRotator = wheelObj.GetComponent<WheelRotator>();
        }

        if (carTransform == null || velocityController == null || wheelRotator == null)
        {
            Debug.LogWarning("[Telemetry] Atenção: Algumas referências do carro não foram encontradas na cena!");
        }
        else
        {
            Debug.Log("<color=green>[Telemetry] Carro e scripts encontrados dinamicamente com sucesso!</color>");
        }
    }

    private void StartRecording(string sessionID)
    {
        TryFindCarReferences();

        carHistory.Clear();
        gazeHistory.Clear();
        handHistory.Clear();
        
        sessionStartTime = Time.time;
        isRecording = true;
        currentTrackPhase = 0; // Garante que a fase reseta para 0 ao iniciar uma nova sessão

        StartCoroutine(RecordRoutine());
    }

    private void StopRecording()
    {
        isRecording = false;
        StopAllCoroutines();
        Debug.Log($"[Telemetry] Gravação parada. Carro: {carHistory.Count} | Cabeça: {gazeHistory.Count} | Mãos: {handHistory.Count}");
    }

    private IEnumerator RecordRoutine()
    {
        float waitTime = 1f / recordRateHz;
        WaitForSeconds waitInstruction = new WaitForSeconds(waitTime);

        while (isRecording)
        {
            float currentTimeSinceStart = Time.time - sessionStartTime;

            float currentVel = velocityController != null ? velocityController.Velocity : 0f;
            float currentAcc = velocityController != null ? velocityController.Acceleration : 0f;
            float steerAngle = wheelRotator != null ? wheelRotator.currentAngle : 0f; 
            
            float accRaw = velocityController != null ? (velocityController.RawPedalAcc / 100f) : 0f;
            float brkRaw = velocityController != null ? (velocityController.RawPedalBrake / 100f) : 0f;

            if (sessionCollector != null)
            {
                sessionCollector.UpdateDynamics(Mathf.Abs(currentVel), Mathf.Abs(currentAcc));
            }

            // 1. Fotografia do Carro
            if (carTransform != null)
            {
                CarTelemetryFrame carFrame = new CarTelemetryFrame
                {
                    TimeSinceStart = currentTimeSinceStart,
                    
                    // --- MUDANÇA AQUI: Injeta a fase atual no frame ---
                    TrackPhase = currentTrackPhase, 
                    
                    PositionX = carTransform.position.x,
                    PositionY = carTransform.position.y,
                    PositionZ = carTransform.position.z,
                    RotationX = carTransform.eulerAngles.x,
                    RotationY = carTransform.eulerAngles.y,
                    RotationZ = carTransform.eulerAngles.z,
                    CurrentSpeed = currentVel,
                    SteeringWheelAngle = steerAngle,
                    AccPedal = accRaw,
                    BrakePedal = brkRaw
                };
                carHistory.Add(carFrame);
            }

            // 2. Fotografia da Cabeça/Olhar
            if (headTransform != null)
            {
                GazeTelemetryFrame gazeFrame = new GazeTelemetryFrame
                {
                    TimeSinceStart = currentTimeSinceStart,
                    HeadPositionX = headTransform.position.x,
                    HeadPositionY = headTransform.position.y,
                    HeadPositionZ = headTransform.position.z,
                    HeadRotationX = headTransform.eulerAngles.x,
                    HeadRotationY = headTransform.eulerAngles.y,
                    HeadRotationZ = headTransform.eulerAngles.z
                };
                gazeHistory.Add(gazeFrame);
            }

            // 3. FOTOGRAFIA DAS MÃOS
            bool isLeftTracked = leftHand != null && leftHand.IsTracked;
            bool isRightTracked = rightHand != null && rightHand.IsTracked;

            HandTelemetryFrame handFrame = new HandTelemetryFrame
            {
                TimeSinceStart = currentTimeSinceStart,
                
                IsLeftTracked = isLeftTracked,
                LeftPosX = isLeftTracked ? leftHand.transform.position.x : 0f,
                LeftPosY = isLeftTracked ? leftHand.transform.position.y : 0f,
                LeftPosZ = isLeftTracked ? leftHand.transform.position.z : 0f,
                LeftRotX = isLeftTracked ? leftHand.transform.eulerAngles.x : 0f,
                LeftRotY = isLeftTracked ? leftHand.transform.eulerAngles.y : 0f,
                LeftRotZ = isLeftTracked ? leftHand.transform.eulerAngles.z : 0f,

                IsRightTracked = isRightTracked,
                RightPosX = isRightTracked ? rightHand.transform.position.x : 0f,
                RightPosY = isRightTracked ? rightHand.transform.position.y : 0f,
                RightPosZ = isRightTracked ? rightHand.transform.position.z : 0f,
                RightRotX = isRightTracked ? rightHand.transform.eulerAngles.x : 0f,
                RightRotY = isRightTracked ? rightHand.transform.eulerAngles.y : 0f,
                RightRotZ = isRightTracked ? rightHand.transform.eulerAngles.z : 0f
            };
            handHistory.Add(handFrame);

            yield return waitInstruction;
        }
    }

    public List<CarTelemetryFrame> GetCarHistory() { return carHistory; }
    public List<GazeTelemetryFrame> GetGazeHistory() { return gazeHistory; }
    public List<HandTelemetryFrame> GetHandHistory() { return handHistory; }
}