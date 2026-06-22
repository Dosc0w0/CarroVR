using System;
using System.Collections.Generic;

// ==========================================
// A RAIZ DO BANCO DE DADOS
// ==========================================
[Serializable]
public class MasterDatabase
{
    public string ExperimentName;
    public string ExportDate;
    
    // Lista com todos os participantes qualificados
    public List<ParticipantRecord> Participants = new List<ParticipantRecord>();
}

// ==========================================
// OS DADOS DEMOGRÁFICOS E FILTROS
// ==========================================
[Serializable]
public class ParticipantRecord
{
    // Usaremos o E-mail (sem o @cin.ufpe.br, se preferir) como ID
    public string Email;
    
    // Filtros extraídos da Planilha
    public string Gender;            // Masculino, Feminino
    public string HasDriverLicense;  // Sim, Não
    public string InitialExperience; // MR, RV
    
    // O Dicionário guarda a sessão. A chave (string) será "MR" ou "RV".
    // Nota: A Unity nativa não serializa dicionários facilmente, 
    // mas o Newtonsoft.Json, que iremos usar, fá-lo perfeitamente!
    public Dictionary<string, SessionRecord> Sessions = new Dictionary<string, SessionRecord>();
}

// ==========================================
// OS DADOS BRUTOS DA SESSÃO
// ==========================================
[Serializable]
public class SessionRecord
{
    // Reutilizamos o seu SessionData atual! O JSON preencherá as métricas aqui diretamente.
    public SessionData Metrics; 
    
    // Os "blocões de texto" com o conteúdo dos ficheiros CSV
    public string CarTelemetryCSV;
    public string GazeTelemetryCSV;
    public string HandTelemetryCSV;
}