// Models.cs
// Coloca esto en un archivo llamado Models.cs
// Estas son clases simples para almacenar y pasar datos.

// Un objeto para la dirección
using System;

[System.Serializable]
public class UserAddress
{
    public string estado;
    public string calle;
    public string numExterior;
    public string cp;
    public string municipio;
}

// Un objeto para el nombre
[System.Serializable]
public class UserName
{
    public string Nombre;
    public string ApellidoPaterno;
    public string ApellidoMaterno;
}

[Serializable]
public class NewBaseType
{
    public UserName NombreCompleto;
    public UserAddress Direccion;
    public int saldo;
}

// El objeto principal que se guarda en "users/userId"
[System.Serializable]
public class UserData : NewBaseType
{
    public UserData()
    {
        NombreCompleto = new UserName();
        Direccion = new UserAddress();
        saldo = 0;
    }
}