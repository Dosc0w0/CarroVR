using UnityEngine;
using UnityEngine.UI; 
using TMPro;          
using System.Collections.Generic;
using System.Linq;

public class LabUIController : MonoBehaviour
{
    [Header("Sistemas Core")]
    public DatabaseManager dbManager;
    public PhysicsMetricsExtractor metricsExtractor;
    public TelemetryPlaybackEngine visualEngine; // O motor antigo apenas para o slider

    [Header("Filtros de Dados")]
    public TMP_Dropdown dropdownCondition; 

    [Header("Lista (Seleção Unitária)")]
    public Transform scrollViewContent;
    public GameObject checkboxPrefab;
    public ToggleGroup singleSelectionGroup; // <--- O TRUQUE DE UX AQUI

    [Header("Ações e Visualização")]
    public Button btnLoadVisual;
    public Slider timelineSlider;
    public TMP_Text txtTimeline;

    [Header("Ações de Extração")]
    public Button btnExtractLap1;
    public Button btnExtract5Min;
    public TMP_Text txtResults;

    private Dictionary<Toggle, ParticipantRecord> activeCheckboxes = new Dictionary<Toggle, ParticipantRecord>();

    void Start()
    {
        dropdownCondition.onValueChanged.AddListener(delegate { PopulateList(); });

        btnLoadVisual.onClick.AddListener(LoadVisualGhost);
        btnExtractLap1.onClick.AddListener(ExtractSliderTime);
        btnExtract5Min.onClick.AddListener(Extract5MinLimit);

        timelineSlider.onValueChanged.AddListener(OnTimelineScrub);

        if (dbManager.CurrentDatabase != null || dbManager.LoadDatabase()) PopulateList();
    }

    public void PopulateList()
    {
        foreach (Transform child in scrollViewContent) Destroy(child.gameObject);
        activeCheckboxes.Clear();

        if (dbManager.CurrentDatabase == null) return;

        // Para o laboratório, queremos ver todos de uma vez e escolher um, por isso removemos filtros de sexo/CNH
        var participants = dbManager.CurrentDatabase.Participants;

        foreach (var p in participants)
        {
            GameObject newCheckbox = Instantiate(checkboxPrefab, scrollViewContent);
            Toggle toggle = newCheckbox.GetComponent<Toggle>();
            Text label = newCheckbox.GetComponentInChildren<Text>(); 
            
            // Oculta a mágica da caixa única
            toggle.group = singleSelectionGroup;
            
            if (label != null) label.text = $"{p.Email}";
            
            activeCheckboxes.Add(toggle, p);
        }
    }

    // Retorna o único participante que o utilizador deixou marcado
    private ParticipantRecord GetSelectedParticipant()
    {
        foreach (var kvp in activeCheckboxes) { if (kvp.Key.isOn) return kvp.Value; }
        return null;
    }

    // ==========================================
    // 1. CARREGAR PARA O SLIDER (VISUAL)
    // ==========================================
    private void LoadVisualGhost()
    {
        ParticipantRecord p = GetSelectedParticipant();
        if (p == null) return;

        string condition = dropdownCondition.options[dropdownCondition.value].text;
        string actualKey = condition == "Primeira Rodada" ? p.InitialExperience : (p.InitialExperience == "MR" ? "VR" : "MR");

        if (p.Sessions.ContainsKey(actualKey))
        {
            visualEngine.LoadFromCSV(p.Sessions[actualKey].CarTelemetryCSV);
            timelineSlider.maxValue = visualEngine.MaxTime;
            timelineSlider.value = 0f;
            txtResults.text = $"Visualizando: {p.Email}\nDeslize o slider para encontrar o fim da Volta 1.";
        }
    }

    private void OnTimelineScrub(float timeValue)
    {
        visualEngine.SetTime(timeValue);
        if (txtTimeline != null) txtTimeline.text = $"{timeValue:F1} s";
    }

    // ==========================================
    // 2. EXTRAIR MÉTRICAS (FÍSICA)
    // ==========================================
    private void ExtractSliderTime()
    {
        ParticipantRecord p = GetSelectedParticipant();
        if (p == null) return;

        string condition = dropdownCondition.options[dropdownCondition.value].text;
        txtResults.text = "A processar matemática complexa...";
        
        // Manda o motor físico rodar até ao tempo em que você parou o Slider!
        metricsExtractor.RunExtraction(p, condition, timelineSlider.value, OnExtractionFinished);
    }

    private void Extract5MinLimit()
    {
        ParticipantRecord p = GetSelectedParticipant();
        if (p == null) return;

        string condition = dropdownCondition.options[dropdownCondition.value].text;
        txtResults.text = "A processar experiência completa...";
        
        // 300 segundos cravados
        metricsExtractor.RunExtraction(p, condition, 300f, OnExtractionFinished);
    }

    private void OnExtractionFinished(string resultText)
    {
        txtResults.text = resultText;
    }
}