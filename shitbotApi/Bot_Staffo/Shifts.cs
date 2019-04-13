using System;

namespace Bot_Staffo
{
    public class Shifts
    {
        public string Id;
        public bool Full;
        public DateTime Debut;
        public DateTime Fin;

        public int Get_Duree_Minutes()
        {
            int retour = 0;

            retour = (Fin.Hour * 60 + Fin.Minute ) - (Debut.Hour * 60 + Debut.Minute );
            return retour;
        }
    }

    
}
