using UnityEngine;
using System.IO;
using System.Collections;

public class ScreenshotExporter : MonoBehaviour
{
    [Header("Teclas de Atalho")]
    [Tooltip("A tecla que vai disparar a captura de ecrã.")]
    public KeyCode captureKey = KeyCode.P;

    [Header("Configurações de Resolução")]
    [Tooltip("Multiplicador de resolução da câmara. 1 = Resolução nativa da janela. 2 = Dobro (ex: 4K se a janela for 1080p), 3 = Triplo, etc.")]
    [Range(1, 5)]
    public int resolutionMultiplier = 2;

    [Header("Configurações de Ficheiro")]
    [Tooltip("O texto que aparecerá no início do nome de cada ficheiro de imagem.")]
    public string fileNamePrefix = "SITA_RV_Export";

    [Tooltip("O nome da pasta onde as imagens serão guardadas (será criada ao lado da pasta Assets).")]
    public string folderName = "Screenshots_SITA_RV";

    [Header("Interface")]
    [Tooltip("Arraste o StudioUI aqui para ele ser escondido no momento da foto.")]
    public GameObject uiCanvas;

    private string saveDirectory;

    void Start()
    {
        // Guarda FORA da pasta Assets para evitar que o Unity congele a tentar importar imagens pesadas
        saveDirectory = Path.Combine(Application.dataPath, "../" + folderName);
        
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
            Debug.Log($"<color=cyan>[Exportador]</color> Pasta de imagens criada em: {saveDirectory}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(captureKey))
        {
            StartCoroutine(CaptureRoutine());
        }
    }

    private IEnumerator CaptureRoutine()
    {
        bool wasUiActive = false;

        // 1. Esconde a interface
        if (uiCanvas != null)
        {
            wasUiActive = uiCanvas.activeSelf;
            uiCanvas.SetActive(false);
        }

        // 2. Espera o final da frame para garantir que o ecrã foi desenhado sem a UI
        yield return new WaitForEndOfFrame();

        // 3. Gera o nome do ficheiro com a data e hora exatas
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"{fileNamePrefix}_{timestamp}.png";
        string fullPath = Path.Combine(saveDirectory, filename);

        // 4. Tira a foto mágica
        ScreenCapture.CaptureScreenshot(fullPath, resolutionMultiplier);
        Debug.Log($"<color=magenta>[Exportador]</color> Sucesso! Imagem guardada em: {fullPath} (Resolução base x{resolutionMultiplier})");

        // 5. Devolve a interface 
        if (uiCanvas != null && wasUiActive)
        {
            uiCanvas.SetActive(true);
        }
    }
}