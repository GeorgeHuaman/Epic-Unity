using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class WorldCardUI : MonoBehaviour
{
    public TMP_Text worldName;

    public TMP_Text description;

    public Image thumbnail;

    private WorldData world;

    public void Setup(WorldData data)
    {
        world = data;
        worldName.text = data.name;

        description.text = data.description;

        StartCoroutine(
            LoadThumbnail(data.thumbnail));
        Debug.Log(data.thumbnail);
    }
    public void PlayWorld()
    {
        WorldManager.Instance
            .LoadWorld(world.addressable_key);
    }

    private IEnumerator LoadThumbnail(string url)
    {
        using (var request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Thumbnail error: " + request.error);
                Debug.LogError("URL: " + url);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            thumbnail.sprite = Sprite.Create(
    texture,
    new Rect(0, 0, texture.width, texture.height),
    new Vector2(0.5f, 0.5f)
);

            thumbnail.preserveAspect = true;
            thumbnail.enabled = true;
        }
    }
}
