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

namespace Liaison_BD___CSGO
{
    public enum MatchEvent { MATCH_ENDED, ROUND_ENDED, START_NEXT_MATCH }

    class Program
    {
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr window);
        
        public static int CurrentRoundNumber;
        public static int CurrentMatchId;
        public static int CurrentTeam1Score;
        public static int CurrentTeam2Score;

        private static string NextMap;
        private static bool Team1CTNextMatch;
        private static bool Team1CTCurrentMatch;
        private static Process Serveur;
        private static MySQLWrapper BD;
        private static SourceRcon.SourceRcon ConnectionServeur;

        static void Main(string[] args)
        {
            BackgroundWorker work = new BackgroundWorker();
            work.DoWork += new DoWorkEventHandler(LookForEvents);
            work.ProgressChanged += new ProgressChangedEventHandler(NewEvent);
            work.WorkerSupportsCancellation = true;
            work.WorkerReportsProgress = true;
            BD = new MySQLWrapper();
            Serveur = new Process();
            Serveur.StartInfo.FileName = "C:\\Users\\max_l\\Documents\\steamcmd\\csgoserver\\srcds.exe";
            Serveur.StartInfo.Arguments = "-game csgo -console -usercon -maxplayers_override 11 +rcon_password GoBot +tv_enable 1 +tv_deltacache 2 +tv_title GoBot +sv_hibernate_when_empty 0 +game_type 0 +game_mode 1 +mapgroup mg_active +map de_dust2 +sv_cheats 1 +bot_join_after_player 1 +mp_autoteambalance 0 +mp_limitteams 30";
            Serveur.StartInfo.ErrorDialog = true;
            Serveur.Start();
            Thread.Sleep(60000);
            ConnectionServeur = new SourceRcon.SourceRcon();
            while (!ConnectionServeur.Connect(new IPEndPoint(Dns.GetHostAddresses(Dns.GetHostName())[2], 27016), "GoBot")) ;
            ConnectionServeur.ServerOutput += new StringOutput(x => Console.WriteLine(x));

            PrepareNextMatch();
            Console.Write("Connectez-vous au serveur à l'aide du client, rejoignez les spectateurs, puis appuyez sur ENTER: ");
            Console.ReadKey();
            StartMatch();
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

        private static void PrepareNextMatch()
        {
            DataTable NextMatch = BD.Procedure("NextMatch");

            NextMap = NextMatch.Rows[0]["Map"].ToString();

            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] temp = new byte[1];
            random.GetBytes(temp);
            int TeamCT = Convert.ToInt32(temp[0]) % 2;
            string[] CTIds = new string[5];
            string[] TIds = new string[5];

            StreamWriter OutFile = new StreamWriter(@"C:\Users\max_l\Documents\steamcmd\csgoserver\csgo\cfg\gamestart.cfg", false);

            OutFile.Write("bot_kick; ");
            DataTable BotsCT;
            DataTable BotsT;

            if (TeamCT == 0)
            {
                Team1CTNextMatch = true;
                BotsCT = BD.Procedure("BotFromTeam", new OdbcParameter(":IdTeam", ((int)NextMatch.Rows[0]["Team_IdTeam1"])));
                BotsT = BD.Procedure("BotFromTeam", new OdbcParameter(":IdTeam", ((int)NextMatch.Rows[0]["Team_IdTeam2"])));
            }
            else
            {
                Team1CTNextMatch = false;
                BotsT = BD.Procedure("BotFromTeam", new OdbcParameter(":IdTeam", (int)NextMatch.Rows[0]["Team_IdTeam1"]));
                BotsCT = BD.Procedure("BotFromTeam", new OdbcParameter(":IdTeam", (int)NextMatch.Rows[0]["Team_IdTeam2"]));
            }


            for (int i = 0; i < 5; i++)
            {
                OutFile.Write("bot_add ct \"" + BotsCT.Rows[i]["NomBot"].ToString() + "\"; ");
                OutFile.Write("bot_add t \"" + BotsT.Rows[i]["NomBot"].ToString() + "\"; ");
            }

            DataTable Teams = BD.Procedure("TeamFromMatch", new OdbcParameter(":IdMatch", NextMatch.Rows[0]["IdMatch"]));
            string Team1Name;
            string Team2Name;

            if ((int)Teams.Rows[0]["IdTeam"] == (int)NextMatch.Rows[0]["Team_IdTeam1"])
            {
                Team1Name = Teams.Rows[0]["Name"].ToString();
                Team2Name = Teams.Rows[1]["Name"].ToString();
            }
            else
            {
                Team1Name = Teams.Rows[1]["Name"].ToString();
                Team2Name = Teams.Rows[0]["Name"].ToString();
            }

            OutFile.Write("mp_teamname_1 \"" + Team1Name + "\"; mp_teamname_2 \"" + Team2Name + "\"; ");

            OutFile.Write("mp_warmup_end; log on;");
            OutFile.Flush();
            OutFile.Close();
        }

        private static void StopMatch()
        {
            IntPtr window = Serveur.MainWindowHandle;
            SetForegroundWindow(window);
            ConnectionServeur.ServerCommand("log off");
            ConnectionServeur.ServerCommand("bot_kick");
            //Thread.Sleep(500);
            //SetForegroundWindow(Process.GetProcessesByName("csgo")[0].MainWindowHandle);

            for (int i = 1; i <= 10; i++)
                File.Delete(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + (i < 10 ? "0" + i.ToString() : i.ToString()) + ".txt");

            StreamReader InLog = new StreamReader(Directory.GetFiles(@"C:\Users\max_l\Documents\steamcmd\csgoserver\csgo\logs")[0]);
            Dictionary<string, int> KillsBots = new Dictionary<string, int>();
            Dictionary<string, int> AssistsBots = new Dictionary<string, int>();
            Dictionary<string, int> DeathsBots = new Dictionary<string, int>();
            Dictionary<string, int> IdBots = new Dictionary<string, int>();

            DataTable Teams = BD.Procedure("TeamFromMatch", new OdbcParameter(":IdMatch", CurrentMatchId));

            foreach(DataRow row in Teams.Rows)
            {
                DataTable Bots = BD.Procedure("BotFromTeam", new OdbcParameter(":IdTeam", row["IdTeam"]));
                foreach(DataRow bot in Bots.Rows)
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
                if(ligne.Contains("killed"))
                {
                    KillsBots[ligne.Split('"')[1].Substring(0, ligne.Split('"')[1].IndexOf('<'))] = KillsBots[ligne.Split('"')[1].Substring(0, ligne.Split('"')[1].IndexOf('<'))] + 1;
                    DeathsBots[ligne.Split('"')[3].Substring(0, ligne.Split('"')[3].IndexOf('<'))] = DeathsBots[ligne.Split('"')[3].Substring(0, ligne.Split('"')[3].IndexOf('<'))] + 1;
                }
                if(ligne.Contains("assisted"))
                {
                    AssistsBots[ligne.Split('"')[1].Substring(0, ligne.Split('"')[1].IndexOf('<'))] = AssistsBots[ligne.Split('"')[1].Substring(0, ligne.Split('"')[1].IndexOf('<'))] + 1;
                }
            }

            foreach(KeyValuePair<string, int> bot in IdBots)
            {
                BD.Procedure("SetKDA", new OdbcParameter(":KDA", KillsBots[bot.Key].ToString() + "/" + DeathsBots[bot.Key].ToString() + "/" + AssistsBots[bot.Key].ToString()), new OdbcParameter(":IdBot", bot.Value));
            }

            InLog.Close();
            File.Delete(Directory.GetFiles(@"C:\Users\max_l\Documents\steamcmd\csgoserver\csgo\logs")[0]);
        }

        private static void NewEvent(object sender, ProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage == (int)MatchEvent.MATCH_ENDED)
            {
                DataTable teams = BD.Procedure("TeamFromMatch", new OdbcParameter(":MatchId", CurrentMatchId));
                try
                {
                    int winnerId = CurrentTeam1Score > CurrentTeam2Score ? (int)teams.Rows[0]["IdTeam"] : (int)teams.Rows[1]["IdTeam"];
                    Console.WriteLine("Le match #" + CurrentMatchId + " opposant " + teams.Rows[0]["Name"] + " contre " + teams.Rows[1]["Name"] + " est terminé avec un score de " + CurrentTeam1Score + "-" + CurrentTeam2Score);
                    SetVictoryBets(CurrentMatchId, winnerId);
                    StopMatch();
                }
                catch (Exception)
                {

                }
            }
            else if(e.ProgressPercentage == (int)MatchEvent.ROUND_ENDED)
            {
                CurrentRoundNumber++;
                UploadScores();
                if(CurrentTeam1Score > CurrentTeam2Score)
                {
                    Console.WriteLine("C'est maintenant " + CurrentTeam1Score + "-" + CurrentTeam2Score + " en faveur de " + BD.Procedure("TeamFromMatch", new OdbcParameter(":MatchId", CurrentMatchId)).Rows[0]["Name"]);
                }
                else
                {
                    Console.WriteLine("C'est maintenant " + CurrentTeam1Score + "-" + CurrentTeam2Score + " en faveur de " + BD.Procedure("TeamFromMatch", new OdbcParameter(":MatchId", CurrentMatchId)).Rows[1]["Name"]);
                }
            }
            else if(e.ProgressPercentage == (int)MatchEvent.START_NEXT_MATCH)
            {
                Console.WriteLine("Le match #" + CurrentMatchId + " opposant " + BD.Procedure("TeamFromMatch", new OdbcParameter(":MatchId", CurrentMatchId)).Rows[0]["Name"] + " contre " + BD.Procedure("TeamFromMatch", new OdbcParameter(":MatchId", CurrentMatchId)).Rows[1]["Name"] + " commence à l'instant!");
                StartMatch();
                PrepareNextMatch();
            }
        }

        private static void UploadScores()
        {
            StreamReader InRound = new StreamReader(@"C:\Users\max_l\Documents\steamcmd\csgoserver\csgo\backup_round" + (CurrentRoundNumber - 1).ToString("00") + ".txt");

            int TotalCT = 0;
            int TotalT = 0;

            while (!InRound.EndOfStream)
            {
                string ligne = InRound.ReadLine();
                if(CurrentRoundNumber > 6)
                {
                    if(ligne.Contains("FirstHalfScore"))
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

            if(Team1CTCurrentMatch)
            {
                BD.Procedure("SetRoundTeam1", new OdbcParameter(":NbRound", TotalCT), new OdbcParameter(":IdMatch", CurrentMatchId));
                BD.Procedure("SetRoundTeam2", new OdbcParameter(":NbRound", TotalT), new OdbcParameter(":IdMatch", CurrentMatchId));
            }
            else
            {
                BD.Procedure("SetRoundTeam1", new OdbcParameter(":NbRound", TotalT), new OdbcParameter(":IdMatch", CurrentMatchId));
                BD.Procedure("SetRoundTeam2", new OdbcParameter(":NbRound", TotalCT), new OdbcParameter(":IdMatch", CurrentMatchId));
            }
            
            InRound.Close();
            File.Delete(@"C:\Users\max_l\Documents\steamcmd\csgoserver\csgo\backup_round" + (CurrentRoundNumber - 1).ToString("00") + ".txt");
        }

        private static void StartMatch()
        {
            ConnectionServeur.ServerCommand("changelevel " + NextMap);
            Thread.Sleep(60000);


            Team1CTCurrentMatch = Team1CTNextMatch;
            CurrentMatchId = (int)BD.Procedure("IsMatchCurrent").Rows[0]["IdMatch"];
            CurrentRoundNumber = 1;
            CurrentTeam1Score = 0;
            CurrentTeam2Score = 0;
            ConnectionServeur.ServerCommand("tv_delay 10");
            ConnectionServeur.ServerCommand("exec gamestart");
        }

        private static void LookForEvents(object sender, DoWorkEventArgs e)
        {
            int Round = 1;
            while(true)
            {
                if(((BackgroundWorker)sender).CancellationPending)
                {
                    break;
                }
                if(Round == 0 && CurrentMatchId != (int)BD.Procedure("IsMatchCurrent").Rows[0]["IdMatch"])
                {
                    Round = 1;
                    ((BackgroundWorker)sender).ReportProgress((int)MatchEvent.START_NEXT_MATCH);
                }
                if(File.Exists(Serveur.StartInfo.FileName.Substring(0, Serveur.StartInfo.FileName.Length - 9) + "\\csgo\\backup_round" + Round.ToString("00") + ".txt"))
                {
                    Round++;
                    ((BackgroundWorker)sender).ReportProgress((int)MatchEvent.ROUND_ENDED);
                }

                Thread.Sleep(500);

                if(Round == 11 || CurrentTeam1Score == 6 || CurrentTeam2Score == 6)
                {
                    Thread.Sleep(4500);
                    Round = 0;
                    ((BackgroundWorker)sender).ReportProgress((int)MatchEvent.MATCH_ENDED);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CurrentMatchId">Match ID</param>
        /// <param name="VictoryTeam">Winner's team ID</param>
        private static void SetVictoryBets(int CurrentMatchId, int WinnerID)
        {
            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                DataTable bets = Bd.Procedure("GetBetsFromMatch", new OdbcParameter(":Id", CurrentMatchId));
                DataTable users = Bd.Select("user", "", new List<OdbcParameter>(), "Username", "Credit");

                Bd.Update("matchs", 
                    new List<string>() { "Team_Victoire" }, new List<OdbcParameter>() { new OdbcParameter(":Team_Victoire", WinnerID) }, 
                    "IdMatch = ?", new List<OdbcParameter>() { new OdbcParameter(":IdMatch", CurrentMatchId) });

                if (bets != null && bets.Rows.Count > 0)
                {
                    foreach (DataRow bet in bets.Rows)
                    {
                        try
                        {
                            int toAdd = (int)bet["Team_IdTeam"] == WinnerID ? (int)bet["Mise"] : 0;

                            if (toAdd > 0)
                            {
                                DataRow[] user = users.Select("Username = '" + bet["User_Username"].ToString() + "'");
                                int userCredit = user.Length > 0 ? (int)user[0]["Credit"] : -1;

                                if (userCredit != -1)
                                {
                                    List<OdbcParameter> values = new List<OdbcParameter>() { new OdbcParameter(":Credit", userCredit + toAdd) };
                                    List<OdbcParameter> cond = new List<OdbcParameter>() { new OdbcParameter(":Username", bet["User_Username"]) };
                                    Bd.Update("user", new List<string>() { "Credit" }, values, "Username = ?", cond);
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
