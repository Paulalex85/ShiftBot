using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Bot_Staffo
{
    public class Bloc_Fenetre
    {
        public MetroFramework.Controls.MetroTabPage fenetre;
        public List<Bloc_Jour> list_bloc_jour = new List<Bloc_Jour>();
        
        private int position_y;

        public Bloc_Fenetre(Size taille_fenetre)
        {
            fenetre = new MetroFramework.Controls.MetroTabPage();
            fenetre.Location = new System.Drawing.Point(4, 22);
            fenetre.Name = "fenetre";
            fenetre.Padding = new System.Windows.Forms.Padding(3);
            fenetre.Size = taille_fenetre;
            fenetre.UseVisualStyleBackColor = true;

            position_y = fenetre.Height - 40;
        }

        public void Ajout_Bloc_Jour(int position_x, int position_y, DateTime date, Form form)
        {
            Bloc_Jour ajout = new Bloc_Jour(position_x, position_y, date,this);
            
            fenetre.Controls.Add(ajout.bouton_moins);
            fenetre.Controls.Add(ajout.bouton_plus);
            fenetre.Controls.Add(ajout.label_date);

            list_bloc_jour.Add(ajout);
        }
    }

    
}
