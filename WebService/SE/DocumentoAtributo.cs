using iTextSharp.text.pdf;
using System.IO;

namespace WebService.SE
{
    public class DocumentoAtributo
    {
        public FileInfo Arquivo { get; set; }

        public byte[] ArquivoBinario { get; set; }

        public string NomeArquivo { get; set; }

        public string Matricula { get; set; }

        public string Categoria { get; set; }

        public string Data { get; set; }

        public string Identificacao { get; set; }

        public string Usuario { get; set; }

        public int Paginas
        {
            get
            {
                int pageCount;
                using (var reader = new PdfReader(ArquivoBinario))
                {
                    pageCount = reader.NumberOfPages;
                }

                return pageCount;
            }
        }
    }
}
