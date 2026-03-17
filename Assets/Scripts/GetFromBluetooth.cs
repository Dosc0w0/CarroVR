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

    // Variables
    string device2connect = null;

    // Plugin Bluetooth
    private AndroidJavaObject plugin_obj;
    private bool PluginStarted = false;

    // Comandos iniciais e sensores
    string[] initialCommands = {
        "AT Z",
        "AT E0",
        "AT D",
        "AT E0",
        "AT D0",
        "AT H0",
        "AT L0", 
        "AT SP 0",
        "AT M0",
        "AT S0",
        "AT AT 1",
        "AT AL",
        "AT ST 64"
        };
    string[] sensors = {
        "7E2:000B",
        "7E2:000C",
        "783:0003"
        };
    string endCarac = ">";

    // Sets iniciais
    void Start(){
        // Verifica se está rodando no editor ou no android
        #if UNITY_ANDROID && !UNITY_EDITOR
            plugin_obj = new AndroidJavaObject("com.example.bluetoothplugin.bluetoothplugin", initialCommands, sensors, endCarac);
            PluginStarted = true;
        #else
            Debug.Log("Rodando no editor");
            PluginStarted = false;
        #endif

        Invoke("start_bluetooth", 3f);
    }

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
        
        // Verificar se o plugin foi iniciado
        if (!PluginStarted){
            Debug.Log("Plugin not started!");
            return;
        }

        // Informações
        Debug.Log("Iniciando conexão Bluetooth...");        
        statusText.text = "Searching...";
        feedbackText.text = "";

        // Ativar popup
        popup.SetActive(true);

        // Chamar plugin
        plugin_obj.Call("pluginRefrashMacs");
        
    }

    // Callback chamado pelo plugin após procurar os dispositivos
    public void P2W_RefrashMacs(string message){
        
        if (message.StartsWith("ERROR|")){
            Debug.Log(message);
            feedbackText.color = Color.red;
            feedbackText.text = message;
            return;
        }

        if (message.StartsWith("NO_PAIRED_DEVICES")){
            Debug.Log("No paireded devices found.");
            feedbackText.text = message;
            return;
        }

        if (!message.StartsWith("DEVICELIST|")){
            Debug.Log("Invalid message format.");
            feedbackText.text = "Invalid message format.";
            return;
        }

        // Construir novos macs
        foreach(string device in message.Replace("DEVICELIST|", "").Split(';')){
            if (device2connect.Split('-')[0] == "OBDII"){
                device2connect = device;
                break;
            }
        }

        // Chamar a funcao de conectar, depois de um tempo
        feedbackText.text = "A device named OBDII was found.";
        Invoke(W2P_ConnectDevice, 2f);
    }

    // Chamar conexão com o dispositivo encontrado
    private void W2P_ConnectDevice(){
        statusText.text = "Connecting...";
        plugin_obj.Call("pluginConnectDevice", device2connect);
    }

    // Conectar a um dispositivo (P -> W)
    public void P2W_ConnectDevice(string message){

        Debug.Log("Resposta conexão: " + message);

        // Atualiza a UI
        if(message.StartsWith("SUCCESS")){
            feedbackText.text = "Connected to OBDII!";
            Invoke(nameof(W2P_StartConfigELM), 2f);

        }else if(message.StartsWith("ERROR")){
            feedbackText.color = Color.red;
            feedbackText.text = "Failed to connect to " + selectedMac + "\n" + message;
        }

    }

    // Começa a configurar o ELM (W -> P)
    public void W2P_StartConfigELM(){
        Debug.Log("Iniciando configurações iniciais do ELM");
        popupText.color = Color.white;
        plugin_obj.Call("pluginStartConfigELM");
    }

    // Começa a configurar o ELM (P -> W)
    public void P2W_StartConfigELM(string message){

        if(message.StartsWith("SUCCESS|CONFIG_DONE")){
            feedbackText.text = "Successfully Configured!";
            Invoke(nameof(W2P_StartContinuousRead), 2f);

        }else if(message.StartsWith("SUCCESS|CMD|")){
            feedbackText.text = "Configuring ELM...\n\n" + message.Substring("SUCCESS|CMD|".Length);

        }else if(message.StartsWith("ERROR")){
            feedbackText.color = Color.red;
            feedbackText.text = message;
            popupButton.interactable = true;

        }else{
            feedbackText.text = message;
        }
    }


}