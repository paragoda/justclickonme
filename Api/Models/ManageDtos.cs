using System.ComponentModel.DataAnnotations;

namespace Api.Models;

internal record LinkOutput(

    string Slug,

    string Destination
);

public record CreateLinkInput(

    [Required(ErrorMessage = "Slug is required")]
    string Slug,

    [Required(ErrorMessage = "Destination is required to redirect somewhere")]
    string Destination
);