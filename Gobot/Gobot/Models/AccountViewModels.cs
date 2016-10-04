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
        [MinLength(10, ErrorMessage = "Votre mot de passe doit comporter au moins {0} caractères")]
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
        [Display(Name = "Nom d'utilisateur")]
        public string UserName { get; }

        [Required]
        [MinLength(10, ErrorMessage = "Votre mot de passe doit comporter au moins {0} caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmation du mot de passe")]
        [Compare("Password", ErrorMessage = "Veuillez entrer exactement le mot de passe choisi ci-haut.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
