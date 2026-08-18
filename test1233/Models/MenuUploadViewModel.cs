using Microsoft.AspNetCore.Http;

namespace test1233.Models;

public class MenuUploadViewModel
{
    public IFormFile? MenuFile { get; set; }

    public MenuListItemViewModel? ActiveMenu { get; set; }

    public MenuListItemViewModel? LatestMenu { get; set; }

    public IReadOnlyList<MenuListItemViewModel> Menus { get; set; } = [];
}

public class MenuListItemViewModel
{
    public string Path { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; }

    public bool IsSelected { get; set; }
}
