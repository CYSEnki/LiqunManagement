﻿//JWT內容設計
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiqunManagement.Security
{

    public class JwtObject
    {
        //內容隨意設計，但注意，不要將太過重要的資料放入其中，到期時間一定要記得設定。
        public string Account { get; set; }
        public string Role { get; set; }
        public string Expire { get; set; }
    }
}
