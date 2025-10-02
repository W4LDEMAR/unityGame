using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using System.Threading.Tasks;
using Firebase.Database;
//SignOutButton();

public static class Sesion
{
    public static FirebaseUser User;
}

public class Autenticacion : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;

    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject menuPrincipalPanel;
    public GameObject mainPanel;
    public GameObject errorPanel;
    public GameObject registroPanel;
    public GameObject domicilioPanel;
    public GameObject retiroErrorPanel;
    public GameObject retiroJugadorPanel;


    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    //User Data variables
    [Header("UserData")]
    public TMP_InputField ApPaterno;
    public TMP_InputField ApMaterno;
    public TMP_InputField Saldo;
    public TMP_InputField Estado;
    public TMP_InputField Calle;
    public TMP_InputField NumExterior;
    public TMP_InputField CP;
    public TMP_InputField Minicipio;
    public TMP_InputField MontoRetiro;
    public TMP_InputField MontoDeposito;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
                ClearRegisterFeilds();
                ClearLoginFeilds();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void ClearLoginFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }
    public void ClearRegisterFeilds()
    {
        ApPaterno.text = "";
        ApMaterno.text = "";

        // Limpiar campos de dirección
        Estado.text = "";
        Calle.text = "";
        NumExterior.text = "";
        CP.text = "";
        Minicipio.text = ""; // Nota: Hay un typo en el nombre (debería ser "Municipio")

        // Limpiar campos de saldo y transacciones
        Saldo.text = "";
        MontoRetiro.text = "";
        MontoDeposito.text = "";
    }

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        menuPrincipalPanel.SetActive(false);   // Oculta el panel de login
        registroPanel.SetActive(true);     // Muestra el panel de registro
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    public void SignOutButton()
    {
        auth.SignOut();
        ClearRegisterFeilds();
        ClearLoginFeilds();
        loginPanel.SetActive(true);   // Muetsra el panel de login
        mainPanel.SetActive(false);     // Oculta el panel principal
    }

    public void SaveDataButton()
    {
        StartCoroutine(UpdateUsernameAuth(usernameRegisterField.text));
        StartCoroutine(UpdateUsernameDatabase(usernameRegisterField.text));

        StartCoroutine(UpdateApellidos(ApPaterno.text, ApMaterno.text));
        StartCoroutine(UpdateDireccion(Estado.text, Calle.text, NumExterior.text, CP.text, Minicipio.text));
    }

    public void botonDeposito()
    {
        StartCoroutine(DepositarSaldo(int.Parse(MontoDeposito.text)));
    }

    public void botonRetiro()
    {
        StartCoroutine(RetirarSaldo(int.Parse(MontoRetiro.text)));
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Error de inicio";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Falta Correo";
                    break;
                case AuthError.MissingPassword:
                    message = "Falta Contraseña";
                    break;
                case AuthError.WrongPassword:
                    loginPanel.SetActive(false);   // Oculta el panel de login
                    errorPanel.SetActive(true);     // Muestra el panel principal
                    break;
                case AuthError.InvalidEmail:
                    loginPanel.SetActive(false);   // Oculta el panel de login
                    errorPanel.SetActive(true);     // Muestra el panel principal
                    break;
                case AuthError.UserNotFound:
                    loginPanel.SetActive(false);   // Oculta el panel de login
                    errorPanel.SetActive(true);     // Muestra el panel principal
                    break;
            }
            warningLoginText.text = message;
            loginPanel.SetActive(false);   // Oculta el panel de login
            errorPanel.SetActive(true);     // Muestra el panel de error
            
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = FirebaseAuth.DefaultInstance.CurrentUser;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";

            StartCoroutine(LoadSaldo());

            loginPanel.SetActive(false);   // Oculta el panel de login
            mainPanel.SetActive(true);     // Muestra el panel principal

        }
    }


    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Nombre Faltante";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Las contraseñas no coinciden!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Registro fallido!!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Correo Faltante";
                        break;
                    case AuthError.MissingPassword:
                        message = "Contraseña faltante";
                        break;
                    case AuthError.WeakPassword:
                        message = "Contraseña debil";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Correo ya utilizado";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result.User;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    Task ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Error de registro de usuario!!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        StartCoroutine(RegisterLogin(_email, _password));
                        registroPanel.SetActive(false);   // Oculta el panel del registro de nuevo usuario
                        domicilioPanel.SetActive(true);     // Muestra el panel para la direccion
                    }
                }
            }
        }
    }

    private IEnumerator RegisterLogin(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Error de inicio";
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";

        }
    }

    private IEnumerator UpdateUsernameAuth(string _username)
    {
        //Create a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _username };

        //Call the Firebase auth update user profile function passing the profile with the username
        Task ProfileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            //Auth username is now updated
        }
    }

    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        //Set the currently logged in user username in the database
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("NombreCompleto").Child("Nombre").SetValueAsync(_username);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    private IEnumerator UpdateApellidos(string apellidoP, string apellidoM)
    {
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("NombreCompleto").Child("Apellido Paterno").SetValueAsync(apellidoP);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            DBTask = DBreference.Child("users").Child(User.UserId).Child("NombreCompleto").Child("Apellido Materno").SetValueAsync(apellidoM);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                // Operación exitosa
            }
        }
    }

    private IEnumerator UpdateDireccion(string estado, string calle, string numExterior, string cp, string municipio)
    {
        // Actualizar estado
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("Direccion").Child("estado").SetValueAsync(estado);
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Error al actualizar estado: {DBTask.Exception}");
            yield break; // Terminar la corrutina si hay error
        }

        // Actualizar calle
        DBTask = DBreference.Child("users").Child(User.UserId).Child("Direccion").Child("calle").SetValueAsync(calle);
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Error al actualizar calle: {DBTask.Exception}");
            yield break;
        }

        // Actualizar numExterior
        DBTask = DBreference.Child("users").Child(User.UserId).Child("Direccion").Child("numExterior").SetValueAsync(numExterior);
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Error al actualizar número exterior: {DBTask.Exception}");
            yield break;
        }

        // Actualizar CP
        DBTask = DBreference.Child("users").Child(User.UserId).Child("Direccion").Child("cp").SetValueAsync(cp);
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Error al actualizar CP: {DBTask.Exception}");
            yield break;
        }

        // Actualizar municipio
        DBTask = DBreference.Child("users").Child(User.UserId).Child("Direccion").Child("municipio").SetValueAsync(municipio);
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Error al actualizar municipio: {DBTask.Exception}");
        }
        else
        {
            Debug.Log("Dirección actualizada exitosamente!");
        }
    }

    private IEnumerator UpdateSaldo(int _saldo)
    {
        //Set the currently logged in user xp
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("saldo").SetValueAsync(_saldo);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Xp is now updated
        }
    }

    private IEnumerator LoadSaldo()
    {
        //Get the currently logged in user data
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            Saldo.text = "0";
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            Saldo.text = snapshot.Child("saldo").Value.ToString();
        }
    }

    private IEnumerator DepositarSaldo(int _saldo)
    {
        var reference = DBreference.Child("users").Child(User.UserId).Child("saldo");

        Task<DataSnapshot> transactionTask = reference.RunTransaction(mutableData =>
        {
            int saldoActual = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
            mutableData.Value = saldoActual + _saldo;
            return TransactionResult.Success(mutableData);
        });

        yield return new WaitUntil(() => transactionTask.IsCompleted);

        if (transactionTask.Exception != null)
        {
            Debug.LogWarning($"Error en transacción de saldo: {transactionTask.Exception}");
        }
        else
        {
            // Actualizar UI
            Saldo.text = (int.Parse(Saldo.text) + _saldo).ToString();
            StartCoroutine(UpdateSaldo(int.Parse(Saldo.text)));
            Debug.Log("Depósito realizado con éxito");
        }
    }

    private IEnumerator RetirarSaldo(int _saldoARetirar)
    {
        var reference = DBreference.Child("users").Child(User.UserId).Child("saldo");

        Task<DataSnapshot> transactionTask = reference.RunTransaction(mutableData =>
        {
            // Obtener saldo actual (0 si no existe)
            int saldoActual = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;

            // Validar si el retiro es posible
            if (_saldoARetirar > saldoActual)
            {
                Debug.LogError($"No hay suficiente saldo. Saldo actual: {saldoActual}, intento de retiro: {_saldoARetirar}");
                retiroJugadorPanel.SetActive(false);
                retiroErrorPanel.SetActive(true);
                return TransactionResult.Abort(); // Cancela la transacción
            }

            // Restar el monto
            mutableData.Value = saldoActual - _saldoARetirar;
            return TransactionResult.Success(mutableData);
        });

        yield return new WaitUntil(() => transactionTask.IsCompleted);

        if (transactionTask.Exception != null)
        {
            Debug.LogWarning($"Error en transacción de saldo: {transactionTask.Exception}");
        }
        else
        {
            // Actualizar UI
            int nuevoSaldo = int.Parse(Saldo.text) - _saldoARetirar;
            Saldo.text = nuevoSaldo.ToString();
            StartCoroutine(UpdateSaldo(nuevoSaldo));
            Debug.Log($"Retiro exitoso. Nuevo saldo: {nuevoSaldo}");
        }
    }

}