using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using AstroGathering.Objects;

namespace AstroGathering.Database
{
    public class DatabaseIn
    {
        // Connection to database
        private string connectionString =
            "datasource=127.0.0.1;" +
            "port=3307;" +       
            "username=root;" +
            "password= ;" +
            "database=AstroGathering;";

        // Generic method to insert data and return the new ID
        private int Insert(string query)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                int result = commandDatabase.ExecuteNonQuery();
                return (int)commandDatabase.LastInsertedId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Insert Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return -1; // Return -1 if insert failed
        }

        // Async insert method for better performance
        private async Task<int> InsertAsync(string query)
        {
            using var connection = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(query, connection);

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                return (int)command.LastInsertedId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Insert Error: {ex.Message}");
                return -1;
            }
        }

        // USER OPERATIONS
        public int InsertUser(User user)
        {
            string query = $"INSERT INTO users (google_id, email, first_name, last_name, profile_picture_url, is_admin, created_at) " +
                $"VALUES ('{user.GoogleId}', '{user.Email}', '{user.FirstName}', '{user.LastName}', " +
                $"'{user.ProfilePictureUrl}', {user.IsAdmin}, NOW());";

            return Insert(query);
        }

        // PHOTO OPERATIONS

        public async Task<int> InsertPhotoAsync(Photo photo)
        {
            string dateTaken = photo.DateTaken?.ToString("yyyy-MM-dd HH:mm:ss") ?? "NULL";
            
            string query = $"INSERT INTO photos (user_id, image_url, location, description, date_taken, time_uploaded) " +
                $"VALUES ({photo.UserId}, '{photo.ImageUrl}', '{photo.Location?.Replace("'", "''")}', '{photo.Description?.Replace("'", "''")}', " +
                $"'{dateTaken}', NOW());";

            return await InsertAsync(query);
        }

        // TAG OPERATIONS
        public int InsertTag(string tagName)
        {
            string query = $"INSERT INTO tags (name) VALUES ('{tagName}') " +
                $"ON DUPLICATE KEY UPDATE tag_id = LAST_INSERT_ID(tag_id);";

            return Insert(query);
        }

        public bool AddPhotoTag(int photoId, int tagId)
        {
            string query = $"INSERT INTO photo_tags (photo_id, tag_id) VALUES ({photoId}, {tagId}) " +
                $"ON DUPLICATE KEY UPDATE photo_id = photo_id;";

            return Insert(query) > 0;
        }

        // ASTRONOMICAL EVENTS OPERATIONS

        public bool InsertAstronomicalEvents(List<AstronomicalEvent> events)
        {
            if (!events.Any()) return true;

            var valuesList = new List<string>();
            foreach (var astroEvent in events)
            {
                var values = $"('{astroEvent.EventName.Replace("'", "''")}', " + // Using EventName
                    $"'{astroEvent.Type.Replace("'", "''")}', " +
                    $"'{astroEvent.EventDate:yyyy-MM-dd}', " + // Using EventDate
                    $"'{astroEvent.Description.Replace("'", "''")}', " +
                    $"'{astroEvent.ImageUrl.Replace("'", "''")}', " +
                    $"'{astroEvent.HdImageUrl.Replace("'", "''")}', " +
                    $"'{astroEvent.Time.Replace("'", "''")}', " +
                    $"{(astroEvent.Latitude?.ToString() ?? "NULL")}, " +
                    $"{(astroEvent.Longitude?.ToString() ?? "NULL")}, " +
                    $"'{astroEvent.Source.Replace("'", "''")}')";
                valuesList.Add(values);
            }

            string query = "INSERT INTO astronomical_events (name, type, event_date, description, image_url, hd_image_url, time_info, latitude, longitude, api_source) " +
                $"VALUES {string.Join(", ", valuesList)} " +
                "ON DUPLICATE KEY UPDATE " +
                "name = VALUES(name), description = VALUES(description), image_url = VALUES(image_url), hd_image_url = VALUES(hd_image_url), " +
                "time_info = VALUES(time_info), latitude = VALUES(latitude), longitude = VALUES(longitude), updated_at = NOW();";

            return Insert(query) > 0;
        }

        public bool ClearAstronomicalEventsForMonth(DateTime month)
        {
            var firstDay = new DateTime(month.Year, month.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            string query = $"DELETE FROM astronomical_events " +
                $"WHERE event_date >= '{firstDay:yyyy-MM-dd}' AND event_date <= '{lastDay:yyyy-MM-dd}';";

            return Insert(query) >= 0; // Allow 0 deleted rows
        }

        public bool UpdateUserLastLogin(int userId, DateTime lastLogin)
        {
            string query = $"UPDATE users SET last_login = '{lastLogin:yyyy-MM-dd HH:mm:ss}' WHERE user_id = {userId};";
            return Insert(query) >= 0;
        }

        // HELP CONTENT OPERATIONS
        public bool InsertHelpContent(string section, string title, string content, int order = 0)
        {
            string query = $"INSERT INTO help_content (section, title, content, display_order) " +
                $"VALUES ('{section.Replace("'", "''")}', '{title.Replace("'", "''")}', '{content.Replace("'", "''")}', {order}) " +
                $"ON DUPLICATE KEY UPDATE " +
                $"content = VALUES(content), display_order = VALUES(display_order);";

            return Insert(query) > 0;
        }

        public bool UpdateHelpContent(int sectionId, string title, string content)
        {
            string query = $"UPDATE help_content SET " +
                $"title = '{title.Replace("'", "''")}', " +
                $"content = '{content.Replace("'", "''")}' " +
                $"WHERE section_id = {sectionId};";

            return Insert(query) >= 0;
        }
    }
}
