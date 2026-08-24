using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.Data;

namespace Taskboard.EntityFrameworkCore.Repositories;

public sealed class EfCoreTaskRepository : EfCoreRepository<Domain.Entities.Task>
{
    public EfCoreTaskRepository(TaskboardDbContext context)
        : base(context)
    {
    }
}
