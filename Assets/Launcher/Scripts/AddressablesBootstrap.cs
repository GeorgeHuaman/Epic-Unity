using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;

public class AddressablesBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        Debug.Log("Inicializando Addressables");

        var init = Addressables.InitializeAsync();
        var check = Addressables.CheckForCatalogUpdates();
        yield return check;

        if (check.Result != null && check.Result.Count > 0)
        {
            yield return Addressables.UpdateCatalogs(check.Result);
        }
        while (!init.IsDone)
        {
            Debug.Log("Progreso: " + init.PercentComplete);
            yield return null;
        }

        Debug.Log("Estado: " + init.Status);

        if (init.OperationException != null)
        {
            Debug.LogError(init.OperationException);
        }

        Debug.Log("Addressables Inicializado");
    }
}