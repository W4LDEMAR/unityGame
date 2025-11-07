// TransactionService.cs
// Esta clase NO es un MonoBehaviour. Es lógica pura.

using Firebase.Database;
using System;
using System.Threading.Tasks;

public class TransactionService
{
    private DatabaseReference databaseReference;

    public TransactionService()
    {
        // Se inicializa a sí misma
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    /// <summary>
    /// Intenta depositar una cantidad para un usuario.
    /// </summary>
    /// <returns>Una tupla (bool success, string message)</returns>
    public async Task<(bool success, string message)> DepositAsync(string userId, int amount)
    {
        if (amount <= 0)
        {
            return (false, "El monto debe ser positivo");
        }
        return await ProcessTransactionAsync(userId, amount, true);
    }

    /// <summary>
    /// Intenta retirar una cantidad para un usuario.
    /// </summary>
    /// <returns>Una tupla (bool success, string message)</returns>
    public async Task<(bool success, string message)> WithdrawAsync(string userId, int amount)
    {
        if (amount <= 0)
        {
            return (false, "El monto debe ser positivo");
        }
        return await ProcessTransactionAsync(userId, amount, false);
    }

    /// <summary>
    /// Obtiene el saldo actual de un usuario.
    /// </summary>
    /// <returns>Una tupla (int balance, string error)</returns>
    public async Task<(int balance, string error)> GetBalanceAsync(string userId)
    {
        try
        {
            var dataSnapshot = await databaseReference.Child("users").Child(userId).Child("saldo").GetValueAsync();
            
            if (dataSnapshot == null || !dataSnapshot.Exists)
            {
                return (0, null); // Usuario sin saldo, devuelve 0
            }

            int balance = int.Parse(dataSnapshot.Value.ToString());
            return (balance, null);
        }
        catch (Exception ex)
        {
            return (0, ex.Message); // Devuelve 0 y el mensaje de error
        }
    }

    /// <summary>
    /// El núcleo de la lógica de transacción, ahora como un Task asíncrono.
    /// </summary>
    private async Task<(bool success, string message)> ProcessTransactionAsync(string userId, int amount, bool isDeposit)
    {
        var reference = databaseReference.Child("users").Child(userId).Child("saldo");
        string abortReason = null; // Variable para capturar la razón de aborto

        try
        {
            // Ejecutamos la transacción y esperamos el resultado
            DataSnapshot snapshot = await reference.RunTransaction(mutableData =>
            {
                int currentBalance = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;

                if (!isDeposit && amount > currentBalance)
                {
                    // Si no se puede retirar, se guarda la razón y se aborta
                    abortReason = "Saldo insuficiente";
                    return TransactionResult.Abort();
                }

                mutableData.Value = isDeposit ? currentBalance + amount : currentBalance - amount;
                return TransactionResult.Success(mutableData);
            });

            // Revisamos el resultado DESPUÉS de que la transacción termine
            if (abortReason != null)
            {
                return (false, abortReason); // Abortado por lógica (saldo insuficiente)
            }

            if (snapshot == null)
            {
                // La transacción falló por otras razones (ej. contención)
                return (false, "La transacción falló (contención de datos)");
            }

            // Éxito
            return (true, isDeposit ? "Depósito exitoso" : "Retiro exitoso");
        }
        catch (Exception ex)
        {
            // Captura errores de red, permisos, etc.
            return (false, $"Error de Firebase: {ex.Message}");
        }
    }
}