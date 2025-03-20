using NSubstitute;
using Grpc.Core;
using DiscountService;
using DiscountService.Services;
using DiscountService.Interfaces;
using Storage.Interfaces;

namespace DiscountServiceTests;

[TestFixture]
public class DiscountServiceTests
{
    private DiscountRPCService _service;
    private ServerCallContext _context;
    private ICodeStorageService _codeStorageService;
    private ICodeValidationService _codeValidationService = new CodeValidationService();

    [SetUp]
    public void Setup()
    {
        _codeStorageService = Substitute.For<ICodeStorageService>();
        _codeStorageService.LoadCodes().Returns([]);
        _service = new DiscountRPCService(_codeValidationService, _codeStorageService);
        _context = Substitute.For<ServerCallContext>();
    }

    [TestCase((uint)5, 8)]
    [TestCase((uint)6, 7)]
    public async Task GenerateCodes_ShouldReturn_CorrectNumberOfCodes(uint count, int length)
    {
        var request = new GenerateCodesRequest { Count = count, Length = length };
        var response = await _service.GenerateCodes(request, _context);

        _codeStorageService.Received(1).AddCodes(Arg.Is<HashSet<string>>(codes => codes != null && codes.Count == (int)count && codes.All(code => code.Length == length)));

        Assert.That(response.Result, Is.True);
    }

    [Test]
    public async Task GenerateCodes_ShouldFail_WhenCodeLengthIsInvalid()
    {
        _codeStorageService.LoadCodes().Returns(["testCode", "testCode2", "testCode3"]);
        _service = new DiscountRPCService(_codeValidationService, _codeStorageService);

        var request = new GenerateCodesRequest { Count = 5, Length = 5 };

        var response = await _service.GenerateCodes(request, _context);

        Assert.That(response.Result, Is.False);
    }

    [TestCase((uint)12002)]
    [TestCase((uint)2001)]
    public async Task GenerateCodes_ShouldFail_WhenCodeCountsIsInvalid(uint count)
    {
        var request = new GenerateCodesRequest { Count = count, Length = 8 };

        var response = await _service.GenerateCodes(request, _context);

        Assert.That(response.Result, Is.False);
    }

    [Test]
    public async Task UseCode_ShouldReturn_True_IfCodeExists()
    {
        _codeStorageService.LoadCodes().Returns(["testCode", "testCode2", "testCode3"]);
        _service = new DiscountRPCService(_codeValidationService, _codeStorageService);

        var useCodeRequest = new UseCodeRequest { Code = "testCode" };
        var useCodeResponse = await _service.UseCode(useCodeRequest, _context);

        Assert.That(useCodeResponse.Result, Is.EqualTo(UseCodeResultCode.Success));
    }

    [Test]
    public async Task UseCode_ShouldReturn_False_IfCodeDoesnExists()
    {
        var useCodeRequest = new UseCodeRequest { Code = "ValCode" };
        var useCodeResponse = await _service.UseCode(useCodeRequest, _context);

        Assert.That(useCodeResponse.Result, Is.EqualTo(UseCodeResultCode.NotFound));
    }

    [Test]
    public async Task UseCode_ShouldReturn_False_IfCodeDoesNotExist()
    {
        var useCodeRequest = new UseCodeRequest { Code = "INVAL" };
        var useCodeResponse = await _service.UseCode(useCodeRequest, _context);

        Assert.That(useCodeResponse.Result, Is.EqualTo(UseCodeResultCode.Invalid));
    }
}
