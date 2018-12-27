using System;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using TecnoSet.Ecm.Wpf.Services.SE;
using WebService.com.softexpert.tecfy;
using WebService.Connection;

namespace WebService.SE
{
    public class InsegracaoSE
    {
        #region .: Public :.

        public documentDataReturn VerificarPropriedadesDocumento(string iddocumento)
        {
            SEClient seClient = SEConnection.GetConnection();
            return seClient.viewDocumentData(iddocumento, "", "");
        }

        public bool VerificarPermissaoDocumento(string iddocumento, string usuario)
        {
            SEClient seClient = SEConnection.GetConnection();
            var r = seClient.checkAccessPermission(iddocumento, usuario, 6);

            var inx = r.Split(':');
            if (inx.Count() > 0)
            {
                if (inx.Count() >= 2 && inx[1].Trim().ToUpper().Contains("ACESSO PERMITIDO"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public bool InserirDocumentoBinario(DocumentoAtributo documentoAtributo)
        {
            bool retorno = false;
            try
            {
                SEClient seClient = SEConnection.GetConnection();
                string prefix = WebConfigurationManager.AppSettings["Prefix_Category"];

                documentReturn documentReturnOwner = VerificaDocumentoCadastrado(documentoAtributo.Matricula, WebConfigurationManager.AppSettings["Category_Owner"]);
                if (documentReturnOwner == null)
                {
                    throw new Exception("Sistema não localizou o aluno!");
                }

                // Checks whether the document exists
                documentReturn documentReturn = VerificaDocumentoCadastrado(documentoAtributo.Matricula, documentoAtributo.Categoria);

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

                        seClient.setAttributeValue(documentReturn.IDDOCUMENT, "", WebConfigurationManager.AppSettings["Attribute_Pages"], documentoAtributo.Paginas.ToString());
                        seClient.setAttributeValue(documentReturn.IDDOCUMENT, "", WebConfigurationManager.AppSettings["Attribute_Usuario"], documentoAtributo.Usuario.ToString());

                        UploadDocumento(documentoAtributo, documentReturn.IDDOCUMENT);

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

                        try
                        {
                            string returnAssociation = seClient.newDocumentContainerAssociation(WebConfigurationManager.AppSettings["Category_Owner"], documentReturnOwner.IDDOCUMENT, "", WebConfigurationManager.AppSettings["StructID"], documentoAtributo.Categoria, documentoAtributo.Matricula, out long codeAssociation, out string detailAssociation);
                        }
                        catch
                        { }
                    }
                }

                // If you do not insert a new document
                else
                {
                    documentDataReturn documentDataReturn = VerificarPropriedadesDocumento(documentReturnOwner.IDDOCUMENT);
                    if (documentDataReturn.ATTRIBUTTES.Count() > 0)
                    {
                        var s = seClient.newDocument(documentoAtributo.Categoria, documentoAtributo.Matricula.Trim() + "-" + documentoAtributo.Categoria.Trim(), WebConfigurationManager.AppSettings["Category_Primary_Title"].ToString(), "", "", "", "", null, 0);

                        var inx = s.Split(':');
                        if (inx.Count() > 0)
                        {
                            if (inx.Count() >= 3 && inx[2].ToUpper().Contains("SUCESSO"))
                            {
                                UploadDocumento(documentoAtributo, documentoAtributo.Matricula.Trim() + "-" + documentoAtributo.Categoria.Trim());
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
                                    seClient.setAttributeValue(documentoAtributo.Matricula.Trim() + "-" + documentoAtributo.Categoria.Trim(), "", item.ATTRIBUTTENAME, valor);
                                }
                                catch (Exception)
                                {
                                    throw new Exception("Campo " + item.ATTRIBUTTENAME + " com erro");
                                }
                            }
                        }

                        seClient.setAttributeValue(documentoAtributo.Matricula.Trim() + "-" + documentoAtributo.Categoria.Trim(), "", WebConfigurationManager.AppSettings["Attribute_Pages"], documentoAtributo.Paginas.ToString());
                        seClient.setAttributeValue(documentoAtributo.Matricula.Trim() + "-" + documentoAtributo.Categoria.Trim(), "", WebConfigurationManager.AppSettings["Attribute_Usuario"], documentoAtributo.Usuario.ToString());

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

                        try
                        {
                            string returnAssociation = seClient.newDocumentContainerAssociation(WebConfigurationManager.AppSettings["Category_Owner"], documentReturnOwner.IDDOCUMENT, "", WebConfigurationManager.AppSettings["StructID"], documentoAtributo.Categoria, documentoAtributo.Matricula, out long codeAssociation, out string detailAssociation);
                        }
                        catch
                        { }
                    }
                }

                return retorno;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region .: Private :.

        private documentReturn VerificaDocumentoCadastrado(string matricula, string categoria)
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

        private bool UploadDocumento(DocumentoAtributo Indice, string iddocumento)
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

        #endregion
    }
}
