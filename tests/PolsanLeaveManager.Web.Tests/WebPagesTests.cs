using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PolsanLeaveManager.Web.Tests;

public sealed partial class WebPagesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebPagesTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task GetRoot_Returns200_WithSingleH1AndLangTr()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<html lang=\"tr\"", html, StringComparison.Ordinal);
        Assert.Single(H1().Matches(html));
        Assert.Matches(@"<h1[^>]*>Polsan İzin Yönetimi</h1>", html);
    }

    [Fact]
    public async Task GetUnknownPath_ShowsTurkishNotFound_WithHomeLink()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/boyle-bir-sayfa-yok", TestContext.Current.CancellationToken);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Sayfa bulunamadı", html, StringComparison.Ordinal);
        Assert.Matches(@"<a href=""/"">Ana sayfaya dön</a>", html);
    }

    [GeneratedRegex("<h1[ >]")]
    private static partial Regex H1();
}
