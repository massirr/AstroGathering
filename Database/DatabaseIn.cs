using System;
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

        // USER OPERATIONS
        public int InsertUser(User user)
        {
            string query = $"INSERT INTO users (google_id, email, first_name, last_name, profile_picture_url, is_admin, created_at) " +
                $"VALUES ('{user.GoogleId}', '{user.Email}', '{user.FirstName}', '{user.LastName}', " +
                $"'{user.ProfilePictureUrl}', {user.IsAdmin}, NOW());";

            return Insert(query);
        }

        // PHOTO OPERATIONS  
        public int InsertPhoto(Photo photo)
        {
            string dateTaken = photo.DateTaken?.ToString("yyyy-MM-dd HH:mm:ss") ?? "NULL";
            
            string query = $"INSERT INTO photos (user_id, image_url, location, description, date_taken, time_uploaded) " +
                $"VALUES ({photo.UserId}, '{photo.ImageUrl}', '{photo.Location}', '{photo.Description}', " +
                $"'{dateTaken}', NOW());";

            return Insert(query);
        }

        // EVENT OPERATIONS
        public int InsertEvent(Event eventObj)
        {
            string query = $"INSERT INTO events (user_id, event_name, description, event_date, created_at) " +
                $"VALUES ({eventObj.UserId}, '{eventObj.EventName}', '{eventObj.Description}', " +
                $"'{eventObj.EventDate.ToString("yyyy-MM-dd HH:mm:ss")}', NOW());";

            return Insert(query);
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

        // LIKE OPERATIONS
        public bool AddLike(int userId, int photoId)
        {
            string query = $"INSERT INTO likes (user_id, photo_id, liked_at) " +
                $"VALUES ({userId}, {photoId}, NOW()) " +
                $"ON DUPLICATE KEY UPDATE liked_at = NOW();";

            return Insert(query) >= 0; // LastInsertedId might be 0 for ON DUPLICATE KEY
        }

        // REPORT OPERATIONS
        public int InsertReport(Report report)
        {
            string query = $"INSERT INTO reports (user_id, photo_id, reason, date_reported, report_status) " +
                $"VALUES ({report.UserId}, {report.PhotoId}, '{report.Reason}', NOW(), 'Pending');";

            return Insert(query);
        }

        // HELP CONTENT OPERATIONS
        public int InsertHelpContent(HelpContent helpContent)
        {
            string query = $"INSERT INTO help_content (title, content, last_updated) " +
                $"VALUES ('{helpContent.Title}', '{helpContent.Content}', NOW());";

            return Insert(query);
        }
    }
}
