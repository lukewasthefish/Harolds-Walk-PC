using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public static class Extensions
{
	public static int appid = 1009360;

	public static bool IsValid<T>(this T node) where T : Godot.Node
	{
		return node != null
			&& Godot.Node.IsInstanceValid(node)
			&& !node.IsQueuedForDeletion()  
            && node.IsInsideTree();
	}

    /// <summary>
    /// Translates a scene name to a format that looks normal to humans.
    /// i.e,
    /// scene_name = Scene Name
    /// or
    /// sceneName = Scene Name
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public static string SceneNameToHumanFormat(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return string.Empty;
        }

        //Special cases for the binary stages...
        //I had to implement this because I counted the binary wrong initially!
        //dumb.
        //binary_011 -> binary_10
        if(sceneName == "binary_011")
        {
            sceneName = "binary_10";
        }

        // Replace underscores with spaces
        sceneName = sceneName.Replace("_", " ");

        // Insert space before each uppercase letter (except the first one)
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < sceneName.Length; i++)
        {
            if (i > 0 && char.IsUpper(sceneName[i]) && sceneName[i - 1] != ' ')
            {
                result.Append(' ');
            }
            result.Append(sceneName[i]);
        }

        // Capitalize the first letter of each word
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.ToString().ToLower());
    }

    public static string ParseTimeFromDouble(double time)
    {
        uint minutes = (uint)(time / 60);
        uint seconds = (uint)(time % 60);
        uint milliseconds = (uint)((time * 1000) % 1000);

        return $"{minutes:D2}:{seconds:D2}:{milliseconds:D3}";
    }

    public static double GetDefaultTimeAttackTime()
    {
        return (double)(3.0 * 60.0 - 0.001);
    }

	public static List<Node> GetAllChildrenUnderNode(Node inNode)
	{
		List<Node> children = new List<Node>();

		children.Add(inNode);
		foreach(Node n in inNode.GetChildren(true))
		{
			children.AddRange(GetAllChildrenUnderNode(n));
		}

		return children;
	}
}
