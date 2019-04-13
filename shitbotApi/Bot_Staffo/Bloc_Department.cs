using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bot_Staffo
{
    public class Bloc_Department
    {
        public MetroFramework.Controls.MetroLabel label_titre;
        public MetroFramework.Controls.MetroButton button_inverser;
        public List<Department_Element> list_department = new List<Department_Element>();
        public MetroFramework.Controls.MetroPanel panel_departement;
        public MetroFramework.Controls.MetroTabPage fenetre;

        public Bloc_Department(int position_x, int position_y, MetroFramework.Controls.MetroTabPage fenetre, List<Department> list_dep)
        {
            this.fenetre = fenetre;
            panel_departement = new MetroFramework.Controls.MetroPanel();
            label_titre = new MetroFramework.Controls.MetroLabel();
            button_inverser = new MetroFramework.Controls.MetroButton();

            panel_departement.BorderStyle = BorderStyle.FixedSingle;
            panel_departement.VerticalScrollbar = true;
            panel_departement.Location = new Point(position_x, position_y);
            panel_departement.Size = new Size(150, 230);
            panel_departement.AutoScroll = true;
            fenetre.Controls.Add(panel_departement);

            label_titre.Text = "Quartier";
            label_titre.Location = new Point(10, 10);
            label_titre.Size = new Size(60, 20);
            panel_departement.Controls.Add(label_titre);

            button_inverser.Text = "Inverser";
            button_inverser.Location = new Point(75, 10);
            button_inverser.Size = new Size(60, 20);
            panel_departement.Controls.Add(button_inverser);

            button_inverser.Click += Bouton_inverser_Click;


            for (int i = 0; i < list_dep.Count; i++)
            {
                Department_Element ajout = new Department_Element(list_dep[i], 10, 40 + 20 * i, panel_departement);
                list_department.Add(ajout);
            }
        }
        private void Bouton_inverser_Click(object sender, EventArgs e)
        {
            foreach (Department_Element elem in list_department)
            {
                if (elem.check_dep.Checked)
                {
                    elem.check_dep.Checked = false;
                }
                else
                {
                    elem.check_dep.Checked = true;
                }
            }
        }
    }
}
