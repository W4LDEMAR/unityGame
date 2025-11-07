using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using System.Threading.Tasks; 

public class SceneChanger : MonoBehaviour
{
    public void LoadGameScene(string sceneName)
    {
        if (SesionEstatica.Saldo != 0 || !string.IsNullOrEmpty(SesionEstatica.UserId))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("No tienes dinero suficiente para jugar");
        }
    }
}
