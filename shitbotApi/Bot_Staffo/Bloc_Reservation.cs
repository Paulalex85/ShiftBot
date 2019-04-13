using System.Collections.Generic;
using System.Drawing;

namespace Bot_Staffo
{
    public class Bloc_Reservation
    {
        public MetroFramework.Controls.MetroTabControl tab_control;
        public List<Bloc_Fenetre> list_fenetre = new List<Bloc_Fenetre>();
        public Bloc_Reservation(int position_x, int position_y, Size taille_fenetre)
        {
            tab_control = new MetroFramework.Controls.MetroTabControl();
            tab_control.Location = new System.Drawing.Point(position_x, position_y);
            tab_control.Name = "bloc_reservation";
            tab_control.Size = taille_fenetre;
        }

        public void Ajout_Fenetre(Locations location)
        {
            Bloc_Fenetre ajout = new Bloc_Fenetre(location, tab_control.Size);
            list_fenetre.Add(ajout);
            tab_control.Controls.Add(ajout.fenetre);
            tab_control.ResumeLayout(false);


        }

        public void Reset_TabControl()
        {
            foreach(Bloc_Fenetre bloc in list_fenetre)
            {
                tab_control.Controls.Remove(bloc.fenetre);
            }
        }
    }

    
}
