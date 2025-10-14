using Microsoft.AspNetCore.Mvc.Testing;

namespace AuthService.Tests;

/// <summary>
/// Legacy placeholder retained for backwards compatibility. The updated tests rely on the
/// default <see cref="WebApplicationFactory{TStartup}"/> pipeline, so this type intentionally
/// contains no additional configuration.
/// </summary>
public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
}