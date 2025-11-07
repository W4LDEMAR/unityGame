using Firebase.Auth;
using Firebase.Database; // ¡Importante añadir esto!

public static class SesionEstatica
{
    public static FirebaseUser User;
    public static DatabaseReference DBreference; // Para acceder a la BD desde otras escenas
    public static string UserId;                 // Para saber a qué usuario guardar datos
    public static int Saldo;                     // El saldo actual del jugador
}