using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Api.Auth;

internal record RegisterInput(

    [Required(ErrorMessage = "Nickname is required")]
    string Nickname,

    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    string Password,

    [Required(ErrorMessage = "Password confirmation is required")]
    string ConfirmPassword = ""
);

internal record LoginInput(

    [Required(ErrorMessage = "Email is required")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    string Password
);