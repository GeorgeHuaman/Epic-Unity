using UnityEngine;

public class UIManagerL : MonoBehaviour
{
    public static UIManagerL Instance;

    [Header("Panels")]
    public GameObject loadingScreen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowLoading(bool value)
    {
        loadingScreen.SetActive(value);
    }
}