namespace DiscountService.Interfaces;

public interface ICodeValidationService
{
    public bool ValidateCode(string code);

    public bool ValidateCodeLength(int codeLength);
}
