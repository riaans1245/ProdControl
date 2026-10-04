
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace test1233.Models;

public class ProdBulkUploadViewModel
{
    [Required(ErrorMessage = "Please select a CSV file to upload.")]
    [Display(Name = "Choose CSV File")]
    public IFormFile? CsvFile { get; set; }
}
