using UnityEngine;
using System.IO;
using System.Text;

public class ExportadorBaterias : MonoBehaviour
{
    [Header("Configurações de Exportação")]
    public string nomeArquivo = "posicoes_baterias.csv";

    [ContextMenu("Exportar para CSV")]
    public void ExportarFilhos()
    {
        // Define o caminho: pasta do projeto + nome do arquivo
        string caminho = Path.Combine(Application.dataPath, nomeArquivo);
        
        StringBuilder csvContent = new StringBuilder();
        
        // Cabeçalho do CSV
        csvContent.AppendLine("ID;PosX;PosZ");

        int totalFilhos = transform.childCount;

        if (totalFilhos == 0)
        {
            Debug.LogWarning("O objeto pai não possui filhos para exportar!");
            return;
        }

        for (int i = 0; i < totalFilhos; i++)
        {
            Transform filho = transform.GetChild(i);
            string nomeBateria = $"Bateria {i + 1}";
            
            // Pega posição X e Z (usando localPosition ou position dependendo da sua necessidade)
            // Aqui usei position para coordenadas globais do mundo
            float posX = filho.position.x;
            float posZ = filho.position.z;

            // Adiciona a linha formatada (usando ponto e vírgula como separador para Excel PT-BR)
            csvContent.AppendLine($"{nomeBateria};{posX};{posZ}");
        }

        // Grava o arquivo no disco
        try
        {
            File.WriteAllText(caminho, csvContent.ToString());
            Debug.Log($"<color=green>Sucesso!</color> {totalFilhos} baterias exportadas em: {caminho}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao exportar CSV: {e.Message}");
        }
    }
}