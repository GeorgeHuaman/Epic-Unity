using UnityEngine;

public class OpenAITester : MonoBehaviour
{
    [Header("Configuración")]
    public OpenAIConnector connector;
    public WorldBuilder builder;
    
    [TextArea(3, 10)]
    public string promptDePrueba = "Crea un nivel en Marte con un guía que explique la gravedad.";

    [ContextMenu("Probar Conexión OpenAI")]
    public void TestearIA()
    {
        if (connector == null)
        {
            Debug.LogError("Por favor, asigna el OpenAIConnector en el Inspector.");
            return;
        }

        Debug.Log("Enviando petición a OpenAI...");
        
        StartCoroutine(connector.EnviarPromptALaIA(promptDePrueba, (resultado) => {
            Debug.Log("<color=green>¡Respuesta recibida con éxito!</color>");
            
            if (builder != null)
            {
                builder.ConstruirMundo(resultado);
            }
            else
            {
                Debug.LogWarning("WorldBuilder no asignado. No se puede construir el mundo.");
            }
        }));
    }
}
