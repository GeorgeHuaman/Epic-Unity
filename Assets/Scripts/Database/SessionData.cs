using System;

// Clase estática para guardar los datos del jugador mientras la app está abierta
public static class SessionData
{
    public static string NombreAlumno = "Anonimo";
    public static string CodigoClaseActual = "TEST";
}

// Estructura del paquete de datos que enviaremos a Firebase
[Serializable]
public class MetricaEvento
{
    public string nombre_alumno;
    public string tipo_evento; // Ej: "QUIZ_RESPONDIDO", "NIVEL_COMPLETADO"
    public string detalle;     // Ej: "Correcto: 1810", "Error: 1910"
    public string timestamp;
}
