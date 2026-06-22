using UnityEngine;

public class VisualFloatingEffect : MonoBehaviour
{
    // Usamos localPosition e localRotation para que, se você mover o carro ou a pista,
    // a bateria continue flutuando corretamente em relação ao seu próprio eixo.

    [Header("Configurações de Rotação (Girar)")]
    [Tooltip("Velocidade de rotação em graus por segundo nos eixos X, Y e Z.")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 90f, 0f); // Padrão: Gira só no Y (em pé)

    [Header("Configurações de Flutuação (Ondas)")]
    [Tooltip("A distância máxima que o objeto sobe e desce a partir da posição original (em metros).")]
    [SerializeField] private float floatAmplitude = 0.05f; // 5 centímetros suave

    [Tooltip("A velocidade da flutuação. Números maiores fazem flutuar mais rápido (Hertz/ciclos por segundo).")]
    [SerializeField] private float floatFrequency = 1f; // 1 ciclo por segundo

    [Tooltip("Adiciona um deslocamento de tempo inicial. Útil se você tiver várias baterias alinhadas e quiser que elas subam e desçam em tempos diferentes (efeito 'onda').")]
    [SerializeField] private float timeOffset = 0f;

    // Variável interna para guardar a posição central original (o 'zero' da flutuação)
    private Vector3 startPosition;

    void Start()
    {
        // Guarda a posição local exata em que você colocou a bateria no editor
        startPosition = transform.localPosition;
    }

    void Update()
    {
        // deltaTime garante que o movimento seja suave e constante, independente da taxa de quadros (FPS) do Quest.
        AnimateRotation();
        AnimateFloating();
    }

    private void AnimateRotation()
    {
        // Multiplica a velocidade definida pelo tempo decorrido neste frame
        Vector3 rotationThisFrame = rotationSpeed * Time.deltaTime;
        // Aplica a rotação no espaço local (Space.Self)
        transform.Rotate(rotationThisFrame, Space.Self);
    }

    private void AnimateFloating()
    {
        // Fórmula da Onda Senoidal: f(t) = Amplitude * sen( Frequência * t * 2PI + Offset)
        // O seno retorna um valor entre -1 e 1 em um ciclo suave.
        
        // Multiplicamos a frequência por 2*PI para que a variável 'floatFrequency' signifique "ciclos por segundo".
        float cycles = (Time.time + timeOffset) * floatFrequency;
        float rawSine = Mathf.Sin(cycles * Mathf.PI * 2.0f); // Retorna de -1.0 a 1.0 suavemente

        // Transforma o -1 a 1 na distância que queremos (ex: 0.05 a -0.05)
        float verticalOffset = rawSine * floatAmplitude;

        // Aplica a nova posição baseada na original central
        transform.localPosition = new Vector3(startPosition.x, startPosition.y + verticalOffset, startPosition.z);
    }
}