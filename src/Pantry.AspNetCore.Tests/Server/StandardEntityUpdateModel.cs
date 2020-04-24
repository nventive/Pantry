using System.ComponentModel.DataAnnotations;

namespace Pantry.AspNetCore.Tests.Server
{
    public class StandardEntityUpdateModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
