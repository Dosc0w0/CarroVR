using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class GetFromServer : MonoBehaviour
{
    [SerializeField] string url = "http://localhost:5000/controles"; // Coloque sua URL aqui
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] public WheelRotator wheel;
    [SerializeField] public PedalMover pedalAcc;
    [SerializeField] public PedalMover pedalFreio;

    void Start()
    {
        InvokeRepeating(nameof(CallServer), 0f, 0.6f); // Atualiza a cada 0.1s
    }

    void CallServer()
    {
        StartCoroutine(GetServerData());
    }

    IEnumerator GetServerData()
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Erro na requisição: " + request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            //Debug.Log("JSON recebido: " + json);

            // Parse do JSON
            VehicleData dados = JsonUtility.FromJson<VehicleData>(json);
            string strData = $"Acelerador: {dados.Acc}, Freio: {dados.Freio}, Volante: {dados.Volante}";
            //Debug.Log(strData);
            //Debug.Log(pedalFreio);

            if (wheel)
                wheel.raw = dados.Volante;

            if (pedalAcc)
                pedalAcc.raw = dados.Acc;

            if (pedalFreio)
                pedalFreio.raw = dados.Freio;
            /*
            if(pedalAcc)
                pedalAcc.targetZ = (dados.Acc/10.0f);

            if (pedalFreio)
                pedalFreio.targetZ = (dados.Freio/10.0f); */
        }
    }
}
