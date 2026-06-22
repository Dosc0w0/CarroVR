using UnityEngine;

public class RoadSpawner2 : MonoBehaviour
{
    [Header("Pistas (Prefabs das Fases)")]
    [Tooltip("Element 0: Tutorial | Element 1: Oficial")]
    public GameObject[] trackPrefabs; 
    private int currentTrackIndex = 0;

    [Header("Configurações de Spawn")]
    [Tooltip("A tag que o objeto vazio dentro do Prefab da pista deve ter")]
    public string spawnPointTag = "SpawnPoint";

    private GameObject currentTrackInstance;
    private bool isInitialized = false;

    void Update()
    {
        // Funciona como um "Radar": Não faz absolutamente nada visual até achar o carro
        if (!isInitialized)
        {
            GameObject car = GetCar();
            if (car != null)
            {
                isInitialized = true;
                Debug.Log("<color=green>[RoadSpawner] Carro detetado! Preparando o ambiente invisível...</color>");
                GenerateTrack(0); // Inicia a pista de Tutorial
            }
        }
    }

    public void SwitchTrack(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= trackPrefabs.Length) return;
        
        currentTrackIndex = trackIndex;
        GenerateTrack(currentTrackIndex);
    }

    private void GenerateTrack(int index)
    {
        // 1. Destrói a pista antiga (se houver)
        if (currentTrackInstance != null)
        {
            Destroy(currentTrackInstance);
        }

        // 2. Instancia a nova pista na posição Y=0
        Vector3 spawnPos = new Vector3(this.transform.position.x, 0f, this.transform.position.z);
        currentTrackInstance = Instantiate(trackPrefabs[index], spawnPos, this.transform.rotation);

        // ==========================================
        // A MÁGICA DA VISIBILIDADE: Esconde a pista recém-criada
        // ==========================================
        currentTrackInstance.SetActive(false); 

        // 3. Posiciona o carro enquanto a pista está invisível
        TeleportCarToStart(currentTrackInstance);

        // 4. Tudo pronto! Revela a pista já com o carro no lugar certo
        currentTrackInstance.SetActive(true);
    }

    private void TeleportCarToStart(GameObject trackInstance)
    {
        Transform spawnPoint = null;
        Transform[] allChildren = trackInstance.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag(spawnPointTag))
            {
                spawnPoint = child;
                break;
            }
        }

        if (spawnPoint == null)
        {
            Debug.LogError($"[RoadSpawner] ERRO: Nenhuma tag '{spawnPointTag}' encontrada na pista!");
            return;
        }

        GameObject car = GetCar();
        if (car != null)
        {
            // Posiciona X e Z, preserva a altura Y do carro
            Vector3 newPos = new Vector3(
                spawnPoint.position.x, 
                car.transform.position.y, 
                spawnPoint.position.z
            );
            car.transform.position = newPos;
            
            // Assume a rotação
            car.transform.rotation = spawnPoint.rotation;

            // Zera a inércia física para não deslizar
            Rigidbody carRb = car.GetComponent<Rigidbody>();
            if (carRb != null)
            {
                carRb.velocity = Vector3.zero;
                carRb.angularVelocity = Vector3.zero;
            }
            
            Debug.Log($"<color=yellow>[RoadSpawner] Carro e Pista alinhados e revelados com sucesso!</color>");
        }
    }

    private GameObject GetCar()
    {
        GameObject car = GameObject.FindGameObjectWithTag("Player"); 
        if (car == null) car = GameObject.FindGameObjectWithTag("Car");
        return car;
    }
}