using DiscountService.Interfaces;
using Grpc.Core;
using Storage.Interfaces;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace DiscountService.Services;

public class DiscountRPCService : DiscountRPC.DiscountRPCBase
{
    private readonly ConcurrentDictionary<string, bool> _discountCodes;
    private readonly ICodeValidationService _codeValidationService;
    private readonly ICodeStorageService _codeStorageService;

    public DiscountRPCService(ICodeValidationService codeValidationService, ICodeStorageService codeStorageService)
    {
        _codeValidationService = codeValidationService;
        _codeStorageService = codeStorageService;

        _discountCodes = new ConcurrentDictionary<string, bool>(
            codeStorageService.LoadCodes().Select(code => new KeyValuePair<string, bool>(code, true))
        );
    }

    public override Task<GenerateCodesResponse> GenerateCodes(GenerateCodesRequest request, ServerCallContext context)
    {
        if (request.Count == 0)
            return Task.FromResult(new GenerateCodesResponse { Result = true });

        if (request.Count > 2000 || !_codeValidationService.ValidateCodeLength(request.Length))
            return Task.FromResult(new GenerateCodesResponse { Result = false });

        var newCodes = new ConcurrentBag<string>();

        Parallel.For(0, request.Count, (i, state) =>
        {
            if (context.CancellationToken.IsCancellationRequested)
                state.Stop();

            string code;
            do
            {
                code = GenerateRandomCode(request.Length);
            } while (!_discountCodes.TryAdd(code, true));

            newCodes.Add(code);
        });

        _codeStorageService.AddCodes(newCodes.ToHashSet());

        return Task.FromResult(new GenerateCodesResponse { Result = true });
    }

    public override Task<UseCodeResponse> UseCode(UseCodeRequest request, ServerCallContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (!_codeValidationService.ValidateCode(request.Code))
            return Task.FromResult(new UseCodeResponse { Result = UseCodeResultCode.Invalid });

        if (_discountCodes.TryRemove(request.Code, out _))
        {
            _codeStorageService.RemoveCode(request.Code);
            return Task.FromResult(new UseCodeResponse { Result = UseCodeResultCode.Success });
        }

        return Task.FromResult(new UseCodeResponse { Result = UseCodeResultCode.NotFound });
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var buffer = new byte[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }

        return new string(buffer.Select(b => chars[b % chars.Length]).ToArray());
    }
}
