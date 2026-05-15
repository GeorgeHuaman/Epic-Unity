using UnityEngine;

public class SetPositionPlayer : MonoBehaviour
{
    public static SetPositionPlayer Instance;

    private void Awake()
    {
        Instance = this;
    }
    public GameObject player;

    public void Set()
    {
        player.SetActive(true);
        player.transform.position = GameObject.Find("Slot_Inicio").transform.position;
        Debug.Log("PlayerSet");
    }
}
