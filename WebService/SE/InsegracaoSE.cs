using System;
using System.Data.SqlClient;
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
        #region .: Attributes :.

        readonly bool physicalFile = Convert.ToBoolean(WebConfigurationManager.AppSettings["Physical.File.Sesuite"]);
        readonly string physicalPath = WebConfigurationManager.AppSettings["Physical.Path.Sesuite"];
        readonly string prefix = WebConfigurationManager.AppSettings["Prefix_Category"];
        readonly string categoryOwner = WebConfigurationManager.AppSettings["Category_Owner"];
        readonly string attributePages = WebConfigurationManager.AppSettings["Attribute_Pages"];
        readonly string attributeUsuario = WebConfigurationManager.AppSettings["Attribute_Usuario"];
        readonly string attributeUnityCode = WebConfigurationManager.AppSettings["Attribute_Unity_Code"];
        readonly int newAccessPermissionUserType = int.Parse(WebConfigurationManager.AppSettings["NewAccessPermission.UserType"].ToString());
        readonly string newAccessPermissionPermission = WebConfigurationManager.AppSettings["NewAccessPermission.Permission"].ToString();
        readonly int newAccessPermissionPermissionType = int.Parse(WebConfigurationManager.AppSettings["NewAccessPermission.PermissionType"].ToString());
        readonly string newAccessPermissionFgaddLowerLevel = WebConfigurationManager.AppSettings["NewAccessPermission.FgaddLowerLevel"].ToString();
        readonly string structID = WebConfigurationManager.AppSettings["StructID"];
        readonly string categoryPrimaryTitle = WebConfigurationManager.AppSettings["Category_Primary_Title"];
        readonly string attributeRegistration = WebConfigurationManager.AppSettings["Attribute_Registration"];

        #endregion

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

                documentReturn documentReturnOwner = VerificaDocumentoCadastrado(documentoAtributo.Matricula, categoryOwner);
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

                        seClient.setAttributeValue(documentReturn.IDDOCUMENT, "", attributePages, documentoAtributo.Paginas.ToString());
                        seClient.setAttributeValue(documentReturn.IDDOCUMENT, "", attributeUsuario, documentoAtributo.Usuario.ToString());

                        if (physicalFile)
                        {
                            InsertPhysicalFile(documentoAtributo, documentReturn.IDDOCUMENT);
                        }
                        else
                        {
                            InsertEletronicFile(documentoAtributo, documentReturn.IDDOCUMENT);
                        }

                        if (documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == attributeUnityCode))
                        {
                            string unityCode = documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == attributeUnityCode).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault();

                            SEAdministration seAdministration = SEConnection.GetConnectionAdm();

                            seAdministration.newPosition(unityCode, unityCode, out string status, out string detail, out int code, out string recordid, out string recordKey);
                            seAdministration.newDepartment(unityCode, unityCode, unityCode, "", "", "1");

                            var n = seClient.newAccessPermission(documentReturn.IDDOCUMENT,
                                    unityCode + ";" + unityCode,
                                    newAccessPermissionUserType,
                                    newAccessPermissionPermission,
                                    newAccessPermissionPermissionType,
                                    newAccessPermissionFgaddLowerLevel);
                        }

                        try
                        {
                            string returnAssociation = seClient.newDocumentContainerAssociation(categoryOwner, documentReturnOwner.IDDOCUMENT, "", structID, documentoAtributo.Categoria, documentoAtributo.Matricula, out long codeAssociation, out string detailAssociation);
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
                        var s = seClient.newDocument(documentoAtributo.Categoria, documentoAtributo.Matricula.Trim() + "-" + documentoAtributo.Categoria.Trim(), categoryPrimaryTitle, "", "", "", "", null, 0);

                        var inx = s.Split(':');
                        if (inx.Count() > 0)
                        {
                            if (inx.Count() >= 3 && inx[2].ToUpper().Contains("SUCESSO"))
                            {
                                if (physicalFile)
                                {
                                    InsertPhysicalFile(documentoAtributo, documentoAtributo.Matricula.Trim() + "-" + documentoAtributo.Categoria.Trim());
                                }
                                else
                                {
                                    InsertEletronicFile(documentoAtributo, documentoAtributo.Matricula.Trim() + "-" + documentoAtributo.Categoria.Trim());
                                }
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

                        seClient.setAttributeValue(documentoAtributo.Matricula.Trim() + "-" + documentoAtributo.Categoria.Trim(), "", attributePages, documentoAtributo.Paginas.ToString());
                        seClient.setAttributeValue(documentoAtributo.Matricula.Trim() + "-" + documentoAtributo.Categoria.Trim(), "", attributeUsuario, documentoAtributo.Usuario.ToString());

                        if (documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == attributeUnityCode))
                        {
                            string unityCode = documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == attributeUnityCode).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault();

                            SEAdministration seAdministration = SEConnection.GetConnectionAdm();

                            seAdministration.newPosition(unityCode, unityCode, out string status, out string detail, out int code, out string recordid, out string recordKey);
                            seAdministration.newDepartment(unityCode, unityCode, unityCode, "", "", "1");

                            var n = seClient.newAccessPermission(documentReturn.IDDOCUMENT,
                                    unityCode + ";" + unityCode,
                                    newAccessPermissionUserType,
                                    newAccessPermissionPermission,
                                    newAccessPermissionPermissionType,
                                    newAccessPermissionFgaddLowerLevel);
                        }

                        try
                        {
                            string returnAssociation = seClient.newDocumentContainerAssociation(categoryOwner, documentReturnOwner.IDDOCUMENT, "", structID, documentoAtributo.Categoria, documentoAtributo.Matricula, out long codeAssociation, out string detailAssociation);
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
                IDATTRIBUTE = attributeRegistration,
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

        private bool InsertEletronicFile(DocumentoAtributo Indice, string iddocumento)
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertPhysicalFile(DocumentoAtributo Indice, string iddocumento)
        {
            try
            {
                #region .: Save File :.

                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(physicalPath);
                }

                if (File.Exists(Path.Combine(physicalPath, Path.GetFileName(Indice.Arquivo.FullName))))
                {
                    File.Delete(Path.Combine(physicalPath, Path.GetFileName(Indice.Arquivo.FullName)));
                }

                File.WriteAllBytes(Path.Combine(physicalPath, Path.GetFileName(Indice.Arquivo.FullName)), Indice.ArquivoBinario);

                #endregion

                #region .: Query :.

                string queryStringInsert = @"INSERT INTO ADINTERFACE (CDINTERFACE, FGIMPORT, CDISOSYSTEM, FGOPTION, NMFIELD01, NMFIELD02, NMFIELD03, NMFIELD04, NMFIELD05, NMFIELD07) VALUES((SELECT COALESCE(MAX(CDINTERFACE),0)+1 FROM ADINTERFACE), 1, 73, 97, '{0}','00','{1}','{2}','{3}','{4}')";
                string connectionString = WebConfigurationManager.ConnectionStrings["DefaultSesuite"].ConnectionString;

                #endregion

                #region .: Insert Sesuite :.

                using (SqlConnection connectionInsert = new SqlConnection(connectionString))
                {
                    var queryInsert = string.Format(queryStringInsert,
                                                   iddocumento, //Identificador do Documento
                                                   Path.GetFileName(Indice.Arquivo.FullName), //Nome do Arquivo
                                                   Indice.Usuario, //Matrícula do Usuário
                                                   physicalPath, //Caminho do Arquivo
                                                   Indice.Categoria.Trim() //Identificador da categoria
                                                   );
                    SqlCommand commandInsert = new SqlCommand(queryInsert, connectionInsert);
                    connectionInsert.Open();
                    commandInsert.ExecuteNonQuery();
                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
