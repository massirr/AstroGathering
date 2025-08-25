-- This creates the complete database structure as shown in your ERD

CREATE DATABASE IF NOT EXISTS AstroGathering;
USE astrogathering;

-- Drop tables in reverse dependency order if they exist
DROP TABLE IF EXISTS photo_tags;
DROP TABLE IF EXISTS likes;
DROP TABLE IF EXISTS reports;
DROP TABLE IF EXISTS photos;
DROP TABLE IF EXISTS tags;
DROP TABLE IF EXISTS help_content;
DROP TABLE IF EXISTS users;

-- Users table
CREATE TABLE users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    google_id VARCHAR(255) UNIQUE NOT NULL,
    email VARCHAR(255) NOT NULL,
    profile_picture_url VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    is_admin BOOLEAN DEFAULT FALSE
);

-- Photos table (user uploaded astronomical photos)
CREATE TABLE photos (
    photo_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    image_url VARCHAR(500) NOT NULL,
    event_name VARCHAR(255),
    location VARCHAR(255),
    latitude DOUBLE,
    longitude DOUBLE,
    description TEXT,
    date_taken DATETIME,
    time_uploaded TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- Tags table
CREATE TABLE tags (
    tag_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL
);

-- Photo tags relationship table
CREATE TABLE photo_tags (
    photo_id INT NOT NULL,
    tag_id INT NOT NULL,
    PRIMARY KEY (photo_id, tag_id),
    FOREIGN KEY (photo_id) REFERENCES photos(photo_id) ON DELETE CASCADE,
    FOREIGN KEY (tag_id) REFERENCES tags(tag_id) ON DELETE CASCADE
);

-- Likes table
CREATE TABLE likes (
    user_id INT NOT NULL,
    photo_id INT NOT NULL,
    liked_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, photo_id),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (photo_id) REFERENCES photos(photo_id) ON DELETE CASCADE
);

-- Reports table
CREATE TABLE reports (
    report_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    photo_id INT NOT NULL,
    reason TEXT NOT NULL,
    date_reported TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    report_status ENUM('Pending', 'Resolved', 'Dismissed') DEFAULT 'Pending',
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (photo_id) REFERENCES photos(photo_id) ON DELETE CASCADE
);

-- Help content table
CREATE TABLE help_content (
    section_id INT AUTO_INCREMENT PRIMARY KEY,
    section VARCHAR(255) NOT NULL,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    display_order INT,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Astronomical events table (separate from user events - these come from APIs like NASA)
CREATE TABLE astronomical_events (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    type VARCHAR(100) NOT NULL,
    event_date DATE NOT NULL,
    description TEXT,
    image_url VARCHAR(500),
    hd_image_url VARCHAR(500),
    time_info VARCHAR(255),
    latitude DOUBLE,
    longitude DOUBLE,
    api_source VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY unique_event (name, event_date, api_source)
);

-- Create indexes for better performance
CREATE INDEX idx_users_google_id ON users(google_id);
CREATE INDEX idx_photos_user ON photos(user_id);
CREATE INDEX idx_photos_date ON photos(time_uploaded);
CREATE INDEX idx_astronomical_events_date ON astronomical_events(event_date);
CREATE INDEX idx_astronomical_events_type ON astronomical_events(type);
CREATE INDEX idx_likes_photo ON likes(photo_id);
CREATE INDEX idx_reports_status ON reports(report_status);
