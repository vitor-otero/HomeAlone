public static class AppConfig
{
    
    //Debug
    //private static readonly string _connectionString = "Server=localhost;Database=mydatabase;User=myuser;Password=mypassword;";

    //Production
    private static readonly string _connectionString = "Server=db;Database=mydatabase;User=myuser;Password=mypassword;";
    public static string DBConnection
    {
        get { return _connectionString; }
    }
}
