using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class CloudSyncManager : MonoBehaviour
{
    [Header("Configuração de Rede")]
    [Tooltip("Cole aqui a 'URL do app da Web' gerada no Google Apps Script")]
    public string googleWebAppUrl = "COLE_SUA_URL_AQUI";

    private bool isSyncing = false;

    private void Start()
    {
        // Assim que o app abre (ou o Quest acorda), ele tenta enviar o que ficou pendente de ontem
        TrySync();
    }

    private void OnEnable()
    {
        // Quando uma sessão nova acaba, ele espera 2 segundos (para dar tempo de salvar no disco) e tenta enviar
        SimulationEvents.OnSessionEnded += TrySyncDelay;
    }

    private void OnDisable()
    {
        SimulationEvents.OnSessionEnded -= TrySyncDelay;
    }

    private void TrySyncDelay()
    {
        StartCoroutine(WaitAndSync());
    }

    private IEnumerator WaitAndSync()
    {
        yield return new WaitForSeconds(2f);
        TrySync();
    }

    // Método que qualquer botão pode chamar para forçar um sync manual
    public void TrySync()
    {
        if (isSyncing) return;

        // Checagem de Internet do Android/Quest
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("[CloudSync] Sem conexão Wi-Fi. Os dados continuarão na fila de pendentes.");
            return;
        }

        StartCoroutine(SyncRoutine());
    }

    private IEnumerator SyncRoutine()
    {
        isSyncing = true;
        string basePath = Application.persistentDataPath;
        string pendentesPath = Path.Combine(basePath, "Sessoes_Pendentes");
        string sincronizadosPath = Path.Combine(basePath, "Sessoes_Sincronizadas");

        // Se não existir nada pendente, encerra silenciosamente
        if (!Directory.Exists(pendentesPath))
        {
            isSyncing = false;
            yield break;
        }

        // Garante que a gaveta final exista
        if (!Directory.Exists(sincronizadosPath)) Directory.CreateDirectory(sincronizadosPath);

        // Varre a pasta atrás de sessões
        string[] sessionFolders = Directory.GetDirectories(pendentesPath);

        foreach (string folder in sessionFolders)
        {
            string sessionId = new DirectoryInfo(folder).Name;
            string[] files = Directory.GetFiles(folder);
            bool allFilesSuccess = true;

            Debug.Log($"[CloudSync] Tentando subir sessão {sessionId}...");

            // Envia cada arquivo (JSON, Carro, Cabeça) daquela sessão
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string fileContent = File.ReadAllText(filePath);

                yield return StartCoroutine(UploadFile(sessionId, fileName, fileContent, (success) => {
                    if (!success) allFilesSuccess = false;
                }));

                // Se um dos arquivos da sessão falhar por queda de rede, a gente aborta essa pasta
                // para não mover um pedaço só e corromper o histórico.
                if (!allFilesSuccess) break; 
            }

            // Se todos os 3 arquivos bateram no Google com código 200 (OK)
            if (allFilesSuccess && files.Length > 0)
            {
                string targetPath = Path.Combine(sincronizadosPath, sessionId);
                Directory.Move(folder, targetPath);
                Debug.Log($"<color=green>[CloudSync] SUCESSO! Sessão {sessionId} movida para Sincronizados.</color>");
            }
            else
            {
                Debug.LogError($"[CloudSync] Falha na sessão {sessionId}. Ficará na fila para tentar depois.");
            }
        }

        isSyncing = false;
    }

    private IEnumerator UploadFile(string sessionId, string fileName, string content, Action<bool> callback)
    {
        // Monta o "pacote" exatamente como o Google Apps Script está esperando
        SyncPayload payload = new SyncPayload
        {
            sessionId = sessionId,
            fileName = fileName,
            fileContent = content
        };

        string jsonPayload = JsonUtility.ToJson(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(googleWebAppUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true);
            }
            else
            {
                Debug.LogError($"[CloudSync] Erro no arquivo {fileName}: {request.error}");
                callback(false);
            }
        }
    }

    // Classe puramente para formatar o JSON que viaja pela rede
    [Serializable]
    private class SyncPayload
    {
        public string sessionId;
        public string fileName;
        public string fileContent;
    }
}