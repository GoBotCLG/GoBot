using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
        public static int NextMatchId;

        private static DataTable CurrentMatch;
        private static DataTable NextMatch;
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

            CurrentMatch = BD.Procedure("IsCurrentMatch");
            NextMatch = BD.Procedure("NextMatch");
            CurrentMatchId = (int)CurrentMatch.Rows[0]["IdMatch"];
            NextMatchId = (int)NextMatch.Rows[0]["IdMatch"];

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

        private static void RefreshTables(object sender, ProgressChangedEventArgs e)
        {
            CurrentMatch = BD.Procedure("IsCurrentMatch");
            NextMatch = BD.Procedure("NextMatch");
        }

        private static void NewEvent(object sender, ProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage == 0)
            {

            }
            else if(e.ProgressPercentage == 1)
            {

            }
        }

        private static void UploadScores(object sender, ProgressChangedEventArgs e)
        {
            
        }

        private static void LookForEvents(object sender, DoWorkEventArgs e)
        {
            DataTable Match;
            while(true)
            {
                if(((BackgroundWorker)sender).CancellationPending)
                {
                    break;
                }
                Match = BD.Procedure("NextMatch");




                Thread.Sleep(300);
            }
        }
    }
}
