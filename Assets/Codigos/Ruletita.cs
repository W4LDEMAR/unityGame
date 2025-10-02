using UnityEngine;

public class RouletteCasino : MonoBehaviour
{
    // Orden real de los números en una ruleta europea (sentido horario)
    private readonly int[] rouletteOrder = new int[]
    {
        0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27,
        13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1,
        20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
    };

    private const int totalSectors = 37;
    private bool isSpinning = false;
    private float spinDuration;
    private float spinTimer = 0f;
    private float startSpeed;

    public void StartSpin()
    {
        if (isSpinning) return;

        spinDuration = Random.Range(4f, 7f);
        startSpeed = Random.Range(360f, 720f);
        spinTimer = 0f;
        isSpinning = true;
    }

    void Update()
    {
        if (isSpinning)
        {
            spinTimer += Time.deltaTime;
            float t = spinTimer / spinDuration;
            float currentSpeed = Mathf.Lerp(startSpeed, 0f, t);
            transform.Rotate(0f, 0f, currentSpeed * Time.deltaTime);

            if (spinTimer >= spinDuration)
            {
                isSpinning = false;
                DetectWinningNumber();
            }
        }
    }

    void DetectWinningNumber()
    {
        float zRotation = transform.eulerAngles.z;
        float angle = 360f - zRotation; // Invertimos porque gira antihorario
        angle = (angle + 360f) % 360f;

        float sectorAngle = 360f / totalSectors;
        int sectorIndex = Mathf.FloorToInt(angle / sectorAngle);

        int winningNumber = rouletteOrder[sectorIndex];

        Debug.Log($" La ruleta se detuvo en el número: {winningNumber}");
    }
}
