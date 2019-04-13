using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bot_Staffo
{
    public class Bloc_Shift_Interval
    {
        public DateTimePicker debut_periode;
        public DateTimePicker fin_periode;

        public Bloc_Shift_Interval(int position_x, int position_y)
        {
            debut_periode = new DateTimePicker();
            fin_periode = new DateTimePicker();

            debut_periode.CustomFormat = "HH:mm";
            debut_periode.Format = DateTimePickerFormat.Custom;
            debut_periode.Location = new Point(position_x, position_y);
            debut_periode.Name = "debut_periode_date";
            debut_periode.ShowUpDown = true;
            debut_periode.Size = new Size(55, 20);
            debut_periode.Value = new DateTime(1900, 1, 1, 0, 0, 0);

            fin_periode.CustomFormat = "HH:mm";
            fin_periode.Format = DateTimePickerFormat.Custom;
            fin_periode.Location = new Point(position_x, (int)(position_y + debut_periode.Size.Height * 1.2));
            fin_periode.Name = "fin_periode_date";
            fin_periode.ShowUpDown = true;
            fin_periode.Size = new Size(55, 20);
            fin_periode.Value = new DateTime(1900, 1, 1, 23, 59, 0);
        }
    }
}
