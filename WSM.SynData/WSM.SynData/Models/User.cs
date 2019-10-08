namespace WSM.SynData.Models
{
    public class User
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Privilege { get; set; }
        public bool Enabled { get; set; }

        public User(string name, string password, string userName, int privilege, bool enabled)
        {
            Name = name;
            Password = password;
            Privilege = privilege;
            Enabled = enabled;
            UserName = userName;
        }
    }
}
