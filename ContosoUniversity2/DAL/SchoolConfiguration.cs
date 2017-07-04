using System.Data.Entity;
using System.Data.Entity.SqlServer;
using ContosoUniversity2.DAL;
using System.Data.Entity.Infrastructure.Interception;

namespace ContosoUniversity2.DAL
{
    public class SchoolConfiguration : DbConfiguration
    {
        public SchoolConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
            DbInterception.Add(new SchoolInterceptorTransientErrors());
            DbInterception.Add(new SchoolInterceptorLogging());
        }
    }
}