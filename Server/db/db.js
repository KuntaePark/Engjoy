const mysql = require("mysql2/promise");

const pool = mysql.createPool({
  host: "210.219.170.244",
  port: 3306,
  user: "abc",
  password: "1234",
  database: "engjoy",
  waitForConnections: true,
  connectionLimit: 10,
  queueLimit: 0,
});

module.exports = pool;

console.log("Database connection pool created successfully.");
