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
            {
                return RedirectToAction("Index", "Home");
            }

            MySQLWrapper Bd = new MySQLWrapper();
            User user = Bd.GetUserFromDB(((User)Session["User"]).Username); ;
            Session["User"] = user;
            Session["User_img"] = user.ProfilPic == "" ? "/Images/profiles/anonymous.png" : user.ProfilPic;

            return View((User)Session["User"]);
        }

        [HttpPost]
        public JsonResult UpdatePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Les deux mots de passe ne sont pas identiques.";
            }
            else if (confirmPassword.Length > 45 || confirmPassword.Length < 6)
            {
                ViewBag.Error = "Le mot de passes saisi est invalide. Le mot de passe doit comporter un minimum de 6 caractères et un maximum de 45 caractères.";
            }
            else
            {
                try
                {
                    MySQLWrapper Bd = new MySQLWrapper();
                    DataTable user = Bd.Select("user", "Username = ?", new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) }, "Password");
                    if (user != null && user.Rows.Count > 0)
                    {
                        if (user.Rows[0]["Password"].ToString() == PasswordEncrypter.EncryptPassword(oldPassword))
                        {
                            int result = Bd.Update("user",
                                new List<string>() { "Password" },
                                new List<OdbcParameter>() { new OdbcParameter(":Password", PasswordEncrypter.EncryptPassword(newPassword)) },
                                "Username = ?", new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) });

                            if (result == 1)
                                ViewBag.Success = "Votre mot de passe a été modifié avec succès.";
                            else
                                ViewBag.Error = "Une erreur est survenue lors de la modification du mot de passe.";
                        }
                        else
                        {
                            ViewBag.Error = "Le mot de passe courant saisi est invalide.";
                        }
                    }
                }
                catch (Exception)
                {
                    ViewBag.Error = "Une erreur est survenue lors de la modification du mot de passe.";
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
                        ViewBag.Error = "Échec du téléversement de l'image.";
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
                    string fileName;
                    if (format.Equals(ImageFormat.Jpeg))
                    {
                        fileName = getFileName("jpg");
                        bmp.Save(fileName, ImageFormat.Jpeg);
                    }
                    else
                    {
                        fileName = getFileName("png");
                        bmp.Save(fileName, ImageFormat.Png);
                    }

                    
                    string toDelete = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"Gobot\profiles\{0}");
                    try {
                        System.IO.File.Delete(String.Format(toDelete, getImagePathFromDb(((User)Session["User"]).Username)));
                    }
                    catch (Exception) { }

                    setImagePathToDb(((User)Session["User"]).Username, fileName);
                    

                    ViewBag.Success = "Votre image de profil a été modifiée avec succès.";
                }
                catch (Exception)
                {
                    ViewBag.Error = "Erreur lors du téléversement de l'image.";
                }
            }
            else
            {
                ViewBag.Error = "Le type de l'image téléversée est invalide. Les types valides sont: .jpeg et .png.";
            }
        }

        private string getImagePathFromDb(string user)
        {
            return ""; // TODO : Get old image path from DB
        }

        private void setImagePathToDb(string user, string path)
        {
            // TODO : Update new image path in DB to 'path'
        }

        private string getFileName(string ext)
        {
            // AppDomain.CurrentDomain.BaseDirectory ???
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"Gobot\profiles\{0}." + ext);
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
                ViewBag.Error = "Les deux adresse courriels saisies ne sont pas identiques.";
            }
            else if (!new Regex("^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,}$").IsMatch(confirmEmail.ToUpper()))
            {
                ViewBag.Error = "L'adresse courriel saisie est invalide.";
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
                        ViewBag.Success = "Votre adresse courriel a été modifiée avec succès.";
                    else
                        ViewBag.Error = "Une erreur est survenue lors de la modification de votre adresse courriel.";
                }
                catch (Exception)
                {
                    ViewBag.Error = "Une erreur est survenue lors de la modification de votre adresse courriel.";
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
                ViewBag.Error = "Le nom d'utilisateur est déja utilisé.";
                Erreur = true;
            }

            if (!Erreur)
            {
                if (user.Username.Length < 6 || user.Username.Length > 45)
                {
                    ViewBag.Error = "Le nom d'utilisateur saisi est invalide. Il doit comporter entre 6 et 45 caractères.";
                    Erreur = true;
                }
            }

            if (!Erreur)
            {
                Regex r = new Regex("^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,}$");
                if (!r.IsMatch(user.Email.ToUpper()))
                {
                    ViewBag.Error = "L'adresse courriel saisie est invalide.";
                    Erreur = true;
                }
            }

            if (!Erreur)
            {
                if (user.Password.Length < 6)
                {
                    //Le plus gros mot de passe acceptable pour notre système d'encryption c'est ~= 2'091'752 terabytes, pas besoin de le spécifier?
                    ViewBag.Error = "Le mot de passe saisi est invalide. Il doit comporter entre 6 et 45 caractères.";
                    Erreur = true;
                }
            }

            if (!Erreur)
            {
                if (user.Password != user.ConfirmPassword) // If both passwords aren't the same
                {
                    ViewBag.Error = "Les mots de passe saisis ne sont pas identiques.";
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

                return RedirectToAction("Index", "Account");
            }
        }

        [HttpPost]
        public JsonResult Modify(string member, object newValue, string password = "")
        {
            try
            {
                MySQLWrapper Bd = new MySQLWrapper();

                List<string> col = new List<string>();
                if (member == "Image")
                {
                    col.Add("Image");
                }
                else if (member == "SteamProfile")
                {
                    col.Add("SteamProfile");
                }
                else if (member == "Password")
                {
                    List<OdbcParameter> list = new List<OdbcParameter>();
                    list.Add(new OdbcParameter(":Username", ((User)Session["User"]).Username));
                    string salt = Bd.Select("user", "Username = ?", list, "Password").Rows[0][0].ToString().Substring(0, 64);

                    DataTable ConnectResult = Bd.Procedure("Connect", new OdbcParameter(":Username", ((User)Session["User"]).Username), new OdbcParameter(":Password", PasswordEncrypter.EncryptPassword(password, salt)));

                    if (ConnectResult.Rows.Count > 0)
                    {
                        col.Add("Password");
                        newValue = PasswordEncrypter.EncryptPassword(newValue.ToString());
                    }
                    else
                    {
                        return Json(-1);
                    }
                }

                if (col.Count > 0)
                {
                    List<OdbcParameter> param = new List<OdbcParameter>() { new OdbcParameter(":" + col, newValue) };
                    List<OdbcParameter> usernameList = new List<OdbcParameter>();
                    OdbcParameter user = new OdbcParameter(":Username", OdbcType.VarChar, 45);
                    user.Value = ((User)Session["User"]).Username;
                    usernameList.Add(user);
                    int result = Bd.Update("user", col, param, "Username = ?", usernameList);
                    if (result == 1)
                    {
                        Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                        return Json(1, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(0, JsonRequestBehavior.DenyGet);
                    }
                }
                else
                {
                    return Json(0, JsonRequestBehavior.DenyGet);
                }

            }
            catch
            {
                //Error
                return Json(0, JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        public ActionResult Remove()
        {
            MySQLWrapper Bd = new MySQLWrapper();
            List<OdbcParameter> list = new List<OdbcParameter>();
            OdbcParameter username = new OdbcParameter(":Username", OdbcType.VarChar, 45);
            username.Value = ((User)Session["User"]).Username;
            list.Add(username);
            int DeleteResult = Bd.Delete("user", "Username = ?", list);
            if (DeleteResult == 1)
            {
                Session["User"] = null;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
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