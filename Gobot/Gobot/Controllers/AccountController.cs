using System.Web.Mvc;
using Gobot.Models;

namespace Gobot.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string UserName, string PassWord)
        {

            return RedirectToAction("Index", "Watch");
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