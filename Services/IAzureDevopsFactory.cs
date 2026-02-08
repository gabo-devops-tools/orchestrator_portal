using Microsoft.Identity.Web;
using orchestrator_portal.dto;
using orchestrator_portal.Dto;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

public interface IAzureDevopsFactory
{
    Task<bool> IsPipelineRunningAsync(string token,string organization, string coreproject, string projecttobecreated, int pipelineId);
    Task TriggerPipelineRunAsync(string token ,string organization, string coreproject, int pipelineId, DataSelectionRequest request, string pool);
    Task<string> GetDevOpsTokenAsync(ClaimsPrincipal user, string tenantId);
    Task<List<DevOpsProject>> SearchforProjects(string token, string organization);
}

public class AzureDevopsFactory : IAzureDevopsFactory
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly ILogger<AzureArmService> _logger;


    public AzureDevopsFactory(ITokenAcquisition tokenAcquisition, ILogger<AzureArmService> logger)
    {
        _tokenAcquisition = tokenAcquisition;
        _logger = logger;
    }

    async Task<List<DevOpsProject>> IAzureDevopsFactory.SearchforProjects(string token, string organization)
    {
        var url = $"https://dev.azure.com/{organization}/_apis/projects?api-version=7.0";

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
         new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<DevopsProjectResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        // Return the list of DevOpsProject, or empty list if null
        return result?.value ?? new List<DevOpsProject>();
    }


    async Task<bool> IAzureDevopsFactory.IsPipelineRunningAsync(string token, string organization, string coreproject, string projecttobecreated, int pipelineId)
    {
        var url = $"https://dev.azure.com/{organization}/{coreproject}/_apis/build/builds" +
                    $"?definitions={pipelineId}" +
                    $"&statusFilter=inProgress" +
                    $"&tagFilters={projecttobecreated}" +
                    $"&api-version=7.1-preview.7";

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BuildListResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        return result?.Count > 0;
    }


    public async Task TriggerPipelineRunAsync(string token, string organization, string coreproject, int pipelineId, DataSelectionRequest request, string pool)
    {
        var url =
            $"https://dev.azure.com/{organization}/{coreproject}/_apis/pipelines/{pipelineId}/runs" +
            $"?api-version=7.1-preview.1";

        //Convert lists → JSON strings encoded as another string (pipeline expects strings)
        var payloadJson = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            WriteIndented = false
        })));
        _logger.LogInformation("subscriptions JSON:\n{subscriptionsJson}", payloadJson);

        var payload = new
        {
            stagesToSkip = Array.Empty<string>(),
            resources = new
            {
                repositories = new
                {
                    self = new
                    {
                        refName = "refs/heads/master"
                    }
                }
            },
            templateParameters = new
            {
                payload = payloadJson,
                agentPool = "Azure Pipelines", //"aks pool", Azure Pipelines,
                automation_principal = "management-group-azure-devops-orchestation-automation"
            },
            variables = new { }
        };


        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(
                $"Failed to trigger pipeline. Status: {response.StatusCode}\n{error}");
        }
    }
    public async Task<string> GetDevOpsTokenAsync(ClaimsPrincipal user, string tenantId)
    {
        try
        {
            return await _tokenAcquisition.GetAccessTokenForUserAsync(
                new[] { "499b84ac-1321-427f-aa17-267ca6975798/.default" },
                user: user,
                tenantId: tenantId
            );
        }             
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            // Bubble up the exception
            throw;
        }
    }
}