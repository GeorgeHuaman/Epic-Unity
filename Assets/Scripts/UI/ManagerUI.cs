using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        chat.GetComponent<TextMeshProUGUI>().text = "�Que construiremos hoy?";
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
    public void StartTyping()
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(AnimateDots());
    }
    public void StopTyping(string finalText = "")
    {
        if (anim != null) StopCoroutine(anim);
        if (textAI != null) textAI.text = finalText;
    }

    IEnumerator AnimateDots()
    {
        int dots = 1;

        while (true)
        {
            if (textAI != null)
            {
                textAI.text = new string('.', dots);
                dots = dots % 3 + 1;
            }

            yield return new WaitForSeconds(speed);
        }
    }
}
