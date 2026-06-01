using UnityEngine;

public class LauncherManager : MonoBehaviour
{
    public WorldService worldService;

    public Transform content;

    public GameObject worldCardPrefab;

    private void Start()
    {
        StartCoroutine(
            worldService.GetWorlds(OnWorldsLoaded));
    }

    private void OnWorldsLoaded(WorldData[] worlds)
    {
        foreach (var world in worlds)
        {
            GameObject card =
                Instantiate(
                    worldCardPrefab,
                    content);

            card.GetComponent<WorldCardUI>()
                .Setup(world);
        }
    }
}
