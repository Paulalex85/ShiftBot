using System;
using System.Collections.Generic;
using System.Drawing;
using RestSharp;
using RestSharp.Authenticators;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using MetroFramework.Forms;
using System.Threading;

namespace Bot_Staffo
{
    public partial class Form1 : MetroForm
    {
        const bool DEBUG = false;
        const string DECIM_PI = "14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127372458700660631558817488152092096282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912983367336244065664308602139494639522473719070217986094370277053921717629317675238467481846766940513200056812714526356082778577134275778960917363717872146844090122495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859502445945534690830264252230825334468503526193118817101000313783875288658753320838142061717766914730359825349042875546873115956286388235378759375195778185778053217122680661300192787661119590921642019893809525720106548586327886593615338182796823030195203530185296899577362259941389124972177528347913151557485724245415069595082953311686172785588907509838175463746493931925506040092770167113900984882401285836160356370766010471018194295559619894676783744944825537977472684710404753464620804668425906949129331367702898915210475216205696602405803815019351125338243003558764024749647326391419927260426992279678235478163600934172164121992458631503028618297455570674983850549458858692699569092721079750930295532116534498720275596023648066549911988183479775356636980742654252786255181841757467289097777279380008164706001614524919217321721477235014144197356854816136115735255213347574184946843852332390739414333454776241686251898356948556209921922218427255025425688767179049460165346680498862723279178608578438382796797668145410095388378636095068006422512520511739298489608412848862694560424196528502221066118630674427862203919494504712371378696095636437191728746776465757396241389086583264599581339047802759009946576407895126946839835259570982582262052248940772671947826848260147699090264013639443745530506820349625245174939965143142980919065925093722169646151570985838741059788595977297549893016175392846813826868386894277415599185592524595395943104997252468084598727364469584865383673622262609912460805124388439045124413654976278079771569143599770012961608944169486855584840635342207222582848864815845602850";

        int index_pi = 10;

        private List<Locations> list_location = new List<Locations>();
        private List<Schedules> list_schedules = new List<Schedules>();
        private static List<Shifts> list_shifts = new List<Shifts>();
        private List<Users> list_users = new List<Users>();
        private List<Inscription> list_incriptions = new List<Inscription>();
        private static List<Time_Interval> listInterval = new List<Time_Interval>();
        private static Gestion_Interval_User gestion_interval = new Gestion_Interval_User();

        private static Users info_utilisateur = new Users();

        private string password = "";
        private string username = "";
        string user_agent = "";
        private string string1, string2, string3, string4, string5, string6;

        Bloc_Reservation bloc_location;
        
        bool procedureShiftsComplet = false;
        static bool shiftComplet = true;

        private string cle_client = "ck_96344a223467f8c5cf585b4da88f7ca8a7ed4b04";
        private string secret_client = "cs_ac6a70bc3c010b5db506e7f4802f6aa4fdae0b66";

        StreamWriter writer;

        string log_path = "";
        DateTime date_fin_sub;
        bool sub_ok = false;
        DateTime time_api;

        RestClient client_w = new RestClient("https://shiftbot.fr/");
        RestClient client;
        RestClient client_t = new RestClient("https://script.google.com/macros/s/AKfycbyd5AcbAnWi2Yn0xhFRbyzS4qMq1VucMVgVvhul5XqS9HkAyJY/exec");
        

        string id_customer;

        Stopwatch temps_prochain_ping = new Stopwatch();
        Stopwatch tempsGetAssignation = new Stopwatch();
        Stopwatch tempsGetShifts = new Stopwatch();


        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {

            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            log_path = path + "/log.txt";
            if(!File.Exists(log_path))
            {
                writer = new StreamWriter(log_path);
                writer.Close();
            }

            Ajout_Log("Demarrage avec " + this.Text);

            string1 = "NOM VILLE : ";
            string2 = "ID LOCATION : ";
            string3 = "EMAIL : ";
            string4 = "NOM : ";
            string5 = "PRENOM : ";
            string6 = "ID USER : ";

            /*label1.Text = string1;
            label2.Text = string2;
            label3.Text = string3;
            label4.Text = string4;
            label5.Text = string5;
            label6.Text = string6;*/

            label11.Text = "Abonnement :";
            label14.Text = "Nombre de shifts inscrit : ";
            label13.Text = "Nombre d'heures inscrit :";

            bloc_location = new Bloc_Reservation(430, 80, new Size(880, 350));
            this.Controls.Add(bloc_location.tab_control);
            bloc_location.tab_control.ResumeLayout(false);

            dateTimePicker1.Value = DateTime.Now;
            Set_Valeur_Debut_Semaine();

            /*if(DEBUG)
            {
                button3.Enabled = true;
                button4.Enabled = true;
                button6.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
                button4.Enabled = false;
                button6.Enabled = false;
            }*/
        }

        private void metroButton1_Click(object sender, System.EventArgs e)
        {
            Ajout_Log("Click Bouton Connexion");

            username = metroTextBox2.Text;
            user_agent = username;
            password = metroTextBox3.Text;

            Ajout_Log("Mail = " + username);
            if(password == null || password == "")
            {
                Ajout_Log("Mot de passe vide");
            }
            else
            {
                Ajout_Log("Mot de passe renseigne");
            }

            client = new RestClient(metroTextBox1.Text);
            Get_Location();

            Get_User_Gorgeade();
            Get_Subscription_Gorgeade();
            Get_Time_API(false);
            Check_Sub_OK();


            Get_Data_User();
            Set_Fenetre_Schedule();
        }

        public void Update_Information_Inscription()
        {
            double nb_heures = 0.0;
            int nb_shift = 0;

            foreach(Inscription inscription in list_incriptions)
            {
                nb_shift++;
                nb_heures += inscription.Temps.Hours;
                nb_heures += inscription.Temps.Minutes * 100 / 6000;
            }

            label14.Text = "Nombre de shifts inscrit : " + nb_shift;
            label13.Text = "Nombre d'heures inscrit : " + nb_heures;

        }

        #region VerifSubscription
        public void Get_User_Gorgeade()
        {
            user_agent = username;
            try
            {
                client_w.Authenticator = new HttpBasicAuthenticator(cle_client, secret_client);
                var request = new RestRequest("wp-json/wc/v1/customers?email=" + username + "&consumer_key="+ cle_client +"&consumer_secret="+ secret_client +"&role=all", Method.GET);
                var response = client_w.Execute(request);

                JArray obj = JArray.Parse(response.Content);

                foreach (JObject o in obj.Children<JObject>())
                {
                    id_customer = (string)o["id"];
                }
                Ajout_Log("User Shiftbot recuperer");
            }
            catch
            {
                Ajout_Text(false,"Mauvais identifiants ShiftBot.fr");
            }
        }

        public void Get_Subscription_Gorgeade()
        {
            List<DateTime> list_date_fin_sub = new List<DateTime>();
            if (id_customer == null)
            {
                Ajout_Text(false, "Identifiants ShiftBot.fr incorrect");
            }
            else
            {
                client_w.Authenticator = new HttpBasicAuthenticator(cle_client, secret_client);
                var request = new RestRequest("wp-json/wc/v1/subscriptions?customer=" + id_customer + "&consumer_key="+ cle_client +"&consumer_secret="+ secret_client +"&role=all", Method.GET);
                var response = client_w.Execute(request);
                JArray obj = JArray.Parse(response.Content);

                foreach (JObject o in obj.Children<JObject>())
                {
                    if((String)o["status"] == "active")
                    {
                        list_date_fin_sub.Add((DateTime)o["end_date"]);
                    }
                }
                if (list_date_fin_sub.Count == 0)
                {
                    date_fin_sub = new DateTime();
                }
                else
                {
                    date_fin_sub = list_date_fin_sub[0];

                    for (int i = 1; i < list_date_fin_sub.Count; i++)
                    {
                        if (date_fin_sub < list_date_fin_sub[i])
                        {
                            date_fin_sub = list_date_fin_sub[i];
                            Ajout_Log("Recuperation sub ok");
                        }
                    }
                }
            }
        }

        public void Get_Time_API(bool async)
        {
            Ajout_Text(false, "Récupération de l'heure");
            var request = new RestRequest("", Method.GET);
            if (async)
            {
                client_t.ExecuteAsync(request, response =>
                {
                    JObject obj = JObject.Parse(response.Content);

                    time_api = (DateTime)obj["fulldate"];
                    time_api.AddHours(1);
                });
                Check_Sub_OK();
            }
            else
            {
                var response = client_t.Execute(request);
                JObject obj = JObject.Parse(response.Content);

                time_api = (DateTime)obj["fulldate"];
                time_api.AddHours(1);
            }
        }

        public void Check_Sub_OK()
        {
            if (time_api < date_fin_sub)
            {
                TimeSpan temps_restant = date_fin_sub - time_api;
                sub_ok = true;
                label11.Text = "Il vous reste : " + temps_restant.Days + " jours, " + temps_restant.Hours + " heures, " + temps_restant.Minutes + " minutes";
                label11.ForeColor = Color.Green;
                Ajout_Log("Sub encore valide = " + temps_restant);
            }
            else
            {
                sub_ok = false;
                label11.Text = "Vous n'etes pas abonné";
                label11.ForeColor = Color.Red;
                Ajout_Log("Plus abonné");
            }
        }

#endregion

        public void Get_Location()
        {
            user_agent = username;
            try
            {
                list_location = new List<Locations>();

                client.Authenticator = new HttpBasicAuthenticator(username, password);
                //DATA LOCATION
                var request2 = new RestRequest("locations.json", Method.GET);
                request2.AddHeader("Content-type", "application/json");
                request2.AddHeader("User-Agent", user_agent);
                request2.RequestFormat = DataFormat.Json;
                var response2 = client.Execute(request2);

                JArray obj2 = JArray.Parse(response2.Content);
                foreach (JObject o in obj2.Children<JObject>())
                {
                    Locations ajout = new Locations((string)o["name"], (string)o["id"]);
                    list_location.Add(ajout);
                }
                Ajout_Text(false, "Localisation résussi");
            }
            catch
            {
                Ajout_Text(false, "Probleme de connexion");
            }
        }

        public List<Department> Get_Department(string location)
        {
            List<Department> list_dep = new List<Department>();
            try
            {
                var request = new RestRequest("locations/" + location + "/departments.json", Method.GET);
                request.AddHeader("Content-type", "application/json");
                request.AddHeader("User-Agent", user_agent);
                request.RequestFormat = DataFormat.Json;
                var response = client.Execute(request);

                JArray obj = JArray.Parse(response.Content);
                foreach (JObject o in obj.Children<JObject>())
                {
                    Department ajout = new Department((string)o["name"], (string)o["id"]);
                    list_dep.Add(ajout);
                }
                Ajout_Text(false, "Department trouvé");
            }
            catch
            {
                Ajout_Text(false, "Probleme : pas de department pour " + location);
            }

            return list_dep;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            Set_Valeur_Debut_Semaine();
        }

        public void Set_Valeur_Debut_Semaine()
        {
            while (dateTimePicker1.Value.DayOfWeek != DayOfWeek.Monday)
            {
                dateTimePicker1.Value = dateTimePicker1.Value.AddDays(-1);
            }
            Ajout_Log("Changement date : " + dateTimePicker1.Value);
        }

        public void Ajout_Text(bool effacer, string text)
        {
            if (!effacer)
            {
                textBox2.AppendText(text);
                textBox2.AppendText(Environment.NewLine);
            }
            else
            {
                textBox2.Text = text;
            }

            using (StreamWriter w = File.AppendText(log_path))
            {
                w.WriteLine(Get_PI_DECIM() + " " + DateTime.Now +" : " + text);
            }
        }

        public void Ajout_Log(string text)
        {
            using (StreamWriter w = File.AppendText(log_path))
            {
                w.WriteLine(Get_PI_DECIM() + " " + DateTime.Now + " : " + text);
            }
        }

        public string Get_PI_DECIM()
        {
            string str_return = "";
            if(index_pi + 3 > DECIM_PI.Length)
            {
                int ajout_old = DECIM_PI.Length - index_pi;
                int ajout_new = 3 - ajout_old;
                str_return += DECIM_PI.Substring(index_pi, ajout_old);
                str_return += DECIM_PI.Substring(10, ajout_new);
                index_pi = 10 + ajout_new;
            }
            else
            {
                str_return += DECIM_PI.Substring(index_pi, 3);
                index_pi += 3;
            }
            return str_return;
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            foreach (Bloc_Fenetre caca in bloc_location.list_fenetre)
            {
                for (int i = 0; i < 7; i++)
                {
                    caca.list_bloc_jour[i].date_jour = dateTimePicker1.Value.AddDays(i);
                    caca.list_bloc_jour[i].Change_Jour_Label(caca.list_bloc_jour[i].date_jour);
                }
            }
            Ajout_Log("Click Bouton Changement Date");
        }

        public void Set_Fenetre_Schedule()
        {
            comboBox1.Items.Clear();

            foreach (Locations caca in list_location)
            {
                bloc_location.Reset_TabControl();
            }
            bloc_location.list_fenetre = new List<Bloc_Fenetre>();

            foreach (Locations caca in list_location)
            {
                Ajout_Log("Creation fenetre location : " + caca.Name);
                bloc_location.Ajout_Fenetre(caca);
                comboBox1.Items.Add(caca);
            }
            

            foreach (Bloc_Fenetre caca in bloc_location.list_fenetre)
            {
                int position_x = 0;
                for (int i = 0; i < 7; i++)
                {
                    position_x = 100 * i;
                    caca.Ajout_Bloc_Jour(100 * i, 20, dateTimePicker1.Value.AddDays(i), this);
                }
                position_x += 100;

                caca.Ajout_Bloc_Department(position_x, 20, Get_Department(caca.location.Id_Location));

            }
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (info_utilisateur.Id != null)
            {
                Ajout_Log("Click Bouton Lancement recherche");
                Ajout_Log("Parametres : " + comboBox1.SelectedItem.ToString());

                Ajout_Log("Parametres Temps : ");
                foreach (Bloc_Fenetre caca in bloc_location.list_fenetre)
                {
                    if (caca.location.Id_Location == (comboBox1.SelectedItem as Locations).Id_Location)
                    {
                        foreach (Bloc_Jour jour in caca.list_bloc_jour)
                        {
                            foreach (Bloc_Shift_Interval interval in jour.list_interval)
                            {
                                Ajout_Log(interval.debut_periode + " - " + interval.fin_periode);
                            }
                        }
                        break;
                    }
                }

                Active_Desactive_Recherche();
            }
            else
            {
                Ajout_Text(false, "Lancement de l inscription impossible : Probleme info Staffomatic");
            }
        }

        private void Active_Desactive_Recherche()
        {

            if (timer_recherche.Enabled)
            {
                //DESACTIVE LA RECHERCHE
                timer_recherche.Enabled = false;
                timer_update_countdown.Enabled = false;
                label15.Text = "";
                metroButton3.Text = "Lancer Recherche";
                Ajout_Log("Stop Recherche");
                Active_Desactive_Recherche_Location();
                temps_prochain_ping.Stop();
                temps_prochain_ping.Reset();
            }
            else
            {
                //LANCE RECHERCHE
                if (Active_Desactive_Recherche_Location())
                {
                    timer_recherche.Enabled = true;
                    timer_update_countdown.Enabled = true;
                    metroButton3.Text = "Stop Recherche";
                    Ajout_Log("Lance Recherche");
                    temps_prochain_ping.Start();
                    listInterval = new List<Time_Interval>();
                    Recherche_Timer();
                }
            }
        }

        private void timer_recherche_Tick(object sender, EventArgs e)
        {
            Recherche_Timer();
            temps_prochain_ping.Reset();
            temps_prochain_ping.Start();
        }

        private bool Active_Desactive_Recherche_Location()
        {
            int nb_erreur = 0;
            int nb_location = 0;
            bool pb_department = false;
            foreach (Bloc_Fenetre caca in bloc_location.list_fenetre)
            {
                nb_location++;
                bool is_empty = true;
                foreach(Department_Element jambon in caca.bloc_dep.list_department)
                {
                    if(jambon.check_dep.Checked)
                    {
                        is_empty = false;
                        break;
                    }
                }
                if(is_empty)
                {
                    pb_department = true;
                    break;
                }
                

                try
                {
                    if (caca.location.Id_Location == (comboBox1.SelectedItem as Locations).Id_Location)
                    {
                        if (caca.recherche_en_cours)
                        {
                            caca.recherche_en_cours = false;
                        }
                        else
                        {
                            caca.recherche_en_cours = true;
                        }
                    }
                }
                catch
                {
                    nb_erreur++;
                }
            }
            if(nb_erreur == nb_location)
            {
                Ajout_Text(false, "Probleme : Ajouter quel endroit effectuer l'inscription");
                return false;
            }
            if (pb_department)
            {
                Ajout_Text(false, "Probleme : Pas de department selectionné");
                return false;
            }
            return true;
        }

        private void timer_timeapi_Tick(object sender, EventArgs e)
        {
            Get_Time_API(true);
        }

        private void Recherche_Timer()
        {
            foreach (Bloc_Fenetre caca in bloc_location.list_fenetre)
            {
                if (caca.recherche_en_cours)
                {
                    int temps_seconde_total_sortie = (caca.temps_sortie_planning.Value.Hour * 3600) +
                         (caca.temps_sortie_planning.Value.Minute * 60) +
                         caca.temps_sortie_planning.Value.Second;

                    int temps_seconde_total_now = (DateTime.Now.Hour * 3600) +
                         (DateTime.Now.Minute * 60) +
                         DateTime.Now.Second;
                    int difference = temps_seconde_total_sortie - temps_seconde_total_now;

                    if (difference <= 30)//inferieur 30s
                    {
                        timer_recherche.Interval = 1000;
                    }
                    else if (difference <= 60) //inferieur 1min
                    {
                        timer_recherche.Interval = 2000;
                    }
                    else if (difference <= 300) //inferieur 5min
                    {
                        timer_recherche.Interval = 10000;
                    }
                    else
                    {
                        timer_recherche.Interval = 120000;
                    }

                    Ajout_Log("Sortie planning execution");
                    Get_Shedules();
                    Get_Shifts();
                    if (list_shifts.Count > 0)
                    {
                        Assignation_Shifts();
                    }
                    else if (procedureShiftsComplet)
                    {
                        Active_Desactive_Recherche();
                    }
                }
            }
        }

        private void timer_update_countdown_Tick(object sender, EventArgs e)
        {
            label15.Text = "Prochaine recherche dans " + ((timer_recherche.Interval - temps_prochain_ping.ElapsedMilliseconds) / 1000) + "s";
        }

        public void Get_Data_User()
        { //RECUPERE DATA DE USER
            user_agent = username;
            bool connexionRealise = false;
            bool nextPage = false;
            int currentPageApi = 1;
            Ajout_Log("Lance Recuperation donnees users");
            try
            {
                while (!connexionRealise)
                {
                    list_users = new List<Users>();
                    RestRequest request5;
                    if (nextPage)
                    {
                        request5 = new RestRequest("locations/" + list_location[0].Id_Location + "/users.json?page=" + currentPageApi, Method.GET);
                    }
                    else
                    {
                        request5 = new RestRequest("locations/" + list_location[0].Id_Location + "/users.json", Method.GET);
                    }

                    request5.AddHeader("Content-type", "application/json");
                    request5.AddHeader("User-Agent", user_agent);
                    request5.RequestFormat = DataFormat.Json;
                    var response5 = client.Execute(request5);

                    JArray obj4 = JArray.Parse(response5.Content);
                    if (obj4.Count > 0)
                    {
                        foreach (JObject o in obj4.Children<JObject>())
                        {
                            Users ajout = new Users();
                            ajout.Id = (string)o["id"];
                            ajout.Nom = (string)o["last_name"];
                            ajout.Prenom = (string)o["first_name"];
                            ajout.Email = (string)o["email"];
                            list_users.Add(ajout);
                        }
                        if (list_users.Count > 0)
                        {
                            Users u = list_users.Find(x => x.Email == username);
                            if (u != null && u.Id != null)
                            {
                                info_utilisateur = u;
                                Ajout_Log("Mail : " + info_utilisateur.Email);
                                Ajout_Log("Nom : " + info_utilisateur.Nom);
                                Ajout_Log("Prenom : " + info_utilisateur.Prenom);
                                Ajout_Log("Id : " + info_utilisateur.Id);
                                connexionRealise = true;
                                nextPage = false;
                                Ajout_Text(false, "Connexion réalisé avec succes");
                            }
                            else
                            {
                                currentPageApi++;
                                nextPage = true;
                                Ajout_Text(false, "Recherche de l utilisateur en cours, page : " + currentPageApi);
                            }
                        }
                        else
                        {
                            Ajout_Text(false, "Probleme avec les infos de Staffomatic");
                        }
                    }
                    else
                    {
                        connexionRealise = true;
                        Ajout_Text(false, "Impossible de vous trouver dans la base de Staffomatic");
                    }
                }
            }
            catch(Exception e)
            {
                Ajout_Log(e.Message);
                Ajout_Text(false, "Problème de connexion : Verifier vos identifiants");
            }
        }

        public void Get_Shedules()
        {
            if (sub_ok)
            {
                try
                {
                    for (int i = 0; i < bloc_location.list_fenetre.Count; i++)
                    {
                        bloc_location.list_fenetre[i].schedule = null;
                        if (bloc_location.list_fenetre[i].recherche_en_cours)
                        {
                            list_schedules = new List<Schedules>();
                            var request3 = new RestRequest("locations/"
                            + bloc_location.list_fenetre[i].location.Id_Location +
                            "/schedules.json", Method.GET);

                            request3.AddHeader("Content-type", "application/json");
                            request3.AddHeader("User-Agent", user_agent);
                            request3.RequestFormat = DataFormat.Json;
                            var response3 = client.Execute(request3);

                            JArray obj3 = JArray.Parse(response3.Content);
                            foreach (JObject o in obj3.Children<JObject>())
                            {
                                Schedules ajout = new Schedules();
                                ajout.Id = (string)o["id"];
                                ajout.State = (string)o["state"]; //published
                                ajout.Debut = (DateTime)o["bop"];
                                ajout.Fin = (DateTime)o["eop"];
                                ajout.location = list_location[i];
                                list_schedules.Add(ajout);
                            }
                            DateTime date_debut_semaine = dateTimePicker1.Value;
                            for (int h = 0; h < list_schedules.Count; h++)
                            {
                                if (list_schedules[h].Debut >= dateTimePicker1.Value.Date && list_schedules[h].State == "published")
                                {

                                    bloc_location.list_fenetre[i].schedule = list_schedules[h];
                                    Ajout_Text(false, "Schedule trouvé");
                                    
                                    break;
                                }
                            }
                            if (bloc_location.list_fenetre[i].schedule == null)
                            {
                                Ajout_Text(false, DateTime.Now.ToString() + " : Pas de planning de sortie");
                            }
                        }
                    }
                }
                catch
                {
                    Ajout_Text(false, DateTime.Now.ToString() + " : Probleme, verifier la connexion");
                }
            }
            else
            {
                Ajout_Text(false, "Vous etes pas abonné, nous ne pouvons pas vous inscrire");
            }
        }

        #region Shifts
        public void Get_Shifts()
        {
            Ajout_Log("Lancement méthode get shift");
            tempsGetShifts = Stopwatch.StartNew();
            shiftComplet = true;
            bool scheduleFounded = false;
            
            if (sub_ok)
            {
                try
                {
                    list_shifts = new List<Shifts>();
                    for (int i = 0; i < bloc_location.list_fenetre.Count; i++)
                    {
                        if (bloc_location.list_fenetre[i].recherche_en_cours && bloc_location.list_fenetre[i].schedule != null)
                        {
                            Ajout_Text(false, "Recuperation des shifts en cours");
                            listInterval = gestion_interval.Set_List_Interval_User(bloc_location.list_fenetre[i], list_incriptions);
                            bool finPage = false;
                            int currentPageApi = 1;
                            scheduleFounded = true;

                            Shift_API_return apiInfos = Get_Shift_API("schedules/"
                                     + bloc_location.list_fenetre[i].schedule.Id +
                                     "/shifts.json" + String_Department_API(bloc_location.list_fenetre[i]) + "&page=" + currentPageApi);
                            currentPageApi++;
                            if (shiftComplet && !apiInfos.shiftComplet)
                            {
                                shiftComplet = false;
                            }
                            finPage = apiInfos.finPage;

                            if (apiInfos.presenceHeaderPage)
                            {
                                if (apiInfos.pagesMax > 1)
                                {
                                    Ajout_Log("Lancement des threads shifts, nombre : " + (apiInfos.pagesMax - 1));
                                    Thread[] arrayThreads = new Thread[apiInfos.pagesMax - 1];

                                    for (int j = 0; j < arrayThreads.Length; j++)
                                    {
                                        String p = currentPageApi.ToString();
                                        arrayThreads[j] = new Thread(() => Shift_API_Thread("schedules/"
                                         + bloc_location.list_fenetre[i].schedule.Id +
                                         "/shifts.json" + String_Department_API(bloc_location.list_fenetre[i]) + "&page=" + p));
                                        arrayThreads[j].Start();
                                        currentPageApi++;
                                    }

                                    bool threadRunning = true;
                                    while (threadRunning)
                                    {
                                        foreach (Thread t in arrayThreads)
                                        {
                                            if (t.IsAlive)
                                            {
                                                threadRunning = true;
                                                break;
                                            }
                                            else
                                            {
                                                threadRunning = false;
                                            }
                                        }
                                    }
                                }
                                Ajout_Text(false, "Tout les shifts recupérés");
                            }
                            else
                            {
                                while (!finPage)
                                {
                                    apiInfos = Get_Shift_API("schedules/"
                                     + bloc_location.list_fenetre[i].schedule.Id +
                                     "/shifts.json" + String_Department_API(bloc_location.list_fenetre[i]) + "&page=" + currentPageApi);
                                    
                                    currentPageApi++;

                                    if(shiftComplet && !apiInfos.shiftComplet)
                                    {
                                        shiftComplet = false;
                                    }
                                    finPage = apiInfos.finPage;
                                }
                            }

                            break;
                        }
                        else if(bloc_location.list_fenetre[i].recherche_en_cours)
                        {
                            Ajout_Text(false, "Pas de planning pour " + bloc_location.list_fenetre[i].location.Name);
                        }
                    }
                }
                catch
                {
                    Ajout_Text(false, "Probleme de connexion");
                }
            }
            else
            {
                Ajout_Text(false, "Vous etes pas abonné, nous ne pouvons pas vous inscrire");
            }

            tempsGetShifts.Stop();
            if (shiftComplet && scheduleFounded)
            {
                procedureShiftsComplet = true;
                Ajout_Text(false, "Tout les shifts sont complets - Fin de la procédure d'inscription");
            }

            if (scheduleFounded)
            {
                Bilan_Log_Shifts_Get();
                Ajout_Text(false, "Temps recuperation shifts : " + tempsGetShifts.ElapsedMilliseconds.ToString() + " ms");
            }
        }

        public void Bilan_Log_Shifts_Get()
        {
            Ajout_Text(false, "===== Shifts Libres =====");
            Ajout_Text(false, "Nombre : " + list_shifts.Count);
        }

        public void Shift_API_Thread(string URI)
        {
            RestRequest request;
            request = new RestRequest(URI, Method.GET);

            request.AddHeader("Content-type", "application/json");
            request.AddHeader("User-Agent", user_agent);
            request.RequestFormat = DataFormat.Json;
            var response = client.Execute(request);

            JArray obj = JArray.Parse(response.Content);
            if (obj.Count > 0)
            {
                foreach (JObject o in obj.Children<JObject>())
                {
                    Shifts ajout = new Shifts();
                    try
                    {
                        ajout.Id = (string)o["id"];
                        ajout.Full = (bool)o["full"];
                        ajout.Debut = (DateTime)o["starts_at"];
                        ajout.Fin = (DateTime)o["ends_at"];
                        
                        if (gestion_interval.Check_Shift_In_Interval(ajout, listInterval) && !ajout.Full)
                        {
                            if (!list_shifts.Exists(x => x.Id == ajout.Id))
                            {
                                list_shifts.Add(ajout);
                                shiftComplet = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        //return si il y a les header page ou non
        public Shift_API_return Get_Shift_API(string URI)
        {
            Shift_API_return apiReturn = new Shift_API_return();

            int page = -1;
            int pages = -1;
            RestRequest request;
            request = new RestRequest(URI, Method.GET);

            request.AddHeader("Content-type", "application/json");
            request.AddHeader("User-Agent", user_agent);
            request.RequestFormat = DataFormat.Json;
            var response = client.Execute(request);

            foreach (var param in response.Headers)
            {
                if (param.Name == "Page")
                {
                    page = int.Parse(param.Value.ToString());
                }
                else if(param.Name == "Pages")
                {
                    pages = int.Parse(param.Value.ToString());
                }
            }

            if(page >0 && pages > 0)
            {
                apiReturn.presenceHeaderPage = true;
                apiReturn.pagesMax = pages;
            }

            Ajout_Log("Nombre pages shifts: " + pages);

            JArray obj = JArray.Parse(response.Content);
            if (obj.Count > 0)
            {
                foreach (JObject o in obj.Children<JObject>())
                {
                    Shifts ajout = new Shifts();
                    try
                    {
                        ajout.Id = (string)o["id"];
                        ajout.Full = (bool)o["full"];
                        ajout.Debut = (DateTime)o["starts_at"];
                        ajout.Fin = (DateTime)o["ends_at"];

                        if (gestion_interval.Check_Shift_In_Interval(ajout, listInterval) && !ajout.Full)
                        {
                            if (!list_shifts.Exists(x => x.Id == ajout.Id))
                            {
                                list_shifts.Add(ajout);
                                apiReturn.shiftComplet = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Ajout_Log("Erreur probleme shift API");
                    }
                }
            }
            else
            {
                apiReturn.finPage = true;
                Ajout_Text(false, "Tout les shifts recupérés");
            }

            return apiReturn;
        }

        public string String_Department_API(Bloc_Fenetre fenetre)
        {
            string retour = "";
            foreach (Department_Element caca in fenetre.bloc_dep.list_department)
            {
                if (caca.check_dep.Checked)
                {
                    if (retour == "")
                    {
                        retour = "?department_ids%5B%5D=" + caca.department.Id_Department;
                    }
                    else
                    {
                        retour += "&department_ids%5B%5D=" + caca.department.Id_Department;
                    }
                }
            }
            return retour;
        }

#endregion 
        
        #region Assignation
        


        public void Assignation_Shifts()
        {
            Ajout_Log("methode assignation shifts execution");
            tempsGetAssignation = Stopwatch.StartNew();
            if (sub_ok)
            {
                List<List<Shifts>> listShifts = GetListShiftsJour(list_shifts);
                Thread[] arrayThreads = new Thread[listShifts.Count];

                for (int i = 0; i < arrayThreads.Length; i++)
                {
                    arrayThreads[i] = new Thread(() => Thread_Jour_Assignation(listShifts[i], listShifts[i][0].Debut.Day));
                    arrayThreads[i].Start();
                }

                bool threadRunning = true;
                while (threadRunning)
                {
                    foreach (Thread t in arrayThreads)
                    {
                        if (t.IsAlive)
                        {
                            threadRunning = true;
                            break;
                        }
                        else
                        {
                            threadRunning = false;
                        }
                    }
                }
                Bilan_Assignation();
            }
            else
            {
                Ajout_Text(false, "Vous etes pas abonné, nous ne pouvons pas vous inscrire");
            }

            Ajout_Log("Fin methode assignation");
        }

        public void Bilan_Assignation()
        {
            tempsGetAssignation.Stop();
            Ajout_Text(false, "Temps assignation : " + tempsGetAssignation.ElapsedMilliseconds.ToString() + " ms");
            Ajout_Text(false, "===== Shifts Obtenus =====");

            foreach(Inscription i in list_incriptions)
            {
                Ajout_Text(false, i.Debut + " - " + i.Fin);
            }
            Ajout_Text(false, "Nombre : " + list_incriptions.Count);
            Ajout_Text(false, "Fin de l'inscription");

            Active_Desactive_Recherche();
            Update_Information_Inscription();

        }

        //Classe les shifts par jour 
        public List<List<Shifts>> GetListShiftsJour(List<Shifts> listShifts)
        {
            List<List<Shifts>> list = new List<List<Shifts>>();

            foreach(Shifts shift in listShifts)
            {
                bool added = false;
                for(int i = 0; i< list.Count; i++)
                {
                    if(list[i].Count > 0)
                    {
                        if (shift.Debut.Day == list[i][0].Debut.Day)
                        {
                            added = true;
                            list[i].Add(shift);
                            break;
                        }
                    }
                    else
                    {
                        added = true;
                        list[i].Add(shift);
                        break;
                    }
                    
                }
                if (!added)
                {
                    list.Add(new List<Shifts>());
                    list[list.Count - 1].Add(shift);
                }
            }

            return list;
        }

        //Thread de gestion de l'optimisation et de l'inscription pour 1 jour
        public void Thread_Jour_Assignation(List<Shifts> listShifts, int day)
        {
            List<Shifts> listShiftsDay = new List<Shifts>(listShifts);

            while(listShiftsDay.Count > 0)
            {
                //Determine les shifts a assigner
                List<Shifts> shiftSansDoublonsHeure = Get_List_Shifts_Sans_Doublons_Horaires(listShiftsDay);

                if (shiftSansDoublonsHeure.Count > 10)
                {
                    shiftSansDoublonsHeure.RemoveRange(10, shiftSansDoublonsHeure.Count - 10);
                }

                Day_Optimisation dayOptimisation = new Day_Optimisation(shiftSansDoublonsHeure);
                List<Shifts> listShiftsToAssign = dayOptimisation.Get_Best_Combinaison();
                

                foreach(Shifts shift in listShiftsToAssign)
                {
                    listShiftsDay.Remove(shift);
                }

                //Inscription

                Thread[] arrayThreads = new Thread[listShiftsToAssign.Count];

                for (int i = 0; i < arrayThreads.Length; i++)
                {
                    arrayThreads[i] = new Thread(() => Thread_Assignation(listShiftsToAssign[i]));
                    arrayThreads[i].Start();
                }

                bool threadRunning = true;
                while (threadRunning)
                {
                    foreach (Thread t in arrayThreads)
                    {
                        if (t.IsAlive)
                        {
                            threadRunning = true;
                            break;
                        }
                        else
                        {
                            threadRunning = false;
                        }
                    }
                }

                //Reduction des Interval de temps + shifts
                lock (listInterval)
                {
                    listInterval = gestion_interval.Change_List_Interval_Inscription(listInterval, list_incriptions);
                    listInterval = gestion_interval.Epuration_List_Interval_Day(listShiftsDay, listInterval,day);
                }
                listShiftsDay = gestion_interval.Epuration_List_Shifts_Day(listShiftsDay, listInterval);
            }
        }

        public List<Shifts> Get_List_Shifts_Sans_Doublons_Horaires(List<Shifts> listShifts)
        {
            List<Shifts> listSansDoublons = new List<Shifts>();

            foreach(Shifts shift in listShifts)
            {
                if(listSansDoublons.Count == 0 || !listSansDoublons.Exists(x => x.Debut == shift.Debut && x.Fin == shift.Fin))
                {
                    listSansDoublons.Add(shift);
                }
            }
            return listSansDoublons;
        }

        public void Thread_Assignation(Shifts shift)
        {
            JObject envoie_json =
                            new JObject(
                            new JProperty("user_id", int.Parse(info_utilisateur.Id)),
                            new JProperty("do", "assign"));

            var request = new RestRequest("shifts/" + shift.Id + "/applications.json", Method.POST);
            request.AddHeader("Content-type", "application/json");
            request.AddHeader("User-Agent", user_agent);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("application/json", envoie_json, ParameterType.RequestBody);
            var response = client.Execute(request);

            JObject o = JObject.Parse(response.Content);
            if (o["errors"] == null && (string)o["state"] == "assigned")
            {
                Inscription ajout = new Inscription();
                ajout.Debut = (DateTime)o["shift_starts_at"];
                ajout.Fin = (DateTime)o["shift_ends_at"];
                ajout.Temps = ajout.Fin - ajout.Debut;
                list_incriptions.Add(ajout);
            }
        }

        #endregion

    }
}
