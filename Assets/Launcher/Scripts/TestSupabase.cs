using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class TestSupabase : MonoBehaviour
{
    public string apiKey;

    IEnumerator Start()
    {
        string url =
        "https://vmmsekjngyulosommkmz.supabase.co/rest/v1/Worlds?select=*";

        UnityWebRequest request =
            UnityWebRequest.Get(url);

        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        Debug.Log("Result = " + request.result);
        Debug.Log("Error = " + request.error);

        if (request.downloadHandler != null)
            Debug.Log(request.downloadHandler.text);
    }
}