using Firebase.Auth;

public class AuthModel
{
    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser User { get; private set; }
    public bool IsAuthenticated => User != null;
    
    public AuthModel()
    {
        Auth = FirebaseAuth.DefaultInstance;
    }
    
    public void UpdateUser(FirebaseUser user)
    {
        User = user;
    }
    
    public void ClearUser()
    {
        User = null;
    }
}