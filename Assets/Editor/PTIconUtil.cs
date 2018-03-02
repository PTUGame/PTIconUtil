using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;
using UnityEditor.Callbacks;


public class PTIconUtil
{

	private static string GetIconFilePath()
	{
		string projectSettingsStr = File.ReadAllText("./ProjectSettings/ProjectSettings.asset");

		Regex id = new Regex(@"m_Icon: {fileID: ([\s\S]*?)}");

		Match match = id.Match (projectSettingsStr);
		string matchValue = match.Value;

		int start = matchValue.IndexOf("guid: ",StringComparison.Ordinal);
		int end = matchValue.IndexOf(", type",StringComparison.Ordinal);

		string iconGuid = matchValue.Substring(start + 6, end - start + 2 - 2 - 6);

		string path = AssetDatabase.GUIDToAssetPath(iconGuid);

		return path;

	}
	
    /// <summary>
    /// iOS要求icon源文件必须为1024x1024 的 不带alpha通道的 png 格式图片
    /// </summary>
    /// <returns></returns>
    public static bool CheckIcon()
    {
        int width = 1024;
        int heigth = 1024;

		string path = GetIconFilePath ();

	    if (!path.EndsWith(".png"))
	    {
		    return false;
	    }

	    Texture2D tmpTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));

        if (tmpTexture.width == width && tmpTexture.height == heigth && !tmpTexture.alphaIsTransparency)
        {
            return true;
        }
        
        return false;
    }

    
	[PostProcessBuildAttribute(100)]
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
		if (target != BuildTarget.iOS)
		{
			return;
		}

		string iconPath = GetIconFilePath ();

		if (!File.Exists(iconPath))
		{
			return;
		}

		string iconFileName = Path.GetFileName(iconPath);
		
		//替换为自己的打包出的Xcode工程路径
		string iconDir = "XCode_Unity/Unity-iPhone/Images.xcassets/AppIcon.appiconset";
		
		string content = File.ReadAllText(iconDir+"/Contents.json");

		XCodeIconContent iconContent = JsonUtility.FromJson<XCodeIconContent>(content);

		bool hasmarketing = false;
		foreach (var image in iconContent.images)
		{
			if (image.idiom == "ios-marketing")
			{
				image.filename = iconFileName;
				hasmarketing = true;
			}
		}
		if (!hasmarketing)
		{
			XCodeIconImage imageItem = new XCodeIconImage()
			{
				size = "1024x1024",
				idiom = "ios-marketing",
				filename = iconFileName,
				scale = "1x"
			};
			iconContent.images.Add(imageItem);
		}
		
		File.Copy(iconPath,Path.Combine(iconDir,iconFileName),true);
	
		string  result = JsonUtility.ToJson(iconContent, true);
		
		File.WriteAllText(Path.Combine(iconDir,"Contents.json"),result);


	}

	[Serializable]
	class XCodeIconContent{

		public List<XCodeIconImage> images;
		public XCodeIconInfo info;
	}


	[Serializable]
	class XCodeIconImage
	{

		public string size;
		public string idiom;
		public string filename;
		public string scale;
	}
	[Serializable]
	class XCodeIconInfo
	{
		public string version;
		public string author;
	}

}