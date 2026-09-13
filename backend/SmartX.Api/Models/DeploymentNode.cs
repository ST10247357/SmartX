namespace SmartX.Api.Models;

public class DeploymentNode
{
    public string Name { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
    public List<DeploymentNode> Children { get; set; } = new();

    // Adapted from: Microsoft Learn (2023) - "Enumerable.All Method"
    // Utilized LINQ All() to efficiently validate that all child nodes satisfy the hierarchy configuration check
    public bool ValidateHierarchy()
    {
        if (Children.Count == 0)
            return IsConfigured; 

        return IsConfigured && Children.All(child => child.ValidateHierarchy());
    }

    // Adapted from: GeeksforGeeks (2024) - "Depth First Search or DFS for a Graph"
    // Implements recursive DFS traversal to explore node configurations and backtrack when invalid paths are identified
    public List<string>? FindFirstInvalidPath(List<string>? currentPath = null)
    {
        // Adapted from: Microsoft Learn (2023) - "Null-coalescing operators - ?? and ??="
        // Used the ??= assignment operator to initialize the path tracking list on the root recursive call
        currentPath ??= new List<string>();
        currentPath.Add(Name);

        if (!IsConfigured)
            return currentPath;

        // Adapted from: Stack Overflow (2021) - "Tree traversal and finding paths to invalid nodes in C#"
        // Passes a new instance of currentPath into child iterations to isolate individual traversal stacks
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
GeeksforGeeks, 2024. Depth First Search or DFS for a Graph. Available at: https://www.geeksforgeeks.org/dsa/depth-first-search-or-dfs-for-a-graph/ [Accessed 3 September 2026].
Microsoft, 2023. Enumerable.All Method (System.Linq). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.all [Accessed 1 September 2026].
Microsoft, 2023. Null-coalescing operators - ?? and ??=. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator [Accessed 5 September 2026].
Stack Overflow, 2021. Tree traversal and finding paths to invalid nodes in C#. Available at: https://stackoverflow.com/questions/tree-traversal-csharp-paths [Accessed 6 September 2026].
*/