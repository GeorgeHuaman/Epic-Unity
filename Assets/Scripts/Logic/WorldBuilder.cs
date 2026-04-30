using UnityEngine;
using System.Collections.Generic;

public class WorldBuilder : MonoBehaviour
{
    public SkyboxManager skyboxManager; 
    public List<GameObject> prefabLibrary; 

    public void ConstruirMundo(WorldConfig config)
    {
        if (skyboxManager != null)
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
                Vector3 posicion = new Vector3(item.pos_x, item.pos_y, item.pos_z);
                Quaternion rotacion = Quaternion.Euler(0, item.rot_y, 0);
                GameObject obj = Instantiate(prefab, posicion, rotacion, this.transform);

                if (obj.TryGetComponent<IConfigurable>(out var configurable))
                {
                    configurable.Setup(item.data);
                }
            }
        }
    }
}

public interface IConfigurable
{
    void Setup(Dictionary<string, string> data);
}