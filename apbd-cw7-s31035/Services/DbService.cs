using apbd_cw7_s31035.Exceptions;
using apbd_cw7_s31035.Models;
using Microsoft.Data.SqlClient;

namespace apbd_cw7_s31035.Services;

public interface IDbService
{
    public Task<IEnumerable<TripGetDTO>> GetAllTripsAsync();
    public Task<IEnumerable<ClientTripGetDTO>> GetClientTripsAsync(int clientId);
}

public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Local");

    public async Task<IEnumerable<TripGetDTO>> GetAllTripsAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var sql = """
                  SELECT 
                  Trip.IdTrip, Trip.Name, Description, DateFrom, DateTo, MaxPeople, C.IdCountry, C.Name
                  FROM Trip
                  JOIN Country_Trip CT ON CT.IdTrip = Trip.IdTrip
                  JOIN Country C ON C.IdCountry = CT.IdCountry
                  """;
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var results = new List<(Trip trip, Country country)>();
        while (await reader.ReadAsync())
        {
            results.Add((
                trip: new Trip
                {
                    IdTrip = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                },
                country: new Country
                {
                    IdCountry = reader.GetInt32(6),
                    Name = reader.GetString(7)
                }
            ));
        }

        return results.GroupBy((result) => result.trip.IdTrip)
            .Select(group =>
            {
                var trip = group.First().trip;
                return new TripGetDTO
                {
                    IdTrip = trip.IdTrip,
                    Name = trip.Name,
                    MaxPeople = trip.MaxPeople,
                    DateFrom = trip.DateFrom,
                    DateTo = trip.DateTo,
                    Description = trip.Description,
                    Countries = group.Select(r => r.country)
                };
            })
            .ToList();
    }

    public async Task<IEnumerable<ClientTripGetDTO>> GetClientTripsAsync(int clientId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var clientPresenceQuery = """
                                  SELECT 1
                                  FROM Client
                                  WHERE Client.IdClient = @clientId
                                  """;
        await using var command = new SqlCommand(clientPresenceQuery, connection);
        command.Parameters.AddWithValue("@clientId", clientId);
        await using (var reader = await command.ExecuteReaderAsync()) 
        {
            if (!reader.HasRows)
            {
                throw new NotFoundException("User not found!");
            }
        }
        
        var sqlQuery = """
                       SELECT CT.IdClient, CT.IdTrip, CT.RegisteredAt, CT.PaymentDate, T.IdTrip, T.Name, T.Description, T.DateFrom, T.DateTo, T.MaxPeople
                       FROM Client_Trip CT
                       JOIN Trip T on T.IdTrip = CT.IdTrip
                       WHERE CT.IdClient = @clientId
                       """;
        var queryCommand = new SqlCommand(sqlQuery, connection);
        queryCommand.Parameters.AddWithValue("@clientId", clientId);
        await using var queryReader = await queryCommand.ExecuteReaderAsync();
        if (!queryReader.HasRows)
        {
            throw new NotFoundException("User has no trips");
        }
        var results = new List<(ClientTrip clientTrip, Trip trip)>();
        while (await queryReader.ReadAsync())
        {
            results.Add((
                clientTrip: new ClientTrip
                {
                    IdClient = queryReader.GetInt32(0),
                    IdTrip = queryReader.GetInt32(1),
                    RegisteredAt = queryReader.GetInt32(2),
                    PaymentDate = queryReader.IsDBNull(3) ? null : queryReader.GetInt32(3),
                },
                trip: new Trip
                {
                    IdTrip = queryReader.GetInt32(4),
                    Name = queryReader.GetString(5),
                    Description = queryReader.GetString(6),
                    DateFrom = queryReader.GetDateTime(7),
                    DateTo = queryReader.GetDateTime(8),
                    MaxPeople = queryReader.GetInt32(9),
                }
            ));
        }

        return results
            .GroupBy(result => result.trip.IdTrip)
            .Select(group =>
            {
                var (clientTrip, trip) = group.First();
                return new ClientTripGetDTO
                {
                    IdClient = clientTrip.IdClient,
                    PaymentDate = clientTrip.PaymentDate,
                    RegisteredAt = clientTrip.RegisteredAt,
                    TripDetails = trip,
                };
            });
    }
}