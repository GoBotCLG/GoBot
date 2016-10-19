using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Liaison_BD___CSGO
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern int SetForegroundWindow(IntPtr window);

        static void Main(string[] args)
        {
            MySQLWrapper BD = new MySQLWrapper();
            Process Serveur = new Process();
            Serveur.StartInfo.FileName = "C:\\Users\\max_l\\Documents\\steamcmd\\csgoserver\\srcds.exe";
            Serveur.StartInfo.Arguments = "-game csgo -console -usercon -maxplayers_override 11 +game_type 0 +game_mode 1 +mapgroup mg_active +map de_dust2 +sv_cheats 1 +bot_join_after_player 1 +mp_autoteambalance 0 +mp_limitteams 30";
            Serveur.StartInfo.ErrorDialog = true;
            Serveur.Start();
            foreach (string arg in Serveur.StartInfo.Verbs)
            {
                Console.Out.WriteLine(arg);
            }
            Console.Out.WriteLine(Serveur.StartInfo.Arguments);
            string ligne = "";
            do
            {
                ligne = Console.In.ReadLine();
                IntPtr window = Serveur.MainWindowHandle;
                SetForegroundWindow(window);
                SendKeys.SendWait(ligne + "\n");
            } while (ligne.ToUpper() != "EXIT");
            Serveur.WaitForExit();
        }
    }
}
