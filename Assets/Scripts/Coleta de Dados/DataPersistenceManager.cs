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
        
        string basePath = Application.persistentDataPath;
        string pendentesPath = Path.Combine(basePath, "Sessoes_Pendentes");
        string sessionFolderPath = Path.Combine(pendentesPath, sessionID);

        if (!Directory.Exists(pendentesPath))
        {
            Directory.CreateDirectory(pendentesPath);
        }

        if (!Directory.Exists(sessionFolderPath))
        {
            Directory.CreateDirectory(sessionFolderPath);
        }

        // --- MUDANÇA AQUI: Adicionamos a chamada para salvar as mãos ---
        SaveSessionJSON(session, sessionFolderPath, sessionID);
        SaveCarTelemetryCSV(sessionFolderPath, sessionID);
        SaveGazeTelemetryCSV(sessionFolderPath, sessionID);
        SaveHandTelemetryCSV(sessionFolderPath, sessionID); 
        
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
        
        sb.AppendLine("TimeSinceStart,PosX,PosY,PosZ,RotX,RotY,RotZ,CurrentSpeed,SteeringAngle,AccPedal,BrakePedal");

        foreach (var frame in history)
        {
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", 
                frame.TimeSinceStart, frame.PositionX, frame.PositionY, frame.PositionZ, 
                frame.RotationX, frame.RotationY, frame.RotationZ, frame.CurrentSpeed, 
                frame.SteeringWheelAngle, frame.AccPedal, frame.BrakePedal));
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

    // ==========================================
    // NOVA FUNÇÃO: SALVAR TELEMETRIA DAS MÃOS
    // ==========================================
    private void SaveHandTelemetryCSV(string folderPath, string sessionID)
    {
        List<HandTelemetryFrame> history = telemetryRecorder.GetHandHistory();
        if (history.Count == 0) return;

        StringBuilder sb = new StringBuilder();
        // Cabeçalho com as 15 colunas
        sb.AppendLine("TimeSinceStart,IsLeftTracked,LeftPosX,LeftPosY,LeftPosZ,LeftRotX,LeftRotY,LeftRotZ,IsRightTracked,RightPosX,RightPosY,RightPosZ,RightRotX,RightRotY,RightRotZ");

        foreach (var frame in history)
        {
            // Convertendo booleanos para 1 ou 0 para facilitar a vida no Python
            int isLeftTrackedInt = frame.IsLeftTracked ? 1 : 0;
            int isRightTrackedInt = frame.IsRightTracked ? 1 : 0;

            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", 
                frame.TimeSinceStart, 
                isLeftTrackedInt, frame.LeftPosX, frame.LeftPosY, frame.LeftPosZ, frame.LeftRotX, frame.LeftRotY, frame.LeftRotZ,
                isRightTrackedInt, frame.RightPosX, frame.RightPosY, frame.RightPosZ, frame.RightRotX, frame.RightRotY, frame.RightRotZ));
        }

        string filePath = Path.Combine(folderPath, $"{sessionID}_HandTelemetry.csv");
        File.WriteAllText(filePath, sb.ToString());
    }
}