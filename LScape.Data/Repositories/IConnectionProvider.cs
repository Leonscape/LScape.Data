using System.Data;

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
        IDbConnection Connection { get; }
    }
}
