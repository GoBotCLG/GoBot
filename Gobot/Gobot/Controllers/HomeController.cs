using System.Web.Mvc;
using Gobot.Models;

namespace Gobot.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            MySQLWrapper test = new MySQLWrapper("Max", "yolo");
            return View();
        }
    }
}