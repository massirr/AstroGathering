-- Sample Data for AstroGathering Database
-- IMPORTANT: Insert data in the correct order to respect foreign key constraints
USE astrogathering;

-- First, insert users (no foreign key dependencies)
INSERT INTO users (google_id, email, first_name, last_name, is_admin) VALUES 
('google_admin_123456', 'admin@astrogathering.com', 'Admin', 'User', TRUE),
('google_john_789012', 'john.doe@example.com', 'John', 'Doe', FALSE),
('google_jane_345678', 'jane.smith@example.com', 'Jane', 'Smith', FALSE);

-- Insert tags (no foreign key dependencies)
INSERT INTO tags (name) VALUES 
('moon'),
('sun'),
('jupiter'),
('saturn'),
('mars'),
('venus'),
('mercury'),
('uranus'),
('neptune'),
('nebula'),
('galaxy'),
('constellation'),
('meteor'),
('comet'),
('eclipse'),
('milky way'),
('star cluster'),
('planet'),
('deep space'),
('andromeda'),
('orion'),
('lunar eclipse'),
('solar eclipse'),
('astrophotography'),
('telescope'),
('binoculars'),
('observatory'),
('space station'),
('satellite'),
('aurora'),
('supernova'),
('black hole'),
('quasar'),
('pulsar');

-- Insert help content (no foreign key dependencies)
INSERT INTO help_content (title, content) VALUES 
('Getting Started', 'Welcome to AstroGathering! This app helps you organize astronomy events and share photos of celestial objects.'),
('How to Upload Photos', 'To upload photos, navigate to the Upload section and select your astronomy images. Add descriptions and tags to help others find your photos.'),
('Creating Events', 'You can create astronomy gathering events by clicking the "Create Event" button and filling in the details like date, location, and description.'),
('Tagging System', 'Use tags like "moon", "jupiter", "nebula", "constellation" to categorize your photos and make them searchable.'),
('Reporting Issues', 'If you find inappropriate content, use the report feature to notify administrators.'),
('Privacy Settings', 'Learn how to manage your privacy settings and control who can see your photos and events.'),
('Community Guidelines', 'Please follow our community guidelines to maintain a respectful and educational environment for all astronomy enthusiasts.');

-- Now insert events (references users table)
INSERT INTO events (user_id, event_name, description, event_date) VALUES 
(1, 'Monthly Star Gazing', 'Join us for our monthly star gazing event. Bring your telescopes and cameras!', '2024-09-15 20:00:00'),
(2, 'Perseid Meteor Shower Viewing', 'Come observe the Perseid meteor shower at its peak. Best viewing after midnight.', '2024-08-12 22:00:00'),
(1, 'Lunar Eclipse Photography Workshop', 'Learn techniques for photographing the upcoming lunar eclipse.', '2024-09-18 19:30:00');

-- Insert photos (references users table)
INSERT INTO photos (user_id, image_url, location, description, date_taken) VALUES 
(2, '/images/sample_moon.jpg', 'Backyard Observatory', 'Beautiful shot of the full moon through my 8-inch telescope', '2024-08-01 23:30:00'),
(3, '/images/sample_jupiter.jpg', 'Dark Sky Site', 'Jupiter with its four largest moons visible', '2024-07-28 22:15:00'),
(2, '/images/sample_milkyway.jpg', 'Desert Location', '30-second exposure of the Milky Way core', '2024-07-20 01:45:00');

-- Insert photo tags (references both photos and tags tables)
INSERT INTO photo_tags (photo_id, tag_id) VALUES 
(1, 1), -- moon photo tagged with 'moon'
(1, 24), -- moon photo tagged with 'astrophotography'
(2, 3), -- jupiter photo tagged with 'jupiter'
(2, 18), -- jupiter photo tagged with 'planet'
(2, 24), -- jupiter photo tagged with 'astrophotography'
(3, 16), -- milky way photo tagged with 'milky way'
(3, 19), -- milky way photo tagged with 'deep space'
(3, 24); -- milky way photo tagged with 'astrophotography'

-- Insert likes (references both users and photos tables)
INSERT INTO likes (user_id, photo_id) VALUES 
(1, 2), -- admin likes jupiter photo
(1, 3), -- admin likes milky way photo
(3, 1), -- jane likes moon photo
(2, 3); -- john likes milky way photo
