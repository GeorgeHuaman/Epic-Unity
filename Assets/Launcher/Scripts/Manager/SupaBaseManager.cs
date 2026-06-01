using UnityEngine;

public class SupaBaseManager : MonoBehaviour
{
    public static SupaBaseManager Instance;

    [Header("Supabase")]
    public string supabaseUrl;

    public string anonKey;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}