﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebBlog.Extension
{
    public static class Extension
    {
        public static string ToVnd(this double donGia)
        {
            return donGia.ToString("#,##0") +"đ"; // 5.000 a
        }
        public static string ToUrlFriendly(this string url)
        {   
            
            var result =url.ToLower().Trim();
            result = Regex.Replace(result,"áàạảãâấầậẩẫăắằặẳẵ" ,"a");
            result = Regex.Replace(result, "éèẹẻẽêếềệểễ", "e");
            result = Regex.Replace(result,"óòọõôốồộổỗơớờợởỡ" ,"o");
            result = Regex.Replace(result,"úùụủũưứừựửữ" ,"u");
            result = Regex.Replace(result, "íìịỉĩ","i");
            result = Regex.Replace(result, "ýỳỵỷỹ", "y");
            result = Regex.Replace(result, "đ", "d");
            result = Regex.Replace(result, "[^а-z0-9-]", "");
            result = Regex.Replace(result, "(-)+", "-");
            return result; 
        }
            

        }
    }

