using System.Web.Mvc;
using Gobot.Models;
using System.Data.Odbc;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;

namespace Gobot.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Index()
        {
            if((User)Session["User"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            MySQLWrapper Bd = new MySQLWrapper("Max", "yolo");
            DataTable UserResult = Bd.Procedure("GetUser", new OdbcParameter(":username", ((User)Session["User"]).Username));

            if (UserResult.Rows.Count > 0)
            {
                User sessionuser = new User();
                sessionuser.Username = UserResult.Rows[0]["Username"].ToString();
                sessionuser.Email = UserResult.Rows[0]["Email"].ToString();
                if (UserResult.Rows[0]["Image"].GetType() != typeof(System.DBNull))
                {
                    byte[] imagebytes = (byte[])UserResult.Rows[0]["Image"];
                    TypeConverter tc = TypeDescriptor.GetConverter(typeof(System.Drawing.Bitmap));
                    sessionuser.ProfilPic = (System.Drawing.Bitmap)tc.ConvertFrom(imagebytes);
                }
                sessionuser.Credits = (int)UserResult.Rows[0]["Credit"];
                sessionuser.SteamID = UserResult.Rows[0]["SteamProfile"].ToString();
                sessionuser.Wins = (int)UserResult.Rows[0]["Win"];
                sessionuser.Games = (int)UserResult.Rows[0]["Game"];
                sessionuser.TotalCredits = (int)UserResult.Rows[0]["TotalCredit"];
                sessionuser.EXP = (int)UserResult.Rows[0]["EXP"];
                sessionuser.Level = (int)UserResult.Rows[0]["LVL"];
                Session["User"] = sessionuser;

                return View((User)Session["User"]);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
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
            if(user.Password == user.ConfirmPassword)
            {
                MySQLWrapper Bd = new MySQLWrapper("Max", "yolo");
                string encPassword = PasswordEncrypter.EncryptPassword(user.Password);
                Bd.Procedure("AddUser", new OdbcParameter(":username", user.Username), new OdbcParameter(":Email", user.Email), new OdbcParameter(":Image", new byte[0]), new OdbcParameter(":steamprofile", ""), new OdbcParameter(":password", encPassword));

                OdbcParameter username = new OdbcParameter(":Username", user.Username);
                List<OdbcParameter> parameters = new List<OdbcParameter>();
                parameters.Add(username);

                DataTable UserResult = Bd.Procedure("GetUser", new OdbcParameter(":username", user.Username));
                User sessionuser = new User();
                sessionuser.Username = UserResult.Rows[0]["Username"].ToString();
                sessionuser.Email = UserResult.Rows[0]["Email"].ToString();
                if (UserResult.Rows[0]["Image"].GetType() != typeof(System.DBNull))
                {
                    if(((byte[])UserResult.Rows[0]["Image"]).Length > 0)
                    {
                        byte[] imagebytes = (byte[])UserResult.Rows[0]["Image"];
                        TypeConverter tc = TypeDescriptor.GetConverter(typeof(System.Drawing.Bitmap));
                        sessionuser.ProfilPic = (System.Drawing.Bitmap)tc.ConvertFrom(imagebytes);
                    }
                    else
                    {
                        sessionuser.ProfilPic = new System.Drawing.Bitmap(1, 1);
                    }
                }
                else
                {
                    sessionuser.ProfilPic = new System.Drawing.Bitmap(1, 1);
                }
                
                sessionuser.Credits = (int)UserResult.Rows[0]["Credit"];
                sessionuser.SteamID = UserResult.Rows[0]["SteamProfile"].ToString();
                sessionuser.Wins = (int)UserResult.Rows[0]["Win"];
                sessionuser.Games = (int)UserResult.Rows[0]["Game"];
                sessionuser.TotalCredits = (int)UserResult.Rows[0]["TotalCredit"];
                sessionuser.EXP = (int)UserResult.Rows[0]["EXP"];
                sessionuser.Level = (int)UserResult.Rows[0]["LVL"];
                Session["User"] = sessionuser;

                return RedirectToAction("Index", "Account");
            }
            else
            {
                ViewBag.Error = "Les mots de passe entrés ne sont pas identiques.";
                return View();
            }
        }

        [HttpPost]
        public JsonResult Modify()
        {
            try
            {
                //Do stuff
                return Json(1);
            }
            catch
            {
                //Error
                return Json(0);
            }
        }

        [HttpPost]
        public ActionResult Remove()
        {
            //Remove from DB
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddCredit()
        {
            return View();
        }
    }
}