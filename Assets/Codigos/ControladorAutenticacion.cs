using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;

public class AuthController : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    private FirebaseAuth auth;

    [Header("Login UI")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;

    [Header("Register UI")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordRegisterField;
    public TMP_Text warningRegisterText;

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public void LoginUser()
    {
        StartCoroutine(LoginUserCoroutine(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginUserCoroutine(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            HandleAuthError(loginTask.Exception, "login");
            yield break;
        }

        // Login exitoso
        Debug.LogFormat("User signed in successfully: {0} ({1})", 
            loginTask.Result.User.DisplayName, 
            loginTask.Result.User.Email);
    }

    public void RegisterUser()
    {
        StartCoroutine(RegisterUserCoroutine(
            emailRegisterField.text, 
            passwordRegisterField.text, 
            usernameRegisterField.text
        ));
    }

    private IEnumerator RegisterUserCoroutine(string email, string password, string username)
    {
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            HandleAuthError(registerTask.Exception, "register");
            yield break;
        }

        // Actualizar perfil con nombre de usuario
        var profile = new UserProfile { DisplayName = username };
        var profileTask = registerTask.Result.User.UpdateUserProfileAsync(profile);
        yield return new WaitUntil(() => profileTask.IsCompleted);

        if (profileTask.Exception != null)
        {
            HandleAuthError(profileTask.Exception, "profile update");
        }
    }

    public void SignOut()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
            ClearLoginFields();
            ClearRegisterFields();
        }
    }

    private void HandleAuthError(System.AggregateException exception, string context)
    {
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        string message = $"Error in {context}: ";
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message += "Missing email";
                break;
            case AuthError.MissingPassword:
                message += "Missing password";
                break;
            case AuthError.WeakPassword:
                message += "Weak password";
                break;
            case AuthError.WrongPassword:
                message += "Wrong password";
                break;
            case AuthError.InvalidEmail:
                message += "Invalid email";
                break;
            case AuthError.EmailAlreadyInUse:
                message += "Email already in use";
                break;
            case AuthError.UserNotFound:
                message += "User not found";
                break;
            default:
                message += "Unknown error";
                break;
        }

        if (context == "login")
        {
            warningLoginText.text = message;
        }
        else if (context == "register")
        {
            warningRegisterText.text = message;
        }

        Debug.LogWarning(message);
    }

    public void ClearLoginFields()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
        warningLoginText.text = "";
    }

    public void ClearRegisterFields()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        confirmPasswordRegisterField.text = "";
        warningRegisterText.text = "";
    }
}