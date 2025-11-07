using UnityEngine;

public class ControladorSalida : MonoBehaviour
{
    public void SalirDelJuego()
    {
        // Esta función cerrará la aplicación (solo funciona en builds)
        Application.Quit();

        // Esta línea solo funcionará en el editor de Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
