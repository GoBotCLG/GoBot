using System.Web.Mvc;
using Gobot.Models;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Gobot.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Index(string Username)
        {
            if ((User)Session["User"] == null || ((User)Session["User"]).Username == "")
                return RedirectToAction("Index", "Home");

            MySQLWrapper Bd = new MySQLWrapper();
            if (string.IsNullOrEmpty(Username))
            {
                User user = Bd.GetUserFromDB(((User)Session["User"]).Username);

                if (user == null)
                    return RedirectToAction("Index", "Home");

                DataTable favTeamBd = Bd.Procedure("GetFavoriteTeam", new MySqlParameter(":Username", user.Username));
                DataTable totalBetsWon = Bd.Procedure("GetTotalBetWon", new MySqlParameter(":Username", user.Username));
                DataTable totalBetsLost = Bd.Procedure("GetTotalBetLost", new MySqlParameter(":Username", user.Username));
                user.favoriteTeam = (favTeamBd == null || favTeamBd.Rows.Count == 0) ? null : favTeamBd.Rows[0][0].ToString();
                user.totalWon = (totalBetsWon == null || totalBetsWon.Rows.Count == 0 || totalBetsWon.Rows[0][0] == null || totalBetsWon.Rows[0][0].ToString() == "") ? 0 : int.Parse(totalBetsWon.Rows[0][0].ToString());
                user.totalLoss = (totalBetsLost == null || totalBetsLost.Rows.Count == 0 || totalBetsLost.Rows[0][0] == null || totalBetsLost.Rows[0][0].ToString() == "") ? 0 : int.Parse(totalBetsLost.Rows[0][0].ToString());

                user.SessionUser = true;
                Session["User"] = user;
                Session["User_img"] = user.ProfilPic == "" ? "/Images/profiles/anonymous.png" : user.ProfilPic;
                Session["limitBetRefreshDate"] = DateTime.Now;
                return View((User)Session["User"]);
            }
            else
            {
                User user = Bd.GetUserFromDB(Username);

                DataTable favTeamBd = Bd.Procedure("GetFavoriteTeam", new MySqlParameter(":Username", user.Username));
                DataTable totalBetsWon = Bd.Procedure("GetTotalBetWon", new MySqlParameter(":Username", user.Username));
                DataTable totalBetsLost = Bd.Procedure("GetTotalBetLost", new MySqlParameter(":Username", user.Username));
                user.favoriteTeam = (favTeamBd == null || favTeamBd.Rows.Count == 0) ? null : favTeamBd.Rows[0][0].ToString();
                user.totalWon = (totalBetsWon == null || totalBetsWon.Rows.Count == 0 || totalBetsWon.Rows[0][0] == null || totalBetsWon.Rows[0][0].ToString() == "") ? 0 : int.Parse(totalBetsWon.Rows[0][0].ToString());
                user.totalLoss = (totalBetsLost == null || totalBetsLost.Rows.Count == 0 || totalBetsLost.Rows[0][0] == null || totalBetsLost.Rows[0][0].ToString() == "") ? 0 : int.Parse(totalBetsLost.Rows[0][0].ToString());

                user.SessionUser = false;
                return View(user);
            }
        }

        public JsonResult UpdateSteamLink(string newLink)
        {
            if ((User)Session["User"] == null || ((User)Session["User"]).Username == "")
                return Json(0, JsonRequestBehavior.DenyGet);

            if (newLink == null || newLink == "" || !newLink.All(c => Char.IsLetterOrDigit(c)))
            {
                TempData["error"] = "Le lien entré contient des caractères invalides.";
                return Json("", JsonRequestBehavior.AllowGet);
            }

            int update = new MySQLWrapper().Update("user", 
                new List<string>() { "SteamProfile" }, new List<MySqlParameter>() { new MySqlParameter(":SteamProfile", newLink) }, 
                "Username = ?", new List<MySqlParameter>() { new MySqlParameter(":Username", ((User)Session["User"]).Username) });

            if (update > 0)
                TempData["success"] = "Le lien vers votre compte Steam à été modifié avec succès.";
            else
                TempData["error"] = "Une erreur est survenue lors de la modification du lien vers votre compte Steam.";

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdatePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if ((User)Session["User"] == null || ((User)Session["User"]).Username == "")
                return Json(0, JsonRequestBehavior.DenyGet);

            if (newPassword != confirmPassword)
            {
                TempData["error"] = "Les deux mots de passe ne sont pas identiques.";
            }
            else if (confirmPassword.Length > 45 || confirmPassword.Length < 6)
            {
                TempData["error"] = "Le mot de passes saisi est invalide. Le mot de passe doit comporter un minimum de 6 caractères et un maximum de 45 caractères.";
            }
            else if (!confirmPassword.All(c => Char.IsLetterOrDigit(c) || c == '_'))
            {
                TempData["error"] = "Le mot de passe saisi est invalide. Il ne peut comporter que des lettres, des chiffres et des barres de soulignement (_).";
            }
            else
            {
                try
                {
                    MySQLWrapper Bd = new MySQLWrapper();
                    DataTable userDB = Bd.Select("user", "Username = ?", new List<MySqlParameter>() { new MySqlParameter(":Username", ((User)Session["User"]).Username) }, "Password");
                    string oldPasswordDB = userDB != null && userDB.Rows.Count > 0 ? userDB.Rows[0]["Password"].ToString() : "";
                    DataTable user = Bd.Procedure("Connect", new MySqlParameter(":username", ((User)Session["User"]).Username), new MySqlParameter(":password", PasswordEncrypter.EncryptPassword(oldPassword, oldPasswordDB.Substring(0, 64))));

                    if (user != null && user.Rows.Count > 0)
                    {
                        int result = Bd.Update("user",
                            new List<string>() { "Password" },
                            new List<MySqlParameter>() { new MySqlParameter(":Password", PasswordEncrypter.EncryptPassword(newPassword)) },
                            "Username = ?", new List<MySqlParameter>() { new MySqlParameter(":Username", ((User)Session["User"]).Username) });

                        if (result == 1)
                            TempData["success"] = "Votre mot de passe a été modifié avec succès.";
                        else
                            TempData["error"] = "Une erreur est survenue lors de la modification du mot de passe.";
                    }
                    else
                        TempData["error"] = "Le mot de passe courant saisi est invalide.";
                }
                catch (Exception)
                {
                    TempData["error"] = "Une erreur est survenue lors de la modification du mot de passe.";
                }
            }
            
            return Json((User)Session["User"], JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateImage(FormCollection data)
        {
            if ((User)Session["User"] == null || ((User)Session["User"]).Username == "")
                return Json(0, JsonRequestBehavior.DenyGet);

            if (Request.Files["ImageUploader"] != null)
            {
                using (var binaryReader = new BinaryReader(Request.Files["ImageUploader"].InputStream))
                {
                    try
                    {
                        Image img;
                        byte[] Imagefile = binaryReader.ReadBytes(Request.Files["ImageUploader"].ContentLength);
                        using (var ms = new MemoryStream(Imagefile))
                        {
                            img = Image.FromStream(ms);

                            ImageFormat format = img.RawFormat;
                            Bitmap bmp = new Bitmap(img);
                            if (format.Equals(ImageFormat.Jpeg) || format.Equals(ImageFormat.Png))
                            {
                                try
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory;
                                    string basePath = Path.Combine(dir, @"Images\profiles\");

                                    if (!Directory.Exists(basePath))
                                        Directory.CreateDirectory(basePath);

                                    string fileName = format.Equals(ImageFormat.Jpeg) ? getFileName(basePath, "jpg") : getFileName(basePath, "png");

                                    try
                                    {
                                        string oldPath = Path.Combine(dir, getImagePathFromDb(((User)Session["User"]).Username).Replace("/", "\\"));
                                        if (oldPath != null && oldPath.IndexOf("anonymous") == -1)
                                        {
                                            if (System.IO.File.Exists(oldPath))
                                                System.IO.File.Delete(oldPath);
                                        }
                                    }
                                    catch (Exception) { }

                                    string toSave = Path.Combine(basePath, fileName);
                                    if (format.Equals(ImageFormat.Jpeg))
                                        bmp.Save(toSave, ImageFormat.Jpeg);
                                    else
                                        bmp.Save(toSave, ImageFormat.Png);

                                    int upload = setImagePathToDb(((User)Session["User"]).Username, "/Images/profiles/" + fileName);
                                    if (upload > 0)
                                        TempData["success"] = "Votre image de profil a été modifiée avec succès.";
                                    else
                                        TempData["error"] = "Une erreur s'est produite lors du téléversement de l'image.";
                                }
                                catch (Exception)
                                {
                                    TempData["error"] = "Une erreur s'est produite lors du téléversement de l'image.";
                                }
                            }
                            else
                            {
                                TempData["error"] = "Le type de l'image téléversée est invalide. Les types valides sont: .jpeg et .png.";
                            }
                            bmp.Dispose();
                        }
                    }
                    catch (ArgumentException)
                    {
                        TempData["error"] = "Une erreur s'est produite lors du téléversement de l'image.";
                    }
                }
            }
            else
                TempData["error"] = "Une erreur s'est produite lors du téléversement de l'image.";

            return RedirectToAction("Index", "Account");
        }

        private string getImagePathFromDb(string user)
        {
            try
            {
                DataTable userImg = new MySQLWrapper().Select("user", "Username = ?", new List<MySqlParameter>() { new MySqlParameter("", user) }, "Image");
                return userImg == null || userImg.Rows.Count == 0 ? null : userImg.Rows[0]["Image"].ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private int setImagePathToDb(string user, string fileName)
        {
            try
            {
                return new MySQLWrapper().Update("user",
                            new List<string>() { "Image" },
                            new List<MySqlParameter>() { new MySqlParameter(":Image", fileName) },
                            "Username = ?", new List<MySqlParameter>() { new MySqlParameter(":Username", user) });
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private string getFileName(string basePath, string ext)
        {
            string path = basePath += @"{0}";
            string fileName;

            do
            {
                fileName = getRandomString() + "." + ext;
            }
            while (System.IO.File.Exists(String.Format(path, fileName)));

            return fileName;
        }

        private string getRandomString()
        {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            return GuidString;
        }

        [HttpPost]
        public JsonResult UpdateEmail(string newEmail, string confirmEmail)
        {
            if ((User)Session["User"] == null || ((User)Session["User"]).Username == "")
                return Json(0, JsonRequestBehavior.DenyGet);

            if (newEmail != confirmEmail)
            {
                TempData["error"] = "Les deux adresse courriels saisies ne sont pas identiques.";
            }
            else if (!new Regex("^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,}$").IsMatch(confirmEmail.ToUpper()))
            {
                TempData["error"] = "L'adresse courriel saisie est invalide.";
            }
            else
            {
                try
                {
                    int result = new MySQLWrapper().Update("user",
                        new List<string>() { "Email" },
                        new List<MySqlParameter>() { new MySqlParameter(":Email", confirmEmail) },
                        "Username = ?", new List<MySqlParameter>() { new MySqlParameter(":Username", ((User)Session["User"]).Username) });

                    if (result == 1)
                        TempData["success"] = "Votre adresse courriel a été modifiée avec succès.";
                    else
                        TempData["error"] = "Une erreur est survenue lors de la modification de votre adresse courriel.";
                }
                catch (Exception)
                {
                    TempData["error"] = "Une erreur est survenue lors de la modification de votre adresse courriel.";
                }
            }

            return Json((User)Session["User"], JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateAccountInfo()
        {
            if ((User)Session["User"] == null || ((User)Session["User"]).Username == "")
                return Json(0, JsonRequestBehavior.DenyGet);

            MySQLWrapper Bd = new MySQLWrapper();
            Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);

            return Json((User)Session["User"], JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Register()
        {
            RegisterViewModel user = new RegisterViewModel();
            return View(user);
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel user)
        {
            bool Erreur = false;

            MySQLWrapper Bd = new MySQLWrapper();

            DataTable isUser = Bd.Procedure("IsUser", new MySqlParameter(":username", user.Username));
            
            if (isUser == null || isUser.Rows.Count == 0)
            {
                TempData["error"] = "Une erreur s'est produite lors de l'inscription. Veuillez réessayer.";
                Erreur = true;
            }
            else if (Convert.ToInt32(isUser.Rows[0][0]) != 0)
            {
                TempData["error"] = "Le nom d'utilisateur est déja utilisé.";
                Erreur = true;
            }

            if (!Erreur)
            {
                if (user.Username.Length < 6 || user.Username.Length > 45)
                {
                    TempData["error"] = "Le nom d'utilisateur saisi est invalide. Il doit comporter entre 6 et 45 caractères.";
                    Erreur = true;
                }
            }

            if (!Erreur)
            {
                if (!user.Username.All(Char.IsLetterOrDigit))
                {
                    TempData["error"] = "Le nom d'utilisateur saisi est invalide. Il ne peut comporter que des lettres et des chiffres.";
                    Erreur = true;
                }
            }

            if (!Erreur)
            {
                if (!user.Username.All(c => Char.IsLetterOrDigit(c) || c == '_'))
                {
                    TempData["error"] = "Le mot de passe saisi est invalide. Il ne peut comporter que des lettres, des chiffres et des barres de soulignement (_).";
                    Erreur = true;
                }
            }

            if (!Erreur)
            {
                Regex r = new Regex("^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,}$");
                if (!r.IsMatch(user.Email.ToUpper()))
                {
                    TempData["error"] = "L'adresse courriel saisie est invalide.";
                    Erreur = true;
                }
            }

            if (!Erreur)
            {
                if (user.Password.Length < 6 || user.Password.Length > 45)
                {
                    TempData["error"] = "Le mot de passe saisi est invalide. Il doit comporter entre 6 et 45 caractères.";
                    Erreur = true;
                }
            }

            if (!Erreur)
            {
                if (user.Password != user.ConfirmPassword)
                {
                    TempData["error"] = "Les mots de passe saisis ne sont pas identiques.";
                    Erreur = true;
                }
            }

            if (Erreur)
                return View(user);
            else
            {
                string encPassword = PasswordEncrypter.EncryptPassword(user.Password);
                Bd.Procedure("AddUser", new MySqlParameter(":username", user.Username), new MySqlParameter(":Email", user.Email), new MySqlParameter(":steamprofile", ""), new MySqlParameter(":password", encPassword));

                Session["User"] = Bd.GetUserFromDB(user.Username);

                double offset;
                try
                {
                    offset = HomeController.GetTimeOffset(Request.Form["clientTime"]);
                }
                catch (Exception)
                {
                    offset = 0;
                }

                Session["timeOffset"] = offset;
                TempData["success"] = "Votre compte a été créé. Vous vous trouvez actuellement sur votre page de gestion de compte où vous pouvez voir tout vos activités sur Gobot.";

                return RedirectToAction("Index", "Account");
            }
        }

        [HttpPost]
        public ActionResult Remove()
        {
            if ((User)Session["User"] == null || ((User)Session["User"]).Username == "")
                return RedirectToAction("Index", "Home");

            MySQLWrapper Bd = new MySQLWrapper();
            List<MySqlParameter> user = new List<MySqlParameter>() { new MySqlParameter(":Username", ((User)Session["User"]).Username) };

            DataTable delete = Bd.Procedure("deleteUser", new MySqlParameter(":Pusername", ((User)Session["User"]).Username));
            try
            {
                if (delete.Rows.Count == 0)
                {
                    Logout();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Account");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Account");
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}