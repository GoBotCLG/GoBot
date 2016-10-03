using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Gobot.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Votre mot de passe doit comporter au moins {0} caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmation du nouveau mot de passe")]
        [Compare("NewPassword", ErrorMessage = "Veuillez entrer exactement le mot de passe choisi ci-haut.")]
        public string ConfirmPassword { get; set; }
    }
}