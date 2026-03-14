package com.example.bluetoothplugin;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.util.Log;
import java.util.UUID;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import java.util.List;
import java.util.Map;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;

import com.unity3d.player.UnityPlayer;

import java.util.Set;

public class bluetoothplugin {

    // TAG's e SSUI de bluetooth SSP
    private static final String TAG = "UnityBluetooth";
    private static final String UNITY_GAME_OBJECT = "BluetoothManager";
    private static final UUID SPP_UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");

    // Objetos bluetooth, 2 dispositivos e 1 ponte.
    private BluetoothAdapter bluetoothAdapter;
    private BluetoothSocket bluetoothSocket;
    private BluetoothDevice connectedDevice;

    // Variáveis internas do Java
    private List<String> initCommands;
    private volatile String currentIniCMD = null;
    private String endCarac = ">";
    private Map<String, List<String>> sensors;
    private Map<String, String> commandIdentifier;
    private BlockingQueue<Map.Entry<String, String>> queue = new LinkedBlockingQueue<>();

    // Construtor
    public bluetoothplugin(String[] initCommandsIN, String[] sensorsIN, String endCaracIN) {
        try {

            // ===== Inicializa estruturas internas =====
            initCommands = new ArrayList<>(); 
            sensors = new HashMap<>();
            commandIdentifier = new HashMap<>();

            // ===== Comandos iniciais vindos do C# =====
            if (initCommandsIN != null) {
                for (String cmd : initCommandsIN) {

                    if (cmd == null) continue;

                    String cleanCmd = cmd
                            .trim()
                            .toUpperCase()
                            .replace(" ", "");

                    if (!cleanCmd.isEmpty()) {
                        initCommands.add(cleanCmd);
                    }
                }
            }

            // ===== Mapeamento de comandos para identificação (ECU + PID) =====
            if (sensorsIN != null) {

                int index = 1;

                for (String cmd : sensorsIN) {

                    if (cmd == null) continue;

                    String clean = cmd.trim().toUpperCase().replace(" ", "");

                    // Esperado: ECU:PID
                    String[] parts = clean.split(":");
                    if (parts.length != 2) continue;

                    String ecu  = parts[0];
                    String did = parts[1];

                    if (ecu.isEmpty() || did.isEmpty()) continue;

                    // Cria lista se não existir
                    if (!sensors.containsKey(ecu)) {
                        sensors.put(ecu, new ArrayList<>());
                    }

                    sensors.get(ecu).add(did);

                    // Salva índice para retorno rápido
                    commandIdentifier.put(ecu+did, String.valueOf(index++));

                }
            }

            // ===== Caractere/token de fim de resposta =====
            if (endCaracIN != null && !endCaracIN.isEmpty()) {
                endCarac = endCaracIN;
            }

            // ===== Inicializa Bluetooth =====
            bluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
            Log.d(TAG, "Plugin Bluetooth iniciado");

            // ===== Logs para conferência =====
            Log.d(TAG, "Comandos iniciais: " + initCommands.toString());
            Log.d(TAG, "Sensores: " + sensors.toString());

        } catch (Exception e) {
            Log.e(TAG, "Erro ao inicializar plugin: " + e.getMessage());
        }
    }


    // Método de enviar infosmações para o wrapper
    private void send(String msg, String wrapper_method) {
        UnityPlayer.UnitySendMessage(
            UNITY_GAME_OBJECT,
            wrapper_method,
            msg);
    }


    // Procurar dispositivos
    public void pluginRefrashMacs() {

        Log.d(TAG, "pluginRefrashMacs()");

        // Tentar procura
        try {

            // Não conseguiu inicializar o bluetooth
            if (bluetoothAdapter == null) {
                send("ERROR|NO_BLUETOOTH", "P2W_RefrashMacs");
                return;
            }

            // Bluetooth desligado
            if (!bluetoothAdapter.isEnabled()) {
                send("ERROR|BT_DISABLED", "P2W_RefrashMacs");
                return;
            }

            // Captura todos os dispositivos já pareados
            Set<BluetoothDevice> dispositivos = bluetoothAdapter.getBondedDevices();

            // Se não pareou com nenhum dispositivo até agora
            if (dispositivos == null || dispositivos.isEmpty()) {
                send("NO_PAIRED_DEVICES", "P2W_RefrashMacs");
                return;
            }

            // Construir a lista com todos os dispositivos encontrados
            StringBuilder builder = new StringBuilder();
            builder.append("DEVICELIST|");

            for (BluetoothDevice device : dispositivos) {

                String nome = device.getName();
                String endereco = device.getAddress();

                builder.append(nome)
                    .append("-")
                    .append(endereco)
                    .append(";");
            }

            send(builder.toString(), "P2W_RefrashMacs");

        // Erro de permissão
        } catch (SecurityException se) {
            send("ERROR|PERMISSION", "P2W_RefrashMacs");

        // Outros erros
        } catch (Exception e) {
            send("ERROR|GENERIC|" + e.getMessage(), "P2W_RefrashMacs");
        }
    }


    // Conectar a um dispositivo
    public void pluginConnectDevice(String macAddress) {

        Log.d(TAG, "pluginConnectDevice(): " + macAddress);

        new Thread(() -> {

            // Tentativa
            try {
                
                // Bluetooth não foi inicializado
                if (bluetoothAdapter == null) {
                    send("ERROR|NO_BLUETOOTH", "P2W_ConnectDevice");
                    return;
                }

                // Bluetooth desligado
                if (!bluetoothAdapter.isEnabled()) {
                    send("ERROR|BT_DISABLED", "P2W_ConnectDevice");
                    return;
                }

                // Cria objeto representando dispositivo e guarda referencia global
                BluetoothDevice device = bluetoothAdapter.getRemoteDevice(macAddress);
                connectedDevice = device;

                // Cria o socket (ponte) de conexão (Adaptador <-----Socket-----> Device)
                bluetoothSocket = device.createRfcommSocketToServiceRecord(SPP_UUID);

                // Cancela a busca por se tiver fazendo scan e faz a conexão dos dois dispositivos
                bluetoothAdapter.cancelDiscovery(); // Importante
                bluetoothSocket.connect();

                // Teve sucesso na conexão
                send("SUCCESS|" + device.getName(), "P2W_ConnectDevice");

            // Erro de permissão
            } catch (SecurityException se) {
                send("ERROR|PERMISSION", "P2W_ConnectDevice");

            // Erros comuns, fora de alcance, UUID errado, timeout, já conectado, etc.
            } catch (IOException io) {
                send("ERROR|CONNECTION_FAILED|" + io.getMessage(), "P2W_ConnectDevice");

                // Fechar socket, IOException só acontece depois do socket ser criado, enquanto permissão, antes
                try {
                    if (bluetoothSocket != null)
                        bluetoothSocket.close();
                } catch (IOException ignored) {}

            // Outros erros genéricos
            } catch (Exception e) {

                send("ERROR|GENERIC|" + e.getMessage(), "P2W_ConnectDevice");
            }

        }).start();
    }


    // Iniciar o estado de configuração
    public void pluginStartConfigELM() {
        
        new Thread(() -> {
            try {

                // Se não está conectado
                if(bluetoothSocket == null || !bluetoothSocket.isConnected()) {
                    send("ERROR|NOT_CONNECTED", "P2W_StartConfigELM");
                    return;
                }

                // Objetos saida/entrada
                OutputStream output = bluetoothSocket.getOutputStream();
                InputStream input = bluetoothSocket.getInputStream();

                // Variáveis locais            
                byte[] buffer = new byte[1024];
                StringBuilder response = new StringBuilder();
                int bytes;
                String incoming;
                boolean commandDone = false;

                // Varredura de comandos iniciais
                for(String cmd : initCommands){

                    // Se estiver vazio, passar para próximo comando
                    if(cmd.isEmpty()) continue;

                    // Guardar comando incial
                    currentIniCMD = cmd;
                    commandDone = false;

                    // Enviar comando
                    while(!commandDone){

                        // Limpar resposta
                        response.setLength(0);

                        // Mandar comando bytes no socket
                        output.write((currentIniCMD + "\r").getBytes());
                        output.flush();

                        // Ler resposta
                        while(true){

                            bytes = input.read(buffer); // BLOQUEANTE

                            // Se tem algo no buffer
                            if(bytes>0){

                                // Cria uma cópia no formato string do começo do buffer até bytes
                                incoming = new String(buffer, 0, bytes);
                                response.append(incoming);

                                // Se achou o caractere de fim de resposta, para de ler e processa a resposta
                                if (incoming.contains(">")) break;
                            }
                        
                        }

                        // Limpa resposta
                        String resp = response.toString()
                        .replace("\r", "")
                        .replace("\n", "")
                        .replace(">", "");

                        // Verifica se a resposta tem echo, se tem, volta pro começo e envia ATE0
                        if(resp.contains(currentIniCMD) && !"ATZ".equals(currentIniCMD)){
                            currentIniCMD = "ATE0";
                            resp = resp + " (echo)";
                        }else{
                            commandDone = true;
                        }

                        // Envia para a unity
                        send("SUCCESS|CMD|" + currentIniCMD + ": " + resp, "P2W_StartConfigELM");

                        // Delay manual
                        if (cmd.equals("ATZ")) {
                            Thread.sleep(2000);
                        } else {
                            Thread.sleep(250);
                        }
                    }
                }

                // Inicia o parser (valida as respostas do ELM)
                parser();
                
                // Terminou a configuração inicial
                send("SUCCESS|CONFIG_DONE", "P2W_StartConfigELM");

            } catch (IOException e) {
            send("ERROR|IO|" + e.getMessage(), "P2W_StartConfigELM");

            } catch (InterruptedException e) {
                Thread.currentThread().interrupt();
                send("ERROR|INTERRUPTED", "P2W_StartConfigELM");

            } catch (Exception e) {
                send("ERROR|UNKNOWN|" + e.getMessage(), "P2W_StartConfigELM");
            }

        }).start();
    }


    // Leitura contínua dos dados
    public void pluginStartContinuousRead(){

        new Thread(() -> {
            try {

                // Se não está conectado
                if (bluetoothSocket == null || !bluetoothSocket.isConnected()) {
                    send("ERROR|NOT_CONNECTED", "P2W_StartContinuousRead");
                    return;
                }

                // Objetos saida/entrada
                OutputStream output = bluetoothSocket.getOutputStream();
                InputStream input = bluetoothSocket.getInputStream();

                // Variáveis locais
                byte[] buffer = new byte[1024];
                StringBuilder response = new StringBuilder();
                boolean goToDID = false;
                int bytes;
                String ECU;
                List<String> DIDS;

                // Loop contínuo (polling)
                while (bluetoothSocket.isConnected()) {

                    // === LOOP EXTERNO: ECUs ===
                    for (Map.Entry<String, List<String>> entry : sensors.entrySet()) {

                        ECU = entry.getKey();
                        DIDS = entry.getValue();

                        // ===== Envia ATSH + ECU =====
                        output.write(("ATSH" + ECU + "\r").getBytes());
                        output.flush();

                        // Espera resposta do ATSH
                        goToDID = false;
                        while (!goToDID) {
                            bytes = input.read(buffer);
                            if (bytes > 0) {
                                for (int i = 0; i < bytes; i++) {
                                    if (buffer[i] == '>') {
                                        goToDID = true;
                                        break;
                                    }
                                }
                            }
                        }

                        // === LOOP INTERNO: DIDs ===
                        for (String DID : DIDS) {

                            response.setLength(0);

                            // ===== Envia 22 + DID =====
                            output.write(("22" + DID + "\r").getBytes());
                            output.flush();

                            // Espera resposta do DID
                            while (true) {
                                bytes = input.read(buffer);
                                if (bytes > 0) {
                                    response.append(new String(buffer, 0, bytes));
                                    if (response.indexOf(endCarac) >= 0) {
                                        break;
                                    }
                                }
                            }

                            // Passa resposta para a fila, para ser processada e enviada para a unity
                            queue.offer(Map.entry(ECU+DID, response.toString()));

                        }
                    }
                }

            } catch (IOException e) {
                send("ERROR|IO|" + e.getMessage(), "P2W_StartContinuousRead");

            } catch (Exception e) {
                send("ERROR|UNKNOWN|" + e.getMessage(), "P2W_StartContinuousRead");
            }

        }).start();
    }


    // Entende as respostas
    public void parser() {

        new Thread(() -> {
            try {

                while (bluetoothSocket.isConnected()) {

                    // Fila bloqueante, fica na espera de novas informações
                    Map.Entry<String, String> entry = queue.take();

                    String sensor = entry.getKey();
                    String payload = entry.getValue();

                    if (payload == null) continue;

                    payload = filtreResponse(payload);
                    if (payload == null) continue;


                    payload = mapResponse(sensor.substring(0, 3), payload);
                    if (payload == null) continue;

                    send(commandIdentifier.get(sensor) + payload, "P2W_StartContinuousRead");
                }

            } catch (Exception e) {
                e.printStackTrace();
            }
        }).start();
    }


    // Desconectar do disposito e limpar variáveis
    public void pluginDisconnect() {

        Log.d(TAG, "pluginDisconnect()");

        new Thread(() -> {

            try {

                // Se não estiver conectado
                if (bluetoothSocket == null) {
                    send("ERROR|NOT_CONNECTED", "P2W_Disconnect");
                    return;
                }

                // Fechar socket (isso desbloqueia o read())
                if (bluetoothSocket.isConnected()) {
                    bluetoothSocket.close();
                }

                // Limpar referências
                bluetoothSocket = null;
                connectedDevice = null;

                send("SUCCESS", "P2W_Disconnect");

            } catch (IOException io) {

                send("ERROR|DISCONNECT_FAILED|" + io.getMessage(), "P2W_Disconnect");

            } catch (Exception e) {

                send("ERROR|GENERIC|" + e.getMessage(), "P2W_Disconnect");
            }

        }).start();
    }


    // Filtrar respostas ELM327
    private String filtreResponse(String resp) {

        if (resp == null) return null;

        try{

            // Remove caracteres indesejados
            resp = resp.replace("\r", "")
                    .replace("\n", "")
                    .replace(">", "");

            // 62 = sucesso e tiver algo depois
            if (resp.startsWith("62") && resp.length() > 2) {
                return resp.substring(6);
            }

        // Retornar nulo se não conseguiu filtrar a resposta
        }catch(Exception e){return null;}

        return null;
    }


    // Mapear valores lidos em unidades reais
    private String mapResponse(String ecu, String payload) {

        if (ecu == null || payload == null) return null;

        try {

            // Carga da 7E2, resposta de 1 byte, valor entre 0-100%, apenas passa para inteiro
            if ("7E2".equals(ecu)) {

                if (payload.length() != 2) return null;
                return String.valueOf(Integer.parseInt(payload, 16));
            }

            // Carga da 783, resposta de 2 bytes, precisa se shiftar
            else if ("783".equals(ecu)) {

                if (payload.length() != 4) return null;

                int valor = Integer.parseInt(payload, 16);

                if (valor > 0x7FFF)
                    valor -= 0x10000;

                return String.valueOf(valor / 10.0);
            }

        } catch (Exception e) {
            return null;
        }

        return null;
    }
}