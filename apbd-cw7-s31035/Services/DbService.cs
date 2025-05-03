using apbd_cw7_s31035.Exceptions;
using apbd_cw7_s31035.Models;
using Microsoft.Data.SqlClient;

namespace apbd_cw7_s31035.Services;

public interface IDbService
{

}

public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Local");
}