namespace Storage.Interfaces;

public interface ICodeStorageService
{
    public void SaveCodes(HashSet<string> codes);

    public HashSet<string> LoadCodes();
}
