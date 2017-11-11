using System.Data;
using System.Threading.Tasks;

namespace LScape.Data.Repositories
{
    /// <summary>
    /// simple Connection interface for repositories
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        /// The DbConnection
        /// </summary>
        IDbConnection Connection();

        /// <summary>
        /// The dbConnection created async
        /// </summary>
        Task<IDbConnection> ConnectionAsync();
    }
}
