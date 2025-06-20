public interface IAzureCreator
{
    string CreateResourceGroup(string resourceGroupName, string location = "North Europe");
    
}