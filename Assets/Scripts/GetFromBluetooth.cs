using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Canvas")]
    public GameObject popup;

    [Header("Textos")]
    public TMP_Text statusText;
    public TMP_Text feedbackText;

    [Header("Botões")]
    public Button retryButton;
    public Button exitButton;

    // Botão do controle esquerdo inicia a conexão Bluetooth
    void Update(){
        //if (Input.GetKeyDown(KeyCode.JoystickButton2)){
        //    start_bluetooth();
        //}

        // Teste pelo teclado
        if (Input.GetKeyDown(KeyCode.B)){
            start_bluetooth();
        }
    }

    // Método para iniciar a conexão Bluetooth
    private void start_bluetooth(){
        
        Debug.Log("Iniciando conexão Bluetooth...");
        
        popup.SetActive(true);
        
    }
}