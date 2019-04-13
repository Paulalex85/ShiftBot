using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Bot_Staffo
{
    public class Bloc_Fenetre
    {
        public MetroFramework.Controls.MetroTabPage fenetre;
        public Locations location;
        public List<Bloc_Jour> list_bloc_jour = new List<Bloc_Jour>();
        public Bloc_Department bloc_dep;

        public Schedules schedule;
        public bool recherche_en_cours = false;

        public MetroFramework.Controls.MetroLabel label_recheche;
        public DateTimePicker temps_sortie_planning;
        
        private int position_y;

        public Bloc_Fenetre(Locations location, Size taille_fenetre)
        {
            this.location = location;
            fenetre = new MetroFramework.Controls.MetroTabPage();
            fenetre.Location = new System.Drawing.Point(4, 22);
            fenetre.Name = "fenetre";
            fenetre.Padding = new System.Windows.Forms.Padding(3);
            fenetre.Size = taille_fenetre;
            fenetre.Text = location.Name;
            fenetre.UseVisualStyleBackColor = true;

            position_y = fenetre.Height - 40;

            label_recheche = new MetroFramework.Controls.MetroLabel();
            label_recheche.AutoSize = true;
            label_recheche.Location = new Point((int)(fenetre.Width *0.5 ),(int)(position_y - label_recheche.Size.Height*1.5));
            label_recheche.Name = "label_recherche";
            label_recheche.Text = "Sortie du planning";

            temps_sortie_planning = new DateTimePicker();
            temps_sortie_planning.CustomFormat = "HH:mm";
            temps_sortie_planning.Format = DateTimePickerFormat.Custom;
            temps_sortie_planning.Location = new Point((int)(fenetre.Width * 0.7), (int)(position_y - label_recheche.Size.Height * 1.5));
            temps_sortie_planning.Name = "debut_periode_date";
            temps_sortie_planning.ShowUpDown = true;
            temps_sortie_planning.Size = new Size(70, 25);
            temps_sortie_planning.Value = new DateTime(1900, 1, 1, 0, 0, 0);



            fenetre.Controls.Add(label_recheche);
            fenetre.Controls.Add(temps_sortie_planning);
        }

        public void Ajout_Bloc_Jour(int position_x, int position_y, DateTime date, Form form)
        {
            Bloc_Jour ajout = new Bloc_Jour(position_x, position_y, date,this);
            
            fenetre.Controls.Add(ajout.bouton_moins);
            fenetre.Controls.Add(ajout.bouton_plus);
            fenetre.Controls.Add(ajout.label_date);

            list_bloc_jour.Add(ajout);
        }

        public void Ajout_Bloc_Department(int position_x,int position_y, List<Department> list_dep)
        {
            bloc_dep = new Bloc_Department(position_x, position_y, fenetre, list_dep);
        }
    }

    
}
