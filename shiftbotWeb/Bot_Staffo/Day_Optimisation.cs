using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot_Staffo
{
    public class Day_Optimisation
    {
        public List<Shifts> list_shift_day;
        public List<Element_Optimisation> list_opt = new List<Element_Optimisation>();

        public Day_Optimisation(List<Shifts> shifts)
        {
            list_shift_day = new List<Shifts>(shifts);
            Initialisation();
        }

        private void Initialisation()
        {
            List<int> list_index = new List<int>();
            for (int i = 0; i < list_shift_day.Count; i++)
            {
                list_index.Add(i);
            }
            foreach (int[] caca in Get_Combinaison(list_index))
            {
                if (Possible_Combinaison(list_shift_day, caca))
                {
                    list_opt.Add(new Element_Optimisation(caca, Get_Nb_Minutes_Shifts(caca, list_shift_day), caca.Length));
                }
            }
        }

        public List<Shifts> Get_Best_Combinaison()
        {
            List<Shifts> list_return = new List<Shifts>();
            list_opt = list_opt.OrderByDescending(x => x.nb_heures).ThenBy(x => x.nb_shifts).ToList();
            foreach (int caca in list_opt[0].array_opti)
            {
                list_return.Add(list_shift_day[caca]);
            }
            return list_return;
        }

        public List<int[]> Get_Combinaison(List<int> num)
        {
            List<int[]> retour = new List<int[]>();
            double count = Math.Pow(2, num.Count);
            for (int i = 1; i < count; i++)
            {
                List<int> ajout = new List<int>();
                string str = Convert.ToString(i, 2).PadLeft(num.Count, '0');
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '1')
                    {
                        ajout.Add(num[j]);
                    }
                }
                retour.Add(ajout.ToArray());
            }
            return retour;
        }

        public bool Possible_Combinaison(List<Shifts> listShifts, int[] index_combi)
        {
            for (int i = 0; i < index_combi.Length - 1; i++)
            {
                for (int j = i + 1; j < index_combi.Length; j++)
                {
                    if (Chevauchement_Shift(listShifts[index_combi[i]], listShifts[index_combi[j]]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool Chevauchement_Shift(Shifts shift_1, Shifts shift_2)
        {
            if (shift_1.Debut < shift_2.Debut && shift_1.Fin > shift_2.Debut)
            {
                return true;
            }
            if (shift_1.Fin > shift_2.Debut && shift_1.Fin < shift_2.Debut)
            {
                return true;
            }
            if (shift_1.Debut >= shift_2.Debut && shift_1.Fin <= shift_2.Fin)
            {
                return true;
            }
            if (shift_1.Debut <= shift_2.Debut && shift_1.Fin >= shift_2.Fin)
            {
                return true;
            }

            return false;
        }

        public int Get_Nb_Minutes_Shifts(int[] list_index, List<Shifts> listShifts)
        {
            int nb_min = 0;

            foreach (int index in list_index)
            {
                nb_min += listShifts[index].Get_Duree_Minutes();
            }

            return nb_min;
        }
    }
}
