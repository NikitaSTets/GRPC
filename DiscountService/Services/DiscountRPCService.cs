using DiscountService.Interfaces;
using Grpc.Core;
using Storage.Interfaces;

namespace DiscountService.Services;

public class DiscountRPCService : DiscountRPC.DiscountRPCBase
{
    private readonly object _lock = new();
    private readonly HashSet<string> _discountCodes;
    private readonly ICodeValidationService _codeValidationService;
    private readonly ICodeStorageService _codeStorageService;

    public DiscountRPCService(ICodeValidationService codeValidationService, ICodeStorageService codeStorageService)
    {
        _codeValidationService = codeValidationService;
        _codeStorageService = codeStorageService;

        _discountCodes = codeStorageService.LoadCodes();
    }

    public override Task<GenerateCodesResponse> GenerateCodes(GenerateCodesRequest request, ServerCallContext context)
    {
        var isCodeLengthValid = _codeValidationService.ValidateCodeLength(request.Length);

        if (request.Count == 0)
        {
            return Task.FromResult(new GenerateCodesResponse { Result = true });
        }

        if (request.Count > 2000 || !isCodeLengthValid)
            return Task.FromResult(new GenerateCodesResponse { Result = false });

        var newCodes = new HashSet<string>();
        lock (_lock)
        {
            while (newCodes.Count < request.Count)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var code = GenerateRandomCode(request.Length);

                if (_discountCodes.Add(code)) 
                {
                    newCodes.Add(code);
                }
            }

            _codeStorageService.SaveCodes(_discountCodes);
        }
        var response = new GenerateCodesResponse { Result = true };

        return Task.FromResult(response);
    }


    public override Task<UseCodeResponse> UseCode(UseCodeRequest request, ServerCallContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var isCodeValid = _codeValidationService.ValidateCode(request.Code);

        if (!isCodeValid)
            return Task.FromResult(new UseCodeResponse()
            {
                Result = UseCodeResultCode.Invalid
            });

        lock (_lock)
        {
            if (_discountCodes.Contains(request.Code))
            {
                _discountCodes.Remove(request.Code);
                _codeStorageService.SaveCodes(_discountCodes);

                return Task.FromResult(new UseCodeResponse()
                {
                    Result = UseCodeResultCode.Success
                });
            }

            return Task.FromResult(new UseCodeResponse()
            {
                Result = UseCodeResultCode.NotFound
            });
        }
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
