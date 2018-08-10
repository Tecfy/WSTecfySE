using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using TecnoSet.Ecm.Wpf.Services.SE;
using WebService.com.softexpert.tecfy;

namespace WebService.SE
{
    public class InsegracaoSE : IDocumento<DocumentoAtributo>
    {
        string Username = WebConfigurationManager.AppSettings["Username"];
        string Password = WebConfigurationManager.AppSettings["Password"];

        public documentDataReturn verificarPropriedadesDocumento(string iddocumento)
        {
            var ass = retornaconexao("https://tecfy.softexpert.com/softexpert/webserviceproxy/se/ws/dc_ws.php");
            return ass.viewDocumentData(iddocumento, "", "");

        }

        public searchCategoryReturn carregarCategorias()
        {
            var ass = retornaconexao("https://tecfy.softexpert.com/softexpert/webserviceproxy/se/ws/dc_ws.php");
            var cat = ass.searchCategory();
            return cat;
        }

        public documentReturn[] verificaDocumentoNome(string nome)
        {
            var ass = retornaconexao("https://tecfy.softexpert.com/softexpert/webserviceproxy/se/ws/dc_ws.php");
            attributeData[] at = new attributeData[1];
            at[0] = new attributeData();
            //busca a matricula
            at[0].IDATTRIBUTE = "ATR01";
            at[0].VLATTRIBUTE = nome;

            List<documentReturn> retorno = new List<documentReturn>();

            searchDocumentFilter sdf = new searchDocumentFilter();
            sdf.IDCATEGORY = "00";
            var d = ass.searchDocument(sdf, "", at);
            documentReturn retornoDocumento = new documentReturn();
            if (d.RESULTS.Count() > 0)
            {
                foreach (var item in d.RESULTS)
                {
                    var propriedade = verificarPropriedadesDocumento(item.IDDOCUMENT);

                    var prop = propriedade.ATTRIBUTTES.Where(_s => _s.ATTRIBUTTEVALUE.FirstOrDefault() == "ATR01").FirstOrDefault();
                    if (prop != null)
                    {

                        if (prop.ATTRIBUTTEVALUE.Contains(nome))
                        {
                            retorno.Add(item);
                        }
                    }

                }





                return retorno.ToArray();
            }
            else
            {
                return null;
            }
        }
        public documentReturn verificaDocumentoCadastrado(string matricula)
        {
            var ass = retornaconexao("https://tecfy.softexpert.com/softexpert/webserviceproxy/se/ws/dc_ws.php");
            attributeData[] at = new attributeData[1];
            at[0] = new attributeData();
            ////busca a matricula
            at[0].IDATTRIBUTE = "ATR02";
            at[0].VLATTRIBUTE = matricula;



            searchDocumentFilter sdf = new searchDocumentFilter();
            sdf.IDCATEGORY = "00";
            var d = ass.searchDocument(sdf, "", at);
            documentReturn retorno = new documentReturn();
            if (d.RESULTS.Count() > 0)
            {
                return d.RESULTS[0];
            }
            else
            {
                return null;
            }
        }
        public documentReturn verificaDocumentoCadastrado(string matricula, string categoria)
        {
            var ass = retornaconexao("https://tecfy.softexpert.com/softexpert/webserviceproxy/se/ws/dc_ws.php");
            attributeData[] at = new attributeData[1];
            at[0] = new attributeData();
            ////busca a matricula
            at[0].IDATTRIBUTE = "ATR02";
            at[0].VLATTRIBUTE = matricula;

            searchDocumentFilter sdf = new searchDocumentFilter();
            sdf.IDCATEGORY = categoria;
            var d = ass.searchDocument(sdf, "", at);
            documentReturn retorno = new documentReturn();
            if (d.RESULTS.Count() > 0)
            {
                return d.RESULTS[0];
            }
            else
            {
                return null;
            }
        }


        public SEClient retornaconexao(String url)
        {
            SEClient ass = new SEClient(url);
            ass.SetAuthentication(Username, Password);

            return ass;

        }


        //public bool inserirDocumentoBinario(DocumentoAtributo Indice)
        //{
        //    bool retorno = false;
        //    try
        //    {
        //        var arquivo = Indice.NomeArquivo;
        //        var indices = arquivo.Split('_');

        //        if (indices.Count() >= 2)
        //        {
        //            SEClient ass = new SEClient("https://tecfy.softexpert.com/softexpert/webserviceproxy/se/ws/dc_ws.php");
        //            ass.SetAuthentication(Username, Password);


        //            var documentoCategoria00 = verificaDocumentoCadastrado(Indice.Matricula, "00");
        //            if (documentoCategoria00 == null)
        //            {
        //                throw new Exception("Sistema não possui Aluno com categoria 00");
        //            }
        //            ///verifica se o documento existe 
        //            var documento = verificaDocumentoCadastrado(Indice.Matricula, Indice.Categoria);
        //            ////já existe o docuemnto na categoria especificada. Portanto faz upload do documento e das propriedades
        //            if (documento != null)
        //            {
        //                var propriedade = verificarPropriedadesDocumento(documentoCategoria00.IDDOCUMENT);
        //                if (propriedade.ATTRIBUTTES.Count() > 0)
        //                {
        //                    String atributos = "";
        //                    foreach (var item in propriedade.ATTRIBUTTES)
        //                    {
        //                        string valor = "";
        //                        if (item.ATTRIBUTTEVALUE.Count() > 0)
        //                        {
        //                            valor = item.ATTRIBUTTEVALUE[0];
        //                        }
        //                        try
        //                        {
        //                            var s = ass.setAttributeValue(documento.IDDOCUMENT, "", item.ATTRIBUTTENAME, valor);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            throw new Exception("Campo " + item.ATTRIBUTTENAME + "Com erro");
        //                        }


        //                    }
        //                    var ds = uploadDocumentoBinario(Indice, documento.IDDOCUMENT);
        //                }
        //            }

        //            ////faz a inserção de um novo documento
        //            else
        //            {
        //                var propriedade = verificarPropriedadesDocumento(documentoCategoria00.IDDOCUMENT);
        //                if (propriedade.ATTRIBUTTES.Count() > 0)
        //                {
        //                    String atributos = "";
        //                    foreach (var item in propriedade.ATTRIBUTTES)
        //                    {
        //                        string valor = "";
        //                        if (item.ATTRIBUTTEVALUE.Count() > 0)
        //                        {
        //                            valor = item.ATTRIBUTTEVALUE[0];
        //                        }

        //                        atributos += item.ATTRIBUTTENAME + "=" + valor + ";";
        //                    }
        //                    var s = ass.newDocument(categoria, "", "Documento:aluno" + matricula, "", "", atributos, "", null, 0);

        //                    var inx = s.Split(':');
        //                    if (inx.Count() > 0)
        //                    {
        //                        if (inx[1].ToUpper().Contains("SUCESSO"))
        //                        {

        //                            uploadDocumento(Indice, inx[0]);
        //                        }
        //                        else
        //                        {
        //                            throw new Exception(s);
        //                        }
        //                    }



        //                }


        //            }
        //        }


        //        return retorno;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //}

        public bool inserirDocumentoBinario(DocumentoAtributo Indice)
        {
            bool retorno = false;
            try
            {

                String categoria = "", matricula = "";

                SEClient ass = new SEClient("https://tecfy.softexpert.com/softexpert/webserviceproxy/se/ws/dc_ws.php");
                ass.SetAuthentication(Username, Password);

                matricula = Indice.Matricula;
                categoria = Indice.Categoria;
                var documentoCategoria00 = verificaDocumentoCadastrado(matricula, "00");
                if (documentoCategoria00 == null)
                {
                    throw new Exception("Sistema não possui Aluno com categoria 00");
                }
                ///verifica se o documento existe 
                var documento = verificaDocumentoCadastrado(matricula, categoria);
                ////já existe o docuemnto na categoria especificada. Portanto faz upload do documento e das propriedades
                if (documento != null)
                {
                    var propriedade = verificarPropriedadesDocumento(documentoCategoria00.IDDOCUMENT);
                    if (propriedade.ATTRIBUTTES.Count() > 0)
                    {
                        String atributos = "";
                        foreach (var item in propriedade.ATTRIBUTTES)
                        {
                            string valor = "";
                            if (item.ATTRIBUTTEVALUE.Count() > 0)
                            {
                                valor = item.ATTRIBUTTEVALUE[0];
                            }
                            try
                            {
                                var s = ass.setAttributeValue(documento.IDDOCUMENT, "", item.ATTRIBUTTENAME, valor);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Campo " + item.ATTRIBUTTENAME + "Com erro");
                            }


                        }
                        var ds = uploadDocumentoBinario(Indice, documento.IDDOCUMENT);
                    }
                }

                ////faz a inserção de um novo documento
                else
                {
                    var propriedade = verificarPropriedadesDocumento(documentoCategoria00.IDDOCUMENT);
                    if (propriedade.ATTRIBUTTES.Count() > 0)
                    {
                        String atributos = "";
                        foreach (var item in propriedade.ATTRIBUTTES)
                        {
                            string valor = "";
                            if (item.ATTRIBUTTEVALUE.Count() > 0)
                            {
                                valor = item.ATTRIBUTTEVALUE[0];
                            }

                            atributos += item.ATTRIBUTTENAME + "=" + valor + ";";
                        }
                        if (Indice.Identificacao != null && Indice.Identificacao != "")
                        {
                            atributos += "atr08=" + Indice.Identificacao + ";";
                        }
                        if (Indice.Data != null && Indice.Data != "")
                        {
                            atributos += "atr09=" + Indice.Identificacao + ";";
                        }
                        var cat = this.carregarCategorias().RESULTARRAY.Where(_s => _s.IDCATEGORY == categoria).FirstOrDefault().NMCATEGORY;

                        var s = ass.newDocument(categoria, "", cat, "", "", atributos, "", null, 0);

                        var inx = s.Split(':');
                        if (inx.Count() > 0)
                        {
                            if (inx[1].ToUpper().Contains("SUCESSO"))
                            {

                                uploadDocumentoBinario(Indice, inx[0]);
                            }
                            else
                            {
                                throw new Exception(s);
                            }
                        }



                    }


                }



                return retorno;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public bool inserirDocumento(DocumentoAtributo Indice)
        {
            bool retorno = false;
            try
            {
                var arquivo = Path.GetFileNameWithoutExtension(Indice.Arquivo.FullName);
                var indices = arquivo.Split('_');
                String categoria = "", matricula = "";
                if (indices.Count() >= 2)
                {
                    SEClient ass = new SEClient("https://tecfy.softexpert.com/softexpert/webserviceproxy/se/ws/dc_ws.php");
                    ass.SetAuthentication(Username, Password);

                    matricula = indices[0];
                    categoria = indices[1];
                    var documentoCategoria00 = verificaDocumentoCadastrado(matricula, "00");
                    if (documentoCategoria00 == null)
                    {
                        throw new Exception("Sistema não possui Aluno com categoria 00");
                    }
                    ///verifica se o documento existe 
                    var documento = verificaDocumentoCadastrado(matricula, categoria);
                    ////já existe o docuemnto na categoria especificada. Portanto faz upload do documento e das propriedades
                    if (documento != null)
                    {
                        var propriedade = verificarPropriedadesDocumento(documentoCategoria00.IDDOCUMENT);
                        if (propriedade.ATTRIBUTTES.Count() > 0)
                        {
                            String atributos = "";
                            foreach (var item in propriedade.ATTRIBUTTES)
                            {
                                string valor = "";
                                if (item.ATTRIBUTTEVALUE.Count() > 0)
                                {
                                    valor = item.ATTRIBUTTEVALUE[0];
                                }
                                try
                                {
                                    var s = ass.setAttributeValue(documento.IDDOCUMENT, "", item.ATTRIBUTTENAME, valor);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Campo " + item.ATTRIBUTTENAME + "Com erro");
                                }


                            }
                            var ds = uploadDocumento(Indice, documento.IDDOCUMENT);
                        }
                    }

                    ////faz a inserção de um novo documento
                    else
                    {
                        var propriedade = verificarPropriedadesDocumento(documentoCategoria00.IDDOCUMENT);
                        if (propriedade.ATTRIBUTTES.Count() > 0)
                        {
                            String atributos = "";
                            foreach (var item in propriedade.ATTRIBUTTES)
                            {
                                string valor = "";
                                if (item.ATTRIBUTTEVALUE.Count() > 0)
                                {
                                    valor = item.ATTRIBUTTEVALUE[0];
                                }

                                atributos += item.ATTRIBUTTENAME + "=" + valor + ";";
                            }
                            var s = ass.newDocument(categoria, "", "Documento:aluno" + matricula, "", "", atributos, "", null, 0);

                            var inx = s.Split(':');
                            if (inx.Count() > 0)
                            {
                                if (inx[1].ToUpper().Contains("SUCESSO"))
                                {

                                    uploadDocumento(Indice, inx[0]);
                                }
                                else
                                {
                                    throw new Exception(s);
                                }
                            }



                        }


                    }
                }


                return retorno;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public bool uploadDocumentoBinario(DocumentoAtributo Indice, string iddocumento)
        {
            try
            {
                var arquivo = Path.GetFileName(Indice.Arquivo.FullName);

                //////id Do novo documento
                SEClient ass = new SEClient("https://tecfy.softexpert.com/softexpert/webserviceproxy/se/ws/dc_ws.php");
                ass.SetAuthentication(Username, Password);

                eletronicFile[] wl = new eletronicFile[2];
                wl[0] = new eletronicFile();

                wl[0].BINFILE = Indice.ArquivoBinario;
                wl[0].ERROR = "";
                wl[0].NMFILE = Path.GetFileName(Indice.Arquivo.FullName);
                ass.uploadEletronicFileAsync(iddocumento, "", "", wl);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public bool uploadDocumento(DocumentoAtributo Indice, string iddocumento)
        {
            var arquivo = Path.GetFileName(Indice.Arquivo.FullName);
            var indices = arquivo.Split('_');
            //////id Do novo documento
            SEClient ass = new SEClient("https://tecfy.softexpert.com/softexpert/webserviceproxy/se/ws/dc_ws.php");
            ass.SetAuthentication(Username, Password);

            eletronicFile[] wl = new eletronicFile[2];
            wl[0] = new eletronicFile();

            wl[0].BINFILE = File.ReadAllBytes(Indice.Arquivo.FullName);
            wl[0].ERROR = "";
            wl[0].NMFILE = Path.GetFileName(Indice.Arquivo.FullName);
            var d = ass.uploadEletronicFile(iddocumento, "", "", wl);

            return true;


        }
    }
}
