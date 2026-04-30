using UnityEngine;
using System;
using System.Collections.Generic;

public class SkyboxManager : MonoBehaviour
{
    [Serializable]
    public struct SkyboxEntry
    {
        public string id;
        public Material material;
    }

    public List<SkyboxEntry> skyboxLibrary;

    public void ChangeSkybox(string skyId)
    {
        SkyboxEntry entry = skyboxLibrary.Find(s => s.id == skyId);
        
        if (entry.material != null)
        {
            RenderSettings.skybox = entry.material;
            DynamicGI.UpdateEnvironment();
            Debug.Log($"Skybox cambiado a: {skyId}");
        }
        else
        {
            Debug.LogWarning($"No se encontró un material de Skybox para el ID: {skyId}");
        }
    }
}
