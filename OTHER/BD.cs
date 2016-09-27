using System;
using System.Data;
using System.Collections.Generic;
using Oracle.DataAccess.Client;


namespace TriviaCrack
{
    /// <summary>
    /// Regroupement d'opérations effectués sur une base de données.
    /// </summary>
    static class BD
    {
        private static string source = "(DESCRIPTION="
                + "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)"
                + "(HOST=mercure.clg.qc.ca)(PORT=1521)))"
                + "(CONNECT_DATA=(SERVICE_NAME=ORCL.clg.qc.ca)))";

        public static string connString = "Data Source=" + source + ";User Id={0};Password={1};";

        /// <summary>
        /// Exécute une commande d'insertion dans la base de données.
        /// </summary>
        /// <param name="conn">Connection à la base de données</param>
        /// <param name="package">Nom du package et de la méthode</param>
        /// <param name="args">Liste d'arguments en IN à passer à la base de données</param>
        public static void insert(OracleConnection conn, string package, List<Args> args) 
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open(); // Ouvrir la connection

            // Définition de l'insertion SQL
            OracleCommand cmd = new OracleCommand(package.Substring(0, package.IndexOf('.')), conn);
            cmd.CommandText = package;
            cmd.CommandType = CommandType.StoredProcedure;

            if (args != null)
            {
                // Arguments en IN
                foreach (Args arg in args)
                {
                    OracleParameter param = new OracleParameter(arg.name, arg.type);
                    param.Direction = ParameterDirection.Input;
                    param.Value = arg.value;
                    cmd.Parameters.Add(param);
                }
            }

            cmd.ExecuteNonQuery(); // Exécuter la query
            conn.Close(); // Fermer la connection
        }

        /// <summary>
        /// Exécute une commande de modification dans la base de données.
        /// </summary>
        /// <param name="conn">Connection à la base de données</param>
        /// <param name="package">Nom du package et de la méthode</param>
        /// <param name="args">Liste d'arguments en IN à passer à la base de données</param>
        public static void modify(OracleConnection conn, string package, List<Args> args)
        {
            insert(conn, package, args);
        }

        /// <summary>
        /// Exécute une commande de suppression dans la base de données.
        /// </summary>
        /// <param name="conn">Connection à la base de données</param>
        /// <param name="package">Nom du package et de la méthode</param>
        /// <param name="args">Liste d'arguments en IN à passer à la base de données</param>
        public static void delete(OracleConnection conn, string package, List<Args> args)
        {
            insert(conn, package, args);
        }

        /// <summary>
        /// Recherche plusieurs objets dans la base de données.
        /// </summary>
        /// <param name="conn">Connection à la base de données</param>
        /// <param name="package">Nom du package et de la méthode</param>
        /// <param name="IN">Liste d'arguments en IN à passer à la base de données</param>
        /// <param name="OUT">Argument de retour de la base de données</param>
        /// <returns>Le dataset contenant les informations</returns>
        public static DataSet getDS(OracleConnection conn, string package, List<Args> IN, Args OUT)
        {
            DataSet ds = new DataSet();
            OracleCommand cmd = getCMD(conn, package, IN, OUT);
            OracleDataAdapter adapt = new OracleDataAdapter(cmd);
            adapt.Fill(ds, "table");
            adapt.Dispose();

            return ds;
        }

        /// <summary>
        /// Recherche un objet dans la base de données.
        /// </summary>
        /// <param name="conn">Connection à la base de données</param>
        /// <param name="package">Nom du package et de la méthode</param>
        /// <param name="IN">Liste d'arguments en IN à passer à la base de données</param>
        /// <param name="OUT">Argument de retour de la base de données</param>
        /// <returns>Le string contenant l'information recherchée.</returns>
        public static string getString(OracleConnection conn, string package, List<Args> IN, Args OUT)
        {
            OracleCommand cmd = getCMD(conn, package, IN, OUT);

            if (conn.State == ConnectionState.Closed)
                conn.Open();
            cmd.ExecuteScalar();
            conn.Close();

            return cmd.Parameters[OUT.name].Value.ToString();
        }

        /// <summary>
        /// Créé la commande prête à être envoyée à la base de données.
        /// </summary>
        /// <param name="conn">Connection à la base de données</param>
        /// <param name="package">Nom du package et de la méthode</param>
        /// <param name="IN">Liste d'arguments en IN à passer à la base de données</param>
        /// <param name="OUT">Argument de retour de la base de données</param>
        /// <returns>La commange prête à être envoyée à la base de données</returns>
        private static OracleCommand getCMD(OracleConnection conn, string package, List<Args> IN, Args OUT)
        {
            OracleCommand cmd = new OracleCommand(package.Substring(0, package.IndexOf('.')), conn);
            cmd.CommandText = package;
            cmd.CommandType = CommandType.StoredProcedure;
            
            // Argument en OUT
            OracleParameter returns = new OracleParameter(OUT.name, OUT.type, 1000);
            returns.Direction = OUT.direction;
            cmd.Parameters.Add(returns);
            
            if (IN != null)
            {
                // Arguments en IN
                foreach (Args arg in IN)
                {
                    OracleParameter param = new OracleParameter(arg.name, arg.type);
                    param.Direction = ParameterDirection.Input;
                    param.Value = arg.value;
                    cmd.Parameters.Add(param);
                }
            }

            return cmd;
        }

        /// <summary>
        /// Exécute une commande pour compter des éléments dans la base de données.
        /// </summary>
        /// <param name="conn">Connection à la base de données</param>
        /// <param name="package">Nom du package et de la méthode</param>
        /// <param name="IN">Liste d'arguments en IN à passer à la base de données</param>
        /// <param name="OUT">Argument de retour de la base de données</param>
        /// <returns>Le résultat du compte effectué</returns>
        public static int count(OracleConnection conn, string package, List<Args> IN, Args OUT)
        {
            OracleCommand cmd = getCMD(conn, package, IN, OUT);

            if (conn.State == ConnectionState.Closed)
                conn.Open();
            cmd.ExecuteScalar();
            conn.Close();

            return int.Parse(cmd.Parameters[OUT.name].Value.ToString());
        }

        /// <summary>
        /// Créé une liste de chaines contenant la donnée des premières colonnes de tout les lignes d'un dataset.
        /// </summary>
        /// <param name="ds">Dataset à transférer dans une liste</param>
        /// <returns>Liste de chaines</returns>
        public static List<string> toList(DataSet ds)
        {
            List<string> strings = new List<string>();

            foreach (DataRow dr in ds.Tables[0].Rows)
                strings.Add(dr[0].ToString());

            return strings;
        }

        /// <summary>
        /// Initialiser la connection à la base de données.
        /// </summary>
        /// <param name="connection">Connection à la base de données Oracle.</param>
        /// <param name="user">Nom d'utilisateur</param>
        /// <param name="password">Mot de passe</param>
        public static void initConnect(OracleConnection connection, string user, string password)
        {
            connection.ConnectionString = string.Format(connString, user, password);
        }

        /// <summary>
        /// Réorganize aléatoirement une liste.
        /// </summary>
        /// <typeparam name="T">Type de la liste</typeparam>
        /// <param name="list">Liste a "randomize"</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    /// <summary>
    /// Contient les informations d'un paramètre passé à une base de données.
    /// </summary>
    public class Args
    {
        public string name { get; private set; }
        public string value { get; private set; }
        public OracleDbType type { get; private set; }
        public ParameterDirection direction { get; private set; }

        /// <summary>
        /// Constructeur paramétrique d'un argument passé en paramètre à une base de données.
        /// </summary>
        /// <param name="name_">Nom du paramètre</param>
        /// <param name="value_">Valeur du paramètre</param>
        /// <param name="type_">Type du paramètre</param>
        /// <param name="direction_">Direction du paramètre</param>
        public Args(string name_, string value_, OracleDbType type_, ParameterDirection direction_ = ParameterDirection.Input)
        {
            name = name_;
            value = value_;
            type = type_;
            direction = direction_;
        }
    }
}
