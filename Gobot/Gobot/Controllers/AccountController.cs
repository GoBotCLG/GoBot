using System.Web.Mvc;
using Gobot.Models;
using System.Data.Odbc;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Gobot.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Index()
        {
            if ((User)Session["User"] == null)
                return RedirectToAction("Index", "Home");

            MySQLWrapper Bd = new MySQLWrapper();
            User user = Bd.GetUserFromDB(((User)Session["User"]).Username); ;
            Session["User"] = user;
            Session["User_img"] = user.ProfilPic == "" ? "/Images/profiles/anonymous.png" : user.ProfilPic;
            Session["limitBetRefreshDate"] = DateTime.Now;

            return View((User)Session["User"]);
        }
        
        public JsonResult UpdatePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["error"] = "Les deux mots de passe ne sont pas identiques.";
            }
            else if (confirmPassword.Length > 45 || confirmPassword.Length < 6)
            {
                TempData["error"] = "Le mot de passes saisi est invalide. Le mot de passe doit comporter un minimum de 6 caractères et un maximum de 45 caractères.";
            }
            else
            {
                try
                {
                    MySQLWrapper Bd = new MySQLWrapper();
                    DataTable userDB = Bd.Select("user", "Username = ?", new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) }, "Password");
                    string oldPasswordDB = userDB != null && userDB.Rows.Count > 0 ? userDB.Rows[0]["Password"].ToString() : "";
                    DataTable user = Bd.Procedure("Connect", new OdbcParameter(":username", ((User)Session["User"]).Username), new OdbcParameter(":password", PasswordEncrypter.EncryptPassword(oldPassword, oldPasswordDB.Substring(0, 64))));

                    if (user != null && user.Rows.Count > 0)
                    {
                        int result = Bd.Update("user",
                            new List<string>() { "Password" },
                            new List<OdbcParameter>() { new OdbcParameter(":Password", PasswordEncrypter.EncryptPassword(newPassword)) },
                            "Username = ?", new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) });

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
        public JsonResult UpdateImage(FormCollection data)
        {
            if (Request.Files["file"] != null)
            {
                using (var binaryReader = new BinaryReader(Request.Files["file"].InputStream))
                {
                    try
                    {
                        Image img;
                        byte[] Imagefile = binaryReader.ReadBytes(Request.Files["file"].ContentLength);
                        using (var ms = new MemoryStream(Imagefile))
                        {
                            img = Image.FromStream(ms);

                            downloadImage(ref img);
                        }
                    }
                    catch (ArgumentException)
                    {
                        TempData["error"] = "Échec du téléversement de l'image.";
                    }
                }
            }
            return Json((User)Session["User"], JsonRequestBehavior.AllowGet);
        }

        private void downloadImage(ref Image img)
        {
            ImageFormat format = img.RawFormat;
            Bitmap bmp = new Bitmap(img);
            if (format.Equals(ImageFormat.Jpeg) || format.Equals(ImageFormat.Png))
            {
                try
                {
                    // AppDomain.CurrentDomain.BaseDirectory ???
                    string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"Gobot\profiles\");

                    if (!Directory.Exists(basePath))
                        Directory.CreateDirectory(basePath);

                    string fileName;
                    if (format.Equals(ImageFormat.Jpeg))
                    {
                        fileName = getFileName(basePath, "jpg");
                        bmp.Save(fileName, ImageFormat.Jpeg);
                    }
                    else
                    {
                        fileName = getFileName(basePath, "png");
                        bmp.Save(fileName, ImageFormat.Png);
                    }
                    
                    try {
                        string oldPath = getImagePathFromDb(((User)Session["User"]).Username);
                        if (oldPath != null && oldPath.IndexOf("anonymous") == -1)
                            System.IO.File.Delete(oldPath);
                    }
                    catch (Exception) { }

                    if (setImagePathToDb(((User)Session["User"]).Username, fileName) == 1)
                        TempData["success"] = "Votre image de profil a été modifiée avec succès.";
                    else
                        TempData["error"] = "Erreur lors du téléversement de l'image.";
                }
                catch (Exception)
                {
                    TempData["error"] = "Erreur lors du téléversement de l'image.";
                }
            }
            else
            {
                TempData["error"] = "Le type de l'image téléversée est invalide. Les types valides sont: .jpeg et .png.";
            }
        }

        private string getImagePathFromDb(string user)
        {
            DataTable userImg = new MySQLWrapper().Select("user", "Username = ?", new List<OdbcParameter>() { new OdbcParameter("", user) }, "Image");
            return userImg == null || userImg.Rows.Count == 0 ? null : userImg.Rows[0]["Image"].ToString();
        }

        private int setImagePathToDb(string user, string fileName)
        {
            return new MySQLWrapper().Update("user",
                        new List<string>() { "Image" },
                        new List<OdbcParameter>() { new OdbcParameter(":Image", PasswordEncrypter.EncryptPassword(fileName)) },
                        "Username = ?", new List<OdbcParameter>() { new OdbcParameter(":Username", user) });
        }

        private string getFileName(string basePath, string ext)
        {
            string path = basePath += @"{0}." + ext;
            string file;

            do
                file = String.Format(path, getRandomString());
            while (System.IO.File.Exists(file));

            return file;
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
                        new List<OdbcParameter>() { new OdbcParameter(":Email", confirmEmail) },
                        "Username = ?", new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) });

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
            if ((User)Session["User"] == null)
            {
                return Json(0, JsonRequestBehavior.DenyGet);
            }

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

            DataTable isUser = Bd.Procedure("IsUser", new OdbcParameter(":username", user.Username));
            
            if (Convert.ToInt32(isUser.Rows[0][0]) != 0)
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
                Bd.Procedure("AddUser", new OdbcParameter(":username", user.Username), new OdbcParameter(":Email", user.Email), new OdbcParameter(":steamprofile", ""), new OdbcParameter(":password", encPassword));

                Session["User"] = Bd.GetUserFromDB(user.Username);
                TempData["success"] = "Votre compte a été créé. Vous vous trouvez actuellement sur votre page de gestion de compte où vous pouvez voir tout vos activités sur Gobot.";

                return RedirectToAction("Index", "Account");
            }
        }

        [HttpPost]
        public ActionResult Remove()
        {
            MySQLWrapper Bd = new MySQLWrapper();
            List<OdbcParameter> user = new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) };

            //int DeleteResult = Bd.Delete("user", "Username = ?", user);
            //if (DeleteResult == 1)
            //{
            //    Logout();
            //    TempData["success"] = "Votre compte à été supprimé.";
            //    return RedirectToAction("Index", "Home");
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Account");
            //}
            return RedirectToAction("Index", "Account");
        }

        public ActionResult AddCredit()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}