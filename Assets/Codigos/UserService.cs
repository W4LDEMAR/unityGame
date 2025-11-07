// UserService.cs
// Esta es tu "función sintetizada". No es un MonoBehaviour.

using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using System.Collections.Generic;

public class UserService
{
    private FirebaseAuth auth;
    private DatabaseReference databaseReference;

    public UserService()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    /// <summary>
    /// Registra un nuevo usuario, actualiza su perfil y guarda sus datos iniciales.
    /// </summary>
    /// <returns>Una tupla (FirebaseUser, string de error)</returns>
    public async Task<(FirebaseUser user, string error)> RegisterAsync(string email, string password, string username, UserData initialData)
    {
        try
        {
            // 1. Crear el usuario en Auth
            var registerTask = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            FirebaseUser user = registerTask.User;

            // 2. Actualizar el perfil (nombre de usuario)
            var profile = new UserProfile { DisplayName = username };
            await user.UpdateUserProfileAsync(profile);

            // 3. Guardar los datos adicionales en la base de datos
            var (saveSuccess, saveError) = await SaveUserDataAsync(user.UserId, initialData);
            
            if (!saveSuccess)
            {
                // Si guardar datos falla, el registro está "a medias", pero lo reportamos
                return (user, $"Usuario creado, pero falló al guardar datos: {saveError}");
            }
            
            return (user, null); // Éxito
        }
        catch (System.AggregateException ex)
        {
            return (null, ParseAuthError(ex, "registro"));
        }
    }

    /// <summary>
    /// Inicia sesión con un usuario.
    /// </summary>
    /// <returns>Una tupla (FirebaseUser, string de error)</returns>
    public async Task<(FirebaseUser user, string error)> LoginAsync(string email, string password)
    {
        try
        {
            var loginTask = await auth.SignInWithEmailAndPasswordAsync(email, password);
            return (loginTask.User, null); // Éxito
        }
        catch (System.AggregateException ex)
        {
            return (null, ParseAuthError(ex, "inicio de sesión"));
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario actual.
    /// </summary>
    public void SignOut()
    {
        auth.SignOut();
    }

    /// <summary>
    /// Guarda la información completa de un usuario en la base de datos.
    /// </summary>
    /// <returns>Una tupla (bool de éxito, string de error)</returns>
    public async Task<(bool success, string error)> SaveUserDataAsync(string userId, UserData data)
    {
        try
        {
            // Creamos un diccionario para la estructura anidada
            var userDataMap = new Dictionary<string, object>
            {
                { "NombreCompleto", new Dictionary<string, object> {
                    { "Nombre", data.NombreCompleto.Nombre },
                    { "Apellido Paterno", data.NombreCompleto.ApellidoPaterno },
                    { "Apellido Materno", data.NombreCompleto.ApellidoMaterno }
                }},
                { "Direccion", new Dictionary<string, object> {
                    { "estado", data.Direccion.estado },
                    { "calle", data.Direccion.calle },
                    { "numExterior", data.Direccion.numExterior },
                    { "cp", data.Direccion.cp },
                    { "municipio", data.Direccion.municipio }
                }},
                { "saldo", data.saldo }
            };

            await databaseReference.Child("users").Child(userId).UpdateChildrenAsync(userDataMap);
            return (true, null);
        }
        catch (System.Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Carga la información de un usuario desde la base de datos.
    /// </summary>
    /// <returns>Una tupla (UserData, string de error)</returns>
    public async Task<(UserData data, string error)> LoadUserDataAsync(string userId)
    {
        try
        {
            var dataTask = await databaseReference.Child("users").Child(userId).GetValueAsync();
            if (!dataTask.Exists)
            {
                return (null, "No se encontraron datos para el usuario.");
            }

            DataSnapshot snapshot = dataTask;
            UserData loadedData = new UserData();

            // Cargar nombre
            if (snapshot.HasChild("NombreCompleto"))
            {
                var nameData = snapshot.Child("NombreCompleto");
                loadedData.NombreCompleto.Nombre = nameData.Child("Nombre").Value?.ToString() ?? "";
                loadedData.NombreCompleto.ApellidoPaterno = nameData.Child("Apellido Paterno").Value?.ToString() ?? "";
                loadedData.NombreCompleto.ApellidoMaterno = nameData.Child("Apellido Materno").Value?.ToString() ?? "";
            }

            // Cargar dirección
            if (snapshot.HasChild("Direccion"))
            {
                var addressData = snapshot.Child("Direccion");
                loadedData.Direccion.estado = addressData.Child("estado").Value?.ToString() ?? "";
                loadedData.Direccion.calle = addressData.Child("calle").Value?.ToString() ?? "";
                loadedData.Direccion.numExterior = addressData.Child("numExterior").Value?.ToString() ?? "";
                loadedData.Direccion.cp = addressData.Child("cp").Value?.ToString() ?? "";
                loadedData.Direccion.municipio = addressData.Child("municipio").Value?.ToString() ?? "";
            }

            // Cargar saldo
            loadedData.saldo = int.Parse(snapshot.Child("saldo").Value?.ToString() ?? "0");

            return (loadedData, null);
        }
        catch (System.Exception ex)
        {
            return (null, ex.Message);
        }
    }

    /// <summary>
    /// Parsea un error de Firebase Auth a un string legible.
    /// </summary>
    private string ParseAuthError(System.AggregateException exception, string operation)
    {
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        string message = $"Error en {operation}: ";
        switch (errorCode)
        {
            case AuthError.MissingEmail: message += "Falta el correo electrónico"; break;
            case AuthError.MissingPassword: message += "Falta la contraseña"; break;
            case AuthError.WeakPassword: message += "Contraseña débil"; break;
            case AuthError.WrongPassword: message += "Contraseña incorrecta"; break;
            case AuthError.InvalidEmail: message += "Correo electrónico inválido"; break;
            case AuthError.EmailAlreadyInUse: message += "Correo electrónico ya en uso"; break;
            case AuthError.UserNotFound: message += "Usuario no encontrado"; break;
            default: message += "Error desconocido"; break;
        }
        return message;
    }
}