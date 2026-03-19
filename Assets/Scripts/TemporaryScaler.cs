using UnityEngine;
using TMPro;

public class TemporaryScaler : MonoBehaviour
{
    [Header("Alvo (Opcional)")]
    [Tooltip("Se vazio, afetará este próprio objeto. Se preenchido, afetará a Referência.")]
    public Transform targetReference;

    [Header("Configurações de Escala (Controle Direito)")]
    public float scaleStep = 0.1f;
    public float minScale = 0.1f;
    public float maxScale = 5.0f;

    [Header("Configurações de Posição (Controle Esquerdo)")]
    [Tooltip("O quanto a posição vai mudar por cada clique/toque no analógico")]
    public float positionStep = 0.1f;
    [Tooltip("O quão longe o analógico precisa ser empurrado para registrar o comando")]
    public float thumbstickThreshold = 0.5f;

    [Header("UI (Opcional)")]
    public TextMeshProUGUI scaleText;

    private Transform target;

    // Variáveis internas para exigir a "recentralização" do analógico esquerdo
    private bool isXAxisInUse = false;
    private bool isYAxisInUse = false;

    void Start()
    {
        // Define o alvo: se você não arrastou nada pro Inspector, ele usa o objeto atual
        target = targetReference != null ? targetReference : transform;

        scaleText = GameObject.FindGameObjectWithTag("ScaleTextUI")?.GetComponent<TextMeshProUGUI>();
        UpdateScaleText();
    }

    void Update()
    {
        HandleScale();
        HandlePosition();
    }

    /// <summary>
    /// Gerencia o aumento e diminuição da escala (Controle Direito)
    /// </summary>
    private void HandleScale()
    {
        // Aumentar: Botão B (por clique)
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            ChangeScale(scaleStep);
        }

        // Diminuir: Grip Direito (por clique)
        if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger))
        {
            ChangeScale(-scaleStep);
        }
    }

    /// <summary>
    /// Gerencia o movimento da referência em passos minuciosos (Controle Esquerdo)
    /// </summary>
    private void HandlePosition()
    {
        // 1. Para Cima: Gatilho Esquerdo (Index Trigger) - Clique para mover
        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
        {
            target.Translate(Vector3.up * positionStep, Space.World);
        }

        // 2. Para Baixo: Grip Esquerdo (Hand Trigger) - Clique para mover
        if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger))
        {
            target.Translate(Vector3.down * positionStep, Space.World);
        }

        // Lê a posição atual do analógico esquerdo
        Vector2 thumbstick = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);

        // 3. Frente e Trás (Eixo Y do Analógico)
        if (Mathf.Abs(thumbstick.y) > thumbstickThreshold)
        {
            // Só move se o eixo não estiver em uso (ou seja, se acabou de ser empurrado)
            if (!isYAxisInUse)
            {
                float direction = Mathf.Sign(thumbstick.y); // Retorna 1 (frente) ou -1 (trás)
                target.Translate(Vector3.forward * direction * positionStep, Space.World);
                isYAxisInUse = true; // Bloqueia novos movimentos até recentralizar
            }
        }
        else if (Mathf.Abs(thumbstick.y) < 0.2f) // Zona morta para recentralizar
        {
            isYAxisInUse = false; // Libera o eixo para o próximo toque
        }

        // 4. Esquerda e Direita (Eixo X do Analógico)
        if (Mathf.Abs(thumbstick.x) > thumbstickThreshold)
        {
            if (!isXAxisInUse)
            {
                float direction = Mathf.Sign(thumbstick.x); // Retorna 1 (direita) ou -1 (esquerda)
                target.Translate(Vector3.right * direction * positionStep, Space.World);
                isXAxisInUse = true;
            }
        }
        else if (Mathf.Abs(thumbstick.x) < 0.2f) // Zona morta para recentralizar
        {
            isXAxisInUse = false;
        }
    }

    private void ChangeScale(float amount)
    {
        Vector3 currentScale = target.localScale;
        float newScaleValue = Mathf.Clamp(currentScale.x + amount, minScale, maxScale);
        
        target.localScale = new Vector3(newScaleValue, newScaleValue, newScaleValue);
        
        UpdateScaleText();
    }

    private void UpdateScaleText()
    {
        if (scaleText != null)
        {
            scaleText.text = $"Escala Atual: {target.localScale.x:F2}";
        }
    }
}