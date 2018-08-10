using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebService.SE
{
    public class DocumentoAtributo
    {
        private string identificacao;
        private string data;
        private string matricula;
        private string categoria;
        private FileInfo arquivo;
        private string nomeArquivo;
        public FileInfo Arquivo { get => arquivo; set => arquivo = value; }
        public byte[] ArquivoBinario { get => arquivoBinario; set => arquivoBinario = value; }
        public string NomeArquivo { get => nomeArquivo; set => nomeArquivo = value; }
        public string Matricula { get => matricula; set => matricula = value; }
        public string Categoria { get => categoria; set => categoria = value; }
        public string Data { get => data; set => data = value; }
        public string Identificacao { get => identificacao; set => identificacao = value; }

        private byte[] arquivoBinario;


    }
}
