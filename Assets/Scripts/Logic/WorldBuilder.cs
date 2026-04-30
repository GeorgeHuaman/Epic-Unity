using UnityEngine;
using System.Collections.Generic;
public class WorldBuilder : MonoBehaviour
{
    public SkyboxManager skyboxManager;
    public List<GameObject> prefabLibrary;

    public void ConstruirMundo(WorldConfig config)
    {
        skyboxManager.ChangeSkybox(config.sky_id);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in config.elementos)
        {
            GameObject prefab = prefabLibrary.Find(p => p.name == item.prefab_id);
            if (prefab != null)
            {
                Vector3 posicion = new Vector3(item.pos_x, 0, item.pos_z);
                GameObject obj = Instantiate(prefab, posicion, Quaternion.identity, this.transform);

                // Aqui se inyecta el texto del docente al objeto
                if (obj.TryGetComponent<IConfigurable>(out var configurable))
                {
                    configurable.Setup(item.data);
                }
            }
        }
    }
}

// Interfaz para que los prefabs sepan recibir datos
public interface IConfigurable
{
    void Setup(Dictionary<string, string> data);
}
