using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using AstroGathering.Objects;

namespace AstroGathering.Database
{
    public class DatabaseOut
    {
        // Connection to database - UPDATE THESE VALUES FOR YOUR SETUP
        private string connectionString =
            "datasource=127.0.0.1;" +
            "port=3307;" +              // Default MySQL port
            "username=root;" +
            "password=;" +             // Add your MySQL password here
            "database=AstroGathering;";

        // Method for single value queries (COUNT, etc.)
        private object SelectSingle(string query)
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

        // USER OPERATIONS
        public User GetUserByGoogleId(string googleId)
        {
            string query = $"SELECT * FROM users WHERE google_id = '{googleId}';";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                MySqlDataReader reader = commandDatabase.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {
                        UserId = Convert.ToInt32(reader["user_id"]),
                        GoogleId = reader["google_id"].ToString(),
                        Email = reader["email"].ToString(),
                        FirstName = reader["first_name"]?.ToString(),
                        LastName = reader["last_name"]?.ToString(),
                        ProfilePictureUrl = reader["profile_picture_url"]?.ToString(),
                        IsAdmin = Convert.ToBoolean(reader["is_admin"]),
                        CreatedAt = Convert.ToDateTime(reader["created_at"]),
                        LastLogin = reader["last_login"] != DBNull.Value ? Convert.ToDateTime(reader["last_login"]) : null
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get User Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return null;
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            string query = "SELECT * FROM users ORDER BY created_at DESC;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                MySqlDataReader reader = commandDatabase.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new User
                    {
                        UserId = Convert.ToInt32(reader["user_id"]),
                        GoogleId = reader["google_id"].ToString(),
                        Email = reader["email"].ToString(),
                        FirstName = reader["first_name"]?.ToString(),
                        LastName = reader["last_name"]?.ToString(),
                        ProfilePictureUrl = reader["profile_picture_url"]?.ToString(),
                        IsAdmin = Convert.ToBoolean(reader["is_admin"]),
                        CreatedAt = Convert.ToDateTime(reader["created_at"]),
                        LastLogin = reader["last_login"] != DBNull.Value ? Convert.ToDateTime(reader["last_login"]) : null
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get All Users Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return users;
        }

        // PHOTO OPERATIONS
        public List<Photo> GetAllPhotos()
        {
            List<Photo> photos = new List<Photo>();
            string query = "SELECT * FROM photos ORDER BY time_uploaded DESC;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                MySqlDataReader reader = commandDatabase.ExecuteReader();

                while (reader.Read())
                {
                    photos.Add(new Photo
                    {
                        PhotoId = Convert.ToInt32(reader["photo_id"]),
                        UserId = Convert.ToInt32(reader["user_id"]),
                        ImageUrl = reader["image_url"].ToString(),
                        Location = reader["location"]?.ToString(),
                        Description = reader["description"]?.ToString(),
                        DateTaken = reader["date_taken"] != DBNull.Value ? Convert.ToDateTime(reader["date_taken"]) : null,
                        TimeUploaded = Convert.ToDateTime(reader["time_uploaded"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get All Photos Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return photos;
        }

        public List<Photo> GetPhotosByUser(int userId)
        {
            List<Photo> photos = new List<Photo>();
            string query = $"SELECT * FROM photos WHERE user_id = {userId} ORDER BY time_uploaded DESC;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                MySqlDataReader reader = commandDatabase.ExecuteReader();

                while (reader.Read())
                {
                    photos.Add(new Photo
                    {
                        PhotoId = Convert.ToInt32(reader["photo_id"]),
                        UserId = Convert.ToInt32(reader["user_id"]),
                        ImageUrl = reader["image_url"].ToString(),
                        Location = reader["location"]?.ToString(),
                        Description = reader["description"]?.ToString(),
                        DateTaken = reader["date_taken"] != DBNull.Value ? Convert.ToDateTime(reader["date_taken"]) : null,
                        TimeUploaded = Convert.ToDateTime(reader["time_uploaded"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get User Photos Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return photos;
        }

        // EVENT OPERATIONS
        public List<Event> GetUpcomingEvents()
        {
            List<Event> events = new List<Event>();
            string query = "SELECT * FROM events WHERE event_date >= NOW() ORDER BY event_date ASC;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                MySqlDataReader reader = commandDatabase.ExecuteReader();

                while (reader.Read())
                {
                    events.Add(new Event
                    {
                        EventId = Convert.ToInt32(reader["event_id"]),
                        UserId = Convert.ToInt32(reader["user_id"]),
                        EventName = reader["event_name"].ToString(),
                        Description = reader["description"]?.ToString(),
                        EventDate = Convert.ToDateTime(reader["event_date"]),
                        CreatedAt = Convert.ToDateTime(reader["created_at"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Events Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return events;
        }

        // TAG OPERATIONS
        public List<string> GetPhotoTags(int photoId)
        {
            List<string> tags = new List<string>();
            string query = $"SELECT t.name FROM tags t " +
                $"JOIN photo_tags pt ON t.tag_id = pt.tag_id " +
                $"WHERE pt.photo_id = {photoId};";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                MySqlDataReader reader = commandDatabase.ExecuteReader();

                while (reader.Read())
                {
                    tags.Add(reader["name"].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Photo Tags Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return tags;
        }

        // LIKE OPERATIONS
        public int GetPhotoLikeCount(int photoId)
        {
            string query = $"SELECT COUNT(*) FROM likes WHERE photo_id = {photoId};";
            object result = SelectSingle(query);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public bool HasUserLikedPhoto(int userId, int photoId)
        {
            string query = $"SELECT COUNT(*) FROM likes WHERE user_id = {userId} AND photo_id = {photoId};";
            object result = SelectSingle(query);
            return result != null && Convert.ToInt32(result) > 0;
        }

        // HELP CONTENT OPERATIONS
        public List<HelpContent> GetAllHelpContent()
        {
            List<HelpContent> helpSections = new List<HelpContent>();
            string query = "SELECT * FROM help_content ORDER BY section_id;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                MySqlDataReader reader = commandDatabase.ExecuteReader();

                while (reader.Read())
                {
                    helpSections.Add(new HelpContent
                    {
                        SectionId = Convert.ToInt32(reader["section_id"]),
                        Title = reader["title"].ToString(),
                        Content = reader["content"].ToString(),
                        LastUpdated = Convert.ToDateTime(reader["last_updated"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Help Content Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return helpSections;
        }
    }
}
