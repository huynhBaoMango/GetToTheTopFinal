using System.Collections.Generic;

[System.Serializable]
public class User
{
    public string email;
    public string username;
    public string password;
    public int coin;
    public List<string> ownedSkins;
    public string selectedSkin;

    public User(string email, string username, string password, int coin)
    {
        this.email = email;
        this.username = username;
        this.password = password;
        this.coin = coin;
        this.ownedSkins = new List<string>();
        this.selectedSkin = null;
    }
}

