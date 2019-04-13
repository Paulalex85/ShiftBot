using OpenQA.Selenium;
using System;

namespace Bot_Staffo
{
    public class Shifts
    {
        public IWebElement webElement;
        public DateTime Debut;
        public DateTime Fin;
        public String id;

        public Shifts(IWebElement element, DateTime debut, DateTime fin,String id)
        {
            this.webElement = element;
            this.Debut = debut;
            this.Fin = fin;
            this.id = id;
        }

        public int Get_Duree_Minutes()
        {
            int retour = 0;

            retour = (Fin.Hour * 60 + Fin.Minute ) - (Debut.Hour * 60 + Debut.Minute );
            return retour;
        }
    }

    
}
