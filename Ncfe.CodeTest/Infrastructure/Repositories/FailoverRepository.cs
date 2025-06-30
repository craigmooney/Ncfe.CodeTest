using System.Collections.Generic;

namespace Ncfe.CodeTest
{
    public class FailoverRepository : IFailoverRepository
    {
        public List<FailoverEntry> GetFailOverEntries()
        {
            // return all failed entries from database
            return new List<FailoverEntry>();
        }
    }
}
