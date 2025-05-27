using System.ComponentModel.DataAnnotations;
using Streetcode.BLL.DTO.Media.Images;

namespace Streetcode.BLL.DTO.Streetcode.TextContent.Fact
{
    public class FactUpdateCreateDTO : FactDTO
    {
        [Required]
        [MaxLength(68, ErrorMessage = "Заголовок факту не може перевищувати 68 символів.")]
        public string Title { get; set; }

        [Required]
        [MaxLength(600, ErrorMessage = "Основний текст факту не може перевищувати 600 символів.")]
        public string FactContent { get; set; }

        [Required]
        public ImageFileBaseCreateDTO Image { get; set; }

        [MaxLength(200, ErrorMessage = "Опис зображення не може перевищувати 200 символів.")]
        public string? ImageDescription { get; set; }
    }
}