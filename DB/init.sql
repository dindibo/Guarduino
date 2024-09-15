CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO users (username, password_hash) VALUES
('admin', '$2a$10$ecrjxO/KYleUBQOOVk2o6e.Qbpz7Y4hF0mfKvF6mHW.Ffn2RflCFW') -- password: password123

CREATE UNIQUE INDEX idx_users_username ON users(username);
