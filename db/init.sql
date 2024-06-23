CREATE TABLE IF NOT EXISTS Users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    role ENUM('admin', 'user') DEFAULT 'user'
);

CREATE TABLE IF NOT EXISTS Needs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT,
    description VARCHAR(255),
    is_met BOOLEAN DEFAULT FALSE,
    percentage_met INT DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS Payments (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT,
    description VARCHAR(255),
    amount DECIMAL(10, 2),
    due_date DATE,
    is_paid BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS Pending (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT,
    description VARCHAR(255),
    due_date DATE,
    is_completed BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS SharedData (
    id INT AUTO_INCREMENT PRIMARY KEY,
    owner_id INT,
    shared_with_id INT,
    module ENUM('Needs', 'Payments', 'Pending'),
    item_id INT,
    FOREIGN KEY (owner_id) REFERENCES Users(id),
    FOREIGN KEY (shared_with_id) REFERENCES Users(id)
);
