using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GetFromBluetooth : MonoBehaviour
{
    [Header("Canvas")]
    public GameObject popup;

    [Header("Textos")]
    public TMP_Text statusText;
    public TMP_Text feedbackText;

    [Header("Controles")]
    [SerializeField] public WheelRotator wheel;
    [SerializeField] public PedalMover pedalAcc;
    [SerializeField] public PedalMover pedalFreio;

    // Variables
    string device2connect = null;
    bool running = false;

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

    }

    // Botão do controle esquerdo inicia a conexão Bluetooth
    void Update(){
        if (OVRInput.GetDown(OVRInput.Button.Four) && !running){
            running = true;
            start_bluetooth();
        }

        // Teste pelo teclado
        //if (Input.GetKeyDown(KeyCode.B)){
        //    start_bluetooth();
        //}
    }

    // Método para iniciar a conexão Bluetooth
    private void start_bluetooth(){
        
        // Ativar popup
        popup.SetActive(true);

        // Verificar se o plugin foi iniciado
        if (!PluginStarted){
            Debug.Log("Plugin not started!");
            feedbackText.text = "Plugin not started!";
            return;
        }

        // Informações
        Debug.Log("Iniciando conexão Bluetooth...");        
        statusText.text = "Searching...";
        feedbackText.text = "";

        // Chamar plugin
        plugin_obj.Call("pluginRefrashMacs");
        
    }

    // Callback chamado pelo plugin após procurar os dispositivos
    public void P2W_RefrashMacs(string message){
        
        if (message.StartsWith("ERROR|")){
            Debug.Log(message);
            feedbackText.color = Color.red;
            feedbackText.text = message;
            running = false;
            return;
        }

        if (message.StartsWith("NO_PAIRED_DEVICES")){
            Debug.Log("No paireded devices found.");
            running = false;
            feedbackText.text = message;
            return;
        }

        if (!message.StartsWith("DEVICELIST|")){
            Debug.Log("Invalid message format.");
            feedbackText.text = "Invalid message format.";
            running = false;
            return;
        }

        // Construir novos macs
        foreach(string device in message.Replace("DEVICELIST|", "").Split(';')){
            if (string.IsNullOrWhiteSpace(device)) continue;

            string[] parts = device.Split('-');
            if (parts.Length < 2) continue;

            string nome = parts[0];

            if (nome == "OBDII"){
                device2connect = device;
                break;
            }
        }

        // Se encontrou varios dispositivos mas nenhum é OBDII
        if (device2connect == null){
            feedbackText.color = Color.red;
            feedbackText.text = "OBDII device NOT found.";
            running = false;
            return;
        }

        // Chamar a funcao de conectar, depois de um tempo
        feedbackText.text = "OBDII found!\n" + device2connect;
        Invoke(nameof(W2P_ConnectDevice), 1.5f);
    }

    // Chamar conexão com o dispositivo encontrado
    private void W2P_ConnectDevice(){
        statusText.text = "Connecting...";
        plugin_obj.Call("pluginConnectDevice", device2connect.Split('-')[1]);
    }

    // Conectar a um dispositivo (P -> W)
    public void P2W_ConnectDevice(string message){

        Debug.Log("Resposta conexão: " + message);

        // Atualiza a UI
        if(message.StartsWith("SUCCESS")){
            feedbackText.text = "Connected to OBDII!";
            Invoke(nameof(W2P_StartConfigELM), 1.5f);

        }else if(message.StartsWith("ERROR")){
            feedbackText.color = Color.red;
            feedbackText.text = "Failed to connect to connect to OBDII.";
            running = false;
        }

    }

    // Começa a configurar o ELM (W -> P)
    public void W2P_StartConfigELM(){
        Debug.Log("Iniciando configurações iniciais do ELM");
        plugin_obj.Call("pluginStartConfigELM");
    }

    // Começa a configurar o ELM (P -> W)
    public void P2W_StartConfigELM(string message){

        if(message.StartsWith("SUCCESS|CONFIG_DONE")){
            statusText.text = "Read Started!";
            feedbackText.text = "Successfully Configured!";
            Invoke(nameof(W2P_StartContinuousRead), 1.5f);

        }else if(message.StartsWith("SUCCESS|CMD|")){
            feedbackText.text = "Config Commands \n\n" + message.Substring("SUCCESS|CMD|".Length);

        }else if(message.StartsWith("ERROR")){
            feedbackText.color = Color.red;
            feedbackText.text = message;
            running = false;

        }else{
            feedbackText.text = message;
        }
    }

    // Começar leitura contínua do ELM (W -> P)
    public void W2P_StartContinuousRead(){
        popup.SetActive(false);
        plugin_obj.Call("pluginStartContinuousRead");
    }

    // Começar leitura contínua do ELM (P -> W)
    public void P2W_StartContinuousRead(string message){

        switch (message[0]){
            case '1':
                pedalAcc.raw = float.Parse(message.Substring(1));
                break;

            case '2':
                pedalFreio.raw = float.Parse(message.Substring(1));
                break;

            case '3':
                wheel.raw = float.Parse(message.Substring(1));
                break;

            default:
                feedbackText.text = "ERROR|" + message;
                running = false;
                popup.SetActive(true);
                StartCoroutine(HidePopupAfterDelay(4f));
                break;

        }

    }
    IEnumerator HidePopupAfterDelay(float delay){
        yield return new WaitForSeconds(delay);
        popup.SetActive(false);
    }
}