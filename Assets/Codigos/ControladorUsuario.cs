// UserController.cs (Versión simplificada)
// Este SÍ es un MonoBehaviour y va en tu GameObject.

using UnityEngine;
using Firebase.Auth; // Necesario para tipo FirebaseUser
using TMPro;
using System.Threading.Tasks; // Necesario para async/await

public class ControladorUsuario : MonoBehaviour
{
    // --- El servicio de lógica "sintetizado" ---
    private UserService userService;
    private FirebaseUser currentUser;

    // Referencias a UI (Sin cambios)
    [Header("UI Elements")]
    public TMP_InputField usernameField;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_InputField confirmPasswordField;
    public TMP_Text warningText;

    [Header("User Data")]
    public TMP_InputField firstNameField;
    public TMP_InputField lastNameField;
    public TMP_InputField motherLastNameField;
    public TMP_InputField balanceField; // Ahora es solo de visualización

    [Header("Address")]
    public TMP_InputField stateField;
    public TMP_InputField streetField;
    public TMP_InputField numberField;
    public TMP_InputField postalCodeField;
    public TMP_InputField municipalityField;

    void Awake()
    {
        // Inicializamos el servicio de lógica
        userService = new UserService();
    }

    #region Authentication Operations

    public async void RegisterUser()
    {
        warningText.text = "Registrando...";
        
        // 1. Validaciones de UI
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

        // 2. Empaquetar los datos (Parámetros a, b, c...)
        UserData data = GatherDataFromUI();

        // 3. Llamar a la función sintetizada
        var (user, error) = await userService.RegisterAsync(emailField.text, passwordField.text, usernameField.text, data);

        // 4. Mostrar el resultado
        if (error != null)
        {
            warningText.text = error;
        }
        else
        {
            currentUser = user;
            warningText.text = $"Registro exitoso: {user.DisplayName}";
            // Opcional: Cargar los datos a la UI (aunque ya están)
            PopulateDataToUI(data); 
        }
    }

    public async void LoginUser()
    {
        warningText.text = "Iniciando sesión...";

        // 1. Llamar a la función sintetizada
        var (user, error) = await userService.LoginAsync(emailField.text, passwordField.text);

        // 2. Mostrar resultado y cargar datos
        if (error != null)
        {
            warningText.text = error;
        }
        else
        {
            currentUser = user;
            usernameField.text = user.DisplayName;
            
            // 3. Cargar datos de la DB a la UI
            await LoadDataToUI(user.UserId);
        }
    }

    public void SignOut()
    {
        userService.SignOut();
        currentUser = null;
        ClearUserData(); // Limpia los campos de la UI
        warningText.text = "Sesión cerrada";
    }

    #endregion

    #region User Data UI

    /// <summary>
    /// Recoge todos los datos de los InputFields y los empaqueta en un objeto UserData.
    /// </summary>
    private UserData GatherDataFromUI()
    {
        var data = new UserData
        {
            NombreCompleto = new UserName
            {
                Nombre = firstNameField.text,
                ApellidoPaterno = lastNameField.text,
                ApellidoMaterno = motherLastNameField.text
            },
            Direccion = new UserAddress
            {
                estado = stateField.text,
                calle = streetField.text,
                numExterior = numberField.text,
                cp = postalCodeField.text,
                municipio = municipalityField.text
            },
            saldo = int.TryParse(balanceField.text, out int s) ? s : 0
        };
        return data;
    }

    /// <summary>
    /// Rellena todos los InputFields a partir de un objeto UserData.
    /// </summary>
    private void PopulateDataToUI(UserData data)
    {
        if (data == null) return;

        // Nombre
        firstNameField.text = data.NombreCompleto.Nombre;
        lastNameField.text = data.NombreCompleto.ApellidoPaterno;
        motherLastNameField.text = data.NombreCompleto.ApellidoMaterno;
        
        // Dirección
        stateField.text = data.Direccion.estado;
        streetField.text = data.Direccion.calle;
        numberField.text = data.Direccion.numExterior;
        postalCodeField.text = data.Direccion.cp;
        municipalityField.text = data.Direccion.municipio;

        // Saldo
        balanceField.text = data.saldo.ToString();
    }
    
    /// <summary>
    /// Llama al servicio para cargar datos y luego los muestra en la UI.
    /// </summary>
    private async Task LoadDataToUI(string userId)
    {
        warningText.text = "Cargando datos...";
        var (data, error) = await userService.LoadUserDataAsync(userId);

        if (error != null)
        {
            warningText.text = error;
        }
        else
        {
            PopulateDataToUI(data);
            warningText.text = "Datos cargados";
        }
    }

    /// <summary>
    /// Limpia todos los campos de la UI.
    /// </summary>
    private void ClearUserData()
    {
        // Auth
        usernameField.text = "";
        emailField.text = "";
        passwordField.text = "";
        confirmPasswordField.text = "";
        
        // Data
        firstNameField.text = "";
        lastNameField.text = "";
        motherLastNameField.text = "";
        balanceField.text = "";
        
        // Address
        stateField.text = "";
        streetField.text = "";
        numberField.text = "";
        postalCodeField.text = "";
        municipalityField.text = "";
    }
    
    // NOTA: He eliminado la región "Balance Operations" (Deposit/Withdraw)
    // porque esa lógica pertenece al TransactionController/TransactionService
    // del ejercicio anterior. Mantenerla aquí rompe el principio de
    // responsabilidad única (este script maneja usuarios, el otro transacciones).

    #endregion
}