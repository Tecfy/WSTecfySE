using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using TecnoSet.Ecm.Wpf.Services.SE;
using WebService.com.softexpert.tecfy;
using WebService.Connection;

namespace WebService.SE
{
    public class InsegracaoSE : IDocumento<DocumentoAtributo>
    {
        public documentDataReturn VerificarPropriedadesDocumento(string iddocumento)
        {
            SEClient seClient = SEConnection.GetConnection();
            return seClient.viewDocumentData(iddocumento, "", "");
        }

        public searchCategoryReturn CarregarCategorias()
        {
            SEClient seClient = SEConnection.GetConnection();
            searchCategoryReturn searchCategoryReturn = seClient.searchCategory();
            return searchCategoryReturn;
        }

        public documentReturn[] VerificaDocumentoNome(string name)
        {
            string attributeName = WebConfigurationManager.AppSettings["Attribute_Name"];

            SEClient seClient = SEConnection.GetConnection();
            attributeData[] attributes = new attributeData[1];
            attributes[0] = new attributeData
            {
                IDATTRIBUTE = attributeName,
                VLATTRIBUTE = name
            };

            List<documentReturn> retorno = new List<documentReturn>();

            searchDocumentFilter searchDocumentFilter = new searchDocumentFilter
            {
                IDCATEGORY = WebConfigurationManager.AppSettings["Category_Owner"]
            };

            searchDocumentReturn searchDocumentReturn = seClient.searchDocument(searchDocumentFilter, "", attributes);

            if (searchDocumentReturn.RESULTS.Count() > 0)
            {
                foreach (var item in searchDocumentReturn.RESULTS)
                {
                    var propriedade = VerificarPropriedadesDocumento(item.IDDOCUMENT);

                    var prop = propriedade.ATTRIBUTTES.Where(_s => _s.ATTRIBUTTEVALUE.FirstOrDefault() == attributeName).FirstOrDefault();
                    if (prop != null)
                    {

                        if (prop.ATTRIBUTTEVALUE.Contains(name))
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

        public documentReturn VerificaDocumentoCadastrado(string matricula)
        {
            SEClient seClient = SEConnection.GetConnection();
            attributeData[] attributes = new attributeData[1];
            attributes[0] = new attributeData
            {
                IDATTRIBUTE = WebConfigurationManager.AppSettings["Attribute_Registration"],
                VLATTRIBUTE = matricula
            };

            searchDocumentFilter searchDocumentFilter = new searchDocumentFilter
            {
                IDCATEGORY = WebConfigurationManager.AppSettings["Category_Owner"]
            };

            searchDocumentReturn searchDocumentReturn = seClient.searchDocument(searchDocumentFilter, "", attributes);

            if (searchDocumentReturn.RESULTS.Count() > 0)
            {
                return searchDocumentReturn.RESULTS[0];
            }
            else
            {
                return null;
            }
        }

        public documentReturn VerificaDocumentoCadastrado(string matricula, string categoria)
        {
            SEClient sEClient = SEConnection.GetConnection();
            attributeData[] attributes = new attributeData[1];
            attributes[0] = new attributeData
            {
                IDATTRIBUTE = WebConfigurationManager.AppSettings["Attribute_Registration"],
                VLATTRIBUTE = matricula
            };

            searchDocumentFilter searchDocumentFilter = new searchDocumentFilter
            {
                IDCATEGORY = categoria
            };

            searchDocumentReturn searchDocumentReturn = sEClient.searchDocument(searchDocumentFilter, "", attributes);

            if (searchDocumentReturn.RESULTS.Count() > 0)
            {
                return searchDocumentReturn.RESULTS[0];
            }
            else
            {
                return null;
            }
        }

        public bool InserirDocumentoBinario(DocumentoAtributo Indice)
        {
            bool retorno = false;
            try
            {
                SEClient seClient = SEConnection.GetConnection();
                string prefix = WebConfigurationManager.AppSettings["Prefix_Category"];

                documentReturn documentReturnOwner = VerificaDocumentoCadastrado(Indice.Matricula, WebConfigurationManager.AppSettings["Category_Owner"]);
                if (documentReturnOwner == null)
                {
                    throw new Exception("Sistema não localizou o aluno!");
                }

                // Checks whether the document exists
                documentReturn documentReturn = VerificaDocumentoCadastrado(Indice.Matricula, Indice.Categoria);

                // If the document exists in the specified category it uploads the document and replica the properties of the owner document
                if (documentReturn != null)
                {
                    documentDataReturn documentDataReturn = VerificarPropriedadesDocumento(documentReturnOwner.IDDOCUMENT);
                    if (documentDataReturn.ATTRIBUTTES.Count() > 0)
                    {
                        foreach (var item in documentDataReturn.ATTRIBUTTES)
                        {
                            if (item.ATTRIBUTTENAME.Contains(prefix))
                            {
                                string valor = "";
                                if (item.ATTRIBUTTEVALUE.Count() > 0)
                                {
                                    valor = item.ATTRIBUTTEVALUE[0];
                                }

                                try
                                {
                                    seClient.setAttributeValue(documentReturn.IDDOCUMENT, "", item.ATTRIBUTTENAME, valor);
                                }
                                catch (Exception)
                                {
                                    throw new Exception("Campo " + item.ATTRIBUTTENAME + " com erro");
                                }
                            }
                        }

                        seClient.setAttributeValue(documentReturn.IDDOCUMENT, "", WebConfigurationManager.AppSettings["Attribute_Pages"], Indice.Paginas.ToString());
                        seClient.setAttributeValue(documentReturn.IDDOCUMENT, "", WebConfigurationManager.AppSettings["Attribute_Usuario"], Indice.Usuario.ToString());

                        UploadDocumentoBinario(Indice, documentReturn.IDDOCUMENT);

                        if (documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity_Code"]))
                        {                            
                            string unityCode = documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity_Code"]).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault();

                            SEAdministration seAdministration = SEConnection.GetConnectionAdm();

                            seAdministration.newPosition(unityCode, unityCode, out string status, out string detail, out int code, out string recordid, out string recordKey);
                            seAdministration.newDepartment(unityCode, unityCode, unityCode, "", "", "1");

                            var n = seClient.newAccessPermission(documentReturn.IDDOCUMENT,
                                    unityCode + ";" + unityCode,
                                    int.Parse(WebConfigurationManager.AppSettings["NewAccessPermission.UserType"].ToString()),
                                    WebConfigurationManager.AppSettings["NewAccessPermission.Permission"].ToString(),
                                    int.Parse(WebConfigurationManager.AppSettings["NewAccessPermission.PermissionType"].ToString()),
                                    WebConfigurationManager.AppSettings["NewAccessPermission.FgaddLowerLevel"].ToString());
                        }
                    }
                }

                // If you do not insert a new document
                else
                {
                    documentDataReturn documentDataReturn = VerificarPropriedadesDocumento(documentReturnOwner.IDDOCUMENT);
                    if (documentDataReturn.ATTRIBUTTES.Count() > 0)
                    {          
                        var cat = this.CarregarCategorias().RESULTARRAY.Where(_s => _s.IDCATEGORY == Indice.Categoria).FirstOrDefault().NMCATEGORY;

                        var s = seClient.newDocument(Indice.Categoria, Indice.Matricula.Trim() + "-" + Indice.Categoria.Trim(), cat, "", "", "", "", null, 0);

                        var inx = s.Split(':');
                        if (inx.Count() > 0)
                        {
                            if (inx.Count() >= 3 && inx[2].ToUpper().Contains("SUCESSO"))
                            {
                                UploadDocumentoBinario(Indice, Indice.Matricula.Trim() + "-" + Indice.Categoria.Trim());
                            }
                            else
                            {
                                throw new Exception(s);
                            }
                        }

                        foreach (var item in documentDataReturn.ATTRIBUTTES)
                        {
                            if (item.ATTRIBUTTENAME.Contains(prefix))
                            {
                                string valor = "";
                                if (item.ATTRIBUTTEVALUE.Count() > 0)
                                {
                                    valor = item.ATTRIBUTTEVALUE[0];
                                }

                                try
                                {
                                    seClient.setAttributeValue(Indice.Matricula.Trim() + "-" + Indice.Categoria.Trim(), "", item.ATTRIBUTTENAME, valor);
                                }
                                catch (Exception)
                                {
                                    throw new Exception("Campo " + item.ATTRIBUTTENAME + " com erro");
                                }
                            }
                        }

                        seClient.setAttributeValue(Indice.Matricula.Trim() + "-" + Indice.Categoria.Trim(), "", WebConfigurationManager.AppSettings["Attribute_Pages"], Indice.Paginas.ToString());
                        seClient.setAttributeValue(Indice.Matricula.Trim() + "-" + Indice.Categoria.Trim(), "", WebConfigurationManager.AppSettings["Attribute_Usuario"], Indice.Usuario.ToString());

                        if (documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity_Code"]))
                        {
                            string unityCode = documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity_Code"]).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault();

                            SEAdministration seAdministration = SEConnection.GetConnectionAdm();

                            seAdministration.newPosition(unityCode, unityCode, out string status, out string detail, out int code, out string recordid, out string recordKey);
                            seAdministration.newDepartment(unityCode, unityCode, unityCode, "", "", "1");

                            var n = seClient.newAccessPermission(documentReturn.IDDOCUMENT,
                                    unityCode + ";" + unityCode,
                                    int.Parse(WebConfigurationManager.AppSettings["NewAccessPermission.UserType"].ToString()),
                                    WebConfigurationManager.AppSettings["NewAccessPermission.Permission"].ToString(),
                                    int.Parse(WebConfigurationManager.AppSettings["NewAccessPermission.PermissionType"].ToString()),
                                    WebConfigurationManager.AppSettings["NewAccessPermission.FgaddLowerLevel"].ToString());
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

        public bool InserirDocumento(DocumentoAtributo Indice)
        {
            bool retorno = false;
            try
            {
                var arquivo = Path.GetFileNameWithoutExtension(Indice.Arquivo.FullName);
                var indices = arquivo.Split('_');
                String categoria = "", matricula = "";
                if (indices.Count() >= 2)
                {
                    SEClient seClient = SEConnection.GetConnection();

                    matricula = indices[0];
                    categoria = indices[1];

                    documentReturn documentReturnOwner = VerificaDocumentoCadastrado(matricula, WebConfigurationManager.AppSettings["Category_Owner"]);
                    if (documentReturnOwner == null)
                    {
                        throw new Exception("Sistema não localizou o aluno Aluno!");
                    }

                    // Checks whether the document exists
                    documentReturn documentReturn = VerificaDocumentoCadastrado(matricula, categoria);

                    // If the document exists in the specified category it uploads the document and replica the properties of the owner document
                    if (documentReturn != null)
                    {
                        documentDataReturn documentDataReturn = VerificarPropriedadesDocumento(documentReturnOwner.IDDOCUMENT);
                        if (documentDataReturn.ATTRIBUTTES.Count() > 0)
                        {
                            foreach (var item in documentDataReturn.ATTRIBUTTES)
                            {
                                string valor = "";
                                if (item.ATTRIBUTTEVALUE.Count() > 0)
                                {
                                    valor = item.ATTRIBUTTEVALUE[0];
                                }

                                try
                                {
                                    seClient.setAttributeValue(documentReturn.IDDOCUMENT, "", item.ATTRIBUTTENAME, valor);
                                }
                                catch (Exception)
                                {
                                    throw new Exception("Campo " + item.ATTRIBUTTENAME + "Com erro");
                                }
                            }

                            seClient.setAttributeValue(documentReturn.IDDOCUMENT, "", WebConfigurationManager.AppSettings["Attribute_Pages"], Indice.Paginas.ToString());

                            UploadDocumento(Indice, documentReturn.IDDOCUMENT);
                        }
                    }

                    // If you do not insert a new document
                    else
                    {
                        documentDataReturn documentDataReturn = VerificarPropriedadesDocumento(documentReturnOwner.IDDOCUMENT);
                        if (documentDataReturn.ATTRIBUTTES.Count() > 0)
                        {
                            String atributos = "";
                            foreach (var item in documentDataReturn.ATTRIBUTTES)
                            {
                                string valor = "";
                                if (item.ATTRIBUTTEVALUE.Count() > 0)
                                {
                                    valor = item.ATTRIBUTTEVALUE[0];
                                }

                                atributos += item.ATTRIBUTTENAME + "=" + valor + ";";
                            }

                            atributos += WebConfigurationManager.AppSettings["Attribute_Pages"] + "=" + Indice.Paginas + ";";

                            var s = seClient.newDocument(categoria, "", "Documento:aluno" + matricula, "", "", atributos, "", null, 0);

                            var inx = s.Split(':');
                            if (inx.Count() > 0)
                            {
                                if (inx[1].ToUpper().Contains("SUCESSO"))
                                {

                                    UploadDocumento(Indice, inx[0]);
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

        public bool UploadDocumentoBinario(DocumentoAtributo Indice, string iddocumento)
        {
            try
            {
                var arquivo = Path.GetFileName(Indice.Arquivo.FullName);

                SEClient seClient = SEConnection.GetConnection();

                eletronicFile[] eletronicFiles = new eletronicFile[2];

                eletronicFiles[0] = new eletronicFile
                {
                    BINFILE = Indice.ArquivoBinario,
                    ERROR = "",
                    NMFILE = Path.GetFileName(Indice.Arquivo.FullName)
                };

                var var = seClient.uploadEletronicFile(iddocumento, "", "", eletronicFiles);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UploadDocumento(DocumentoAtributo Indice, string iddocumento)
        {
            try
            {
                var arquivo = Path.GetFileName(Indice.Arquivo.FullName);
                var indices = arquivo.Split('_');

                SEClient seClient = SEConnection.GetConnection();

                eletronicFile[] eletronicFiles = new eletronicFile[2];

                eletronicFiles[0] = new eletronicFile
                {
                    BINFILE = File.ReadAllBytes(Indice.Arquivo.FullName),
                    ERROR = "",
                    NMFILE = Path.GetFileName(Indice.Arquivo.FullName)
                };

                seClient.uploadEletronicFile(iddocumento, "", "", eletronicFiles);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
