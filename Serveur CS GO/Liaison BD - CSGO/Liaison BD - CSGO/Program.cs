using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using SourceRcon;
using System.Net;
using MySql.Data.MySqlClient;

namespace Liaison_BD___CSGO
{
    public enum MatchEvent { MATCH_ENDED, ROUND_ENDED, START_NEXT_MATCH, START_KNIFE_ROUND, TEAM1_WON_KNIFE_ROUND, TEAM2_WON_KNIFE_ROUND }

    class Program
    {
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr window);

        public static int CurrentRoundNumber;
        public static int CurrentMatchId;
        public static int CurrentTeam1Score;
        public static int CurrentTeam2Score;
        public static MySQLWrapper BD;
        public static StreamWriter Journal;

        private static string NextMap;
        private static bool Team1CTNextMatch;
        private static bool Team1CTCurrentMatch;
        private static Process Serveur;
        private static SourceRcon.SourceRcon ConnectionServeur;

        static void Main(string[] args)
        {
            BackgroundWorker work = new BackgroundWorker();
            work.DoWork += new DoWorkEventHandler(LookForEvents);
            work.ProgressChanged += new ProgressChangedEventHandler(NewEvent);
            work.WorkerSupportsCancellation = true;
            work.WorkerReportsProgress = true;

            BD = new MySQLWrapper();

            Journal = new StreamWriter("C:\\Users\\max_l\\Documents\\JOURNAL.txt");

            InitializeServer();

            Console.Write("Connectez-vous au serveur à l'aide du client, rejoignez les spectateurs, puis appuyez sur ENTER: ");
            Console.ReadKey();
            //Enlever StartMatch() dans l'implémentation finale du programme
            StartMatch();
            PrepareNextMatch();
            work.RunWorkerAsync();

            string ligne = "";
            do
            {
                ligne = Console.In.ReadLine();
                ConnectionServeur.ServerCommand(ligne);
            } while (ligne.ToUpper() != "EXIT");
            work.CancelAsync();
            Serveur.WaitForExit();
        }

        private static void InitializeServer()
        {
            Serveur = new Process();
            Serveur.StartInfo.FileName = "C:\\Users\\max_l\\Documents\\steamcmd\\csgoserver\\srcds.exe";
            Serveur.StartInfo.Arguments = "-game csgo -console -usercon +maxplayers_override 11 +rcon_password GoBot +tv_enable 1 +tv_advertise_watchable 1 +tv_deltacache 2 +tv_title GoBot +sv_hibernate_when_empty 0 +game_type 0 +game_mode 1 +mapgroup mg_active +map de_dust2 +sv_cheats 1 +mp_defuser_allocation 1 +bot_join_after_player 1 +mp_autoteambalance 0 +mp_limitteams 30";
            Serveur.StartInfo.ErrorDialog = true;
            Serveur.Start();
            Thread.Sleep(20000);
            ConnectionServeur = new SourceRcon.SourceRcon();
            while (!ConnectionServeur.Connect(new IPEndPoint(Dns.GetHostAddresses(Dns.GetHostName())[1], 27016), "GoBot")) ;
            ConnectionServeur.ServerOutput += new StringOutput(x => Console.WriteLine(x));

            ConnectionServeur.ServerCommand("sv_lan 1");

            for (int i = 1; i <= 11; i++)
            {
                if (File.Exists(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + i.ToString("00") + ".txt"))
                    File.Delete(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + i.ToString("00") + ".txt");
            }

            string[] files = Directory.GetFiles(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + @"\csgo\logs");
            foreach (string f in files)
            {
                File.Delete(f);
            }

            Monitor.Enter(BD);
            try
            {
                CurrentMatchId = (int)BD.Procedure("IsMatchCurrent").Rows[0]["idMatch"];
            }
            finally
            {
                Monitor.Exit(BD);
            }

            //Changer tout ce qui suit pour PrepareNextMatch() à l'implémentation finale
            DataTable CurrentMatch = new DataTable();
            Monitor.Enter(BD);
            try
            {
                CurrentMatch = BD.Procedure("IsMatchCurrent");
            }
            finally
            {
                Monitor.Exit(BD);
            }

            Journal.WriteLine("Préparation du match #" + CurrentMatch.Rows[0]["IdMatch"].ToString() + " ---------------------------------");
            Journal.WriteLine("Team 1: " + CurrentMatch.Rows[0]["Team_IdTeam1"].ToString());
            Journal.WriteLine("Team 2: " + CurrentMatch.Rows[0]["Team_IdTeam2"].ToString());

            NextMap = CurrentMatch.Rows[0]["Map"].ToString();

            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] temp = new byte[1];
            random.GetBytes(temp);
            int TeamCT = Convert.ToInt32(temp[0]) % 2;
            string[] CTIds = new string[5];
            string[] TIds = new string[5];

            StreamWriter OutFile = new StreamWriter(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + @"\csgo\cfg\gamestart.cfg", false);

            OutFile.Write("bot_kick; ");
            DataTable BotsCT;
            DataTable BotsT;

            Monitor.Enter(BD);
            try
            {
                if (TeamCT == 0)
                {
                    Journal.WriteLine("La team 1 commence en tant que CT");
                    Team1CTNextMatch = true;
                    BotsCT = BD.Procedure("BotFromTeam", new MySqlParameter("PIdTeam", ((int)CurrentMatch.Rows[0]["Team_IdTeam1"])));
                    BotsT = BD.Procedure("BotFromTeam", new MySqlParameter("PIdTeam", ((int)CurrentMatch.Rows[0]["Team_IdTeam2"])));
                    Journal.WriteLine("Bots pour la team 1 (CT):");
                    foreach (DataRow bot in BotsCT.Rows)
                    {
                        Journal.WriteLine(bot["NomBot"].ToString());
                    }
                    Journal.WriteLine("Bots pour la team 2 (T):");
                    foreach (DataRow bot in BotsT.Rows)
                    {
                        Journal.WriteLine(bot["NomBot"].ToString());
                    }
                }
                else
                {
                    Journal.WriteLine("La team 2 commence en tant que CT");
                    Team1CTNextMatch = false;
                    BotsT = BD.Procedure("BotFromTeam", new MySqlParameter("PIdTeam", (int)CurrentMatch.Rows[0]["Team_IdTeam1"]));
                    BotsCT = BD.Procedure("BotFromTeam", new MySqlParameter("PIdTeam", (int)CurrentMatch.Rows[0]["Team_IdTeam2"]));
                    Journal.WriteLine("Bots pour la team 2 (CT):");
                    foreach (DataRow bot in BotsCT.Rows)
                    {
                        Journal.WriteLine(bot["NomBot"].ToString());
                    }
                    Journal.WriteLine("Bots pour la team 1 (T):");
                    foreach (DataRow bot in BotsT.Rows)
                    {
                        Journal.WriteLine(bot["NomBot"].ToString());
                    }
                }
            }
            finally
            {
                Monitor.Exit(BD);
            }


            for (int i = 0; i < 5; i++)
            {
                OutFile.Write("bot_add ct \"" + BotsCT.Rows[i]["NomBot"].ToString() + "\"; ");
                OutFile.Write("bot_add t \"" + BotsT.Rows[i]["NomBot"].ToString() + "\"; ");
            }

            DataTable Teams = new DataTable();

            Monitor.Enter(BD);
            try
            {
                Teams = BD.Procedure("TeamFromMatch", new MySqlParameter("PIdMatch", CurrentMatch.Rows[0]["IdMatch"]));
            }
            finally
            {
                Monitor.Exit(BD);
            }

            string Team1Name;
            string Team2Name;

            if ((int)Teams.Rows[0]["IdTeam"] == (int)CurrentMatch.Rows[0]["Team_IdTeam1"])
            {
                if (Team1CTNextMatch)
                {
                    Team1Name = Teams.Rows[0]["Name"].ToString();
                    Team2Name = Teams.Rows[1]["Name"].ToString();
                }
                else
                {
                    Team1Name = Teams.Rows[1]["Name"].ToString();
                    Team2Name = Teams.Rows[0]["Name"].ToString();
                }
            }
            else
            {
                if (Team1CTNextMatch)
                {
                    Team1Name = Teams.Rows[1]["Name"].ToString();
                    Team2Name = Teams.Rows[0]["Name"].ToString();
                }
                else
                {
                    Team1Name = Teams.Rows[0]["Name"].ToString();
                    Team2Name = Teams.Rows[1]["Name"].ToString();
                }
            }

            OutFile.Write("mp_teamname_1 \"" + Team1Name + "\"; mp_teamname_2 \"" + Team2Name + "\"; ");

            OutFile.Write("mp_restartgame 1; mp_defuser_allocation 1; mp_warmup_end; log on;");
            OutFile.Flush();
            OutFile.Close();
            Journal.WriteLine("Fin de la préparation du match #" + CurrentMatch.Rows[0]["IdMatch"].ToString() + " ---------------------------------");
        }

        private static void PrepareNextMatch()
        {
            DataTable NextMatch = new DataTable();
            Monitor.Enter(BD);
            try
            {
                NextMatch = BD.Procedure("NextMatch");
            }
            finally
            {
                Monitor.Exit(BD);
            }

            Journal.WriteLine("Préparation du match #" + NextMatch.Rows[0]["IdMatch"].ToString() + " ---------------------------------");
            Journal.WriteLine("Team 1: " + NextMatch.Rows[0]["Team_IdTeam1"].ToString());
            Journal.WriteLine("Team 2: " + NextMatch.Rows[0]["Team_IdTeam2"].ToString());

            NextMap = NextMatch.Rows[0]["Map"].ToString();

            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] temp = new byte[1];
            random.GetBytes(temp);
            int TeamCT = Convert.ToInt32(temp[0]) % 2;
            string[] CTIds = new string[5];
            string[] TIds = new string[5];

            StreamWriter OutFile = new StreamWriter(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + @"\csgo\cfg\gamestart.cfg", false);

            OutFile.Write("bot_kick; ");
            DataTable BotsCT;
            DataTable BotsT;

            Monitor.Enter(BD);
            try
            {
                if (TeamCT == 0)
                {
                    Journal.WriteLine("La team 1 commence en tant que CT");
                    Team1CTNextMatch = true;
                    BotsCT = BD.Procedure("BotFromTeam", new MySqlParameter("PIdTeam", ((int)NextMatch.Rows[0]["Team_IdTeam1"])));
                    BotsT = BD.Procedure("BotFromTeam", new MySqlParameter("PIdTeam", ((int)NextMatch.Rows[0]["Team_IdTeam2"])));
                    Journal.WriteLine("Bots pour la team 1 (CT):");
                    foreach (DataRow bot in BotsCT.Rows)
                    {
                        Journal.WriteLine(bot["NomBot"].ToString());
                    }
                    Journal.WriteLine("Bots pour la team 2 (T):");
                    foreach (DataRow bot in BotsT.Rows)
                    {
                        Journal.WriteLine(bot["NomBot"].ToString());
                    }
                }
                else
                {
                    Journal.WriteLine("La team 2 commence en tant que CT");
                    Team1CTNextMatch = false;
                    BotsT = BD.Procedure("BotFromTeam", new MySqlParameter("PIdTeam", (int)NextMatch.Rows[0]["Team_IdTeam1"]));
                    BotsCT = BD.Procedure("BotFromTeam", new MySqlParameter("PIdTeam", (int)NextMatch.Rows[0]["Team_IdTeam2"]));
                    Journal.WriteLine("Bots pour la team 1 (CT):");
                    foreach (DataRow bot in BotsCT.Rows)
                    {
                        Journal.WriteLine(bot["NomBot"].ToString());
                    }
                    Journal.WriteLine("Bots pour la team 2 (T):");
                    foreach (DataRow bot in BotsT.Rows)
                    {
                        Journal.WriteLine(bot["NomBot"].ToString());
                    }
                }
            }
            finally
            {
                Monitor.Exit(BD);
            }

            for (int i = 0; i < 5; i++)
            {
                OutFile.Write("bot_add ct \"" + BotsCT.Rows[i]["NomBot"].ToString() + "\"; ");
                OutFile.Write("bot_add t \"" + BotsT.Rows[i]["NomBot"].ToString() + "\"; ");
            }

            DataTable Teams = new DataTable();
            Monitor.Enter(BD);
            try
            {
                Teams = BD.Procedure("TeamFromMatch", new MySqlParameter("PIdMatch", NextMatch.Rows[0]["IdMatch"]));
            }
            finally
            {
                Monitor.Exit(BD);
            }
            string Team1Name;
            string Team2Name;

            if ((int)Teams.Rows[0]["IdTeam"] == (int)NextMatch.Rows[0]["Team_IdTeam1"])
            {
                if (Team1CTNextMatch)
                {
                    Team2Name = Teams.Rows[1]["Name"].ToString();
                    Team1Name = Teams.Rows[0]["Name"].ToString();
                }
                else
                {
                    Team1Name = Teams.Rows[0]["Name"].ToString();
                    Team2Name = Teams.Rows[1]["Name"].ToString();
                }
            }
            else
            {
                if (Team1CTNextMatch)
                {
                    Team1Name = Teams.Rows[0]["Name"].ToString();
                    Team2Name = Teams.Rows[1]["Name"].ToString();
                }
                else
                {
                    Team2Name = Teams.Rows[1]["Name"].ToString();
                    Team1Name = Teams.Rows[0]["Name"].ToString();
                }
            }

            OutFile.Write("mp_teamname_1 \"" + Team1Name + "\"; mp_teamname_2 \"" + Team2Name + "\"; ");

            OutFile.Write("mp_restartgame 1; mp_defuser_allocation 1; mp_warmup_end; log on;");
            OutFile.Flush();
            OutFile.Close();
            Journal.WriteLine("Fin de la préparation du match #" + NextMatch.Rows[0]["IdMatch"].ToString() + " ---------------------------------");
        }

        private static void StopMatch()
        {
            IntPtr window = Serveur.MainWindowHandle;
            SetForegroundWindow(window);
            ConnectionServeur.ServerCommand("log off");
            ConnectionServeur.ServerCommand("bot_kick");
            Thread.Sleep(2000);
            //SetForegroundWindow(Process.GetProcessesByName("csgo")[0].MainWindowHandle);

            for (int i = 1; i <= 11; i++)
                File.Delete(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + i.ToString("00") + ".txt");

            StreamReader InLog = new StreamReader(Directory.GetFiles(@"C:\Users\max_l\Documents\steamcmd\csgoserver\csgo\logs")[0]);
            Dictionary<string, int> KillsBots = new Dictionary<string, int>();
            Dictionary<string, int> AssistsBots = new Dictionary<string, int>();
            Dictionary<string, int> DeathsBots = new Dictionary<string, int>();
            Dictionary<string, int> IdBots = new Dictionary<string, int>();

            DataTable Teams = new DataTable();
            Monitor.Enter(BD);
            try
            {
                Teams = BD.Procedure("TeamFromMatch", new MySqlParameter("PIdMatch", CurrentMatchId));
            }
            finally
            {
                Monitor.Exit(BD);
            }

            foreach (DataRow row in Teams.Rows)
            {
                DataTable Bots = new DataTable();
                Monitor.Enter(BD);
                try
                {
                    Bots = BD.Procedure("BotFromTeam", new MySqlParameter("PIdTeam", row["IdTeam"]));
                }
                finally
                {
                    Monitor.Exit(BD);
                }
                foreach (DataRow bot in Bots.Rows)
                {
                    IdBots.Add(bot["NomBot"].ToString(), (int)bot["IdBot"]);
                    KillsBots.Add(bot["NomBot"].ToString(), Convert.ToInt32(bot["KDA"].ToString().Split('/')[0]));
                    AssistsBots.Add(bot["NomBot"].ToString(), Convert.ToInt32(bot["KDA"].ToString().Split('/')[1]));
                    DeathsBots.Add(bot["NomBot"].ToString(), Convert.ToInt32(bot["KDA"].ToString().Split('/')[2]));
                }
            }

            while (!InLog.EndOfStream)
            {
                string ligne = InLog.ReadLine();
                if (ligne.Contains("killed"))
                {
                    KillsBots[ligne.Split('"')[1].Substring(0, ligne.Split('"')[1].IndexOf('<'))] = KillsBots[ligne.Split('"')[1].Substring(0, ligne.Split('"')[1].IndexOf('<'))] + 1;
                    DeathsBots[ligne.Split('"')[3].Substring(0, ligne.Split('"')[3].IndexOf('<'))] = DeathsBots[ligne.Split('"')[3].Substring(0, ligne.Split('"')[3].IndexOf('<'))] + 1;
                }
                if (ligne.Contains("assisted"))
                {
                    AssistsBots[ligne.Split('"')[1].Substring(0, ligne.Split('"')[1].IndexOf('<'))] = AssistsBots[ligne.Split('"')[1].Substring(0, ligne.Split('"')[1].IndexOf('<'))] + 1;
                }
            }

            Monitor.Enter(BD);
            try
            {
                foreach (KeyValuePair<string, int> bot in IdBots)
                {
                    BD.Procedure("SetKDA", new MySqlParameter("PKDA", KillsBots[bot.Key].ToString() + "/" + DeathsBots[bot.Key].ToString() + "/" + AssistsBots[bot.Key].ToString()), new MySqlParameter("PidBot", bot.Value));
                }
            }
            finally
            {
                Monitor.Exit(BD);
            }
            CurrentTeam1Score = 0;
            CurrentTeam2Score = 0;

            InLog.Close();
            File.Delete(Directory.GetFiles(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + @"\csgo\logs")[0]);
        }

        private static void NewEvent(object sender, ProgressChangedEventArgs e)
        {
            DataTable teams = new DataTable();
            Monitor.Enter(BD);
            try
            {
                teams = BD.Procedure("TeamFromMatch", new MySqlParameter("PIdMatch", CurrentMatchId));
            }
            finally
            {
                Monitor.Exit(BD);
            }
            if (e.ProgressPercentage == (int)MatchEvent.MATCH_ENDED)
            {
                if (CurrentTeam1Score > CurrentTeam2Score)
                {
                    ConnectionServeur.ServerCommand("tv_msg \"" + teams.Rows[0]["Name"] + " won \"");
                    Console.WriteLine("Le match #" + CurrentMatchId + " opposant " + teams.Rows[0]["Name"] + " contre " + teams.Rows[1]["Name"] + " est terminé. Le gagnant est " + teams.Rows[0]["Name"] + " avec un score de " + CurrentTeam1Score + "-" + CurrentTeam2Score);
                }
                else
                {
                    Console.WriteLine("Le match #" + CurrentMatchId + " opposant " + teams.Rows[0]["Name"] + " contre " + teams.Rows[1]["Name"] + " est terminé. Le gagnant est " + teams.Rows[1]["Name"] + " avec un score de " + CurrentTeam2Score + "-" + CurrentTeam1Score);
                }

                SetVictoryBets(CurrentMatchId, (int)(CurrentTeam1Score > CurrentTeam2Score ? teams.Rows[0]["IdTeam"] : teams.Rows[1]["IdTeam"]));
                StopMatch();
            }
            else if (e.ProgressPercentage == (int)MatchEvent.ROUND_ENDED)
            {
                CurrentRoundNumber++;
                UploadScores();
                if (CurrentRoundNumber >= 12)
                {
                    ConnectionServeur.ServerCommand("mp_give_player_c4 1");
                    ConnectionServeur.ServerCommand("bot_all_weapons");
                }

                if (CurrentTeam1Score > CurrentTeam2Score)
                {
                    Console.WriteLine("C'est maintenant " + CurrentTeam1Score + "-" + CurrentTeam2Score + " en faveur de " + teams.Rows[0]["Name"]);
                }
                else
                {
                    Console.WriteLine("C'est maintenant " + CurrentTeam2Score + "-" + CurrentTeam1Score + " en faveur de " + teams.Rows[1]["Name"]);
                }
            }
            else if (e.ProgressPercentage == (int)MatchEvent.START_NEXT_MATCH)
            {
                Console.WriteLine("Le match #" + CurrentMatchId + " opposant " + teams.Rows[0]["Name"] + " contre " + teams.Rows[1]["Name"] + " commence à l'instant!");
                StartMatch();
                PrepareNextMatch();
            }
            else if (e.ProgressPercentage == (int)MatchEvent.START_KNIFE_ROUND)
            {
                Console.WriteLine("Un knife round est lancé pour déterminer l'équipe gagnante!");
                ConnectionServeur.ServerCommand("mp_give_player_c4 0");
                ConnectionServeur.ServerCommand("bot_knives_only");
            }
        }

        private static void UploadScores()
        {
            StreamReader InRound = new StreamReader(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + @"\csgo\backup_round" + (CurrentRoundNumber - 1).ToString("00") + ".txt");

            int TotalCT = 0;
            int TotalT = 0;

            while (!InRound.EndOfStream)
            {
                string ligne = InRound.ReadLine();
                if (CurrentRoundNumber > 6)
                {
                    if (ligne.Contains("FirstHalfScore"))
                    {
                        InRound.ReadLine();                             //{
                        ligne = InRound.ReadLine();                     //  "team1"     "2"
                        TotalCT = int.Parse(ligne.Split('"')[3]);
                        ligne = InRound.ReadLine();                     //  "team2"     "3"
                        TotalT = int.Parse(ligne.Split('"')[3]);
                        InRound.ReadLine();                             //}
                        InRound.ReadLine();                             //SecondHalfScore
                        InRound.ReadLine();                             //{
                        ligne = InRound.ReadLine();                     //  "team1"     "4"
                        TotalCT += int.Parse(ligne.Split('"')[3]);
                        ligne = InRound.ReadLine();                     //  "team2"     "1"
                        TotalT += int.Parse(ligne.Split('"')[3]);
                        break;
                    }
                }
                else
                {
                    if (ligne.Contains("FirstHalfScore"))
                    {
                        InRound.ReadLine();                             //{
                        ligne = InRound.ReadLine();                     //  "team1"     "2"
                        TotalCT = int.Parse(ligne.Split('"')[3]);
                        ligne = InRound.ReadLine();                     //  "team2"     "3"
                        TotalT = int.Parse(ligne.Split('"')[3]);
                        break;
                    }
                }
            }

            Monitor.Enter(BD);
            try
            {
                if (Team1CTCurrentMatch)
                {
                    BD.Procedure("SetRoundTeam1", new MySqlParameter("Pround", TotalCT), new MySqlParameter("PIdMatch", CurrentMatchId));
                    CurrentTeam1Score = TotalCT;
                    BD.Procedure("SetRoundTeam2", new MySqlParameter("Pround", TotalT), new MySqlParameter("PIdMatch", CurrentMatchId));
                    CurrentTeam2Score = TotalT;
                }
                else
                {
                    BD.Procedure("SetRoundTeam1", new MySqlParameter("Pround", TotalT), new MySqlParameter("PIdMatch", CurrentMatchId));
                    CurrentTeam1Score = TotalT;
                    BD.Procedure("SetRoundTeam2", new MySqlParameter("Pround", TotalCT), new MySqlParameter("PIdMatch", CurrentMatchId));
                    CurrentTeam2Score = TotalCT;
                }
            }
            finally
            {
                Monitor.Exit(BD);
            }

            InRound.Close();
            for (int i = 1; i <= 11; i++)
            {
                if (File.Exists(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + i.ToString("00") + ".txt"))
                    File.Delete(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + i.ToString("00") + ".txt");
            }
        }

        private static void EndKnifeRound()
        {
            StreamReader InRound = new StreamReader(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + @"\csgo\backup_round" + (CurrentRoundNumber - 1).ToString("00") + ".txt");

            int TotalCT = 5;
            int TotalT = 5;

            while (!InRound.EndOfStream)
            {
                string ligne = InRound.ReadLine();
                if (ligne.Contains("FirstHalfScore"))
                {
                    InRound.ReadLine();                             //{
                    ligne = InRound.ReadLine();                     //  "team1"     "2"
                    TotalCT += int.Parse(ligne.Split('"')[3]);
                    ligne = InRound.ReadLine();                     //  "team2"     "3"
                    TotalT += int.Parse(ligne.Split('"')[3]);
                }
            }

            Monitor.Enter(BD);
            try
            {
                if (Team1CTCurrentMatch)
                {
                    BD.Procedure("SetRoundTeam1", new MySqlParameter("Pround", TotalCT), new MySqlParameter("PIdMatch", CurrentMatchId));
                    CurrentTeam1Score = TotalCT;
                    BD.Procedure("SetRoundTeam2", new MySqlParameter("Pround", TotalT), new MySqlParameter("PIdMatch", CurrentMatchId));
                    CurrentTeam2Score = TotalT;
                }
                else
                {
                    BD.Procedure("SetRoundTeam1", new MySqlParameter("Pround", TotalT), new MySqlParameter("PIdMatch", CurrentMatchId));
                    CurrentTeam1Score = TotalT;
                    BD.Procedure("SetRoundTeam2", new MySqlParameter("Pround", TotalCT), new MySqlParameter("PIdMatch", CurrentMatchId));
                    CurrentTeam2Score = TotalCT;
                }
            }
            finally
            {
                Monitor.Exit(BD);
            }

            InRound.Close();
            for (int i = 1; i <= 11; i++)
            {
                if (File.Exists(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + i.ToString("00") + ".txt"))
                    File.Delete(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + i.ToString("00") + ".txt");
            }

            ConnectionServeur.ServerCommand("bot_all_weapons");

        }

        private static void StartMatch()
        {
            ConnectionServeur.ServerCommand("changelevel " + NextMap);
            SetForegroundWindow(Process.GetProcessesByName("csgo")[0].MainWindowHandle);
            Thread.Sleep(30000);


            Team1CTCurrentMatch = Team1CTNextMatch;
            Monitor.Enter(BD);
            try
            {
                CurrentMatchId = (int)BD.Procedure("IsMatchCurrent").Rows[0]["IdMatch"];
            }
            finally
            {
                Monitor.Exit(BD);
            }
            CurrentRoundNumber = 1;
            CurrentTeam1Score = 0;
            CurrentTeam2Score = 0;
            ConnectionServeur.ServerCommand("tv_delay 10");
            ConnectionServeur.ServerCommand("exec gamestart");


        }

        private static void LookForEvents(object sender, DoWorkEventArgs e)
        {
            //Changer à Round = 0 à l'implémentation finale
            int Round = 1;
            while (true)
            {
                if (((BackgroundWorker)sender).CancellationPending)
                {
                    break;
                }
                Monitor.Enter(BD);
                try
                {
                    if (Round == 0 && CurrentMatchId != (int)BD.Procedure("IsMatchCurrent").Rows[0]["IdMatch"])
                    {
                        Round = 1;
                        ((BackgroundWorker)sender).ReportProgress((int)MatchEvent.START_NEXT_MATCH);
                    }
                }
                finally
                {
                    Monitor.Exit(BD);
                }

                if (File.Exists(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + Round.ToString("00") + ".txt"))
                {
                    Round++;
                    ((BackgroundWorker)sender).ReportProgress((int)MatchEvent.ROUND_ENDED);
                }
                

                if (Round == 11 || CurrentTeam1Score == 6 || CurrentTeam2Score == 6)
                {
                    if (Round == 11 && CurrentTeam1Score < 6 && CurrentTeam2Score < 6)
                    {
                        ((BackgroundWorker)sender).ReportProgress((int)MatchEvent.START_KNIFE_ROUND);
                        while (!File.Exists(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + Round.ToString("00") + ".txt"))
                        {
                            Thread.Sleep(1000);
                        }
                        ((BackgroundWorker)sender).ReportProgress((int)MatchEvent.ROUND_ENDED);
                        ConnectionServeur.ServerCommand("bot_all_weapons");
                        ConnectionServeur.ServerCommand("mp_give_player_c4 1");
                    }
                    Thread.Sleep(10000);
                    Round = 0;
                    ((BackgroundWorker)sender).ReportProgress((int)MatchEvent.MATCH_ENDED);
                    Thread.Sleep(10000);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentMatchId">Match ID</param>
        /// <param name="VictoryTeam">Winner's team ID</param>
        private static void SetVictoryBets(int currentMatchId, int WinnerID)
        {
            int xpWin = 100; // Amount of xp a user gain when winning bet.
            int xpLvl = 1500;
            DataTable bets = new DataTable();
            DataTable users = new DataTable();

            Monitor.Enter(BD);
            try
            {
                bets = BD.Procedure("GetBetsFromMatch", new MySqlParameter("IDMatch", currentMatchId));
                users = BD.Select("user", "", new List<MySqlParameter>(), "Username", "Credit", "EXP", "LVL");

                BD.Procedure("SetVictoire", new MySqlParameter("PidMatch", currentMatchId), new MySqlParameter("IdTeam", WinnerID));
            }
            finally
            {
                Monitor.Exit(BD);
            }

            var totalBets = getTotalBets(ref bets, WinnerID);

            if (bets != null && bets.Rows.Count > 0)
            {
                decimal remains = 0;
                foreach (DataRow bet in bets.Rows)
                {
                    try
                    {
                        if ((int)bet["Team_IdTeam"] == WinnerID)
                        {
                            List<decimal> gains = getGain((int)bet["Mise"], totalBets["winner"], totalBets["losers"]);
                            remains += gains[1];

                            int toAdd = (int)bet["Team_IdTeam"] == WinnerID ? (int)gains[0] : 0;

                            DataRow[] user = users.Select("Username = '" + bet["User_Username"].ToString() + "'");
                            int userCredit = user != null && user.Length > 0 ? (int)user[0]["Credit"] : -1;

                            if (userCredit != -1)
                            {
                                int xp = (int)user[0]["EXP"] + xpWin;
                                int lvl = (int)user[0]["LVL"];
                                if (xp >= xpLvl)
                                {
                                    xp -= xpLvl;
                                    lvl += 1;
                                }
                                Monitor.Enter(BD);
                                try
                                {
                                    BD.Procedure("AddEXP", new MySqlParameter("Pusername", bet["User_Username"]), new MySqlParameter("Pexp", xp), new MySqlParameter("Plevel", lvl));
                                    BD.Procedure("AddFunds", new MySqlParameter("UserNames", bet["User_Username"]), new MySqlParameter("Argent", userCredit + toAdd));
                                }
                                finally
                                {
                                    Monitor.Exit(BD);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string ex = e.Message;
                    }
                }
                updateBetsAdmin((int)Math.Floor(totalBets["admin"] + remains));
            }
        }

        private static Dictionary<string, int> getTotalBets(ref DataTable bets, int winner)
        {
            decimal reduction = 0.9m;
            int loserTotal = 0, winnerTotal = 0, total = 0;

            foreach (DataRow row in bets.Rows)
            {
                if ((int)row["Team_IdTeam"] == winner)
                    winnerTotal += (int)row["Mise"];
                else
                    loserTotal += (int)row["Mise"];
            }

            total = loserTotal;
            loserTotal = (int)Math.Floor(decimal.Multiply(reduction, loserTotal));
            winnerTotal = (int)Math.Floor(decimal.Multiply(reduction, winnerTotal));
            int admin = total - loserTotal;

            return new Dictionary<string, int>() { { "winner", winnerTotal }, { "loser", loserTotal }, { "admin", total } };
        }

        private static List<decimal> getGain(int bet, int total, int losers)
        {
            decimal totalGain = decimal.Multiply(decimal.Divide(bet, total), losers);
            decimal roundedDown = Math.Floor(totalGain);
            return new List<decimal>() { roundedDown, totalGain - roundedDown };
        }

        private static void updateBetsAdmin(int amount)
        {
            Monitor.Enter(BD);
            try
            {
                BD.Procedure("AddFunds", new MySqlParameter("UserNames", "admin"), new MySqlParameter("Argent", amount));
            }
            catch (Exception e)
            {
                string ex = e.Message;
            }
            finally
            {
                Monitor.Exit(BD);
            }
        }

        /*public static double GetTimeOffset(string time)
        {
            DateTime bdTime = DateTime.Now;
            Monitor.Enter(BD);
            try
            {
                bdTime = BD.GetBDTime();
                DateTime clientTime = DateTime.Now;
                double offset = (clientTime - bdTime).TotalMinutes / 60;
                return (double)(Math.Round(2 * (offset))) / 2;
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                Monitor.Exit(BD);
            }
        }*/
    }
}
