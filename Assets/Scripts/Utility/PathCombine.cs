using UnityEngine;
using System.Collections;
using System.IO;

public static class PathCombine {

    /**<summary> Combine path sections with / or \ </summary>*/
	public static string Combine(this string path1, string path2)
	{
		if (string.IsNullOrEmpty(path2))
		{
			return path1;
		}
		else
		{
			if (Path.IsPathRooted(path2))
			{
				path2 = path2.TrimStart(Path.DirectorySeparatorChar);
				path2 = path2.TrimStart(Path.AltDirectorySeparatorChar);
			}
			return Path.Combine(path1, path2);
		}
	}

    /**<summary> Uniform all slash characters to / </summary>*/
	public static string UniformSlash(this string path)
	{
		return path.Replace("\\", "/");
	}
}
