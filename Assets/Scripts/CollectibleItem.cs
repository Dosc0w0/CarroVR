using UnityEngine;

// Estas marcações garantem que a Unity adicione automaticamente os componentes necessários
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class CollectibleItem : MonoBehaviour
{
    [Header("Configurações do Coletável")]
    [Tooltip("Tag do objeto que pode coletar este item (o carro)")]
    public string collectorTag = "Player";

    // --- NOVA VARIÁVEL: BIFURCAÇÃO DE LÓGICA ---
    [Header("Modo de Gatilho")]
    [Tooltip("Se ativado, este item NÃO soma pontos. Ele serve apenas como a linha de partida para o teste oficial.")]
    [SerializeField] private bool isOfficialStartLine = false;

    private MeshRenderer meshRenderer;
    private Collider itemCollider;
    private AudioSource audioSource;
    
    // Trava de segurança para impedir que a Unity colete o mesmo item duas vezes no mesmo milissegundo
    private bool isCollected = false;

    private void Awake()
    {
        // Busca as referências no próprio objeto
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        itemCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        // Força o colisor a ser um "Trigger" (não bloqueia fisicamente o carro)
        itemCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Se já foi pego, ignora qualquer outra colisão
        if (isCollected) return;

        // Verifica se quem bateu tem a etiqueta correta (ex: "Player")
        if (other.CompareTag(collectorTag))
        {
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;

        // ==========================================
        // A MÁGICA DOS DOIS PORTÕES (TWO GATES)
        // ==========================================
        if (isOfficialStartLine)
        {
            // É a linha de partida oficial! Grita o novo evento.
            SimulationEvents.TriggerOfficialTrackStarted();
        }
        else
        {
            // É um item normal. Soma um ponto no placar.
            SimulationEvents.TriggerItemCollected();
        }
        // ==========================================

        // 2. Esconde o objeto visualmente (para parecer que sumiu)
        if (meshRenderer != null) 
        {
            meshRenderer.enabled = false;
        }

        // 3. Desativa o colisor para não causar novas colisões fantasmas
        itemCollider.enabled = false;

        // 4. Toca o feedback sonoro e agenda a destruição para quando o som acabar
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            // Destrói o objeto apenas após os segundos de duração do áudio
            Destroy(gameObject, audioSource.clip.length);
        }
        else
        {
            // Fallback: se esquecer de colocar um som, ele destrói imediatamente
            Destroy(gameObject);
        }
    }
}