
using System.Text.Json;

namespace AcademiaAPI.Services;

public class JsonService
{
    private readonly string _dbPath;

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonService(IWebHostEnvironment environment)
    {
        _dbPath = Path.Combine(
            environment.ContentRootPath,
            "DB"
        );

        Directory.CreateDirectory(_dbPath);
    }

    // Lê todos os registros de um arquivo JSON
    public async Task<List<T>> ReadAsync<T>(string fileName)
    {
        await _lock.WaitAsync();

        try
        {
            string path = GetFilePath(fileName);

            if (!File.Exists(path))
            {
                await File.WriteAllTextAsync(path, "[]");
            }

            string json = await File.ReadAllTextAsync(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                await File.WriteAllTextAsync(path, "[]");
                return new List<T>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<T>>(
                    json,
                    _options
                ) ?? new List<T>();
            }
            catch (JsonException)
            {
                await File.WriteAllTextAsync(path, "[]");
                return new List<T>();
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    // Grava uma lista de registros no arquivo JSON
    public async Task WriteAsync<T>(
        string fileName,
        List<T> data)
    {
        await _lock.WaitAsync();

        string? tempPath = null;

        try
        {
            string path = GetFilePath(fileName);

            string json = JsonSerializer.Serialize(
                data,
                _options
            );

            tempPath = path + "." + Guid.NewGuid() + ".tmp";

            await File.WriteAllTextAsync(tempPath, json);

            File.Move(tempPath, path, true);
        }
        finally
        {
            if (tempPath != null && File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            _lock.Release();
        }
    }

    // Monta e valida o caminho do arquivo
    private string GetFilePath(string fileName)
    {
        if (Path.GetFileName(fileName) != fileName ||
            !fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Nome de arquivo inválido.");
        }

        return Path.Combine(_dbPath, fileName);
    }
}
