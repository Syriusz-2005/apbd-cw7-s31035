using apbd_cw7_s31035.Exceptions;
using apbd_cw7_s31035.Models;
using Microsoft.Data.SqlClient;

namespace apbd_cw7_s31035.Services;

public interface IDbService
{
    public Task<IEnumerable<TripGetDTO>> GetAllTripsAsync();
    public Task<IEnumerable<ClientTripGetDTO>> GetClientTripsAsync(int clientId);
    public Task<int> CreateClientAsync(ClientCreateDTO client);
    public Task RegisterClientOnTripAsync(int clientId, int tripId);
    public Task DeleteClientTrip(int clientId, int tripId);
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

    public async Task<int> CreateClientAsync(ClientCreateDTO client)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = """
                    INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) 
                    VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel); SELECT scope_identity() 
                    """;
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);
        var clientId = Convert.ToInt32(await command.ExecuteScalarAsync());
        return clientId;
    }

    public async Task RegisterClientOnTripAsync(int clientId, int tripId)
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
                throw new NotFoundException("Client not found!");
            }
        }
        
        var tripPresenceQuery = """
                                  SELECT 1
                                  FROM Trip
                                  WHERE Trip.IdTrip = @tripId
                                  """;
        await using var command2 = new SqlCommand(tripPresenceQuery, connection);
        command2.Parameters.AddWithValue("@tripId", tripId);
        await using (var reader = await command2.ExecuteReaderAsync()) 
        {
            if (!reader.HasRows)
            {
                throw new NotFoundException("Trip not found!");
            }
        }

        var insertQuery = """
                          INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
                          VALUES (@IdClient, @IdTrip, @RegisteredAt, null)
                          """;
        await using var insertCommand = new SqlCommand(insertQuery, connection);
        insertCommand.Parameters.AddWithValue("@IdClient", clientId);
        insertCommand.Parameters.AddWithValue("@IdTrip", tripId);
        var now = DateTime.Now;
        var timestamp = now.Day + now.Month * 100 + now.Year * 10_000;
        insertCommand.Parameters.AddWithValue("@RegisteredAt", timestamp);
        await insertCommand.ExecuteNonQueryAsync();
    }

    public async Task DeleteClientTrip(int clientId, int tripId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var clientTripPresenceQuery = """
                                  SELECT 1
                                  FROM Client_Trip CT
                                  WHERE CT.IdClient = @clientId and CT.IdTrip = @tripId
                                  """;
        await using var command = new SqlCommand(clientTripPresenceQuery, connection);
        command.Parameters.AddWithValue("@clientId", clientId);
        command.Parameters.AddWithValue("@tripId", tripId);
        await using (var reader = await command.ExecuteReaderAsync()) 
        {
            if (!reader.HasRows)
            {
                throw new NotFoundException("Client trip not found!");
            }
        }

        var deleteQuery = """
                          DELETE FROM Client_Trip
                          WHERE IdClient = @clientId and IdTrip = @tripId
                          """;
        await using var deleteCommand = new SqlCommand(deleteQuery, connection);
        deleteCommand.Parameters.AddWithValue("@clientId", clientId);
        deleteCommand.Parameters.AddWithValue("@tripId", tripId);
        await deleteCommand.ExecuteNonQueryAsync();
    }
}