using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.Data;

namespace Taskboard.EntityFrameworkCore.Repositories;

public sealed class EfCoreProjectRepository : EfCoreRepository<Project>
{
    public EfCoreProjectRepository(TaskboardDbContext context)
        : base(context)
    {
    }
}
