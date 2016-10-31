using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gobot.Models
{

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Nom d'utilisateur")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Nom d'utilisateur")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Adresse courriel")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Votre mot de passe doit comporter au moins {0} caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmation du mot de passe")]
        [Compare("Password", ErrorMessage = "Veuillez entrer exactement le mot de passe choisi ci-haut.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [MinLength(6, ErrorMessage = "Votre mot de passe doit comporter au moins {0} caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe courant")]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Votre mot de passe doit comporter au moins {0} caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmation du mot de passe")]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe entrés ne sont pas identiques.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ResetEmailViewModel
    {
        [Required]
        [Display(Name = "Nouvelle adresse courriel")]
        [EmailAddress]
        public string NewEmail { get; set; }

        [Required]
        [Display(Name = "Confirmer adresse courriel")]
        [Compare("NewEmail", ErrorMessage = "Les deux adresses courriels entrées ne sont pas identiques.")]
        [EmailAddress]
        public string ConfirmEmail { get; set; }
    }

    public class ResetImageViewModel
    {

    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
