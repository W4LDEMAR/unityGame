using UnityEngine;

public class RouletteBall : MonoBehaviour
{
    public Transform ballPivot;       // Objeto vacío que rota la bola
    public Transform rouletteWheel;   // Ruleta (para compensar su rotación)
    private float spinSpeed = 1000f;
    private bool isSpinning = false;
    private int winningNumber = -1;

    // Orden real de la ruleta
    private readonly int[] rouletteOrder = new int[]
    {
        0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27,
        13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1,
        20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
    };

    public void StartBallSpin()
    {
        isSpinning = true;
        winningNumber = -1;
    }

    public void StopBallOnNumber(int number)
    {
        isSpinning = false;
        winningNumber = number;

        // Encontrar el índice del número en el orden de la ruleta
        int index = System.Array.IndexOf(rouletteOrder, winningNumber);
        float sectorAngle = 360f / 37f;
        float angle = index * sectorAngle;

        // Alineamos la bola con el sector ganador, compensando la rotación de la ruleta
        float finalAngle = angle + rouletteWheel.eulerAngles.z;
        ballPivot.localEulerAngles = new Vector3(0f, 0f, -finalAngle);

        Debug.Log($"🎯 Bola alineada con el número {winningNumber} en ángulo {angle}°");
    }

    void Update()
    {
        if (isSpinning)
        {
            // Giramos la bola constantemente en sentido contrario
            ballPivot.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
        }
    }
}
