using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public record CreateLinkInput(

    [Required(ErrorMessage = "Slug is required")]
    string Slug,

    [Required(ErrorMessage = "Destination is required to redirect somewhere")]
    string Destination
);