
public class Teacher
{
    public string Name;
    public string Email;
    public string Password;
    public string Phone;

    public string CurrentRoomNo;
    private string Status;

    public Teacher(string name, string email, string password, string phone)
    {
        Name = name;
        Email = email;
        this.Password = password;
        this.Phone = phone;
    }

    public void setStatus(string status)
    {
        this.Status = status;
    }

    public string getStatus()
    {
        return this.Status;
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}
