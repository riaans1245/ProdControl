using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
namespace test1233.Models;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class RegisterViewModel : UserCreateViewModel
{
    private string? GetDebuggerDisplay()
    {
        return ToString();
    }

}