using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using AstroGathering.Objects;

namespace AstroGathering.Database
{
    public class DatabaseOut
    {
        // Connection to database - UPDATE THESE VALUES FOR YOUR SETUP
        private string connectionString =
            "datasource=127.0.0.1;" +
            "port=3307;" +              
            "username=root;" +
            "password= ;" +           
            "database=AstroGathering;";

        // Method for single value queries (COUNT, etc.)
        private object? SelectSingle(string query)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                return commandDatabase.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Select Single Error: {ex.Message}");
                return null;
            }
            finally
            {
                connection?.Close();
            }
        }

        // PHOTO OPERATIONS
        public async Task<List<Photo>> GetAllPhotosAsync()
        {
            var photos = new List<Photo>();
            MySqlConnection? connection = null;

            try
            {
                connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                string query = "SELECT * FROM photos ORDER BY time_uploaded DESC;";
                var command = new MySqlCommand(query, connection);
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    photos.Add(new Photo
                    {
                        PhotoId = Convert.ToInt32(reader["photo_id"]),
                        UserId = Convert.ToInt32(reader["user_id"]),
                        ImageUrl = reader["image_url"].ToString() ?? "",
                        Location = reader["location"]?.ToString(),
                        Description = reader["description"]?.ToString(),
                        DateTaken = reader["date_taken"] != DBNull.Value ? Convert.ToDateTime(reader["date_taken"]) : null,
                        TimeUploaded = Convert.ToDateTime(reader["time_uploaded"])
                    });
                }

                Console.WriteLine($"Found {photos.Count} user photos");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get All Photos Async Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return photos;
        }

        // ASTRONOMICAL EVENTS OPERATIONS
        public List<AstronomicalEvent> GetAstronomicalEventsForMonth(DateTime month)
        {
            var events = new List<AstronomicalEvent>();
            MySqlConnection? connection = null;

            try
            {
                var firstDay = new DateTime(month.Year, month.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);

                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "SELECT id, name, type, event_date, description, image_url, hd_image_url, time_info, latitude, longitude, api_source, created_at " +
                    "FROM astronomical_events " +
                    $"WHERE event_date >= '{firstDay:yyyy-MM-dd}' AND event_date <= '{lastDay:yyyy-MM-dd}' " +
                    "ORDER BY event_date ASC, type ASC;";

                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    events.Add(new AstronomicalEvent
                    {
                        EventId = Convert.ToInt32(reader["id"]), // Map id to EventId
                        EventName = reader["name"].ToString() ?? "", // Map name to EventName
                        Type = reader["type"].ToString() ?? "",
                        EventDate = Convert.ToDateTime(reader["event_date"]), // Map event_date to EventDate
                        Description = reader["description"].ToString() ?? "",
                        ImageUrl = reader["image_url"].ToString() ?? "",
                        HdImageUrl = reader["hd_image_url"].ToString() ?? "",
                        Time = reader["time_info"].ToString() ?? "",
                        Latitude = reader["latitude"] == DBNull.Value ? null : Convert.ToDouble(reader["latitude"]),
                        Longitude = reader["longitude"] == DBNull.Value ? null : Convert.ToDouble(reader["longitude"]),
                        Source = reader["api_source"].ToString() ?? "",
                        CreatedAt = Convert.ToDateTime(reader["created_at"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Astronomical Events Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return events;
        }

        // Method to get all astronomical events that have images for the gallery
        public List<AstronomicalEvent> GetAllAstronomicalEventsWithImages()
        {
            var events = new List<AstronomicalEvent>();
            MySqlConnection? connection = null;

            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "SELECT id, name, type, event_date, description, image_url, hd_image_url, time_info, latitude, longitude, api_source, created_at " +
                    "FROM astronomical_events " +
                    "WHERE image_url IS NOT NULL AND image_url != '' " +
                    "ORDER BY event_date DESC, created_at DESC;";

                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    events.Add(new AstronomicalEvent
                    {
                        EventId = Convert.ToInt32(reader["id"]), // Map id to EventId
                        EventName = reader["name"].ToString() ?? "", // Map name to EventName
                        Type = reader["type"].ToString() ?? "",
                        EventDate = Convert.ToDateTime(reader["event_date"]), // Map event_date to EventDate
                        Description = reader["description"].ToString() ?? "",
                        ImageUrl = reader["image_url"].ToString() ?? "",
                        HdImageUrl = reader["hd_image_url"].ToString() ?? "",
                        Time = reader["time_info"].ToString() ?? "",
                        Latitude = reader["latitude"] == DBNull.Value ? null : Convert.ToDouble(reader["latitude"]),
                        Longitude = reader["longitude"] == DBNull.Value ? null : Convert.ToDouble(reader["longitude"]),
                        Source = reader["api_source"].ToString() ?? "",
                        CreatedAt = Convert.ToDateTime(reader["created_at"])
                    });
                }

                Console.WriteLine($"Found {events.Count} astronomical events with images for gallery");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Astronomical Events With Images Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return events;
        }
    }
}
