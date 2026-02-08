
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using orchestrator_portal.Controllers;
using orchestrator_portal.Dto;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
public class AzureArmService
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly AppDbContext _db;
    private readonly ILogger<AzureArmService> _logger;
    public AzureArmService(ILogger<AzureArmService> logger,ITokenAcquisition tokenAcquisition, AppDbContext db)
    {
        _tokenAcquisition = tokenAcquisition;
        _db = db;
        _logger = logger;

    }
    private async Task<string> GetArmTokenAsync(ClaimsPrincipal user, string tenant)
    {
        try { 
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(
                new[] { "https://management.azure.com/user_impersonation" },
                user: user,
                tenantId: tenant
             );
            return token;
        }
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            // Bubble up the exception
            throw;
        }
    }
    private async Task<List<AzureSubscription>> GetUserSubscriptionsSafeAsync(string token)
    {

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await http.GetAsync(
            "https://management.azure.com/subscriptions?api-version=2020-01-01");

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<AzureSubscriptionResponse>(json);

        return result?.value ?? new List<AzureSubscription>();
    }
    private async Task<List<StorageAccount>> GetStorageAccountsBySubscriptionSafeAsync(string token,string subscriptionId)
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url =
                $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Storage/storageAccounts" +
                "?api-version=2023-01-01";

            _logger.LogInformation($"url used to get resources: {url}");


            var response = await http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<StorageAccountResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Value ?? new();
        }
        catch (HttpRequestException ex)
        {
            // TODO: log ex + subscriptionId
            return new List<StorageAccount>();
        }
    }

    private async Task<List<ResourceGroups>> GetResourceGroupsBySubscriptionSafeAsync(string token, string subscriptionId)
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url =
                $"https://management.azure.com/subscriptions/{subscriptionId}/resourcegroups" +
                "?api-version=2021-04-01";

            _logger.LogInformation($"url used to get resources: {url}");


            var response = await http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ResourceGroupsResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Value ?? new();
        }
        catch (HttpRequestException ex)
        {
            // TODO: log ex + subscriptionId
            return new List<ResourceGroups>();
        }
    }
    private async Task<List<KeyVault>> GetKeyvaultBySubscriptionSafeAsync(string token, string subscriptionId)
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            var url =
                $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.KeyVault/vaults" +
                "?api-version=2025-05-01";

            _logger.LogInformation($"url used to get resources: {url}");

            var response = await http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<KeyVaultResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.value ?? new();
        }
        catch (HttpRequestException ex)
        {
            // TODO: log ex + subscriptionId
            return new List<KeyVault>();
        }
    }
   
    public async Task<List<ResourceGroups>> SyncUserResourceGroupsAsync(ClaimsPrincipal user, string tenantId, ResourceGroupsRequest request)
    {
        var token = await GetArmTokenAsync(user, tenantId);

        if (request.SubscriptionIds == null || request.SubscriptionIds.Count == 0)
            return new List<ResourceGroups>();

        var tasks = request.SubscriptionIds.Distinct().Select(subId => GetResourceGroupsBySubscriptionSafeAsync(token, subId));

        var results = await Task.WhenAll(tasks);

        var allresults = results.SelectMany(x => x);

        // client side search
        if (!string.IsNullOrWhiteSpace(request.search))
        {
            var search = request.search.Trim();

            allresults = allresults.Where(sa =>
                sa.Id.Contains(search, StringComparison.OrdinalIgnoreCase)
            );
        }
        return allresults.ToList();
    }


    public async Task<List<KeyVault>> SyncUserKeyVaultAsync(ClaimsPrincipal user, string tenantId, KeyVaultRequest request)
    {
        var token = await GetArmTokenAsync(user, tenantId);

        if (request.SubscriptionIds == null || request.SubscriptionIds.Count == 0)
            return new List<KeyVault>();

        var tasks = request.SubscriptionIds.Distinct().Select(subId => GetKeyvaultBySubscriptionSafeAsync(token, subId));

        var results = await Task.WhenAll(tasks);

        var allresults = results.SelectMany(x => x);

        // client side search
        if (!string.IsNullOrWhiteSpace(request.search))
        {
            var search = request.search.Trim();

            allresults = allresults.Where(sa =>
                sa.Id.Contains(search, StringComparison.OrdinalIgnoreCase) 
            );
        }
        return allresults.ToList();
    }

    public async Task<List<StorageAccount>> SyncUserStorageAccountAsync(ClaimsPrincipal user, string tenantId, StorageAccountRequest request)
    {
        var token = await GetArmTokenAsync(user, tenantId);

        if (request.SubscriptionIds == null || request.SubscriptionIds.Count == 0)
            return new List<StorageAccount>();

        var tasks = request.SubscriptionIds.Distinct().Select(subId => GetStorageAccountsBySubscriptionSafeAsync(token, subId));

        var results = await Task.WhenAll(tasks);

        var allresults = results.SelectMany(x => x);

        // client side search
        if (!string.IsNullOrWhiteSpace(request.search))
        {
            var search = request.search.Trim();

            allresults = allresults.Where(sa =>
                sa.Id.Contains(search, StringComparison.OrdinalIgnoreCase) 
            );
        }
        return allresults.ToList();
    }
    public async Task<List<AzureSubscription>> SyncUserAzureSubscriptionAsync(ClaimsPrincipal user, string tenantId, AzureSubscriptionRequest request)
    {
        var token = await GetArmTokenAsync(user, tenantId);

        if (request.SubscriptionIds == null || request.SubscriptionIds.Count == 0)
            return new List<AzureSubscription>();

        var subscriptions = (await GetUserSubscriptionsSafeAsync(token)).ToList();

        subscriptions = subscriptions
            .Where(sa =>
                !string.IsNullOrEmpty(sa.subscriptionId) &&
                request.SubscriptionIds.Contains(sa.subscriptionId))
                .Select(sa =>
                {
                    sa.Id = $"/subscriptions/{sa.subscriptionId}";
                    return sa;
                })
            .ToList();
        // client side search
        if (!string.IsNullOrWhiteSpace(request.search))
        {
            var search = request.search.Trim();

            subscriptions = subscriptions
               .Where(sa =>
                   !string.IsNullOrEmpty(sa.displayName) &&
                   sa.displayName.Contains(search, StringComparison.OrdinalIgnoreCase)
               )
               .ToList();
        }


        return subscriptions;
    }

    public async Task SyncUserSubscriptionsAsync(ClaimsPrincipal user,  string tenantId)
    {
        var token = await GetArmTokenAsync(user, tenantId);

        var subscriptions = await GetUserSubscriptionsSafeAsync(token);

        var existingIds = (await _db.Subscriptions
                        .Select(p => p.subscriptionId)
                        .ToListAsync())
                        .ToHashSet();

        var newSubscriptions = subscriptions
            .Where(s => s.state == "Enabled")
            .Where(s => !existingIds.Contains(s.subscriptionId))
            .Select(s => new Subscriptions
            {
                displayName = s.displayName,
                subscriptionId = s.subscriptionId,
                state = s.state,
                fordeploy = false
            });

        if (newSubscriptions.Any())
        {
            _db.Subscriptions.AddRange(newSubscriptions);
            await _db.SaveChangesAsync();
        }
    }
}

