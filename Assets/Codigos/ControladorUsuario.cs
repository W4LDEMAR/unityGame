using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;

public class UserController : MonoBehaviour
{
    // Referencias a Firebase
    private FirebaseAuth auth;
    private FirebaseUser user;
    private DatabaseReference databaseReference;

    // Referencias a UI
    [Header("UI Elements")]
    public TMP_InputField usernameField;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_InputField confirmPasswordField;
    public TMP_Text warningText;

    // Datos de usuario
    [Header("User Data")]
    public TMP_InputField firstNameField;
    public TMP_InputField lastNameField;
    public TMP_InputField motherLastNameField;
    public TMP_InputField balanceField;

    // Dirección
    [Header("Address")]
    public TMP_InputField stateField;
    public TMP_InputField streetField;
    public TMP_InputField numberField;
    public TMP_InputField postalCodeField;
    public TMP_InputField municipalityField;

    void Awake()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    #region Authentication Operations

    public void RegisterUser()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            warningText.text = "Nombre de usuario requerido";
            return;
        }

        if (passwordField.text != confirmPasswordField.text)
        {
            warningText.text = "Las contraseñas no coinciden";
            return;
        }

        StartCoroutine(RegisterUserCoroutine(emailField.text, passwordField.text, usernameField.text));
    }

    private IEnumerator RegisterUserCoroutine(string email, string password, string username)
    {
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            HandleAuthError(registerTask.Exception, "registro");
            yield break;
        }

        user = registerTask.Result.User;
        yield return UpdateUserProfile(username);

        // Guardar datos adicionales del usuario
        yield return SaveUserData();
    }

    private IEnumerator UpdateUserProfile(string displayName)
    {
        var profile = new UserProfile { DisplayName = displayName };
        var profileTask = user.UpdateUserProfileAsync(profile);
        yield return new WaitUntil(() => profileTask.IsCompleted);

        if (profileTask.Exception != null)
        {
            HandleAuthError(profileTask.Exception, "actualización de perfil");
        }
    }

    public void LoginUser()
    {
        StartCoroutine(LoginUserCoroutine(emailField.text, passwordField.text));
    }

    private IEnumerator LoginUserCoroutine(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            HandleAuthError(loginTask.Exception, "inicio de sesión");
            yield break;
        }

        user = loginTask.Result.User;
        yield return LoadUserData();
    }

    public void SignOut()
    {
        auth.SignOut();
        ClearUserData();
    }

    private void HandleAuthError(System.AggregateException exception, string operation)
    {
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        string message = $"Error en {operation}: ";
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message += "Falta el correo electrónico";
                break;
            case AuthError.MissingPassword:
                message += "Falta la contraseña";
                break;
            case AuthError.WeakPassword:
                message += "Contraseña débil";
                break;
            case AuthError.WrongPassword:
                message += "Contraseña incorrecta";
                break;
            case AuthError.InvalidEmail:
                message += "Correo electrónico inválido";
                break;
            case AuthError.EmailAlreadyInUse:
                message += "Correo electrónico ya en uso";
                break;
            case AuthError.UserNotFound:
                message += "Usuario no encontrado";
                break;
            default:
                message += "Error desconocido";
                break;
        }

        warningText.text = message;
        Debug.LogWarning(message);
    }

    #endregion

    #region User Data Operations

    private IEnumerator SaveUserData()
    {
        // Guardar nombre completo
        yield return SaveUserName(firstNameField.text, lastNameField.text, motherLastNameField.text);

        // Guardar dirección
        yield return SaveUserAddress(
            stateField.text,
            streetField.text,
            numberField.text,
            postalCodeField.text,
            municipalityField.text
        );

        // Inicializar saldo si es nuevo usuario
        if (string.IsNullOrEmpty(balanceField.text))
        {
            balanceField.text = "0";
        }
        yield return UpdateBalance(int.Parse(balanceField.text));
    }

    private IEnumerator SaveUserName(string firstName, string lastName, string motherLastName)
    {
        var nameTask = databaseReference.Child("users").Child(user.UserId).Child("NombreCompleto")
            .Child("Nombre").SetValueAsync(firstName);
        yield return new WaitUntil(() => nameTask.IsCompleted);

        if (nameTask.Exception != null)
        {
            Debug.LogWarning($"Error al guardar nombre: {nameTask.Exception}");
            yield break;
        }

        var lastNameTask = databaseReference.Child("users").Child(user.UserId).Child("NombreCompleto")
            .Child("Apellido Paterno").SetValueAsync(lastName);
        yield return new WaitUntil(() => lastNameTask.IsCompleted);

        var motherLastNameTask = databaseReference.Child("users").Child(user.UserId).Child("NombreCompleto")
            .Child("Apellido Materno").SetValueAsync(motherLastName);
        yield return new WaitUntil(() => motherLastNameTask.IsCompleted);
    }

    private IEnumerator SaveUserAddress(string state, string street, string number, string postalCode, string municipality)
    {
        var addressData = new System.Collections.Generic.Dictionary<string, object>
        {
            { "estado", state },
            { "calle", street },
            { "numExterior", number },
            { "cp", postalCode },
            { "municipio", municipality }
        };

        var addressTask = databaseReference.Child("users").Child(user.UserId).Child("Direccion")
            .UpdateChildrenAsync(addressData);
        yield return new WaitUntil(() => addressTask.IsCompleted);

        if (addressTask.Exception != null)
        {
            Debug.LogWarning($"Error al guardar dirección: {addressTask.Exception}");
        }
    }

    private IEnumerator LoadUserData()
    {
        var dataTask = databaseReference.Child("users").Child(user.UserId).GetValueAsync();
        yield return new WaitUntil(() => dataTask.IsCompleted);

        if (dataTask.Exception != null)
        {
            Debug.LogWarning($"Error al cargar datos: {dataTask.Exception}");
            yield break;
        }

        DataSnapshot snapshot = dataTask.Result;
        if (snapshot.Exists)
        {
            // Cargar nombre completo
            if (snapshot.HasChild("NombreCompleto"))
            {
                var nameData = snapshot.Child("NombreCompleto");
                firstNameField.text = nameData.Child("Nombre").Value?.ToString() ?? "";
                lastNameField.text = nameData.Child("Apellido Paterno").Value?.ToString() ?? "";
                motherLastNameField.text = nameData.Child("Apellido Materno").Value?.ToString() ?? "";
            }

            // Cargar dirección
            if (snapshot.HasChild("Direccion"))
            {
                var addressData = snapshot.Child("Direccion");
                stateField.text = addressData.Child("estado").Value?.ToString() ?? "";
                streetField.text = addressData.Child("calle").Value?.ToString() ?? "";
                numberField.text = addressData.Child("numExterior").Value?.ToString() ?? "";
                postalCodeField.text = addressData.Child("cp").Value?.ToString() ?? "";
                municipalityField.text = addressData.Child("municipio").Value?.ToString() ?? "";
            }

            // Cargar saldo
            balanceField.text = snapshot.Child("saldo").Value?.ToString() ?? "0";
        }
    }

    private void ClearUserData()
    {
        firstNameField.text = "";
        lastNameField.text = "";
        motherLastNameField.text = "";
        balanceField.text = "";
        stateField.text = "";
        streetField.text = "";
        numberField.text = "";
        postalCodeField.text = "";
        municipalityField.text = "";
    }

    #endregion

    #region Balance Operations

    public void Deposit()
    {
        if (int.TryParse(balanceField.text, out int amount))
        {
            StartCoroutine(UpdateBalanceTransaction(amount));
        }
        else
        {
            warningText.text = "Monto inválido";
        }
    }

    public void Withdraw()
    {
        if (int.TryParse(balanceField.text, out int amount))
        {
            StartCoroutine(WithdrawBalance(amount));
        }
        else
        {
            warningText.text = "Monto inválido";
        }
    }

    private IEnumerator UpdateBalance(int newBalance)
    {
        var balanceTask = databaseReference.Child("users").Child(user.UserId).Child("saldo")
            .SetValueAsync(newBalance);
        yield return new WaitUntil(() => balanceTask.IsCompleted);

        if (balanceTask.Exception != null)
        {
            Debug.LogWarning($"Error al actualizar saldo: {balanceTask.Exception}");
        }
    }

    private IEnumerator UpdateBalanceTransaction(int amount)
    {
        var reference = databaseReference.Child("users").Child(user.UserId).Child("saldo");

        var transactionTask = reference.RunTransaction(mutableData =>
        {
            int currentBalance = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
            mutableData.Value = currentBalance + amount;
            return TransactionResult.Success(mutableData);
        });

        yield return new WaitUntil(() => transactionTask.IsCompleted);

        if (transactionTask.Exception != null)
        {
            Debug.LogWarning($"Error en transacción de saldo: {transactionTask.Exception}");
        }
        else
        {
            balanceField.text = (int.Parse(balanceField.text) + amount).ToString();
        }
    }

    private IEnumerator WithdrawBalance(int amount)
    {
        var reference = databaseReference.Child("users").Child(user.UserId).Child("saldo");

        var transactionTask = reference.RunTransaction(mutableData =>
        {
            int currentBalance = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
            
            if (amount > currentBalance)
            {
                warningText.text = "Saldo insuficiente";
                return TransactionResult.Abort();
            }

            mutableData.Value = currentBalance - amount;
            return TransactionResult.Success(mutableData);
        });

        yield return new WaitUntil(() => transactionTask.IsCompleted);

        if (transactionTask.Exception != null)
        {
            Debug.LogWarning($"Error en transacción de retiro: {transactionTask.Exception}");
        }
        else
        {
            balanceField.text = (int.Parse(balanceField.text) - amount).ToString();
        }
    }

    #endregion
}