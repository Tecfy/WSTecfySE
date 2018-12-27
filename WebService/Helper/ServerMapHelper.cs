﻿using System.Web;

namespace WebService.Helper
{
    public static class ServerMapHelper
    {
        public static string GetServerMap(string appPath)
        {
            string location = HttpContext.Current.Server.MapPath("~");

            location = location.Substring(0, location.Length - 1);
            location = location.Substring(0, location.LastIndexOf("\\"));
            location = location + appPath;

            return location;
        }
    }
}