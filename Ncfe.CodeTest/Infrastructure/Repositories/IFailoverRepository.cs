using System.Collections.Generic;

namespace Ncfe.CodeTest
{
    public interface IFailoverRepository
    {
        List<FailoverEntry> GetFailOverEntries();
    }
}