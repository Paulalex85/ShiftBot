using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RestSharp;
using RestSharp.Authenticators;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using MetroFramework.Forms;

namespace Bot_Staffo
{
    public partial class Form1 : MetroForm
    {
        const bool DEBUG = false;
        const string DECIM_PI = "14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127372458700660631558817488152092096282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912983367336244065664308602139494639522473719070217986094370277053921717629317675238467481846766940513200056812714526356082778577134275778960917363717872146844090122495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859502445945534690830264252230825334468503526193118817101000313783875288658753320838142061717766914730359825349042875546873115956286388235378759375195778185778053217122680661300192787661119590921642019893809525720106548586327886593615338182796823030195203530185296899577362259941389124972177528347913151557485724245415069595082953311686172785588907509838175463746493931925506040092770167113900984882401285836160356370766010471018194295559619894676783744944825537977472684710404753464620804668425906949129331367702898915210475216205696602405803815019351125338243003558764024749647326391419927260426992279678235478163600934172164121992458631503028618297455570674983850549458858692699569092721079750930295532116534498720275596023648066549911988183479775356636980742654252786255181841757467289097777279380008164706001614524919217321721477235014144197356854816136115735255213347574184946843852332390739414333454776241686251898356948556209921922218427255025425688767179049460165346680498862723279178608578438382796797668145410095388378636095068006422512520511739298489608412848862694560424196528502221066118630674427862203919494504712371378696095636437191728746776465757396241389086583264599581339047802759009946576407895126946839835259570982582262052248940772671947826848260147699090264013639443745530506820349625245174939965143142980919065925093722169646151570985838741059788595977297549893016175392846813826868386894277415599185592524595395943104997252468084598727364469584865383673622262609912460805124388439045124413654976278079771569143599770012961608944169486855584840635342207222582848864815845602850";

        int index_pi = 10;

        private List<Locations> list_location = new List<Locations>();
        private List<Schedules> list_schedules = new List<Schedules>();
        private List<Shifts> list_shifts = new List<Shifts>();
        private List<Users> list_users = new List<Users>();
        private List<Inscription> list_incriptions = new List<Inscription>();

        private Users info_utilisateur = new Users();

        private string password = "";
        private string username = "";
        string user_agent = "";
        private string string1, string2, string3, string4, string5, string6;

        Bloc_Reservation bloc_location;

        bool procedure_recherche_fini = true;

        private string cle_client = "ck_96344a223467f8c5cf585b4da88f7ca8a7ed4b04";
        private string secret_client = "cs_ac6a70bc3c010b5db506e7f4802f6aa4fdae0b66";

        StreamWriter writer;

        string log_path = "";
        DateTime date_fin_sub;
        bool sub_ok = false;
        DateTime time_api;
        int nb_shift_traite;
        int progress_bar_value_avant_assign;

        RestClient client_w = new RestClient("https://shiftbot.fr/");
        RestClient client;
        RestClient client_t = new RestClient("https://script.google.com/macros/s/AKfycbyd5AcbAnWi2Yn0xhFRbyzS4qMq1VucMVgVvhul5XqS9HkAyJY/exec");
        

        string id_customer;

        Stopwatch temps_prochain_ping = new Stopwatch();
        Stopwatch watch = new Stopwatch();

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
                nb_heures += inscription.Temps.Minutes * 100 / 60;
            }

            label14.Text = "Nombre de shifts inscrit : " + nb_shift;
            label13.Text = "Nombre d'heures inscrit : " + nb_heures ;

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
                    list_date_fin_sub.Add((DateTime)o["end_date"]);
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

        #region Async
        private void Ajout_Text_Async(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(Ajout_Text_Async);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                textBox2.AppendText(text);
                textBox2.AppendText(Environment.NewLine);

                using (StreamWriter w = File.AppendText(log_path))
                {
                    w.WriteLine(Get_PI_DECIM() + " " + DateTime.Now + " : " + text);
                }
                Update_Information_Inscription();
            }
        }

        delegate void SetTextCallback(string text);

        private void Perform_ProgressBar_Async()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.progressBar1.InvokeRequired)
            {
                StepProgressCallback d = new StepProgressCallback(Perform_ProgressBar_Async);
                this.Invoke(d, new object[] {  });
            }
            else
            {
                float pourcentage_realise = (float)nb_shift_traite / list_shifts.Count;
                progressBar1.Value = progress_bar_value_avant_assign + (int)((progressBar1.Maximum - progress_bar_value_avant_assign) * pourcentage_realise);
            }
        }

        delegate void StepProgressCallback();

#endregion

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
            Ajout_Log("Click Bouton Lancement recherche");
            Ajout_Log("Parametres : " + comboBox1.SelectedItem.ToString());
            if (radioButton1.Checked)
            {
                Ajout_Log("Type : sortie planning");
            }
            else
            {
                Ajout_Log("Type : Recuperation");
            }

            Ajout_Log("Parametres Temps : ");
            foreach (Bloc_Fenetre caca in bloc_location.list_fenetre)
            {
                if (caca.location.Id_Location == (comboBox1.SelectedItem as Locations).Id_Location)
                {
                    foreach (Bloc_Jour jour in caca.list_bloc_jour)
                    {
                        foreach (Bloc_Shift_interval interval in jour.list_interval)
                        {
                            Ajout_Log(interval.debut_periode + " - " + interval.fin_periode);
                        }
                    }
                    break;
                }
            }

            Active_Desactive_Recherche();
        }

        private void Active_Desactive_Recherche()
        {

            if (timer_recherche.Enabled)
            {
                //DESACTIVE LA RECHERCHE
                timer_recherche.Enabled = false;
                timer_update_countdown.Enabled = false;
                panel1.Enabled = true;
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
                    panel1.Enabled = false;
                    timer_recherche.Enabled = true;
                    timer_update_countdown.Enabled = true;
                    metroButton3.Text = "Stop Recherche";
                    Ajout_Log("Lance Recherche");
                    temps_prochain_ping.Start();
                    timer_async_fin_assign.Enabled = true;
                    watch = Stopwatch.StartNew();
                    progressBar1.Visible = true;
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
            foreach (Bloc_Fenetre caca in bloc_location.list_fenetre)
            {
                nb_location++;
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
                    if (radioButton2.Checked) // récupération shift
                    {
                        timer_recherche.Interval = 30000; // 30 secondes
                        progressBar1.Visible = false;
                    }
                    else
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
                    }

                    if (procedure_recherche_fini)
                    {
                        procedure_recherche_fini = false;
                        if (radioButton1.Checked)//sortie planning
                        {
                            Get_Shedules();
                            progressBar1.PerformStep();
                            Get_Shifts();
                            progressBar1.PerformStep();
                            Assignation_Shifts();


                            if (list_shifts.Count > 0)
                            {
                                Active_Desactive_Recherche();
                                Ajout_Log("Fin en cours");
                            }

                            procedure_recherche_fini = true;
                        }
                        else
                        {
                            if(list_schedules.Count <=0)
                            {
                                Get_Shedules();
                            }
                            Get_Shifts();
                            Assignation_Shifts();
                            procedure_recherche_fini = true;
                        }
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
            Ajout_Log("Lance Recuperation donnees users");
            try
            {
                list_users = new List<Users>();
                var request5 = new RestRequest("locations/" + list_location[0].Id_Location + "/users.json", Method.GET);
                request5.AddHeader("Content-type", "application/json");
                request5.AddHeader("User-Agent", user_agent);
                request5.RequestFormat = DataFormat.Json;
                var response5 = client.Execute(request5);

                JArray obj4 = JArray.Parse(response5.Content);
                foreach (JObject o in obj4.Children<JObject>())
                {
                    Users ajout = new Users();
                    ajout.Id = (string)o["id"];
                    ajout.Nom = (string)o["last_name"];
                    ajout.Prenom = (string)o["first_name"];
                    ajout.Email = (string)o["email"];
                    list_users.Add(ajout);
                }

                for (int i = 0; i < list_users.Count; i++)
                {
                    if (list_users[i].Email == username)
                    {
                        info_utilisateur = list_users[i];
                        /*label3.Text = string3 + info_utilisateur.Email;
                        label4.Text = string4 + info_utilisateur.Nom;
                        label5.Text = string5 + info_utilisateur.Prenom;
                        label6.Text = string6 + info_utilisateur.Id;*/
                        Ajout_Log("Mail : "+ info_utilisateur.Email);
                        Ajout_Log("Nom : " + info_utilisateur.Nom);
                        Ajout_Log("Prenom : "+ info_utilisateur.Prenom);
                        Ajout_Log("Id : " + info_utilisateur.Id);
                        Ajout_Text(false, "Connexion réalisé avec succes");
                        break;
                    }
                }
            }
            catch
            {
                Ajout_Text(false, "Problème de connexion : Verifier vos identifiants");
            }
        }

        private void timer_async_fin_assign_Tick(object sender, EventArgs e)
        {
            if (radioButton1.Checked && nb_shift_traite == list_shifts.Count)
            {
                Fin_Procedure_Inscription();
            }
        }

        private void Fin_Procedure_Inscription()
        {
            timer_async_fin_assign.Enabled = false;
            progressBar1.Visible = false;
            progressBar1.Value = 0;
            label10.Text = "Temps de la procedure: " + watch.ElapsedMilliseconds.ToString() + " ms";
            watch.Stop();
            if(nb_shift_traite > 0)
            {
                Ajout_Text_Async("Fin de la procédure d'inscription");

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

        public void Get_Shifts()
        {
            if (sub_ok)
            {
                try
                {
                    list_shifts = new List<Shifts>();
                    for (int i = 0; i < bloc_location.list_fenetre.Count; i++)
                    {
                        if (bloc_location.list_fenetre[i].recherche_en_cours && bloc_location.list_fenetre[i].schedule != null)
                        {

                            var request4 = new RestRequest("schedules/"
                                + bloc_location.list_fenetre[i].schedule.Id +
                                "/shifts.json", Method.GET);
                            request4.AddHeader("Content-type", "application/json");
                            request4.AddHeader("User-Agent", user_agent);
                            request4.RequestFormat = DataFormat.Json;
                            var response4 = client.Execute(request4);

                            JArray obj5 = JArray.Parse(response4.Content);
                            foreach (JObject o in obj5.Children<JObject>())
                            {
                                Shifts ajout = new Shifts();
                                ajout.Id = (string)o["id"];
                                ajout.Full = (bool)o["full"];
                                ajout.Debut = (DateTime)o["starts_at"];
                                ajout.Fin = (DateTime)o["ends_at"];
                                foreach (Bloc_Jour jour in bloc_location.list_fenetre[i].list_bloc_jour)
                                {
                                    foreach (Bloc_Shift_interval interval in jour.list_interval)
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
                                        
                                        if (ajout.Debut >= temps_debut && ajout.Fin <= temps_fin)
                                        {
                                            list_shifts.Add(ajout);
                                            if (ajout.Full)
                                            {
                                                Ajout_Text(false, "Shift " + ajout.Debut + " - " + ajout.Fin + " Statut : Complet");
                                            }
                                            else
                                            {
                                                Ajout_Text(false, "Shift " + ajout.Debut + " - " + ajout.Fin + " Statut : Libre");
                                            }
                                        }
                                    }
                                }
                            }
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
        }

        public void Verification_Fin_Procedure_Inscription()
        {
            if (nb_shift_traite != list_shifts.Count)
            {
                nb_shift_traite++;
                Perform_ProgressBar_Async();
            }
        }

        #region Assignation
        public void Assignation_Async(Shifts shift)
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
            //var response = client.Execute(request);
            client.ExecuteAsync(request, response =>
            {
                JObject o = JObject.Parse(response.Content);
                if (o["errors"] == null && (string)o["state"] == "assigned")
                {
                    Inscription ajout = new Inscription();
                    ajout.Debut = (DateTime)o["shift_starts_at"];
                    ajout.Fin = (DateTime)o["shift_ends_at"];
                    ajout.Temps = ajout.Fin - ajout.Debut;
                    list_incriptions.Add(ajout);
                    Ajout_Text_Async(shift.Debut.DayOfWeek + " - " + shift.Debut.Hour +
                        ":" + shift.Debut.Minute + " - " + shift.Fin.Hour + ":" + shift.Fin.Minute + " = Assign !");
                    Verification_Fin_Procedure_Inscription();
                }
                else
                {
                    Ajout_Text_Async(shift.Debut.DayOfWeek + " - " + shift.Debut.Hour +
                        ":" + shift.Debut.Minute + " - " + shift.Fin.Hour + ":" + shift.Fin.Minute + " : Inscription Impossible : Complet !");
                    Verification_Fin_Procedure_Inscription();
                }
            });
        }


        public void Assignation_Shifts()
        {
            if (sub_ok)
            {
                progress_bar_value_avant_assign = progressBar1.Value;
                nb_shift_traite = 0;
                list_incriptions = new List<Inscription>();
                for (int i = 0; i < list_shifts.Count; i++)
                {
                    if (list_shifts[i].Full == false)//TODO METTRE FAUX
                    {
                        Assignation_Async(list_shifts[i]);   
                    }

                    else
                    {
                        Ajout_Text(false, list_shifts[i].Debut.DayOfWeek + " - " + list_shifts[i].Debut.Hour +
                                    ":" + list_shifts[i].Debut.Minute + " - " + list_shifts[i].Fin.Hour + ":" + list_shifts[i].Fin.Minute + " = Full");
                        
                        Verification_Fin_Procedure_Inscription();
                    }
                }
            }
            else
            {
                Ajout_Text(false, "Vous etes pas abonné, nous ne pouvons pas vous inscrire");
            }
        }
        #endregion

        /*public void Optimisation_Shift(List<Shifts> list_shift_to_opti)
        {
            Solver solver = Solver.CreateSolver("ShiftOpti", "CBC_MIXED_INTEGER_PROGRAMMING");

            int nb_shift = list_shift_to_opti.Count;
            int[,] array_shift = new int[nb_shift, 2];
            Variable[,] array_bool = new Variable[nb_shift,2];

            Objective objective = solver.Objective();

            for (int i = 1; i < nb_shift; i++)
            {
                array_shift[i, 0] = list_shift_to_opti[i].Debut.Hour * 60 + list_shift_to_opti[i].Debut.Minute;
                array_shift[i, 1] = list_shift_to_opti[i].Fin.Hour * 60 + list_shift_to_opti[i].Fin.Minute;
                array_bool[i, 0] = solver.MakeBoolVar(list_shift_to_opti[i].Debut.Hour + ":" + list_shift_to_opti[i].Debut.Minute + " - " + list_shift_to_opti[i].Fin.Hour + ":" + list_shift_to_opti[i].Fin.Minute);
                //array_bool[i] = solver.MakeIntVar(0.0, 1.0, list_shift_to_opti[i].Debut.Hour + ":" + list_shift_to_opti[i].Debut.Minute + " - " + list_shift_to_opti[i].Fin.Hour + ":" + list_shift_to_opti[i].Fin.Minute);
                solver.ma
                //objective.SetCoefficient(array_bool[i], 1);
            }
            solver.Maximize(solver)

        }*/

    }

    #region classLocations
    public class Locations
    {
        public string Name;
        public string Id_Location;

        public Locations (string nom, string id)
        {
            this.Name = nom;
            this.Id_Location = id;
        }

        public override string ToString()
        {
            // Generates the text shown in the combo box
            return Name;
        }
    }
    #endregion

    public class Department
    {
        public string Name;
        public string Id_Department;

        public Department(string nom, string id)
        {
            this.Name = nom;
            this.Id_Department = id;
        }
    }

    public class Schedules
    {
        public string Id;
        public string State;
        public Locations location;
        public DateTime Debut;
        public DateTime Fin;
        public List<Shifts> list_shift_inscription;
    }

    public class Shifts
    {
        public string Id;
        public bool Full;
        public DateTime Debut;
        public DateTime Fin;
    }

    public class Users
    {
        public string Id;
        public string Nom;
        public string Prenom;
        public string Email;
    }

    public class Inscription
    {
        public DateTime Debut;
        public DateTime Fin;
        public TimeSpan Temps;
    }

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

    public class Bloc_Jour
    {
        public DateTime date_jour;
        public MetroFramework.Controls.MetroLabel label_date;
        public List<Bloc_Shift_interval> list_interval = new List<Bloc_Shift_interval>();
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

            list_interval.Add(new Bloc_Shift_interval(bouton_moins.Location.X,
                bouton_moins.Location.Y + bouton_plus.Size.Height*2));
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
                list_interval.Add(new Bloc_Shift_interval(list_interval[0].debut_periode.Location.X,
                    list_interval[list_interval.Count - 1].debut_periode.Location.Y + (int)(list_interval[0].debut_periode.Height * 3)));

                fenetre.fenetre.Controls.Add(list_interval[list_interval.Count - 1].debut_periode);
                fenetre.fenetre.Controls.Add(list_interval[list_interval.Count - 1].fin_periode);
            }
        }

        public void Remove_Bloc_Shift()
        {
            if(list_interval.Count > 1)
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

    public class Bloc_Shift_interval
    {
        public DateTimePicker debut_periode;
        public DateTimePicker fin_periode;

        public Bloc_Shift_interval(int position_x, int position_y)
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
            fin_periode.Value = new DateTime(1900, 1,1, 23, 59, 0);
        }
    }
}
