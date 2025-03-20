using Storage.Interfaces;
using System.Text;

namespace Storage;

public class FileCodeStorageService : ICodeStorageService
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _fileLock = new(1, 1);

    public FileCodeStorageService(string filePath)
    {
        _filePath = filePath;
    }

    public void AddCodes(HashSet<string> codes)
    {
        _fileLock.Wait();
        try
        {
            if (File.Exists(_filePath))
            {
                File.AppendAllText(_filePath, "," + string.Join(",", codes), Encoding.UTF8);

            }
            else
            {
                File.WriteAllText(_filePath, string.Join(",", codes), Encoding.UTF8);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public void RemoveCode(string code)
    {
        _fileLock.Wait();
        try
        {
            var fileContent = File.ReadAllText(_filePath, Encoding.UTF8);
            var codes = fileContent.Split([','], StringSplitOptions.RemoveEmptyEntries).ToHashSet();   
            var isCodeRemoved = codes.Remove(code);

            if (isCodeRemoved)
            {
                var updatedContent = string.Join(",", codes);
                File.WriteAllText(_filePath, updatedContent, Encoding.UTF8);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public HashSet<string> LoadCodes()
    {
        var codes = new HashSet<string>();

        if (File.Exists(_filePath))
        {
            var fileContent = File.ReadAllText(_filePath, Encoding.UTF8);
            var codesArray = fileContent.Split(',');

            foreach (var code in codesArray)
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    codes.Add(code);
                }
            }
        }

        return codes;
    }
}
