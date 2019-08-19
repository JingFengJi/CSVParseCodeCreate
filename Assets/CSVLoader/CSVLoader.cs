using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*===============================================================*/
/**
* CSVを読み込むクラス
* 2014年12月23日 Buravo
*/
public class CSVLoader 
{
	/*===============================================================*/
	/**
	* @brief コンストラクタ
	*/
	public CSVLoader ()
	{
		this.Initialize();
	}
	/*===============================================================*/

	/*===============================================================*/
	/**
    * @brief 初期化
    */
	public void Initialize ()
	{
	}
	/*===============================================================*/

	/*===============================================================*/
	/**
    * @brief 実行処理
    */
	public void Execution ()
	{
	}

	public CSVTable LoadCSV(TextAsset csvAsset)
	{
		return LoadCSVFromContent(csvAsset.text);
	}
	/*===============================================================*/

	/*===============================================================*/
	/**
    * @brief CSVを読み込んで、レコードを所持するデータテーブルを渡す関数
    * @param string 読み込むCSVのファイルパス
    * @return CSVTable CSVのデータテーブルクラス
    */
	public CSVTable LoadCSV (string t_csv_path)
	{
		// テキストアセットとしてCSVをロード.
		TextAsset csvTextAsset = Resources.Load(t_csv_path) as TextAsset;
//		//Debug.Log (string.Format("<color=cyan>[LoadCSV] : {0} </color>", t_csv_path));
		// OS環境ごとに適切な改行コードをCR(=キャリッジリターン)に置換.
		return LoadCSVFromContent(csvTextAsset.text);
	}

	#if UNITY_EDITOR
	public CSVTable LoadCSVAsset(string csvAssetPath)
	{
		TextAsset csvTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(csvAssetPath);
		return LoadCSVFromContent(csvTextAsset.text);
	}
	#endif
	
	
	private CSVTable LoadCSVFromContent(string csvContent)
	{
		// データテーブルクラスの生成.
		CSVTable csvTable = new CSVTable();
		string csvText = csvContent.Replace(Environment.NewLine, "\r");
		// テキストデータの前後からCRを取り除く.
		csvText = csvText.Trim('\r');
		csvText = csvText.Replace ("\r\r","\r");

		// CRを区切り文字として分割して配列に変換.
		string[] csv = csvText.Split('\r');
		// 複数の行を元にリストの生成.
		List<string> rows = new List<string>(csv);
		// 項目名の取得.
		string[] headers = rows[0].Split(',');
		// 項目の格納.
		foreach (string header in headers)
		{
			csvTable.AddHeaders(header);
		}
		// 項目名の削除.
		rows.RemoveAt(0);
		rows.RemoveAt (0);
		// 1件分のデータであるレコードを生成して追加.
		foreach (string row in rows)
		{
			// 各項目の値へと分割.
			string[] fields = row.Split(',');
			// レコードを追加.
			csvTable.AddRecord(CreateRecord(headers, fields));
		}
		return csvTable;
	}
	/*===============================================================*/

	/*===============================================================*/
	/**
    * @brief 項目名をキーに入力項目を格納するレコードを生成する関数
    * @param string[] 項目名
    * @param string[] 入力項目
    * @return CSVRecord 項目名をキーに入力項目を格納するレコード
    */
	private CSVRecord CreateRecord (string[] t_headers, string[] t_fields)
	{
		// レコードを生成.
		CSVRecord record = new CSVRecord();
		// 項目名をキーに入力項目をレコードへ格納.
		for (int i = 0; i < t_headers.Length; ++i)
		{
			//if(t_headers[1] == "stepid")
			//	UnityEngine.Debug.LogError(t_fields[i]);
			record.AddField(t_headers[i], t_fields[i]);
		}
		return record;
	}
	/*===============================================================*/
}
/*===============================================================*/


