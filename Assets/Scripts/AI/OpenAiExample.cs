using OpenAI;
using System.Collections.Generic;
using UnityEngine;

public class OpenAiExample : MonoBehaviour
{
    // TODO: Move the API key to a secure location (e.g., environment variable or local config file)
    private OpenAIApi openai = new OpenAIApi(string.Empty);

    async void Start()
    {
        var request = new CreateChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = "Hola, �c�mo puedo usar IA en Unity?" }
            }
        };

        var response = await openai.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            Debug.Log(response.Choices[0].Message.Content);
        }
    }
}
