using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Liaison_BD___CSGO
{
    class Program
    {
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr window);

        public static int CurrentMatchId;
        public static int CurrentRoundNumber;
        public static int NextMatchId;
        
        private static Process Serveur;
        private static MySQLWrapper BD;

        static void Main(string[] args)
        {
            BackgroundWorker work = new BackgroundWorker();
            work.DoWork += new DoWorkEventHandler(LookForEvents);
            work.ProgressChanged += new ProgressChangedEventHandler(NewEvent);
            work.WorkerSupportsCancellation = true;
            BD = new MySQLWrapper();
            Serveur = new Process();
            Serveur.StartInfo.FileName = "C:\\Users\\max_l\\Documents\\steamcmd\\csgoserver\\srcds.exe";
            Serveur.StartInfo.Arguments = "-game csgo -console -usercon -maxplayers_override 11 +game_type 0 +game_mode 1 +mapgroup mg_active +map de_dust2 +sv_cheats 1 +bot_join_after_player 1 +mp_autoteambalance 0 +mp_limitteams 30";
            Serveur.StartInfo.ErrorDialog = true;
            Serveur.Start();
            
            CurrentMatchId = (int)BD.Procedure("IsCurrentMatch").Rows[0]["IdMatch"];
            NextMatchId = (int)BD.Procedure("NextMatch").Rows[0]["IdMatch"];
            

            string ligne = "";
            do
            {

                ligne = Console.In.ReadLine();
                IntPtr window = Serveur.MainWindowHandle;
                SetForegroundWindow(window);
                SendKeys.SendWait(ligne);
                SendKeys.SendWait("{ENTER}");
            } while (ligne.ToUpper() != "EXIT");
            work.CancelAsync();
            Serveur.WaitForExit();
        }

        private static void PrepareNextMatch()
        {
            DataTable NextMatch = BD.Procedure("NextMatch");
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] temp = new byte[1];
            random.GetBytes(temp);
            int TeamCT = Convert.ToInt32(temp[0]) % 2;
            string[] CTIds = new string[5];
            string[] TIds = new string[5];

            if(TeamCT == 0)
            {
                DataTable Bots = BD.Procedure("BotFromTeam", new System.Data.Odbc.OdbcParameter(":IdTeam", ((int)NextMatch.Rows[0]["Team_IdTeam1"])));

            }
            else
            {

            }
        }

        private static void NewEvent(object sender, ProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage == 0)
            {
                PrepareNextMatch();
            }
            else if(e.ProgressPercentage == 1)
            {
                StartMatch();
            }
            else if(e.ProgressPercentage == 2)
            {
                UploadScores();
            }
        }

        private static void UploadScores()
        {
            
        }

        private static void StartMatch()
        {
            Interlocked.Exchange(ref CurrentRoundNumber, 1);
            IntPtr window = Serveur.MainWindowHandle;
            SetForegroundWindow(window);
            SendKeys.SendWait("exec gamestart");
            SendKeys.SendWait("{ENTER}");
        }

        private static void LookForEvents(object sender, DoWorkEventArgs e)
        {
            DataTable Match;
            int Round = 1;
            while(true)
            {
                if(((BackgroundWorker)sender).CancellationPending)
                {
                    break;
                }
                if(Round == 1)
                {
                    ((BackgroundWorker)sender).ReportProgress(1);
                }
                Match = BD.Procedure("NextMatch");




                Thread.Sleep(300);
            }
        }
    }
}
