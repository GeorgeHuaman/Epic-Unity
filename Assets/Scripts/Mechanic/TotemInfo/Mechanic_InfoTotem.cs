using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Mechanic_InfoTotem : MonoBehaviour, IConfigurable
{
    [Header("Referencias UI")]
    public GameObject mensajeInteraccion; // El texto flotante "Presiona E"
    public GameObject panelLectura;       // El panel de pantalla completa
    public TMP_Text textoContenido;       // Donde va el texto largo

    private string contenidoLargo;
    private bool jugadorCerca = false;
    private bool estaLeyendo = false;
    private PlayerMovement playerMovement;
    private InputAction interactAction;

    public void Setup(Dictionary<string, string> data)
    {
        if (data.ContainsKey("contenido_largo"))
        {
            contenidoLargo = data["contenido_largo"];
            textoContenido.text = contenidoLargo;
        }
        mensajeInteraccion.SetActive(false);
        panelLectura.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            mensajeInteraccion.SetActive(true);
            playerMovement = other.GetComponent<PlayerMovement>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            mensajeInteraccion.SetActive(false);
            CerrarLectura(); // Por si se aleja mientras lee
        }
    }

    void Start()
    {
        // Cacheamos la acción de interactuar desde el InputSystem global
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    void Update()
    {
// Detectar si presiona la E cuando est� cerca
        if (jugadorCerca && interactAction != null && interactAction.WasPressedThisFrame())
{
            if (!estaLeyendo) AbrirLectura();
            else CerrarLectura();
        }
    }

    private void AbrirLectura()
    {
        estaLeyendo = true;
        mensajeInteraccion.SetActive(false);
        panelLectura.SetActive(true);
        playerMovement.SetMovement(false); // Detenemos al jugador
    }

    public void CerrarLectura()
    {
        estaLeyendo = false;
        panelLectura.SetActive(false);
        if (jugadorCerca) mensajeInteraccion.SetActive(true);
        if (playerMovement != null) playerMovement.SetMovement(true); // Devolvemos el movimiento
    }
}
