namespace SmartX.Api.Models;

public class DeploymentNode
{
    public string Name {get;set;} = string.Empty;
    public bool IsConfigured {get;set;}
    public List<DeploymentNode> Children {get;set;} = new();

    public bool ValidateHierarchy()
    {
        if (Children.Count == 0)
            return IsConfigured; 

        return IsConfigured && Children.All(child => child.ValidateHierarchy());
    }

    public List<string>? FindFirstInvalidPath(List<string>? currentPath = null)
    {
        currentPath ??= new List<string>();
        currentPath.Add(Name);

        if (!IsConfigured)
            return currentPath;

        foreach (var child in Children)
        {
            var result = child.FindFirstInvalidPath(new List<string>(currentPath));
            if (result != null)
                return result;
        }

        return null;
    }
}