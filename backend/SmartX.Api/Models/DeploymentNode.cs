namespace SmartX.Api.Models;

// Adapted from: GeeksforGeeks (2024) - "Depth First Search or DFS for a Graph"
// Implements recursive DFS traversal to validate node configuration states and backtrack invalid paths
public class DeploymentNode
{
    public string Name { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
    public List<DeploymentNode> Children { get; set; } = new();

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

/*
References:
GeeksforGeeks, 2024. Depth First Search or DFS for a Graph. Available at: https://www.geeksforgeeks.org/dsa/depth-first-search-or-dfs-for-a-graph/ [Accessed 10 September 2026].
*/