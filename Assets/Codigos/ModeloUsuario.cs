using System;
using Firebase.Auth;
using Firebase.Database;

[Serializable]
public class UserModel
{
    // Datos básicos de autenticación
    public string UserId;
    public string Email;
    public string DisplayName;
    
    // Datos personales
    public UserName Name;
    public UserAddress Address;
    
    // Datos financieros
    public int Balance;
    
    // Constructor
    public UserModel(FirebaseUser user)
    {
        if (user != null)
        {
            UserId = user.UserId;
            Email = user.Email;
            DisplayName = user.DisplayName;
        }
        
        Name = new UserName();
        Address = new UserAddress();
        Balance = 0;
    }
    
    // Actualizar desde FirebaseUser
    public void UpdateFromFirebaseUser(FirebaseUser user)
    {
        if (user != null)
        {
            Email = user.Email;
            DisplayName = user.DisplayName;
        }
    }
    
    // Cargar datos desde DataSnapshot
    public void LoadFromSnapshot(DataSnapshot snapshot)
    {
        if (snapshot.HasChild("NombreCompleto"))
        {
            var nameData = snapshot.Child("NombreCompleto");
            Name.FirstName = nameData.Child("Nombre").Value?.ToString() ?? "";
            Name.LastName = nameData.Child("Apellido Paterno").Value?.ToString() ?? "";
            Name.MotherLastName = nameData.Child("Apellido Materno").Value?.ToString() ?? "";
        }
        
        if (snapshot.HasChild("Direccion"))
        {
            var addressData = snapshot.Child("Direccion");
            Address.State = addressData.Child("estado").Value?.ToString() ?? "";
            Address.Street = addressData.Child("calle").Value?.ToString() ?? "";
            Address.Number = addressData.Child("numExterior").Value?.ToString() ?? "";
            Address.PostalCode = addressData.Child("cp").Value?.ToString() ?? "";
            Address.Municipality = addressData.Child("municipio").Value?.ToString() ?? "";
        }
        
        Balance = snapshot.Child("saldo").Value != null ? int.Parse(snapshot.Child("saldo").Value.ToString()) : 0;
    }
    
    // Submodelo para nombre completo
    [Serializable]
    public class UserName
    {
        public string FirstName;
        public string LastName;
        public string MotherLastName;
        
        public UserName()
        {
            FirstName = "";
            LastName = "";
            MotherLastName = "";
        }
        
        public string FullName => $"{FirstName} {LastName} {MotherLastName}".Trim();
    }
    
    // Submodelo para dirección
    [Serializable]
    public class UserAddress
    {
        public string State;
        public string Street;
        public string Number;
        public string PostalCode;
        public string Municipality;
        
        public UserAddress()
        {
            State = "";
            Street = "";
            Number = "";
            PostalCode = "";
            Municipality = "";
        }
        
        public string FullAddress => $"{Street} {Number}, {Municipality}, {State}, {PostalCode}".Trim();
    }
}