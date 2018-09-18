using System;
using System.IO;

namespace WebService
{
	public class CreateLogFiles
	{
		private readonly string sLogFormat;

		private readonly string sErrorTime;

		private readonly string fileLogName = "";

		public CreateLogFiles(string sPathName)
		{
			string str = DateTime.Now.ToShortDateString().ToString();
			DateTime now = DateTime.Now;
			this.sLogFormat = string.Concat(str, " ", now.ToLongTimeString().ToString(), " ==> ");
			string str1 = DateTime.Now.Year.ToString();
			string str2 = DateTime.Now.Month.ToString();
			string str3 = DateTime.Now.Day.ToString();
			this.sErrorTime = string.Concat(str1, str2, str3);
			string str4 = DateTime.Now.ToString("yyyyMMdd0000");
			this.fileLogName = string.Concat(sPathName, "Log_", str4, ".txt");
		}

		public void ErrorLog(string sErrMsg)
		{
			try
			{
				StreamWriter streamWriter = new StreamWriter(this.fileLogName, true);
				streamWriter.WriteLine(string.Concat(this.sLogFormat, sErrMsg));
				streamWriter.Flush();
				streamWriter.Close();
			}
			catch (Exception)
            {
			}
		}
	}
}