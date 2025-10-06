namespace System.Challenge.FIAP.Configuration;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public string Server { get; set; } = string.Empty;
    public string ServerPublic { get; set; } = string.Empty;
    public string Port { get; set; } = "1433";
    public string PortPublic { get; set; } = string.Empty;
    public string Username { get; set; } = "sa";
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = "master";
    public bool UsePublicServer { get; set; } = false;

    public string GetConnectionString()
    {
        var server = UsePublicServer && !string.IsNullOrWhiteSpace(ServerPublic) 
            ? ServerPublic 
            : Server;
        
        var port = UsePublicServer && !string.IsNullOrWhiteSpace(PortPublic) 
            ? PortPublic 
            : Port;

        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(Password))
        {
            throw new InvalidOperationException("Configurações do banco de dados inválidas!");
        }

        return $"Server=tcp:{server},{port};Database={Name};User Id={Username};Password={Password};TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=True;Connection Timeout=30;";
    }
}
