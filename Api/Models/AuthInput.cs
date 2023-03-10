using System.ComponentModel.DataAnnotations;

namespace Api.Models;

internal record RegisterInput(

    [EmailAddress(ErrorMessage = "Input isn't email")]
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
internal record GoogleInput(

    [Required(ErrorMessage = "IdToken is required")]
    string IdToken
);