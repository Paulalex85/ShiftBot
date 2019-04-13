namespace Bot_Staffo
{
    public class Locations
    {
        public string Name;
        public string Id_Location;

        public Locations (string nom, string id)
        {
            this.Name = nom;
            this.Id_Location = id;
        }

        public override string ToString()
        {
            // Generates the text shown in the combo box
            return Name;
        }
    }

    
}
