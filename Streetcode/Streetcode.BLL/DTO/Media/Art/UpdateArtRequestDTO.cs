using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.DTO.Media.Art;

public record UpdateArtRequestDTO
{
    [Required(ErrorMessage = "Ідентифікатор мистецтва є обов'язковим.")]
    public int Id { get; init; }

    [MaxLength(150, ErrorMessage = "Назва мистецтва не може перевищувати 150 символів.")]
    public string? Title { get; init; }

    [MaxLength(400, ErrorMessage = "Опис мистецтва не може перевищувати 400 символів.")]
    public string? Description { get; init; }
}
