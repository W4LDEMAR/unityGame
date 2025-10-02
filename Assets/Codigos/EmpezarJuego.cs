using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneName = "Juego"; // Nombre exacto de la escena que quieres cargar

    public void LoadGameScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
