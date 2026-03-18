using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Globalization;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("Referências")]
    public SessionCollector sessionCollector;
    public TelemetryRecorder telemetryRecorder;

    private void OnEnable()
    {
        SimulationEvents.OnSessionEnded += SaveAllDataLocally;
    }

    private void OnDisable()
    {
        SimulationEvents.OnSessionEnded -= SaveAllDataLocally;
    }

    private void SaveAllDataLocally()
    {
        SessionData session = sessionCollector.GetCurrentSessionData();
        if (session == null || string.IsNullOrEmpty(session.SessionID)) return;

        string sessionID = session.SessionID;
        
        // --- MUDANÇA AQUI: Criando a lógica de Pendentes ---
        string basePath = Application.persistentDataPath;
        string pendentesPath = Path.Combine(basePath, "Sessoes_Pendentes");
        string sessionFolderPath = Path.Combine(pendentesPath, sessionID);

        // Cria a pasta "Sessoes_Pendentes" se for a primeira vez rodando o app
        if (!Directory.Exists(pendentesPath))
        {
            Directory.CreateDirectory(pendentesPath);
        }

        // Cria a subpasta da sessão atual
        if (!Directory.Exists(sessionFolderPath))
        {
            Directory.CreateDirectory(sessionFolderPath);
        }

        // 2. Salva os arquivos DENTRO da nova subpasta
        SaveSessionJSON(session, sessionFolderPath, sessionID);
        SaveCarTelemetryCSV(sessionFolderPath, sessionID);
        SaveGazeTelemetryCSV(sessionFolderPath, sessionID);
        
        Debug.Log($"<color=cyan><b>[Persistência] Dados salvos offline em: {sessionFolderPath}</b></color>");
    }

    private void SaveSessionJSON(SessionData session, string folderPath, string sessionID)
    {
        string json = JsonUtility.ToJson(session, true);
        string filePath = Path.Combine(folderPath, $"{sessionID}_Session.json");
        File.WriteAllText(filePath, json);
    }

    private void SaveCarTelemetryCSV(string folderPath, string sessionID)
    {
        List<CarTelemetryFrame> history = telemetryRecorder.GetCarHistory();
        if (history.Count == 0) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("TimeSinceStart,PosX,PosY,PosZ,RotX,RotY,RotZ,CurrentSpeed,SteeringAngle,PedalInput");

        foreach (var frame in history)
        {
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", frame.TimeSinceStart, frame.PositionX, frame.PositionY, frame.PositionZ, frame.RotationX, frame.RotationY, frame.RotationZ, frame.CurrentSpeed, frame.SteeringWheelAngle, frame.PedalInput));
        }
        string filePath = Path.Combine(folderPath, $"{sessionID}_CarTelemetry.csv");
        File.WriteAllText(filePath, sb.ToString());
    }

    private void SaveGazeTelemetryCSV(string folderPath, string sessionID)
    {
        List<GazeTelemetryFrame> history = telemetryRecorder.GetGazeHistory();
        if (history.Count == 0) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("TimeSinceStart,HeadPosX,HeadPosY,HeadPosZ,HeadRotX,HeadRotY,HeadRotZ");

        foreach (var frame in history)
        {
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6}", frame.TimeSinceStart, frame.HeadPositionX, frame.HeadPositionY, frame.HeadPositionZ, frame.HeadRotationX, frame.HeadRotationY, frame.HeadRotationZ));
        }

        string filePath = Path.Combine(folderPath, $"{sessionID}_GazeTelemetry.csv");
        File.WriteAllText(filePath, sb.ToString());
    }
}