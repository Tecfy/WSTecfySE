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

        public int CurrentVersion { get; set; }

        public string DocumentIdOwner
        {
            get
            {
                return Registration.Trim();
            }
        }

        public string DocumentIdPrimary
        {
            get
            {
                return Registration.Trim() + "-" + CategoryPrimary.Trim() + "-" + (CurrentVersion + 1).ToString("000");
            }
        }

        public string FileName
        {
            get
            {
                return DocumentIdPrimary + "-" + DateTime.Now.ToString("ddMMyyyy-HHmmss") + Extension;
            }
        }

        public int Paginas
        {
            get
            {
                int pageCount;
                using (var reader = new PdfReader(FileBinary))
                {
                    pageCount = reader.NumberOfPages;
                }

                return pageCount;
            }
        }
    }
}
