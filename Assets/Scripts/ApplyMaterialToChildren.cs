using System.Collections;
using UnityEngine;

public class ApplyMaterialToChildren : MonoBehaviour
{
    [Header("Configurações")]
    public Material materialToApply;

    public GameObject parentObject;

    public bool hasTimer;

    public float timerDuration  = 5.0f;

    void Start()
    {

        // Verificação se o material e objeto foi selecionado
        if (materialToApply == null || parentObject == null)
        {
            Debug.LogError("[ApplyMaterialToChilder] Referência de Material ou Objeto não colocadas.");
            return;
        }
        StartCoroutine(applyMaterial());
    }

    IEnumerator applyMaterial()
    {
        if(hasTimer == true)
        {
            Debug.Log($"[ApplyMaterialToChilder] Timer ativado para '{parentObject.name}', esperando {timerDuration} segundos.");
            yield return new WaitForSeconds(timerDuration);
        }

        // Pega todos os Renderers dentro do parentObject
        Renderer[] allRenderers = parentObject.GetComponentsInChildren<Renderer>(true); // O 'true' faz ele procurar até mesmo em objetos que estejam desativados
        Debug.Log($"[ApplyMaterialToChilder] Aplicando em {allRenderers.Length} Renderers, filhos de '{parentObject.name}'.");

        // Irá passar por cada filho para aplicar o material
        foreach (Renderer renderer in allRenderers)
        {
            // Cria um novo array de materiais para caso tenha um objeto com múltiplos materiais (segurança apenas)
            Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = materialToApply;
            }
            
            // Aplica o material desejado no objeto filho
            renderer.sharedMaterials = newMaterials;
        }
    }
}