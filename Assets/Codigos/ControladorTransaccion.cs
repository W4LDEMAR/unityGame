using System.Collections;
using UnityEngine;
using Firebase.Database;
using TMPro;

public class TransactionController : MonoBehaviour
{
    private DatabaseReference databaseReference;
    private string userId;

    [Header("UI Elements")]
    public TMP_InputField amountInputField;
    public TMP_Text balanceText;
    public TMP_Text warningText;

    void Start()
    {
        // Obtener referencia a la base de datos
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        
        // Aquí deberías obtener el userId del usuario autenticado
        // userId = AuthController.Instance.UserId;
    }

    public void Deposit()
    {
        if (int.TryParse(amountInputField.text, out int amount))
        {
            StartCoroutine(ProcessTransaction(amount, true));
        }
        else
        {
            warningText.text = "Monto inválido";
        }
    }

    public void Withdraw()
    {
        if (int.TryParse(amountInputField.text, out int amount))
        {
            StartCoroutine(ProcessTransaction(amount, false));
        }
        else
        {
            warningText.text = "Monto inválido";
        }
    }

    private IEnumerator ProcessTransaction(int amount, bool isDeposit)
    {
        var reference = databaseReference.Child("users").Child(userId).Child("saldo");

        var transactionTask = reference.RunTransaction(mutableData =>
        {
            int currentBalance = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;

            if (!isDeposit && amount > currentBalance)
            {
                warningText.text = "Saldo insuficiente";
                return TransactionResult.Abort();
            }

            mutableData.Value = isDeposit ? currentBalance + amount : currentBalance - amount;
            return TransactionResult.Success(mutableData);
        });

        yield return new WaitUntil(() => transactionTask.IsCompleted);

        if (transactionTask.Exception != null)
        {
            Debug.LogWarning($"Error en transacción: {transactionTask.Exception}");
            warningText.text = "Error en la transacción";
        }
        else
        {
            // Actualizar UI
            yield return UpdateBalanceDisplay();
            amountInputField.text = "";
        }
    }

    private IEnumerator UpdateBalanceDisplay()
    {
        var balanceTask = databaseReference.Child("users").Child(userId).Child("saldo").GetValueAsync();
        yield return new WaitUntil(() => balanceTask.IsCompleted);

        if (balanceTask.Exception != null)
        {
            Debug.LogWarning($"Error al obtener saldo: {balanceTask.Exception}");
            yield break;
        }

        balanceText.text = balanceTask.Result.Value?.ToString() ?? "0";
    }

    public void ClearTransactionFields()
    {
        amountInputField.text = "";
        warningText.text = "";
    }
}