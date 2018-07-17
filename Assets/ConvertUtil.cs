using UnityEngine;
using System.Collections;
using System;

public class ConvertUtil
{
    public static long Str2Long(string str)
    {
        if(string.IsNullOrEmpty(str)) 
        {
            return 0;
        }
        return Convert.ToInt64(Convert.ToDecimal(str));
    }

    public static int Str2Int(string str)
    {
        return Convert.ToInt32(Convert.ToDecimal(str));
    }

    public static double Str2Double(string str)
    {
        return Convert.ToDouble(Convert.ToDecimal(str));
    }

}
