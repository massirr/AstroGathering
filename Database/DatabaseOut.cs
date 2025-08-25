using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using AstroGathering.Objects;

namespace AstroGathering.Database
{
    public class HelpContent
    {
        public int SectionId { get; set; }
        public string Section { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public int DisplayOrder { get; set; }
        public DateTime LastUpdated { get; set; }
    }

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

        // USER OPERATIONS
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
                        GoogleId = reader["google_id"]?.ToString() ?? "",
                        Email = reader["email"]?.ToString() ?? "",
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

        public User? GetUserByEmail(string email)
        {
            MySqlConnection? connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = @"SELECT user_id, google_id, email, profile_picture_url, 
                               first_name, last_name, created_at, last_login, is_admin 
                               FROM users WHERE email = @email";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@email", email);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        UserId = Convert.ToInt32(reader["user_id"]),
                        GoogleId = reader["google_id"]?.ToString() ?? "",
                        Email = reader["email"]?.ToString() ?? "",
                        ProfilePictureUrl = reader["profile_picture_url"]?.ToString(),
                        FirstName = reader["first_name"]?.ToString(),
                        LastName = reader["last_name"]?.ToString(),
                        CreatedAt = Convert.ToDateTime(reader["created_at"]),
                        LastLogin = reader["last_login"] == DBNull.Value ? null : Convert.ToDateTime(reader["last_login"]),
                        IsAdmin = Convert.ToBoolean(reader["is_admin"])
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get User By Email Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return null;
        }

        public bool MakeUserAdmin(string email)
        {
            MySqlConnection? connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "UPDATE users SET is_admin = true WHERE email = @email";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@email", email);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Make User Admin Error: {ex.Message}");
                return false;
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
                        EventName = reader["event_name"]?.ToString(),
                        Location = reader["location"]?.ToString(),
                        Latitude = reader["latitude"] == DBNull.Value ? null : Convert.ToDouble(reader["latitude"]),
                        Longitude = reader["longitude"] == DBNull.Value ? null : Convert.ToDouble(reader["longitude"]),
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

        // ADMIN OPERATIONS
        public int GetTotalUsersCount()
        {
            MySqlConnection? connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "SELECT COUNT(*) FROM users";
                using var command = new MySqlCommand(query, connection);
                
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Total Users Count Error: {ex.Message}");
                return 0;
            }
            finally
            {
                connection?.Close();
            }
        }

        public int GetTotalPhotosCount()
        {
            MySqlConnection? connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "SELECT COUNT(*) FROM photos";
                using var command = new MySqlCommand(query, connection);
                
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Total Photos Count Error: {ex.Message}");
                return 0;
            }
            finally
            {
                connection?.Close();
            }
        }

        public int GetPendingReportsCount()
        {
            MySqlConnection? connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "SELECT COUNT(*) FROM reports WHERE report_status = 'Pending'";
                using var command = new MySqlCommand(query, connection);
                
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Pending Reports Count Error: {ex.Message}");
                return 0;
            }
            finally
            {
                connection?.Close();
            }
        }

        public int GetUserPhotosCount(int userId)
        {
            MySqlConnection? connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "SELECT COUNT(*) FROM photos WHERE user_id = @userId";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);
                
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get User Photos Count Error: {ex.Message}");
                return 0;
            }
            finally
            {
                connection?.Close();
            }
        }

        public int GetUserLikesCount(int userId)
        {
            MySqlConnection? connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = @"SELECT COUNT(*) FROM likes l 
                               INNER JOIN photos p ON l.photo_id = p.photo_id 
                               WHERE p.user_id = @userId";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);
                
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get User Likes Count Error: {ex.Message}");
                return 0;
            }
            finally
            {
                connection?.Close();
            }
        }

        // REPORT OPERATIONS
        public List<Report> GetAllReports()
        {
            List<Report> reports = new List<Report>();
            MySqlConnection? connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = @"SELECT r.report_id, r.user_id, r.photo_id, r.reason, r.date_reported, r.report_status,
                               u.email as reporter_email, u.first_name as reporter_first_name, u.last_name as reporter_last_name,
                               p.image_url, p.description as photo_description, 
                               pu.email as photo_owner_email, pu.first_name as photo_owner_first_name, pu.last_name as photo_owner_last_name
                               FROM reports r
                               LEFT JOIN users u ON r.user_id = u.user_id
                               LEFT JOIN photos p ON r.photo_id = p.photo_id
                               LEFT JOIN users pu ON p.user_id = pu.user_id
                               ORDER BY r.date_reported DESC";

                using var command = new MySqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    reports.Add(new Report
                    {
                        ReportId = Convert.ToInt32(reader["report_id"]),
                        UserId = Convert.ToInt32(reader["user_id"]),
                        PhotoId = reader["photo_id"] != DBNull.Value ? Convert.ToInt32(reader["photo_id"]) : null,
                        Reason = reader["reason"]?.ToString() ?? "",
                        DateReported = Convert.ToDateTime(reader["date_reported"]),
                        ReportStatus = reader["report_status"]?.ToString() ?? "Pending"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get All Reports Error: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            return reports;
        }

        public bool UpdateReportStatus(int reportId, string status)
        {
            MySqlConnection? connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "UPDATE reports SET report_status = @status WHERE report_id = @reportId";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@reportId", reportId);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update Report Status Error: {ex.Message}");
                return false;
            }
            finally
            {
                connection?.Close();
            }
        }

        public bool DeleteReport(int reportId)
        {
            MySqlConnection? connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "DELETE FROM reports WHERE report_id = @reportId";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@reportId", reportId);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete Report Error: {ex.Message}");
                return false;
            }
            finally
            {
                connection?.Close();
            }
        }

        // DELETE OPERATIONS
        public bool DeleteUser(int userId)
        {
            MySqlConnection? connection = null;
            MySqlTransaction? transaction = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
                transaction = connection.BeginTransaction();

                // Delete user's likes first (foreign key constraint)
                string deleteLikesQuery = "DELETE FROM likes WHERE user_id = @userId";
                using (var command1 = new MySqlCommand(deleteLikesQuery, connection, transaction))
                {
                    command1.Parameters.AddWithValue("@userId", userId);
                    command1.ExecuteNonQuery();
                }

                // Delete user's reports
                string deleteReportsQuery = "DELETE FROM reports WHERE user_id = @userId";
                using (var command2 = new MySqlCommand(deleteReportsQuery, connection, transaction))
                {
                    command2.Parameters.AddWithValue("@userId", userId);
                    command2.ExecuteNonQuery();
                }

                // Delete user's photos (and associated photo_tags, likes on their photos)
                string deletePhotoTagsQuery = "DELETE pt FROM photo_tags pt INNER JOIN photos p ON pt.photo_id = p.photo_id WHERE p.user_id = @userId";
                using (var command3 = new MySqlCommand(deletePhotoTagsQuery, connection, transaction))
                {
                    command3.Parameters.AddWithValue("@userId", userId);
                    command3.ExecuteNonQuery();
                }

                string deletePhotoLikesQuery = "DELETE l FROM likes l INNER JOIN photos p ON l.photo_id = p.photo_id WHERE p.user_id = @userId";
                using (var command4 = new MySqlCommand(deletePhotoLikesQuery, connection, transaction))
                {
                    command4.Parameters.AddWithValue("@userId", userId);
                    command4.ExecuteNonQuery();
                }

                string deletePhotosQuery = "DELETE FROM photos WHERE user_id = @userId";
                using (var command5 = new MySqlCommand(deletePhotosQuery, connection, transaction))
                {
                    command5.Parameters.AddWithValue("@userId", userId);
                    command5.ExecuteNonQuery();
                }

                // Finally delete the user
                string deleteUserQuery = "DELETE FROM users WHERE user_id = @userId";
                using (var command6 = new MySqlCommand(deleteUserQuery, connection, transaction))
                {
                    command6.Parameters.AddWithValue("@userId", userId);
                    int rowsAffected = command6.ExecuteNonQuery();
                    
                    if (rowsAffected > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete User Error: {ex.Message}");
                transaction?.Rollback();
                return false;
            }
            finally
            {
                transaction?.Dispose();
                connection?.Close();
            }
        }

        public bool DeletePhoto(int photoId)
        {
            MySqlConnection? connection = null;
            MySqlTransaction? transaction = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
                transaction = connection.BeginTransaction();

                // Delete photo tags first
                string deletePhotoTagsQuery = "DELETE FROM photo_tags WHERE photo_id = @photoId";
                using (var command1 = new MySqlCommand(deletePhotoTagsQuery, connection, transaction))
                {
                    command1.Parameters.AddWithValue("@photoId", photoId);
                    command1.ExecuteNonQuery();
                }

                // Delete likes on this photo
                string deleteLikesQuery = "DELETE FROM likes WHERE photo_id = @photoId";
                using (var command2 = new MySqlCommand(deleteLikesQuery, connection, transaction))
                {
                    command2.Parameters.AddWithValue("@photoId", photoId);
                    command2.ExecuteNonQuery();
                }

                // Delete reports for this photo
                string deleteReportsQuery = "DELETE FROM reports WHERE photo_id = @photoId";
                using (var command3 = new MySqlCommand(deleteReportsQuery, connection, transaction))
                {
                    command3.Parameters.AddWithValue("@photoId", photoId);
                    command3.ExecuteNonQuery();
                }

                // Finally delete the photo
                string deletePhotoQuery = "DELETE FROM photos WHERE photo_id = @photoId";
                using (var command4 = new MySqlCommand(deletePhotoQuery, connection, transaction))
                {
                    command4.Parameters.AddWithValue("@photoId", photoId);
                    int rowsAffected = command4.ExecuteNonQuery();
                    
                    if (rowsAffected > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete Photo Error: {ex.Message}");
                transaction?.Rollback();
                return false;
            }
            finally
            {
                transaction?.Dispose();
                connection?.Close();
            }
        }

        // HELP CONTENT OPERATIONS
        public async Task<List<HelpContent>> GetHelpContentBySectionAsync(string section)
        {
            var helpContent = new List<HelpContent>();
            
            using var connection = new MySqlConnection(connectionString);
            string query = $"SELECT section_id, section, title, content, display_order, last_updated " +
                          $"FROM help_content WHERE section = '{section.Replace("'", "''")}' " +
                          $"ORDER BY display_order ASC, last_updated ASC;";
            
            using var command = new MySqlCommand(query, connection);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    helpContent.Add(new HelpContent
                    {
                        SectionId = Convert.ToInt32(reader["section_id"]),
                        Section = reader["section"]?.ToString() ?? "",
                        Title = reader["title"]?.ToString() ?? "",
                        Content = reader["content"]?.ToString() ?? "",
                        DisplayOrder = reader["display_order"] == DBNull.Value ? 0 : Convert.ToInt32(reader["display_order"]),
                        LastUpdated = Convert.ToDateTime(reader["last_updated"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching help content: {ex.Message}");
            }

            return helpContent;
        }

        public async Task<List<HelpContent>> GetAllHelpContentAsync()
        {
            var helpContent = new List<HelpContent>();
            
            using var connection = new MySqlConnection(connectionString);
            string query = "SELECT section_id, section, title, content, display_order, last_updated " +
                          "FROM help_content ORDER BY section, display_order ASC, last_updated ASC;";
            
            using var command = new MySqlCommand(query, connection);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    helpContent.Add(new HelpContent
                    {
                        SectionId = Convert.ToInt32(reader["section_id"]),
                        Section = reader["section"]?.ToString() ?? "",
                        Title = reader["title"]?.ToString() ?? "",
                        Content = reader["content"]?.ToString() ?? "",
                        DisplayOrder = reader["display_order"] == DBNull.Value ? 0 : Convert.ToInt32(reader["display_order"]),
                        LastUpdated = Convert.ToDateTime(reader["last_updated"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching help content: {ex.Message}");
            }

            return helpContent;
        }
    }
}
