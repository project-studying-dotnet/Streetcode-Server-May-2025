using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Streetcode;
using Streetcode.DAL.Repositories.Realizations.Base;

namespace Streetcode.DAL.Repositories.Realizations.Streetcode;
    
public class CommentRepository : RepositoryBase<Comment>, ICommentRepository
{
    public CommentRepository(StreetcodeDbContext context) 
    : base(context) 
    { 
    }
}
