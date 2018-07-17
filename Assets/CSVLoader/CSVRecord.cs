using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/*===============================================================*/
/**
* CSVの1件分のデータであるレコードクラス
* 2014年12月23日 Buravo
*/
public class CSVRecord 
{

	/*===============================================================*/
	/**
	* @brief 1件分のデータ
	*/
	private Dictionary<string, string> m_record = new Dictionary<string, string>();
	/*===============================================================*/


	/*===============================================================*/
	/**
	* @brief コンストラクタ
	*/
	public CSVRecord ()
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
	/*===============================================================*/

	/*===============================================================*/
	/**
    * @brief 1入力項目分のデータを追加.
    * @param string 項目名
    * @param string 入力項目
    */
	public void AddField (string t_header, string t_field)
	{
		// キーの存在チェック.
		if (!m_record.ContainsKey(t_header))
		{
			m_record.Add(t_header, t_field);
		}
	}
	/*===============================================================*/

	/*===============================================================*/
	/**
    * @brief 項目名をキーに入力項目を取得する.
    * @param string キーである項目名
    * @return string 入力項目
    */
	public string GetField (string t_header)
	{
		// キーの存在チェック.
		if (m_record.ContainsKey(t_header))
		{
			return m_record[t_header];
		}
		return null;
	}

	public override string ToString(){
		string result = "";
		foreach(KeyValuePair<string, string> entry in m_record ){
			result += string.Format (" [{0}  {1} ]",entry.Key,entry.Value);
		}
		return result;
	}
	/*===============================================================*/
}
/*===============================================================*/




