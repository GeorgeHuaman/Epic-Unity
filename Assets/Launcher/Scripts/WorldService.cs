using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
public class WorldService : MonoBehaviour
{
    public IEnumerator GetWorlds(Action<WorldData[]> callback)
    {
        string url =
            SupaBaseManager.Instance.supabaseUrl +
            "/rest/v1/Worlds?select=*"; 
        UnityWebRequest request =
            UnityWebRequest.Get(url);

        request.SetRequestHeader(
            "apikey",SupaBaseManager.Instance.anonKey);

        request.SetRequestHeader(
            "Authorization",
            "Bearer " +SupaBaseManager.Instance.anonKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        string json =
            request.downloadHandler.text;

        string wrapped =
            "{\"items\":" + json + "}";

        WorldWrapper worlds =
            JsonUtility.FromJson<WorldWrapper>(wrapped);
        callback?.Invoke(worlds.items);
    }
}
