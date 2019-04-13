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
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;

namespace Bot_Staffo
{
    public partial class Form1 : MetroForm
    {
        const bool DEBUG = false;
        const string DECIM_PI = "14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127372458700660631558817488152092096282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912983367336244065664308602139494639522473719070217986094370277053921717629317675238467481846766940513200056812714526356082778577134275778960917363717872146844090122495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859502445945534690830264252230825334468503526193118817101000313783875288658753320838142061717766914730359825349042875546873115956286388235378759375195778185778053217122680661300192787661119590921642019893809525720106548586327886593615338182796823030195203530185296899577362259941389124972177528347913151557485724245415069595082953311686172785588907509838175463746493931925506040092770167113900984882401285836160356370766010471018194295559619894676783744944825537977472684710404753464620804668425906949129331367702898915210475216205696602405803815019351125338243003558764024749647326391419927260426992279678235478163600934172164121992458631503028618297455570674983850549458858692699569092721079750930295532116534498720275596023648066549911988183479775356636980742654252786255181841757467289097777279380008164706001614524919217321721477235014144197356854816136115735255213347574184946843852332390739414333454776241686251898356948556209921922218427255025425688767179049460165346680498862723279178608578438382796797668145410095388378636095068006422512520511739298489608412848862694560424196528502221066118630674427862203919494504712371378696095636437191728746776465757396241389086583264599581339047802759009946576407895126946839835259570982582262052248940772671947826848260147699090264013639443745530506820349625245174939965143142980919065925093722169646151570985838741059788595977297549893016175392846813826868386894277415599185592524595395943104997252468084598727364469584865383673622262609912460805124388439045124413654976278079771569143599770012961608944169486855584840635342207222582848864815845602850";

        int index_pi = 10;

        private static List<Shifts> list_shifts = new List<Shifts>();
        private List<Inscription> list_incriptions = new List<Inscription>();
        private static List<Time_Interval> listInterval = new List<Time_Interval>();
        private static Gestion_Interval_User gestion_interval = new Gestion_Interval_User();

        private string password = "";
        private string username = "";

        Bloc_Fenetre bloc_fenetre;

        private string cle_client = "ck_96344a223467f8c5cf585b4da88f7ca8a7ed4b04";
        private string secret_client = "cs_ac6a70bc3c010b5db506e7f4802f6aa4fdae0b66";

        StreamWriter writer;

        string log_path = "";
        DateTime date_fin_sub;
        bool sub_ok = false;
        DateTime time_api;

        RestClient client_w = new RestClient("https://shiftbot.fr/");
        RestClient client_t = new RestClient("https://script.google.com/macros/s/AKfycbyd5AcbAnWi2Yn0xhFRbyzS4qMq1VucMVgVvhul5XqS9HkAyJY/exec");

        string id_customer;

        public MetroFramework.Controls.MetroTabControl tab_control;
        
        String chemin = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        IWebDriver driver;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            log_path = path + "/log.txt";
            if (!File.Exists(log_path))
            {
                writer = new StreamWriter(log_path);
                writer.Close();
            }

            Ajout_Log("Demarrage avec " + this.Text);

            label11.Text = "Abonnement :";

            dateTimePicker1.Value = DateTime.Now;
            Set_Valeur_Debut_Semaine();
        }

        private void metroButton1_Click(object sender, System.EventArgs e)
        {
            Ajout_Log("Click Bouton Connexion");

            username = metroTextBox2.Text;
            password = metroTextBox3.Text;

            Ajout_Log("Mail = " + username);
            if (password == null || password == "")
            {
                Ajout_Text("Mot de passe vide"); 
            }
            else
            {
                Ajout_Text("Mot de passe renseigne");
            }

            Get_User_Gorgeade();
            Get_Subscription_Gorgeade();
            Get_Time_API(false);
            Check_Sub_OK();

            Set_Fenetre_Schedule();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            if (null != bloc_fenetre)
            {
                for (int i = 0; i < 7; i++)
                {
                    bloc_fenetre.list_bloc_jour[i].date_jour = dateTimePicker1.Value.AddDays(i);
                    bloc_fenetre.list_bloc_jour[i].Change_Jour_Label(bloc_fenetre.list_bloc_jour[i].date_jour);
                }
            }
            Ajout_Log("Click Bouton Changement Date");
        }

        #region VerifSubscription
        public void Get_User_Gorgeade()
        {
            try
            {
                client_w.Authenticator = new HttpBasicAuthenticator(cle_client, secret_client);
                var request = new RestRequest("wp-json/wc/v1/customers?email=" + username + "&consumer_key=" + cle_client + "&consumer_secret=" + secret_client + "&role=all", Method.GET);
                var response = client_w.Execute(request);

                JArray obj = JArray.Parse(response.Content);

                foreach (JObject o in obj.Children<JObject>())
                {
                    id_customer = (string)o["id"];
                }
                Ajout_Text("User Shiftbot ok");
            }
            catch
            {
                Ajout_Text("Mauvais identifiants ShiftBot.fr");
            }
        }

        public void Get_Subscription_Gorgeade()
        {
            List<DateTime> list_date_fin_sub = new List<DateTime>();
            if (id_customer == null)
            {
                Ajout_Text("Identifiants ShiftBot.fr incorrect");
            }
            else
            {
                client_w.Authenticator = new HttpBasicAuthenticator(cle_client, secret_client);
                var request = new RestRequest("wp-json/wc/v1/subscriptions?customer=" + id_customer + "&consumer_key=" + cle_client + "&consumer_secret=" + secret_client + "&role=all", Method.GET);
                var response = client_w.Execute(request);
                JArray obj = JArray.Parse(response.Content);

                foreach (JObject o in obj.Children<JObject>())
                {
                    if ((String)o["status"] == "active")
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
            Ajout_Log("Récupération de l'heure");
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

        public void dateTimePicker1_ValueChanged(object sender, EventArgs e)
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

        public void Ajout_Log(string text)
        {
            using (StreamWriter w = File.AppendText(log_path))
            {
                w.WriteLine(Get_PI_DECIM() + " " + DateTime.Now + " : " + text);
            }
        }

        public void Ajout_Text(string text)
        {
            textBox2.Text = text;

            using (StreamWriter w = File.AppendText(log_path))
            {
                w.WriteLine(Get_PI_DECIM() + " " + DateTime.Now + " : " + text);
            }
        }

        public string Get_PI_DECIM()
        {
            string str_return = "";
            if (index_pi + 3 > DECIM_PI.Length)
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

        public void Set_Fenetre_Schedule()
        {
            tab_control = new MetroFramework.Controls.MetroTabControl();
            tab_control.Location = new System.Drawing.Point(430, 80);
            tab_control.Name = "bloc_reservation";
            tab_control.Size = new Size(880, 350);

            this.Controls.Add(tab_control);

            bloc_fenetre = new Bloc_Fenetre(tab_control.Size);
            tab_control.Controls.Add(bloc_fenetre.fenetre);
            tab_control.ResumeLayout(false);

            int position_x = 200;
            for (int i = 0; i < 7; i++)
            {
                position_x = 100 * i;
                bloc_fenetre.Ajout_Bloc_Jour(100 * i, 20, dateTimePicker1.Value.AddDays(i), this);
            }
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (sub_ok)
            {
                List<string> listQuartiers = new List<string>();
                String s1 = metroTextBox4.Text.ToLower().Replace(" ", "");
                String s2 = metroTextBox5.Text.ToLower().Replace(" ", "");
                if (s1.Length > 0)
                {
                    listQuartiers.Add(s1);
                }
                if (s2.Length > 0)
                {
                    listQuartiers.Add(s2);
                }
                
                Ajout_Log("Click Bouton Lancement recherche");

                Ajout_Log("Parametres Temps : ");
                foreach (Bloc_Jour jour in bloc_fenetre.list_bloc_jour)
                {
                    foreach (Bloc_Shift_Interval interval in jour.list_interval)
                    {
                        Ajout_Log(interval.debut_periode + " - " + interval.fin_periode);
                    }
                }

                driver = new FirefoxDriver(chemin);

                BotSelenium botExecution = new BotSelenium(driver,metroTextBox1.Text,listQuartiers,bloc_fenetre, metroTextBox2.Text, metroTextBox3.Text);
            }
            else
            {
                Ajout_Text("Lancement de l inscription impossible : Probleme info Staffomatic");
            }
        }

        private void timer_timeapi_Tick(object sender, EventArgs e)
        {
            Get_Time_API(true);
        }


        /*//Determine les shifts a assigner
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
            }*/

        //Inscription


    }
}
