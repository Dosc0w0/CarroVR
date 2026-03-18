using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelemetryRecorder : MonoBehaviour
{
    [Header("Referências da Cena")]
    [Tooltip("Arraste o modelo 3D do carro real/virtual aqui")]
    public Transform carTransform;
    [Tooltip("Arraste a câmera do Quest (CenterEyeAnchor) aqui")]
    public Transform headTransform;

    [Header("Configurações de Gravação")]
    [Tooltip("Quantas vezes por segundo vamos gravar as posições? (10 é um bom padrão)")]
    public float recordRateHz = 10f; 

    // Listas que guardarão o histórico completo da corrida
    private List<CarTelemetryFrame> carHistory = new List<CarTelemetryFrame>();
    private List<GazeTelemetryFrame> gazeHistory = new List<GazeTelemetryFrame>();

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
        sessionStartTime = Time.time;
        isRecording = true;

        // Inicia a rotina que roda em background
        StartCoroutine(RecordRoutine());
    }

    private void StopRecording()
    {
        isRecording = false;
        StopAllCoroutines();
        Debug.Log($"[Telemetry] Gravação parada. Frames do Carro: {carHistory.Count} | Frames do Olhar: {gazeHistory.Count}");
        // Na Fase 4, pegaremos essas listas e salvaremos como CSV.
    }

    private IEnumerator RecordRoutine()
    {
        // Calcula o tempo de espera entre cada frame gravado (Ex: 10Hz = 0.1s de espera)
        float waitTime = 1f / recordRateHz;
        WaitForSeconds waitInstruction = new WaitForSeconds(waitTime);

        while (isRecording)
        {
            float currentTimeSinceStart = Time.time - sessionStartTime;

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
                    // CurrentSpeed e SteeringWheelAngle podem ser puxados do seu CarController aqui futuramente
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

            // Pausa a rotina até o próximo ciclo
            yield return waitInstruction;
        }
    }

    // Métodos públicos para o DataPersistenceManager conseguir ler as listas
    public List<CarTelemetryFrame> GetCarHistory() { return carHistory; }
    public List<GazeTelemetryFrame> GetGazeHistory() { return gazeHistory; }
}