using Gobot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;

namespace Gobot.Controllers
{
    public class SupportController : Controller
    {
        public ActionResult FAQ()
        {
            if ((User)Session["User"] == null || ((User)Session["User"]).Username == "")
                return RedirectToAction("Index", "Home");
            else
                return View();
        }

        public ActionResult Contact()
        {
            return RedirectToAction("Index", "Home");
            //if ((User)Session["User"] == null || ((User)Session["User"]).Username == "")
            //    return RedirectToAction("Index", "Home");
            //else
            //    return View();
        }

        public ActionResult SendEmail()
        {
            if ((User)Session["User"] == null || ((User)Session["User"]).Username == "")
                return RedirectToAction("Index", "Home");

            string name = Request.Form["name"];
            string msg = Request.Form["msg"];

            if (name == null || name == "")
            {
                TempData["error"] = "Le nom ne peut pas vide.";
            }
            else if (msg != null && msg.Length > 15)
            {
                try
                {
                    SmtpClient smtpClient = new SmtpClient();
                    NetworkCredential basicCredential = new NetworkCredential("go.bots@outlook.com", "Yolo1234Sw4g1234");
                    MailMessage message = new MailMessage();

                    smtpClient.Host = "smtp-mail.outlook.com";
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = basicCredential;
                    smtpClient.EnableSsl = true;

                    message.From = new MailAddress("go.bots@outlook.com");
                    message.Subject = "Besoin d'assistance! - " + name + "(" + ((User)Session["User"]).Username + ")";
                    message.IsBodyHtml = true;
                    message.Body = "Name: " + name + "<br/>Username: " + ((User)Session["User"]).Username + "<br/>Email: " + ((User)Session["User"]).Email + "<br/><br/>" + msg;
                    message.To.Add("go.bots@outlook.com");
                    smtpClient.Send(message);

                    TempData["success"] = "Votre courriel à été envoyé avec succès. Notre équipe vous rejoindra sous peu.";
                }
                catch (Exception)
                {
                    TempData["error"] = "Une erreur est survenue lors de l'envoi du courriel.";
                }
            }
            else
                TempData["error"] = "Le message doit contenir au moins 15 caractères.";

            return RedirectToAction("Contact", "Support");
        }
    }
}