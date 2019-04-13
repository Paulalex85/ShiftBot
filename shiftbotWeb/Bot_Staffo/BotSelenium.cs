using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Globalization;

namespace Bot_Staffo
{
    class BotSelenium
    {
        IWebDriver driver;
        List<String> listUserDepartment;
        List<Inscription> listInscription;
        Bloc_Fenetre fenetre;
        List<Time_Interval> listInterval;
        Gestion_Interval_User gestionInterval;
        String url;
        String mail;
        String mdp;

        int numeroSemaine = 0;

        public BotSelenium(IWebDriver driver,String url, List<string> listQuartier, Bloc_Fenetre fenetre, String mail, String mdp)
        {
            this.driver = driver;

            listUserDepartment = listQuartier;
            this.listInscription = new List<Inscription>();
            this.fenetre = fenetre;
            gestionInterval = new Gestion_Interval_User();
            this.url = url;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            numeroSemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                fenetre.list_bloc_jour[0].date_jour, 
                DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, 
                DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);

            listInterval = gestionInterval.Set_List_Interval_User(fenetre, listInscription);

            this.mail = mail;
            this.mdp = mdp;

            Login();
            MoveToPlanning();
            PlanningPage();
        }

        public void Login()
        {
            driver.Navigate().GoToUrl(url);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));

            wait.Until<IWebElement>(ExpectedConditions.ElementToBeClickable(By.Id("user_email")));

            IWebElement mailElement = driver.FindElement(By.Id("user_email"));
            mailElement.SendKeys(mail);

            IWebElement mdpElement = driver.FindElement(By.Id("user_password"));
            mdpElement.SendKeys(mdp);

            mdpElement.Submit();
        }

        public void MoveToPlanning()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
            wait.Until<IWebElement>(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa-calendar")));

            IWebElement calendarElement = driver.FindElement(By.CssSelector(".fa-calendar"));
            calendarElement.Click();
        }

        public void PlanningPage()
        {
            while (!isPlanningPublish()) ;

            //page chargée
            while (listInterval.Count > 0)
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".ccc-event-header")));

                List<IWebElement> listDepartement = getListDepartmentWanted();
                List<Shifts> listShifts = ExtractListShiftsFromDepartment(listDepartement);

                listInterval = gestionInterval.Change_List_Interval_Inscription(listInterval, listInscription);

                listInterval = gestionInterval.Change_List_Interval_Inscription(listInterval, listInscription);
                listInterval = gestionInterval.Epuration_List_Interval(listShifts, listInterval);
                listShifts = gestionInterval.Epuration_List_Shifts(listShifts, listInterval);

                if (listShifts.Count > 0)
                {
                    List<Shifts> shiftSansDoublonsHeure = Get_List_Shifts_Sans_Doublons_Horaires(listShifts);
                    List<List<Shifts>> listShiftsJour = GetListShiftsJour(shiftSansDoublonsHeure);

                    List<Shifts> listShiftsToAssign = new List<Shifts>();
                    foreach(List<Shifts> listShiftDay in listShiftsJour)
                    {
                        List<Shifts> listOrdered = listShiftDay.OrderByDescending(x => x.Get_Duree_Minutes()).ToList();
                        if (listOrdered.Count > 10)
                        {
                            listOrdered.RemoveRange(10, listOrdered.Count - 10);
                        }

                        Day_Optimisation dayOptimisation = new Day_Optimisation(listOrdered);
                        listShiftsToAssign.AddRange(dayOptimisation.Get_Best_Combinaison());
                    }

                    foreach(Shifts s in listShiftsToAssign)
                    {
                        inscriptionShift(s);
                    }
                }

            }
        }

        public void inscriptionShift(Shifts shift)
        {
            IWebElement shiftElement = driver.FindElement(By.CssSelector("."+ shift.id));
            shiftElement.Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));

            try
            {
                wait.Until<IWebElement>(ExpectedConditions.ElementToBeClickable(By.Id("applied-users")));

                if (driver.FindElements(By.CssSelector(".action-assign")).Count > 0)
                {
                    IWebElement assignButton = driver.FindElement(By.CssSelector(".action-assign"));
                    assignButton.Click();
                    wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
                    wait.Until<IWebElement>(ExpectedConditions.ElementToBeClickable(By.CssSelector(".close")));
                }

                while (driver.FindElements(By.CssSelector(".close")).Count > 0)
                {
                    IWebElement closeButton = driver.FindElement(By.CssSelector(".close"));
                    closeButton.Click();
                }
            }
            catch (Exception)
            {
                IWebElement closeButton = driver.FindElement(By.CssSelector(".close"));
                closeButton.Click();
            }

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".shift")));
            Thread.Sleep(TimeSpan.FromSeconds(0.3));

        }
        
        public bool isPlanningPublish()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
            wait.Until<IWebElement>(ExpectedConditions.ElementToBeClickable(By.CssSelector(".shift")));

            IList<IWebElement> allPlanning = driver.FindElements(By.CssSelector("div.schedule-item-container"));
            foreach (IWebElement element in allPlanning)
            {
                if (element.FindElement(By.TagName("small")).Text == numeroSemaine.ToString())
                {
                    if (!element.GetAttribute("class").Contains("active"))
                    {
                        element.Click();
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                    return true;
                }
            }
            driver.Navigate().Refresh();
            return false;
        }

        public List<IWebElement> getListDepartmentWanted()
        {
            List<IWebElement> listDepartment = new List<IWebElement>();

            IList<IWebElement> allDepartment = driver.FindElements(By.CssSelector(".ccc-category"));
            foreach (IWebElement element in allDepartment)
            {
                IWebElement titreElement = element.FindElement(By.CssSelector(".ccc-category-name-wrapper"));
                String nomDepartment = titreElement.Text.ToLower().Replace(" ", "");
                if (listUserDepartment.Count == 0 || listUserDepartment.Contains(nomDepartment))
                {
                    listDepartment.Add(element);
                }
            }

            return listDepartment;
        }

        public List<Shifts> ExtractListShiftsFromDepartment(List<IWebElement> listDepartment)
        {
            List<Shifts> listShift = new List<Shifts>();
            listInscription = new List<Inscription>();
            foreach (IWebElement department in listDepartment)
            {
                IList<IWebElement> allJours = department.FindElements(By.CssSelector(".ccc-events-date-wrapper"));
                foreach (IWebElement jour in allJours)
                {
                    String[] dateJourValue = jour.GetAttribute("data-date").Split('-'); //année mois jour
                    if (dateJourValue.Length == 3)
                    {
                        IList<IWebElement> allShiftsDay = jour.FindElements(By.CssSelector(".shift"));
                        foreach (IWebElement shift in allShiftsDay)
                        {
                            String heure = shift.FindElement(By.CssSelector(".ccc-event-header")).Text;
                            String[] heureDebutFin = heure.Split('-');

                            if (heureDebutFin.Length == 2 && heureDebutFin[0].Contains(":") && heureDebutFin[1].Contains(":"))
                            {
                                try
                                {
                                    int indexDebut = heureDebutFin[0].IndexOf(':');
                                    int indexFin = heureDebutFin[1].IndexOf(':');
                                    String[] heureDebut = heureDebutFin[0].Substring(indexDebut - 2, 5).Split(':');
                                    String[] heureFin = heureDebutFin[1].Substring(indexFin - 2, 5).Split(':');

                                    DateTime dateDebut = new DateTime(int.Parse(dateJourValue[0]), int.Parse(dateJourValue[1]), int.Parse(dateJourValue[2]),
                                       int.Parse(heureDebut[0]), int.Parse(heureDebut[1]), 0);
                                    DateTime dateFin = new DateTime(int.Parse(dateJourValue[0]), int.Parse(dateJourValue[1]), int.Parse(dateJourValue[2]),
                                       int.Parse(heureFin[0]), int.Parse(heureFin[1]), 0);

                                    if (isAssigned(shift))
                                    {
                                        listInscription.Add(new Inscription(dateDebut, dateFin));
                                    }
                                    else
                                    {
                                        String id = isShiftFull(shift);
                                        if(id != "")
                                        {
                                            listShift.Add(new Shifts(shift, dateDebut, dateFin,id));
                                        }
                                    }
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                }
            }

            return listShift;
        }

        public String isShiftFull(IWebElement element)
        {
            String[] classElement = element.GetAttribute("class").Split(' ');
            if (classElement.Contains("full"))
            {
                return "";
            }
            foreach(String s in classElement)
            {
                if (s.Contains("shift-id"))
                {
                    return s;
                }
            }
            return "";
        }

        public bool isAssigned(IWebElement element)
        {
            String[] classElement = element.GetAttribute("class").Split(' ');
            if (classElement.Contains("self-assigned"))
            {
                return true;
            }
            return false;
        }

        public List<Inscription> getListInscription(List<Shifts> listShift)
        {
            List<Inscription> list = new List<Inscription>();

            foreach(Shifts shift in listShift)
            {
                if (isAssigned(shift.webElement))
                {
                    list.Add(new Inscription(shift.Debut, shift.Fin));
                }
            }
            return list;
        }

        public List<Shifts> Get_List_Shifts_Sans_Doublons_Horaires(List<Shifts> listShifts)
        {
            List<Shifts> listSansDoublons = new List<Shifts>();

            foreach (Shifts shift in listShifts)
            {
                if (listSansDoublons.Count == 0 || !listSansDoublons.Exists(x => x.Debut == shift.Debut && x.Fin == shift.Fin))
                {
                    listSansDoublons.Add(shift);
                }
            }
            return listSansDoublons;
        }

        //Classe les shifts par jour 
        public List<List<Shifts>> GetListShiftsJour(List<Shifts> listShifts)
        {
            List<List<Shifts>> list = new List<List<Shifts>>();

            foreach (Shifts shift in listShifts)
            {
                bool added = false;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Count > 0)
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
    }
}
