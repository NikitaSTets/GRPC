namespace Storage.Interfaces;

public interface ICodeStorageService
{
    public void AddCodes(HashSet<string> codes);

    public void RemoveCode(string code);

    public HashSet<string> LoadCodes();
}
