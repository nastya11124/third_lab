
using System.Data;
using Dapper;
using Npgsql;
using App3.CoreSpace.Interfaces;

namespace App3.CoreSpace
{
    public class TrackRepository : IRepository
    {
        private readonly string _connectionString;

        public TrackRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task<int?> GetArtistIdByName(string artistName)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();

                string query = @"
                SELECT Id 
                FROM Artists 
                WHERE Name = @ArtistName;
            ";

                return await db.QueryFirstOrDefaultAsync<int?>(query, new { ArtistName = artistName });
            }
        }

        public async Task<bool> CheckTrackExists(string artistName, string title)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();

                int? artistId = await GetArtistIdByName(artistName);

                if (artistId == null)
                {
                    return false;
                }

                string query = @"
            SELECT EXISTS (
                SELECT 1 
                FROM Tracks 
                WHERE Title = @Title AND ArtistId = @ArtistId
            );
        ";

                return await db.ExecuteScalarAsync<bool>(query, new { Title = title, ArtistId = artistId });
            }
        }

        public async Task<bool> DeleteTrack(string artistName, string trackName)
        {
            using (var db = new NpgsqlConnection(_connectionString))
            {
                await db.OpenAsync();

                using (var transaction = await db.BeginTransactionAsync())
                {
                    try
                    {
                        int? artistId = await GetArtistIdByName(artistName);

                        if (!artistId.HasValue)
                        {
                            return false;
                        }

                        // Блокировка строки с использованием SELECT FOR UPDATE
                        string lockQuery = @"
            SELECT 1 
            FROM Tracks 
            WHERE Title = @Title AND ArtistId = @ArtistId 
            FOR UPDATE;
        ";

                        var trackExists = await db.ExecuteScalarAsync<bool>(lockQuery, new { Title = trackName, ArtistId = artistId }, transaction);

                        if (!trackExists)
                        {
                            return false;
                        }

                        // Удаление трека
                        string deleteTrackQuery = @"
            DELETE FROM Tracks 
            WHERE Title = @Title AND ArtistId = @ArtistId;
        ";

                        await db.ExecuteAsync(deleteTrackQuery, new { Title = trackName, ArtistId = artistId }, transaction);

                        // Проверка, остались ли еще треки у артиста
                        string checkArtistTracksQuery = @"
            SELECT EXISTS (
                SELECT 1 
                FROM Tracks 
                WHERE ArtistId = @ArtistId
            );
        ";

                        var artistHasTracks = await db.ExecuteScalarAsync<bool>(checkArtistTracksQuery, new { ArtistId = artistId }, transaction);

                        if (!artistHasTracks)
                        {
                            // Удаление артиста, если у него больше нет треков
                            string deleteArtistQuery = @"
                DELETE FROM Artists 
                WHERE Id = @ArtistId;
            ";

                            await db.ExecuteAsync(deleteArtistQuery, new { ArtistId = artistId }, transaction);
                        }

                        // Фиксация транзакции
                        await transaction.CommitAsync();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Откат транзакции в случае ошибки
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> AddTrack(string artistName, string title)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();

                string query;
                int? artistId = await GetArtistIdByName(artistName);

                if (artistId == null)
                {
                    query = @"
                    INSERT INTO Artists (Name) 
                    VALUES (@ArtistName);";

                    await db.ExecuteAsync(query, new { ArtistName = artistName });


                    artistId = await GetArtistIdByName(artistName);
                    query = @"
                    INSERT INTO Tracks (Title, ArtistId) 
                    VALUES (@Title, @ArtistId);
                ";

                    await db.ExecuteAsync(query, new { Title = title, ArtistId = artistId });
                    return true;
                }

                query = @"
                    INSERT INTO Tracks (Title, ArtistId) 
                    VALUES (@Title, @ArtistId);
                ";

                await db.ExecuteAsync(query, new { Title = title, ArtistId = artistId });
                return true;
            }
        }
        public async Task<Dictionary<string, List<string>>> SearchTracks(bool byAuthor, string criterion, int page, int pageSize)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();

                string query;
                object parameters;
                int? artistId = await GetArtistIdByName(criterion);

                int offset = (page - 1) * pageSize;
                if (byAuthor)
                {
                    if (artistId == null)
                    {
                        return new Dictionary<string, List<string>>();
                    }

                    query = @"
                    SELECT a.Name AS ArtistName, t.Title AS TrackName
                    FROM Tracks t
                    JOIN Artists a ON t.ArtistId = a.Id
                    WHERE t.ArtistId = @ArtistId;
                ";
                    parameters = new { ArtistId = artistId, PageSize = pageSize, Offset = offset };
                }
                else
                {
                    query = @"
                    SELECT a.Name AS ArtistName, t.Title AS TrackName
                    FROM Tracks t
                    JOIN Artists a ON t.ArtistId = a.Id
                    WHERE t.Title ILIKE @Criterion;
                ";
                    parameters = new { Criterion = $"%{criterion}%", PageSize = pageSize, Offset = offset };

                }

                var results = await db.QueryAsync<(string ArtistName, string TrackName)>(query, parameters);
                var tracksDictionary = new Dictionary<string, List<string>>();

                foreach (var result in results)
                {
                    string artistName = result.ArtistName;
                    string trackTitle = result.TrackName;

                    if (!tracksDictionary.ContainsKey(artistName))
                    {
                        tracksDictionary[artistName] = new List<string>();
                    }

                    tracksDictionary[artistName].Add(trackTitle);
                }

                return tracksDictionary;
            }
        }
        public async Task<bool> HasMoreResults(bool byAuthor, string criterion, int page, int pageSize)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();

                string query;
                int? artistId = await GetArtistIdByName(criterion);
                if (byAuthor)
                {
                    if (artistId == null)
                    {
                        return false;
                    }

                    query = @"
                    SELECT COUNT(*)
                    FROM Tracks t
                    JOIN Artists a ON t.ArtistId = a.Id
                    WHERE t.ArtistId = @ArtistId
                    LIMIT @PageSize OFFSET @Offset;
                ";
                }
                else
                {
                    query = @"
                    SELECT COUNT(*)
                    FROM Tracks t
                    JOIN Artists a ON t.ArtistId = a.Id
                    WHERE t.Title ILIKE @Criterion
                    LIMIT @PageSize OFFSET @Offset;
                ";
                }

                var count = await db.ExecuteScalarAsync<int>(query, new { ArtistId = artistId, Criterion = $"%{criterion}%", PageSize = pageSize, Offset = (page - 1) * pageSize });

                return count > 0;
            }
        }
        public async Task<Dictionary<string, List<string>>> Search(int page, int pageSize)
        {
            Dictionary<string, List<string>> artistTracks = new Dictionary<string, List<string>>();
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();

                string query = @"
                SELECT a.Name AS ArtistName, t.Title AS TrackName
                FROM artists a
                JOIN tracks t ON a.Id = t.ArtistId
                ORDER BY a.Name
                LIMIT @PageSize OFFSET @Offset";

                int offset = (page - 1) * pageSize;

                var results = await db.QueryAsync<(string ArtistName, string TrackName)>(query, new { PageSize = pageSize, Offset = offset });
                foreach (var result in results)
                {
                    if (!artistTracks.ContainsKey(result.ArtistName))
                    {
                        artistTracks[result.ArtistName] = new List<string>();
                    }
                    artistTracks[result.ArtistName].Add(result.TrackName);
                }
            }

            return artistTracks;
        }
    }
}
