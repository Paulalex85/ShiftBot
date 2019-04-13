using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot_Staffo
{
    public class Bloc_Jour
    {
        public DateTime date_jour;
        public MetroFramework.Controls.MetroLabel label_date;
        public List<Bloc_Shift_Interval> list_interval = new List<Bloc_Shift_Interval>();
        public MetroFramework.Controls.MetroButton bouton_plus;
        public MetroFramework.Controls.MetroButton bouton_moins;
        Bloc_Fenetre fenetre;

        public Bloc_Jour(int position_x, int position_y, DateTime date, Bloc_Fenetre fenetre)
        {
            date_jour = date;
            date_jour = date_jour.Date;
            this.fenetre = fenetre;

            bouton_moins = new MetroFramework.Controls.MetroButton();
            bouton_plus = new MetroFramework.Controls.MetroButton();
            label_date = new MetroFramework.Controls.MetroLabel();

            bouton_moins.Location = new Point(position_x, position_y + 30);
            bouton_moins.Name = "bouton_moins";
            bouton_moins.Size = new Size(28, 21);
            bouton_moins.Text = "-";
            bouton_moins.UseVisualStyleBackColor = true;

            bouton_plus.Location = new Point(position_x + (int)(bouton_moins.Size.Width * 1.5), position_y + 30);
            bouton_plus.Name = "bouton_plus";
            bouton_plus.Size = new Size(28, 21);
            bouton_plus.Text = "+";
            bouton_plus.UseVisualStyleBackColor = true;

            label_date.AutoSize = true;
            label_date.Location = new Point(position_x, position_y);
            label_date.Name = "label_date";
            label_date.Size = new Size(72, 13);
            Change_Jour_Label(date);

            list_interval.Add(new Bloc_Shift_Interval(bouton_moins.Location.X,
                bouton_moins.Location.Y + bouton_plus.Size.Height * 2));
            fenetre.fenetre.Controls.Add(list_interval[0].debut_periode);
            fenetre.fenetre.Controls.Add(list_interval[0].fin_periode);

            bouton_moins.Click += Bouton_moins_Click;
            bouton_plus.Click += Bouton_plus_Click;
        }

        public void Change_Jour_Label(DateTime date)
        {
            label_date.Text = date.Day + "/" + date.Month + "/" + date.Year;
        }

        private void Bouton_plus_Click(object sender, EventArgs e)
        {
            Ajout_Bloc_Shift();
        }

        private void Bouton_moins_Click(object sender, EventArgs e)
        {
            Remove_Bloc_Shift();
        }

        public void Ajout_Bloc_Shift()
        {
            if (list_interval.Count < 3)
            {
                list_interval.Add(new Bloc_Shift_Interval(list_interval[0].debut_periode.Location.X,
                    list_interval[list_interval.Count - 1].debut_periode.Location.Y + (int)(list_interval[0].debut_periode.Height * 3)));

                fenetre.fenetre.Controls.Add(list_interval[list_interval.Count - 1].debut_periode);
                fenetre.fenetre.Controls.Add(list_interval[list_interval.Count - 1].fin_periode);
            }
        }

        public void Remove_Bloc_Shift()
        {
            if (list_interval.Count > 1)
            {
                if (fenetre.fenetre.Controls.Contains(list_interval[list_interval.Count - 1].debut_periode) &&
                    fenetre.fenetre.Controls.Contains(list_interval[list_interval.Count - 1].fin_periode))
                {
                    fenetre.fenetre.Controls.Remove(list_interval[list_interval.Count - 1].debut_periode);
                    fenetre.fenetre.Controls.Remove(list_interval[list_interval.Count - 1].fin_periode);
                    list_interval[list_interval.Count - 1].fin_periode.Dispose();
                    list_interval[list_interval.Count - 1].debut_periode.Dispose();
                    list_interval.RemoveAt(list_interval.Count - 1);
                }
            }
        }
    }
}
