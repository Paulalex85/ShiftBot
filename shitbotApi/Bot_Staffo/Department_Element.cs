using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bot_Staffo
{
    public class Department_Element
    {
        public Department department;
        public MetroFramework.Controls.MetroCheckBox check_dep;
        public Department_Element(Department department, int position_x, int position_y, Panel panel_dep)
        {
            this.department = department;

            check_dep = new MetroFramework.Controls.MetroCheckBox();

            check_dep.Location = new Point(position_x, position_y);
            check_dep.Name = department.Name;
            check_dep.Text = department.Name;
            check_dep.Size = new Size(130, 15);
            check_dep.Checked = true;

            panel_dep.Controls.Add(check_dep);
        }
    }
}
