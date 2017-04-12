using System.Collections.Generic;

namespace OSBIDE.Web.Models.Queries
{
    public interface IOsbideQuery<out T>
    {
        IEnumerable<T> Execute();
    }
}