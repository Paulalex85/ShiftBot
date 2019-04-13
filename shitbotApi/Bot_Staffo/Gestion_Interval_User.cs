using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot_Staffo
{
    class Gestion_Interval_User
    {

        public Gestion_Interval_User()
        {

        }

        public bool Check_Shift_In_Interval(Shifts shift, List<Time_Interval> list_interval)
        {
            foreach(Time_Interval caca in list_interval)
            {
                if(caca.Debut <= shift.Debut && caca.Fin >= shift.Fin)
                {
                    return true;
                }
            }
            return false;
        }

        public List<Time_Interval> Set_List_Interval_User(Bloc_Fenetre bloc, List<Inscription> list_inscriptions)
        {
            List<Time_Interval> list_time = new List<Time_Interval>();
            foreach (Bloc_Jour jour in bloc.list_bloc_jour)
            {
                foreach (Bloc_Shift_Interval interval in jour.list_interval)
                {
                    DateTime temps_debut = new DateTime();
                    temps_debut = temps_debut.AddYears(jour.date_jour.Year - 1);
                    temps_debut = temps_debut.AddMonths(jour.date_jour.Month - 1);
                    temps_debut = temps_debut.AddDays(jour.date_jour.Day - 1);
                    temps_debut = temps_debut.AddHours(interval.debut_periode.Value.Hour);
                    temps_debut = temps_debut.AddMinutes(interval.debut_periode.Value.Minute);

                    DateTime temps_fin = new DateTime();
                    temps_fin = temps_fin.AddYears(jour.date_jour.Year - 1);
                    temps_fin = temps_fin.AddMonths(jour.date_jour.Month - 1);
                    temps_fin = temps_fin.AddDays(jour.date_jour.Day - 1);
                    temps_fin = temps_fin.AddHours(interval.fin_periode.Value.Hour);
                    temps_fin = temps_fin.AddMinutes(interval.fin_periode.Value.Minute);

                    list_time.Add(new Time_Interval(temps_debut, temps_fin));
                }
            }

            if (list_inscriptions.Count > 0)
            {
                list_time = Change_List_Interval_Inscription(list_time, list_inscriptions);
            }
            return list_time;
        }


        //supprime de la liste interval ceux qui sont en conflit avec les inscriptions deja réalisés 
        public List<Time_Interval> Change_List_Interval_Inscription(List<Time_Interval> list_interval,List<Inscription> list_inscriptions)
        {
            foreach (Inscription inscription in list_inscriptions)
            {
                for (int i = list_interval.Count - 1; i >= 0; i--)
                {
                    if (list_interval[i].Debut == inscription.Debut && list_interval[i].Fin == inscription.Fin)
                    {
                        list_interval.RemoveAt(i);
                        break;
                    }
                    else if (list_interval[i].Debut == inscription.Debut && list_interval[i].Fin > inscription.Fin)
                    {
                        list_interval.Add(new Time_Interval(inscription.Fin, list_interval[i].Fin));
                        list_interval.RemoveAt(i);
                        break;
                    }
                    else if (list_interval[i].Debut < inscription.Debut && list_interval[i].Fin == inscription.Fin)
                    {
                        list_interval.Add(new Time_Interval(list_interval[i].Debut, inscription.Debut));
                        list_interval.RemoveAt(i);
                        break;
                    }
                    else if (list_interval[i].Debut < inscription.Debut && list_interval[i].Fin > inscription.Fin)
                    {
                        list_interval.Add(new Time_Interval(list_interval[i].Debut, inscription.Debut));
                        list_interval.Add(new Time_Interval(inscription.Fin, list_interval[i].Fin));
                        list_interval.RemoveAt(i);
                        break;
                    }
                }
            }
            return list_interval;
        }

        public List<Time_Interval> Epuration_List_Interval_Day(List<Shifts> listShift, List<Time_Interval> list_interval, int day)
        {
            List<Time_Interval> new_list_interval = new List<Time_Interval>();

            foreach(Time_Interval interval in list_interval)
            {
                if (interval.Debut.Day == day)
                {
                    foreach (Shifts shift in listShift)
                    {
                        if (shift.Debut >= interval.Debut && shift.Fin <= interval.Fin)
                        {
                            new_list_interval.Add(interval);
                            break;
                        }
                    }
                }
                else
                {
                    new_list_interval.Add(interval);
                }
            }

            return new_list_interval;
        }

        public List<Shifts> Epuration_List_Shifts_Day(List<Shifts> listShift, List<Time_Interval> list_interval)
        {
            List<Shifts> listShifts = new List<Shifts>();

            foreach(Shifts shift in listShift)
            {
                if (Check_Shift_In_Interval(shift, list_interval))
                {
                    listShifts.Add(shift);
                }
            }
            return listShifts;
        }
    }
}
