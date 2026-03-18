using UnityEngine;

public class MockQuestTester : MonoBehaviour
{
    [Header("Objeto para o Joystick")]
    public Transform objetoParaMover; // Arraste um cubo ou algo visível aqui
    public float velocidade = 2f;

    private bool sessaoIniciada = false;

    void Start()
    {
        // Já inicia a sessão de cara para não precisarmos de mais botões
        string fakeID = "MockQuest_" + System.DateTime.Now.ToString("HHmmss");
        SimulationEvents.TriggerSessionStarted(fakeID);
        sessaoIniciada = true;
        Debug.Log($"[Mock] Sessão iniciada! ID: {fakeID}");
    }

    void Update()
    {
        // 1. Coletar Item (Botões A ou B do controle direito)
        // OVRInput.Button.One = A | OVRInput.Button.Two = B
        if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Two))
        {
            SimulationEvents.TriggerItemCollected();
            Debug.Log("[Mock] Item coletado via A/B!");
        }

        // Bônus: Vamos fingir que o gatilho direito (Index Trigger) é o freio
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            SimulationEvents.TriggerBrakeApplied(1.0f);
            Debug.Log("[Mock] Freio acionado no gatilho!");
        }

        // 2. Mover o objeto com o Joystick Direito (Para frente e para trás)
        Vector2 joystickDireito = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        if (objetoParaMover != null && Mathf.Abs(joystickDireito.y) > 0.05f)
        {
            // Move no eixo Z local (frente/trás)
            objetoParaMover.Translate(Vector3.forward * joystickDireito.y * velocidade * Time.deltaTime);
        }

        // 3. Encerrar a sessão e GERAR OS ARQUIVOS (Botão X do controle esquerdo)
        // OVRInput.Button.Three = X
        if (sessaoIniciada && OVRInput.GetDown(OVRInput.Button.Three))
        {
            SimulationEvents.TriggerSessionEnded();
            sessaoIniciada = false;
            Debug.Log("<color=red>[Mock] Sessão ENCERRADA via botão X. Verifique os arquivos!</color>");
        }
    }
}