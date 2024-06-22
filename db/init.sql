
CREATE TABLE IF NOT EXISTS mytable (
    id INT AUTO_INCREMENT PRIMARY KEY,
    value VARCHAR(255) NOT NULL
);

INSERT INTO mytable (value) VALUES ('Hello, World!');
