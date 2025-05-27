﻿using System.ComponentModel.DataAnnotations;

namespace Streetcode.BLL.DTO.Sources;

public class StreetcodeCategoryContentDTO
{
    [Required]
    [MaxLength(1000)]
    public string Text { get; set; }

    [Required]
    public int SourceLinkCategoryId { get; set; }

    [Required]
    public int StreetcodeId { get; set; }
}