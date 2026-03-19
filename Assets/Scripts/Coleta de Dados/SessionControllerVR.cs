using System;
using UnityEngine;

public class SessionControllerVR : MonoBehaviour
{
    [Header("Configurações do Controle")]
    [Tooltip("Botão do controle esquerdo para iniciar/parar a sessão (Padrão: Botão X)")]
    public OVRInput.Button toggleSessionButton = OVRInput.Button.Three;

    [Header("Feedback (Opcional)")]
    [Tooltip("Arraste um AudioSource aqui se quiser um 'Bip' ao gravar")]
    public AudioSource feedbackAudio;
    public AudioClip startClip;
    public AudioClip stopClip;

    private bool isSessionActive = false;

    void Update()
    {
        // Verifica se o botão escolhido foi pressionado no controle ESQUERDO (LTouch)
        if (OVRInput.GetDown(toggleSessionButton))
        {
            ToggleSession();
        }
    }

    // Deixei público caso você queira chamar por um botão de UI no futuro
    public void ToggleSession()
    {
        if (!isSessionActive)
        {
            // Gera um ID único baseado no relógio (Ex: Sessao_20260319_143000)
            string newSessionID = "Sessao_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            // Dispara o evento que acorda o TelemetryRecorder e o SessionCollector
            SimulationEvents.TriggerSessionStarted(newSessionID);
            isSessionActive = true;

            Debug.Log($"<color=green>[VR Controller] Gravação INICIADA: {newSessionID}</color>");
            PlayFeedback(startClip);
        }
        else
        {
            // Dispara o evento que para a gravação, salva os CSVs e ativa o CloudSyncManager
            SimulationEvents.TriggerSessionEnded();
            isSessionActive = false;

            Debug.Log("<color=red>[VR Controller] Gravação ENCERRADA.</color>");
            PlayFeedback(stopClip);
        }
    }

    private void PlayFeedback(AudioClip clip)
    {
        if (feedbackAudio != null && clip != null)
        {
            feedbackAudio.PlayOneShot(clip);
        }
    }
}