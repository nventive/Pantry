using System.ComponentModel.DataAnnotations;

namespace Pantry.AspNetCore.Tests.Server
{
    public class StandardEntityAttributes
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }
    }
}
