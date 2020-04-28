using Pantry.Mediator.Repositories.Commands;

namespace Pantry.Mediator.AspNetCore.Tests.Server
{
    public class UpdateStandardEntityCommand : UpdateCommand<StandardEntity>
    {
        public int Age { get; set; }
    }
}
