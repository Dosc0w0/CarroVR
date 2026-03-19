using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelemetryRecorder : MonoBehaviour
{
    [Header("Referências da Cena (Carro e Cabeça)")]
    [Tooltip("Arraste o modelo 3D do carro real/virtual aqui")]
    public Transform carTransform;
    [Tooltip("Arraste a câmara do Quest (CenterEyeAnchor) aqui")]
    public Transform headTransform;

    [Header("Referências de Dados do Carro")]
    public VelocityController velocityController;
    public WheelRotator wheelRotator;
    public SessionCollector sessionCollector;

    // ==========================================
    // NOVAS REFERÊNCIAS: HAND TRACKING
    // ==========================================
    [Header("Referências de Hand Tracking (Quest)")]
    [Tooltip("Arraste o objeto OVRHand Prefab da Mão Esquerda")]
    public OVRHand leftHand;
    [Tooltip("Arraste o objeto OVRHand Prefab da Mão Direita")]
    public OVRHand rightHand;
    // ==========================================

    [Header("Configurações de Gravação")]
    [Tooltip("Quantas vezes por segundo vamos gravar as posições? (10 é um bom padrão)")]
    public float recordRateHz = 10f; 

    private List<CarTelemetryFrame> carHistory = new List<CarTelemetryFrame>();
    private List<GazeTelemetryFrame> gazeHistory = new List<GazeTelemetryFrame>();
    
    // MUDANÇA: Nossa nova lista para guardar o histórico das mãos
    private List<HandTelemetryFrame> handHistory = new List<HandTelemetryFrame>();

    private bool isRecording = false;
    private float sessionStartTime = 0f;

    private void OnEnable()
    {
        SimulationEvents.OnSessionStarted += StartRecording;
        SimulationEvents.OnSessionEnded += StopRecording;
    }

    private void OnDisable()
    {
        SimulationEvents.OnSessionStarted -= StartRecording;
        SimulationEvents.OnSessionEnded -= StopRecording;
    }

    private void StartRecording(string sessionID)
    {
        carHistory.Clear();
        gazeHistory.Clear();
        handHistory.Clear(); // Limpa o cache antigo das mãos
        
        sessionStartTime = Time.time;
        isRecording = true;

        StartCoroutine(RecordRoutine());
    }

    private void StopRecording()
    {
        isRecording = false;
        StopAllCoroutines();
        // Atualizei o log para mostrar os frames capturados das mãos também
        Debug.Log($"[Telemetry] Gravação parada. Carro: {carHistory.Count} | Cabeça: {gazeHistory.Count} | Mãos: {handHistory.Count}");
    }

    private IEnumerator RecordRoutine()
    {
        float waitTime = 1f / recordRateHz;
        WaitForSeconds waitInstruction = new WaitForSeconds(waitTime);

        while (isRecording)
        {
            float currentTimeSinceStart = Time.time - sessionStartTime;

            // --- CAPTURA DOS DADOS DINÂMICOS DO CARRO ---
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

            // ==========================================
            // 3. FOTOGRAFIA DAS MÃOS (Hand Tracking)
            // ==========================================
            bool isLeftTracked = leftHand != null && leftHand.IsTracked;
            bool isRightTracked = rightHand != null && rightHand.IsTracked;

            HandTelemetryFrame handFrame = new HandTelemetryFrame
            {
                TimeSinceStart = currentTimeSinceStart,
                
                // Mão Esquerda (Se não estiver rastreada, salva a posição como zero)
                IsLeftTracked = isLeftTracked,
                LeftPosX = isLeftTracked ? leftHand.transform.position.x : 0f,
                LeftPosY = isLeftTracked ? leftHand.transform.position.y : 0f,
                LeftPosZ = isLeftTracked ? leftHand.transform.position.z : 0f,
                LeftRotX = isLeftTracked ? leftHand.transform.eulerAngles.x : 0f,
                LeftRotY = isLeftTracked ? leftHand.transform.eulerAngles.y : 0f,
                LeftRotZ = isLeftTracked ? leftHand.transform.eulerAngles.z : 0f,

                // Mão Direita (Se não estiver rastreada, salva a posição como zero)
                IsRightTracked = isRightTracked,
                RightPosX = isRightTracked ? rightHand.transform.position.x : 0f,
                RightPosY = isRightTracked ? rightHand.transform.position.y : 0f,
                RightPosZ = isRightTracked ? rightHand.transform.position.z : 0f,
                RightRotX = isRightTracked ? rightHand.transform.eulerAngles.x : 0f,
                RightRotY = isRightTracked ? rightHand.transform.eulerAngles.y : 0f,
                RightRotZ = isRightTracked ? rightHand.transform.eulerAngles.z : 0f
            };
            handHistory.Add(handFrame);
            // ==========================================

            yield return waitInstruction;
        }
    }

    public List<CarTelemetryFrame> GetCarHistory() { return carHistory; }
    public List<GazeTelemetryFrame> GetGazeHistory() { return gazeHistory; }
    
    // MUDANÇA: Método para o DataPersistenceManager puxar os dados e salvar no CSV
    public List<HandTelemetryFrame> GetHandHistory() { return handHistory; }
}