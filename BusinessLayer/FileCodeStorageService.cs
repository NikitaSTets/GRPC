using Storage.Interfaces;
using System.Text.Json;

namespace Storage;

public class FileCodeStorageService : ICodeStorageService
{
    private readonly string _filePath;

    public FileCodeStorageService(string filePath)
    {
        _filePath = filePath;
    }

    public void SaveCodes(HashSet<string> codes)
    {
        File.WriteAllText(_filePath, JsonSerializer.Serialize(codes));
    }


    public HashSet<string> LoadCodes()
    {
        return File.Exists(_filePath)
            ? JsonSerializer.Deserialize<HashSet<string>>(File.ReadAllText(_filePath)) ?? []
            : [];
    }
}
