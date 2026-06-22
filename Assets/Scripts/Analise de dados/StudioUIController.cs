using UnityEngine;
using UnityEngine.UI; 
using TMPro;          
using System.Collections.Generic;
using System.Linq;

public class StudioUIController : MonoBehaviour
{
    [Header("Interface Geral")]
    [Tooltip("Arraste o Canvas inteiro aqui para o Modo Imersivo")]
    public GameObject uiCanvas; 

    [Header("Conexão com os Sistemas Core")]
    public DatabaseManager dbManager;
    public VisualizationSpawner spawner;

    [Header("Filtros de Dados")]
    public TMP_Dropdown dropdownGender;
    public TMP_Dropdown dropdownCNH;
    public TMP_Dropdown dropdownCondition; 

    [Header("Lista de Participantes")]
    public Transform scrollViewContent;
    public GameObject checkboxPrefab;

    [Header("Configurações de Visualização")]
    public Toggle toggleGroupColors;       
    public Toggle toggleShowCars;          
    public Toggle toggleShowTrails;        
    public TMP_Dropdown dropdownColorMode; 
    public TMP_Dropdown dropdownTrailMode; // NOVO: Dropdown para Dinâmico vs Completo

    [Header("Ações e Linha do Tempo")]
    public Button btnAddLayer;             
    public Button btnClearAll;             
    public Slider globalTimelineSlider; 
    public Button btnPlayPause; 
    private TMP_Text txtPlayPause;
    
    private bool isPlaying = false;
    private Dictionary<Toggle, ParticipantRecord> activeCheckboxes = new Dictionary<Toggle, ParticipantRecord>();

    void Start()
    {
        dropdownGender.onValueChanged.AddListener(delegate { PopulateList(); });
        dropdownCNH.onValueChanged.AddListener(delegate { PopulateList(); });
        dropdownCondition.onValueChanged.AddListener(delegate { PopulateList(); });

        btnAddLayer.onClick.AddListener(AddLayerToTrack);
        if (btnClearAll != null) btnClearAll.onClick.AddListener(ClearAllData);

        globalTimelineSlider.onValueChanged.AddListener(OnTimelineScrub);
        
        if (btnPlayPause != null)
        {
            txtPlayPause = btnPlayPause.GetComponentInChildren<TMP_Text>();
            btnPlayPause.onClick.AddListener(TogglePlayPause);
        }

        if (dbManager.CurrentDatabase != null || dbManager.LoadDatabase()) PopulateList();
    }

    void Update()
    {
        // LÓGICA DO TEMPO GLOBAL
        if (isPlaying && spawner.ActiveEngines.Count > 0)
        {
            globalTimelineSlider.value += Time.deltaTime;
            if (globalTimelineSlider.value >= globalTimelineSlider.maxValue) TogglePlayPause();
        }

        // ==========================================
        // ATALHOS DE TECLADO (PASSO 4)
        // ==========================================
        // Modo Imersivo: Ocultar/Mostrar UI com TAB ou H
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.H))
        {
            if (uiCanvas != null) uiCanvas.SetActive(!uiCanvas.activeSelf);
        }

        // Ctrl + Z: Desfazer a última camada
        if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            spawner.UndoLastBatch();
        }
    }

    public void PopulateList()
    {
        foreach (Transform child in scrollViewContent) Destroy(child.gameObject);
        activeCheckboxes.Clear();

        if (dbManager.CurrentDatabase == null) return;

        string selectedGender = dropdownGender.options[dropdownGender.value].text;
        string selectedCNH = dropdownCNH.options[dropdownCNH.value].text;
        
        var query = dbManager.CurrentDatabase.Participants.AsEnumerable();
        if (selectedGender != "Todos") query = query.Where(p => p.Gender.Equals(selectedGender, System.StringComparison.OrdinalIgnoreCase));
        if (selectedCNH != "Todos") query = query.Where(p => p.HasDriverLicense.Equals(selectedCNH, System.StringComparison.OrdinalIgnoreCase));

        foreach (var p in query.ToList())
        {
            GameObject newCheckbox = Instantiate(checkboxPrefab, scrollViewContent);
            Toggle toggle = newCheckbox.GetComponent<Toggle>();
            Text label = newCheckbox.GetComponentInChildren<Text>(); 
            
            if (label != null) 
            {
                string genderTag = p.Gender.Equals("Masculino", System.StringComparison.OrdinalIgnoreCase) ? "H" : 
                                   p.Gender.Equals("Feminino", System.StringComparison.OrdinalIgnoreCase) ? "M" : "?";
                string cnhTag = p.HasDriverLicense.Equals("Sim", System.StringComparison.OrdinalIgnoreCase) ? "C" : 
                                p.HasDriverLicense.Equals("Não", System.StringComparison.OrdinalIgnoreCase) ? "S" : "?";
                label.text = $"{p.Email} [{genderTag}-{cnhTag}]";
            }
            activeCheckboxes.Add(toggle, p);
        }
    }

    private void AddLayerToTrack()
    {
        List<ParticipantRecord> selectedParticipants = new List<ParticipantRecord>();
        foreach (var kvp in activeCheckboxes) { if (kvp.Key.isOn) selectedParticipants.Add(kvp.Value); }
        if (selectedParticipants.Count == 0) return;

        string condition = dropdownCondition.options[dropdownCondition.value].text; 
        bool useGroupColors = toggleGroupColors != null && toggleGroupColors.isOn;
        bool showCars = toggleShowCars == null || toggleShowCars.isOn; 
        bool showTrails = toggleShowTrails == null || toggleShowTrails.isOn;
        
        TrailVisualizer.ColorMode selectedColorMode = TrailVisualizer.ColorMode.Solid;
        if (dropdownColorMode != null) selectedColorMode = (TrailVisualizer.ColorMode)dropdownColorMode.value;

        // NOVO: Lê a opção de dinâmica vs completa da interface
        TrailVisualizer.TrailMode selectedTrailMode = TrailVisualizer.TrailMode.DynamicWithCar;
        if (dropdownTrailMode != null) selectedTrailMode = (TrailVisualizer.TrailMode)dropdownTrailMode.value;

        spawner.SpawnLayer(selectedParticipants, condition, selectedColorMode, selectedTrailMode, useGroupColors, showCars, showTrails);
        UpdateTimelineAfterSpawn();
    }

    private void ClearAllData()
    {
        spawner.ClearAll();
        globalTimelineSlider.value = 0f;
        globalTimelineSlider.maxValue = 0f;
        isPlaying = false;
        if (txtPlayPause != null) txtPlayPause.text = "Play";
    }

    private void UpdateTimelineAfterSpawn()
    {
        float maxGlobalTime = 0f;
        foreach (var engine in spawner.ActiveEngines)
            if (engine.MaxTime > maxGlobalTime) maxGlobalTime = engine.MaxTime;
        
        if (maxGlobalTime > globalTimelineSlider.maxValue) globalTimelineSlider.maxValue = maxGlobalTime;
    }

    private void OnTimelineScrub(float timeValue) { foreach (var engine in spawner.ActiveEngines) engine.SetTime(timeValue); }
    private void TogglePlayPause()
    {
        if (spawner.ActiveEngines.Count == 0) return; 
        isPlaying = !isPlaying;
        if (txtPlayPause != null) txtPlayPause.text = isPlaying ? "Pause" : "Play";
    }
}