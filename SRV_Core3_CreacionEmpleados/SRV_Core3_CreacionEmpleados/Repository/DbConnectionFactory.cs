using System.Data;
using MySqlConnector;

namespace Core3.CreacionEmpleados.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection() =>
            new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    }
}
