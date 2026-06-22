using UnityEngine;

public class GhostCarVisualizer : MonoBehaviour
{
    [Header("Conexão com o Cérebro")]
    [Tooltip("Arraste o objeto que tem o TelemetryPlaybackEngine aqui")]
    public TelemetryPlaybackEngine playbackEngine;

    [Header("Modelos Visuais (Para Escalabilidade)")]
    [Tooltip("Arraste o objeto 3D do carro (o modelo normal ou translúcido) aqui")]
    public Transform carModelTransform;

    private void OnEnable()
    {
        // Quando este script é ativado, ele "assina" o canal do Cérebro
        if (playbackEngine != null)
        {
            playbackEngine.OnFrameUpdate += UpdateCarPose;
        }
    }

    private void OnDisable()
    {
        // Boa prática: sempre "cancelar a assinatura" ao desativar para evitar memory leaks
        if (playbackEngine != null)
        {
            playbackEngine.OnFrameUpdate -= UpdateCarPose;
        }
    }

    // NOVO MÉTODO: Conecta o carro ao motor recebido via código
    public void ConnectToEngine(TelemetryPlaybackEngine newEngine)
    {
        // Limpa a assinatura anterior por segurança
        if (playbackEngine != null) playbackEngine.OnFrameUpdate -= UpdateCarPose;
        
        playbackEngine = newEngine;
        
        // Assina imediatamente o canal do novo motor
        if (playbackEngine != null) playbackEngine.OnFrameUpdate += UpdateCarPose;
    }

    /// <summary>
    /// Esta função é chamada automaticamente pelo Cérebro a cada frame da interpolação
    /// </summary>
    private void UpdateCarPose(CarTelemetryFrame currentFrame)
    {
        if (carModelTransform == null) return;

        // 1. Atualizamos a Posição X, Y, Z
        carModelTransform.position = new Vector3(
            currentFrame.PositionX, 
            currentFrame.PositionY, 
            currentFrame.PositionZ
        );

        // 2. Atualizamos a Rotação (usando Quaternions para os Ângulos de Euler)
        carModelTransform.rotation = Quaternion.Euler(
            currentFrame.RotationX, 
            currentFrame.RotationY, 
            currentFrame.RotationZ
        );

        // NOTA PARA O FUTURO: 
        // Se mais tarde quisermos animar as rodas ou acender luzes de travão, 
        // basta ler o currentFrame.SteeringWheelAngle e currentFrame.BrakePedal aqui!
    }
}