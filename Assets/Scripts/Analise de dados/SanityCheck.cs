using UnityEngine;

public class SanityCheck : MonoBehaviour
{
    [Header("Configurações de Teste")]
    public string targetUserEmail = "lfss6";
    public enum SessionCondition { MR, VR }
    public SessionCondition targetCondition = SessionCondition.MR;

    // ---> ADICIONE ESTA LINHA: A referência para o motor
    public TelemetryPlaybackEngine engine; 

    void Start()
    {
        if (DatabaseManager.Instance.LoadDatabase())
        {
            RunParametricTest();
        }
    }

    private void RunParametricTest()
    {
        Debug.Log($"<color=cyan>--- INICIANDO TESTE: {targetUserEmail} | {targetCondition} ---</color>");

        ParticipantRecord target = DatabaseManager.Instance.CurrentDatabase.Participants.Find(
            p => p.Email.ToLower().Contains(targetUserEmail.ToLower())
        );
        
        if (target != null)
        {
            string conditionKey = targetCondition.ToString(); 

            if (target.Sessions.ContainsKey(conditionKey))
            {
                string rawCsv = target.Sessions[conditionKey].CarTelemetryCSV;
                
                if (!string.IsNullOrEmpty(rawCsv))
                {
                    Debug.Log($"<color=green>[Sucesso]</color> Dados encontrados. Enviando para o Motor 3D!");
                    
                    // ---> ADICIONE ESTAS DUAS LINHAS: Passando os dados para o motor e dando Play!
                    engine.LoadFromCSV(rawCsv);
                    engine.Play();
                }
                else
                {
                    Debug.LogWarning($"<color=yellow>[Aviso]</color> A sessão {conditionKey} existe, mas o CSV está vazio.");
                }
            }
        }
    }
}