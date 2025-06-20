namespace WebApp.AzureCreators;

public class PowerShellAzureCreator : IAzureCreator
{
    public string CreateResourceGroup(string resourceGroupName, string location = "North Europe")
    {
        var resourceGroupParams = new
        {
            Name = resourceGroupName,
            Location = location
        };
        var jsonParams = Newtonsoft.Json.JsonConvert.SerializeObject(resourceGroupParams);
        var powerShellScript = $@"
            $params = ConvertFrom-Json '{jsonParams}'
            New-AzResourceGroup -Name $params.Name -Location $params.Location
            ";
        
        return powerShellScript;
    }
}