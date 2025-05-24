using System.ComponentModel.DataAnnotations;
using Streetcode.BLL.DTO.Media.Images;

namespace Streetcode.BLL.MediatR.Media.Art.Create
{
    public record ArtCreateRequestDTO
    {
        [MaxLength(150, ErrorMessage = "Назва мистецтва не може перевищувати 150 символів.")]
        public string? Title { get; init; }

        [MaxLength(400, ErrorMessage = "Опис мистецтва не може перевищувати 400 символів.")]
        public string? Description { get; init; }

        [Required(ErrorMessage = "Дані зображення є обов'язковими.")]
        public ImageFileBaseCreateDTO Image { get; init; }
    }
}
