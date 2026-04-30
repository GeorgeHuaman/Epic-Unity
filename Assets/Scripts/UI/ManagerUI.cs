using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GLTFast.Schema;
using System.Collections;

public class ManagerUI : MonoBehaviour
{
    public GameObject panel;
    public GameObject textArea;
    public GameObject chatAI;
    public GameObject chatPerso;
    private Coroutine anim;
    private TextMeshProUGUI textAI;
    public float speed;

    public static ManagerUI Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void OnEnable()
    {
        GameObject chat = Instantiate(chatAI, textArea.transform);
        chat.GetComponent<TextMeshProUGUI>().text = "¿Que construiremos hoy?";
    }

    private void Start()
    {
        panel.SetActive(true);
    }

    public void Enter(TMP_InputField text)
    {
        GameObject chat = Instantiate(chatPerso, textArea.transform);
        chat.GetComponent<TextMeshProUGUI>().text = text.text;
        OpenAITester.Instance.TestearIA(text.text);
        text.text = "";
        GameObject chatAi = Instantiate(chatAI, textArea.transform);
        textAI = chatAi.GetComponent<TextMeshProUGUI>();
        StartTyping();
    }
    private void StartTyping()
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(AnimateDots());
    }
    public void StopTyping(string finalText = "")
    {
        if (anim != null) StopCoroutine(anim);
        textAI.text = finalText;
    }

    IEnumerator AnimateDots()
    {
        string baseText = "";
        int dots = 0;

        while (true)
        {
            dots = (dots + 1) % 4; // 0,1,2,3
            textAI.text = baseText + new string('.', dots);
            yield return new WaitForSeconds(speed);
        }
    }
}
