using System.IO;
using System.Text.RegularExpressions;

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

        public int Paginas
        {
            get
            {
                int pageCount;
                MemoryStream stream = new MemoryStream(ArquivoBinario);
                using (var r = new StreamReader(stream))
                {
                    string pdfText = r.ReadToEnd();
                    System.Text.RegularExpressions.Regex regx = new Regex(@"/Type\s*/Page[^s]");
                    System.Text.RegularExpressions.MatchCollection matches = regx.Matches(pdfText);
                    pageCount = matches.Count;
                }

                return pageCount;
            }
        }
    }
}
