using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq; // Essencial para os filtros!

public class DatabaseManager : MonoBehaviour
{
    // Padrão Singleton para podermos aceder a partir de qualquer script
    public static DatabaseManager Instance;

    public MasterDatabase CurrentDatabase { get; private set; }

    // Nome do ficheiro que vamos colocar no StreamingAssets
    private string fileName = "MasterDatabase.json"; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Carrega o JSON para a Memória
    public bool LoadDatabase()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"[DatabaseManager] Ficheiro não encontrado em: {filePath}");
            return false;
        }

        try
        {
            string jsonContent = File.ReadAllText(filePath);
            CurrentDatabase = JsonConvert.DeserializeObject<MasterDatabase>(jsonContent);
            
            Debug.Log($"<color=green>[DatabaseManager] Sucesso! Banco de Dados '{CurrentDatabase.ExperimentName}' carregado com {CurrentDatabase.Participants.Count} participantes.</color>");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DatabaseManager] Erro ao decodificar o JSON: {e.Message}");
            return false;
        }
    }

    // ==========================================
    // MÉTODOS DE FILTRO (Exemplos)
    // ==========================================
    
    // Devolve apenas os utilizadores que batem com os filtros específicos
    public List<ParticipantRecord> FilterParticipants(string gender = null, string hasLicense = null, string initialExp = null)
    {
        if (CurrentDatabase == null) return new List<ParticipantRecord>();

        // Usamos LINQ para encadear os filtros
        var query = CurrentDatabase.Participants.AsEnumerable();

        if (!string.IsNullOrEmpty(gender))
            query = query.Where(p => p.Gender.Equals(gender, System.StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(hasLicense))
            query = query.Where(p => p.HasDriverLicense.Equals(hasLicense, System.StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(initialExp))
            query = query.Where(p => p.InitialExperience.Equals(initialExp, System.StringComparison.OrdinalIgnoreCase));

        return query.ToList();
    }
}