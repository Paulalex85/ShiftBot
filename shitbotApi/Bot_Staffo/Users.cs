namespace Bot_Staffo
{
    public class Users
    {
        public string Id;
        public string Nom;
        public string Prenom;
        public string Email;

        public Users()
        {

        }

        public Users(string email)
        {
            this.Email = email;
        }
    }

    
}
