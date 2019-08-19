using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEditor.IMGUI.Controls;
using Rotorz.ReorderableList;
using System.Text;
public class CreateCSVParseCode : EditorWindow 
{
	private CSVLoader csvLoader;

	private CSVLoader CsvLoader
	{
		get
		{
			if (csvLoader == null)
			{
				csvLoader = new CSVLoader();
			}
			return csvLoader;
		}
	}
	
	
	private static CreateCSVParseCode window = null;

	private static string curCSVPath;
	private static string curCSVFileName;
	
	private static List<string> csvFilesPathList = new List<string> ();
	private static string assetPath;
	private bool initialized = false;
	private SearchField searchField;
	private Vector2 scrollWidgetPos;
	private Vector2 csvDataScrollViewPos;
	private Vector2 parseMethodScrollViewPos;
	private Vector2 csvModelClassPos;
	private string searchStr = "";
	private List<CsvData> data = new List<CsvData>();

	private float interval = 20f;

	private string dataModelStr = "";

	private string parseCSVCodeStr = "";

	Rect SearchFieldRect
	{
		get
		{
			return new Rect(interval,interval,position.width * 0.3f,20f);
		}
	}
	
	Rect csvListRect 
	{
        get { return new Rect (interval, interval + SearchFieldRect.yMax, SearchFieldRect.width, window.position.height - SearchFieldRect.yMax - 2 * interval); }
    }
	
	Rect ToolBarRect
	{
		get{return new Rect(SearchFieldRect.xMax + interval,interval,window.position.width - SearchFieldRect.xMax - 2 * interval,20f);}
	}

	Rect csvDataRect
	{
		get{return new Rect(ToolBarRect.x,ToolBarRect.yMax + interval,ToolBarRect.width,window.position.height * 0.3f);}
	}

	Rect ModelClassPreviewRect
	{
		get{return new Rect(csvDataRect.x,csvDataRect.yMax + interval,(csvDataRect.width - 20)/2,window.position.height - csvDataRect.yMax - 2 * interval);}
	}

	Rect CSVDataParsePreviewRect
	{
		get{return new Rect(ModelClassPreviewRect.xMax + interval,ModelClassPreviewRect.yMin,ModelClassPreviewRect.width,ModelClassPreviewRect.height);}
	}

	private static bool isLimitSize = false;

	[MenuItem("Tools/CreateCSVParseCode")]
	public static void CSVCode()
	{
		csvFilesPathList.Clear ();
        assetPath = Application.dataPath;
        GetFiles (new DirectoryInfo (assetPath), "*.csv", ref csvFilesPathList);
		if (window == null)
			window = EditorWindow.GetWindow(typeof(CreateCSVParseCode)) as CreateCSVParseCode;
		window.titleContent = new GUIContent("CreateCSVParseCode");
		if(isLimitSize)
		{
			window.minSize = new Vector2(1420,674);
			window.maxSize = new Vector2(1420,674);
		}
		window.Show();
	}

	public static void GetFiles (DirectoryInfo directory, string pattern, ref List<string> fileList) 
	{
        if (directory != null && directory.Exists && !string.IsNullOrEmpty (pattern)) {
            try {
                foreach (FileInfo info in directory.GetFiles (pattern)) {
                    string path = info.FullName.ToString ();
                    fileList.Add (path.Substring (path.IndexOf ("Assets")));
                }
            } catch (System.Exception) 
			{
                throw;
            }
            foreach (DirectoryInfo info in directory.GetDirectories ()) 
			{
                GetFiles (info, pattern, ref fileList);
            }
        }
    }

	void OnGUI()
	{
		InitIfNeeded();
		DrawWindow();
	}

	private void InitIfNeeded () 
	{
        if (!initialized) 
		{
            if (null == searchField)
                searchField = new SearchField ();
            initialized = true;
			data.Clear();
        }
    }
	

	void DrawWindow()
	{
		DrawSearchField();
		DrawCSVList();
		DrawToolBar();
		DrawCSVDataUI();
		DrawModelClass();
		DrawCSVDataParsePreview();
	}
	
	void DrawSearchField()
	{
		GUI.backgroundColor = Color.white;
		searchStr = searchField.OnGUI (SearchFieldRect, searchStr);
		searchStr = searchStr.ToLower();
	}

	
	void DrawCSVList()
	{
		//EditorGUILayout.Space();
		
		GUI.backgroundColor = Color.white;
			
		GUI.Box(csvListRect,"");
		GUILayout.BeginArea(csvListRect);
		//CSV表列表
		scrollWidgetPos = EditorGUILayout.BeginScrollView(scrollWidgetPos);
		for (int i = 0; i < csvFilesPathList.Count; i++)
		{
			if(CheckShowCSV(csvFilesPathList[i],searchStr))
			{
				if(GUILayout.Button(csvFilesPathList[i]))
				{
					curCSVPath = csvFilesPathList[i];
					curCSVFileName = GetFileName(curCSVPath);
					SpeculativeParseCSV(curCSVPath);
					CreateModelClass();
					CreateModelParseCode();
				}
			}
		}
		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	void DrawToolBar()
	{
		GUILayout.BeginArea(ToolBarRect);
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("生成数据类文件"))
		{
			CreateModelClassScriptFile();
		}
		if(GUILayout.Button("拷贝数据类代码"))
		{
			CopyCodeText(dataModelStr);
		}
		if(GUILayout.Button("拷贝配置解析代码"))
		{
			CopyCodeText(parseCSVCodeStr);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	

	void DrawModelClass()
	{
		GUI.backgroundColor = Color.white;
        GUI.Box(ModelClassPreviewRect, "");
		GUILayout.BeginArea(ModelClassPreviewRect);
        EditorGUILayout.HelpBox("数据结构类代码预览:", MessageType.None);
		csvModelClassPos = EditorGUILayout.BeginScrollView(csvModelClassPos);
		GUILayout.Label(dataModelStr);
		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	void DrawCSVDataParsePreview()
	{
		GUI.backgroundColor = Color.white;
        GUI.Box(CSVDataParsePreviewRect, "");
		GUILayout.BeginArea(CSVDataParsePreviewRect);
        EditorGUILayout.HelpBox("解析函数代码预览:", MessageType.None);
		parseMethodScrollViewPos = EditorGUILayout.BeginScrollView(parseMethodScrollViewPos);
		GUILayout.Label(parseCSVCodeStr.ToString());
		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	/// <summary>
	/// 绘制预解析CSV
	/// </summary>
	void DrawCSVDataUI()
	{
		GUI.Box(csvDataRect, "");
		GUILayout.BeginArea(csvDataRect);
		Rect listRect = new Rect(0,0,csvDataRect.width,csvDataRect.height);
		GUILayout.Label(curCSVPath);
		csvDataScrollViewPos = EditorGUILayout.BeginScrollView(csvDataScrollViewPos);
		
		EditorGUI.BeginChangeCheck();
		ReorderableListGUI.ListField<CsvData>(data, DrawWidget,30,ReorderableListFlags.HideAddButton | ReorderableListFlags.DisableAutoScroll);
		if(EditorGUI.EndChangeCheck())
		{
			CreateModelClass();
			CreateModelParseCode();
		}
        EditorGUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private CsvData DrawWidget(Rect position, CsvData item)
    {
		float midY = position.y + position.height / 2 - 5;
		Rect fieldLableRect = new Rect(position.x,midY,50,position.height);
		Rect headerLableRect = new Rect(fieldLableRect.xMax + interval,midY,200,position.height);
		Rect dataTypeEnumRect = new Rect(headerLableRect.xMax + interval,midY,200,position.height);
		Rect isParseRect = new Rect(dataTypeEnumRect.xMax + interval,midY,200,position.width);
		EditorGUI.LabelField(fieldLableRect,"字段名：");
		EditorGUI.LabelField(headerLableRect,item.header);
		item.dataType = (DataType)EditorGUI.EnumPopup(dataTypeEnumRect,"数据类型：",item.dataType);
		item.isParse = EditorGUI.Toggle(isParseRect,"是否解析该字段",item.isParse);
        return item;
    }

	/// <summary>
	/// 预解析CSV
	/// </summary>
	private void SpeculativeParseCSV(string path)
	{
		//string path = DEFAULT_CSV_PATH + "guide";
//		string csvpath = DEFAULT_CSV_PATH + GetFilePath(path);
		CSVTable csvTable = CsvLoader.LoadCSVAsset(path);
		data.Clear();
		if(csvTable != null)
		{
			foreach (string _header in csvTable.Headers)
			{
				if(!string.IsNullOrEmpty(_header))
				{
					DataType _dataType = GetDataTypeByHeader(csvTable,_header);
					data.Add(new CsvData(){header = _header,dataType = _dataType});
				}
			}
		}
	}

	private void CreateModelClass()
	{
		if(data != null && !string.IsNullOrEmpty(curCSVFileName))
		{
			string className = string.Format(CSVCodeConfig.ClassNameFormat,curCSVFileName);
			StringBuilder fields = new StringBuilder();
			for (int i = 0; i < data.Count; i++)
			{
				if(data[i].isParse)
				{
					string dataTypeStr = IsDataTypeNeedToLower(data[i].dataType) ? data[i].dataType.ToString().ToLower() : data[i].dataType.ToString();
					string field = string.Format(CSVCodeConfig.FieldFormat,dataTypeStr,data[i].header);
					fields.AppendLine(field);
				}
			}
			dataModelStr = string.Format(CSVCodeConfig.DataClassFormat,Uppercase(className),fields.ToString());
		}
	}

	private string Uppercase(string str)
	{
		if(string.IsNullOrEmpty(str)) return string.Empty;
		return str.Substring(0, 1).ToUpper() + str.Substring(1);
	}
	
	private string Lowercase(string str)
	{
		if(string.IsNullOrEmpty(str)) return string.Empty;
		return str.Substring(0, 1).ToLower() + str.Substring(1);
	}

	private void CreateModelParseCode()
	{
		if(data != null && !string.IsNullOrEmpty(curCSVFileName))
		{
			string className = string.Format(CSVCodeConfig.ClassNameFormat,curCSVFileName);
			StringBuilder fields = new StringBuilder();
			StringBuilder codeSb = new StringBuilder();
			string fileLoadPath = GetFileLoadPath(curCSVPath);
			fileLoadPath = "\"" + fileLoadPath + "\"";
			string modelListStr = className + "List";
			modelListStr = Lowercase(modelListStr);
			string publicModelListStr = Uppercase(modelListStr);
			string parseMethodNameStr = string.Format(CSVCodeConfig.parseMethodNameFormat,publicModelListStr); 
			string parseMethodStr = string.Format(CSVCodeConfig.methodFormat,parseMethodNameStr);
			codeSb.AppendLine(string.Format(CSVCodeConfig.ModelListPrivateStatementFormat,className,modelListStr));
			codeSb.AppendLine(string.Format(CSVCodeConfig.ModelListPublicPropertyFormat,className,publicModelListStr,modelListStr,parseMethodNameStr,modelListStr));
			bool isFirstField = true;
			for (int i = 0; i < data.Count; i++)
			{
				if(data[i].isParse)
				{
					string fieldParseCode = "";
					switch (data[i].dataType)
					{
						case DataType.String:
							fieldParseCode = string.Format(CSVCodeConfig.ModelDataFieldValuationFormat_String,data[i].header);
						break;
						case DataType.Int:
							fieldParseCode = string.Format(CSVCodeConfig.ModelDataFieldValuationFormat_Int,data[i].header);
						break;
						case DataType.Double:
							fieldParseCode = string.Format(CSVCodeConfig.ModelDataFieldValuationFormat_Double,data[i].header);
						break;
						case DataType.Bool:
							fieldParseCode = string.Format(CSVCodeConfig.ModelDataFieldValuationFormat_Bool,data[i].header);
						break;
						case DataType.Color:
							fieldParseCode = string.Format(CSVCodeConfig.ModelDataFieldValuetionFormat_Color,data[i].header);
						break;
						default:
							fieldParseCode = string.Format(CSVCodeConfig.ModelDataFieldValuationFormat_String,data[i].header);
						break;
					}
					string fieldParseCodeWithHeaderEqual = "";
					if(isFirstField)
					{
						fieldParseCodeWithHeaderEqual = string.Format(CSVCodeConfig.ModelDataHeaderEqualFormat_1,"\"" + data[i].header + "\"",fieldParseCode);
					}
					else
					{
						fieldParseCodeWithHeaderEqual = string.Format(CSVCodeConfig.ModelDataHeaderEqualFormat_2,"\"" + data[i].header + "\"",fieldParseCode);
					}
					fields.AppendLine(fieldParseCodeWithHeaderEqual);
					isFirstField = false;
				}
			}
			codeSb.AppendLine(string.Format(CSVCodeConfig.ModelDataParseMethodFormat,parseMethodNameStr,modelListStr,className,fileLoadPath,className,className,fields.ToString(),modelListStr));
			parseCSVCodeStr = codeSb.ToString();
		}
	}

	private void CopyCodeText(string dataModelStr)
	{
		TextEditor p = new TextEditor();
        StringBuilder codeAllText = new StringBuilder(dataModelStr);
        p.text = codeAllText.ToString();
        p.OnFocus();
        p.Copy();
		EditorUtility.DisplayDialog("提示", "代码复制成功", "OK");
	}

	private void CreateModelClassScriptFile()
	{
		string className = string.Format(CSVCodeConfig.ClassNameFormat,curCSVFileName);
		className = Uppercase(className);
		string path = EditorPrefs.GetString("create_csv_model_script_folder", "");
        path = EditorUtility.SaveFilePanel("Create Script ", path, className + ".cs", "cs");
		if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(dataModelStr)) return;
		File.WriteAllText(path, dataModelStr, new UTF8Encoding(false));
        AssetDatabase.Refresh();
        EditorPrefs.SetString("create_csv_model_script_folder", path);
	}

	private bool IsDataTypeNeedToLower(DataType dataType)
	{
		bool isneed = false;
		switch (dataType)
		{
			case DataType.Int:
			case DataType.Double:
			case DataType.Bool:
			case DataType.String:
				isneed = true;
				break;
		}
		return isneed;
	}

	private bool IsContainSpitChar(string str,params string[] split)
	{
		if(string.IsNullOrEmpty(str)) return false;
		if(split == null || split.Length == 0) return false;
		for (int i = 0; i < split.Length; i++)
		{
			if(str.Contains(split[i]))
				return true;
		}
		return false;
	}

	private DataType GetDataTypeByHeader(CSVTable table,string header)
	{
		if(table == null) return DataType.String;
		
		foreach (CSVRecord record in table.Records)
		{
			foreach (string _header in table.Headers)
			{
				if(_header == header)
				{
					string str = record.GetField(_header);
					int i = 0;
					double d = 0;
					if(int.TryParse(str,out i))
					{
						if(i == 0 || i == 1)
							return DataType.Bool;
						return DataType.Int;
					}
					else if(double.TryParse(str,out d))
					{
						return DataType.Double;
					}
					else if(IsContainSpitChar(str,"-","|"))
					{
						//TODO:
						return DataType.String;
					}
					else if(CanParseToColor(str))
					{
						return DataType.Color;
					}
					return DataType.String;
				}
			}
		}
		return DataType.String;
	}

	private bool CheckShowCSV(string path,string searchstr)
	{
		if(string.IsNullOrEmpty(searchStr)) return true;
		if(string.IsNullOrEmpty(path)) return false;
		return GetFileNameWithSuffix(path).Contains(searchStr);
	}

	//包括后缀名
	private string GetFileNameWithSuffix(string path)
	{
		if(string.IsNullOrEmpty(path)) return string.Empty;
		return path.Substring(path.LastIndexOf("/")+1);
	}

	//获取文件加载路径，基于Resources下的
	private string GetFileLoadPath(string path)
	{
		if(string.IsNullOrEmpty(path)) return string.Empty;
		string name = path.Substring(path.LastIndexOf("Resources") + 10);
		name = name.Substring(0,name.IndexOf("."));
		return name;
	}

	private string GetFileName(string path)
	{
		if(string.IsNullOrEmpty(path)) return string.Empty;
		string name = path.Substring(path.LastIndexOf(Path.DirectorySeparatorChar)+1);
		name = name.Substring(0,name.IndexOf("."));
		return name;
	}

	private bool CanParseToColor(string str)
	{
		if(string.IsNullOrEmpty(str)) return false;
		if(str.StartsWith("#") && (str.Length == 7 || str.Length == 9)) return true;
		return false;
	}
}

public enum DataType
{
	Int = 0,
	String = 1,
	Double = 2,
	Bool = 3,
	//Vector3 = 4,
	//Vector2 = 5,
	Color = 6,
}

public class CsvData
{
	public string header;
	public DataType dataType;
	public bool isParse = true;
}