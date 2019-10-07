using iTextSharp.text.pdf;
using System;

namespace WebService.SE
{
    public class DocumentoAtributo
    {
        public byte[] FileBinary { get; set; }

        public string Registration { get; set; }

        public string CategoryPrimary { get; set; }

        public string CategoryOwner { get; set; }

        public string User { get; set; }

        public string Extension { get; set; }

        public DateTime Now { get; set; }

        public int Pages { get; set; }

        public string UnityCode { get; set; }

        public string UnityName { get; set; }

        public string DocumentIdPrimary { get; set; }

        public string FileName
        {
            get
            {
                return DocumentIdPrimary + "-" + Now.ToString("ddMMyyyy-HHmmss") + Extension;
            }
        }
    }
}
