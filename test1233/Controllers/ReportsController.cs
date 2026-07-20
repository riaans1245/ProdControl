using System.Collections.Generic;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController(IUserStore userStore) : Controller
{
    private readonly IUserStore _userStore = userStore;
    

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Roles()
    {
        return View(_userStore.GetAllRoles());
    }

    public IActionResult Users()
    {
        return View(_userStore.GetAllUsers());
    }

    public IActionResult Products()
    {
        return View(_userStore.GetAllProducts());
    }

    public IActionResult Categories()
    {
        return View(_userStore.GetAllCategories());
    }

    public IActionResult ContactUs()
    {
        return View(_userStore.GetAllContactUs());
    }

    public IActionResult ExportRoles()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Roles");

        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Code";

        var roles = _userStore.GetAllRoles();
        var row = 2;
        foreach (var role in roles)
        {
            worksheet.Cell(row, 1).Value = role.Id;
            worksheet.Cell(row, 2).Value = role.Name;
            worksheet.Cell(row, 3).Value = role.IdentityCode;
            row++;
        }

        return CreateExcelFile(workbook, "RoleReport.xlsx");
    }

    public IActionResult ExportUsers()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Users");

        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Surname";
        worksheet.Cell(1, 4).Value = "EmailAddress";
        worksheet.Cell(1, 5).Value = "CellNo";
        worksheet.Cell(1, 6).Value = "Username";
        worksheet.Cell(1, 7).Value = "Role";

        var users = _userStore.GetAllUsers();
        var row = 2;
        foreach (var user in users)
        {
            worksheet.Cell(row, 1).Value = user.Id;
            worksheet.Cell(row, 2).Value = user.Name;
            worksheet.Cell(row, 3).Value = user.Surname;
            worksheet.Cell(row, 4).Value = user.EmailAddress;
            worksheet.Cell(row, 5).Value = user.CellNo;
            worksheet.Cell(row, 6).Value = user.Username;
            worksheet.Cell(row, 7).Value = $"{user.RoleId} - {user.Role}";
            row++;
        }

        return CreateExcelFile(workbook, "UserReport.xlsx");
    }

    public IActionResult ExportProducts()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Products");

        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Product";
        worksheet.Cell(1, 3).Value = "Price";
        worksheet.Cell(1, 4).Value = "Category";

        var products = _userStore.GetAllProducts();
        var row = 2;
        foreach (var product in products)
        {
            worksheet.Cell(row, 1).Value = product.Id;
            worksheet.Cell(row, 2).Value = product.Name;
            worksheet.Cell(row, 3).Value = product.Price;
            worksheet.Cell(row, 4).Value = product.CategoryName;
            row++;
        }

        return CreateExcelFile(workbook, "ProductReport.xlsx");
    }

    public IActionResult ExportCategories()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Categories");

        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Name";

        var categories = _userStore.GetAllCategories();
        var row = 2;
        foreach (var category in categories)
        {
            worksheet.Cell(row, 1).Value = category.Id;
            worksheet.Cell(row, 2).Value = category.Name;
            row++;
        }

        return CreateExcelFile(workbook, "CategoryReport.xlsx");
    }

    public IActionResult ExportContactUs()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ContactUs");

        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Surname";
        worksheet.Cell(1, 4).Value = "Cell Number";
        worksheet.Cell(1, 5).Value = "Email Address";

         var contact = _userStore.GetAllContactUs();
         var row = 2;
         foreach (var contactus in contact)
          {
             worksheet.Cell(row, 1).Value = contactus.Id;
             worksheet.Cell(row, 2).Value = contactus.Name;
             worksheet.Cell(row, 3).Value = contactus.Surname;
             worksheet.Cell(row, 3).Value = contactus.CellNo;
             worksheet.Cell(row, 3).Value = contactus.EmailAddress;
             row++;
          }

        return CreateExcelFile(workbook, "ContactUsReport.xlsx");
    }

    private FileContentResult CreateExcelFile(XLWorkbook workbook, string fileName)
    {
        foreach (var worksheet in workbook.Worksheets)
        {
            worksheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
