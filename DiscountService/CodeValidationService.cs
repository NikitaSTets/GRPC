using DiscountService.Interfaces;

namespace DiscountService;

public class CodeValidationService : ICodeValidationService
{
    const int MinLength = 7;
    const int MaxLength = 8;

    public bool ValidateCode(string code)
    {
        return code.Length >= MinLength && code.Length <= MaxLength;
    }

    public bool ValidateCodeLength(int codeLength)
    {
        return codeLength >= MinLength && codeLength <= MaxLength;
    }
}
