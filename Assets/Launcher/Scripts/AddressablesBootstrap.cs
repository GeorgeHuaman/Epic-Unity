using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;

public class AddressablesBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return Addressables.InitializeAsync();
        Debug.Log("W");
        string catalogUrl =
            "https://pub-070e9e9f8f714c068c18e3f6e61a821a.r2.dev/Addressables/StandaloneWindows64/catalog.json";

        var handle = Addressables.LoadContentCatalogAsync(catalogUrl, true);
        yield return handle;

        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            Debug.Log("REMOTE CATALOG ACTIVE");
        }
        else
        {
            Debug.LogError("FAILED TO LOAD REMOTE CATALOG");
        }
    }
}