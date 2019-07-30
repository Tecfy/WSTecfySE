using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using TecnoSet.Ecm.Wpf.Services.SE;
using WebService.com.softexpert.tecfy;
using WebService.Connection;
using WebService.Models.Common.Enum;
using WebService.Models.Unit;

namespace WebService.SE
{
    public class InsegracaoSE
    {
        #region .: Attributes :.

        private readonly bool physicalFile = Convert.ToBoolean(WebConfigurationManager.AppSettings["Sesuite.Folder.Physical"]);
        private readonly string physicalPath = WebConfigurationManager.AppSettings["Sesuite.Physical.Path"];
        private readonly string physicalPathSE = WebConfigurationManager.AppSettings["Sesuite.Physical.Path.SE"];
        private readonly string prefix = WebConfigurationManager.AppSettings["Prefix_Category"];
        private readonly string attributePages = WebConfigurationManager.AppSettings["Attribute_Pages"];
        private readonly string attributeUsuario = WebConfigurationManager.AppSettings["Attribute_Usuario"];
        private readonly string attributeUnityCode = WebConfigurationManager.AppSettings["Attribute_Unity_Code"];
        private readonly int newAccessPermissionUserType = int.Parse(WebConfigurationManager.AppSettings["NewAccessPermission.UserType"].ToString());
        private readonly string newAccessPermissionPermission = WebConfigurationManager.AppSettings["NewAccessPermission.Permission"].ToString();
        private readonly int newAccessPermissionPermissionType = int.Parse(WebConfigurationManager.AppSettings["NewAccessPermission.PermissionType"].ToString());
        private readonly string newAccessPermissionFgaddLowerLevel = WebConfigurationManager.AppSettings["NewAccessPermission.FgaddLowerLevel"].ToString();
        private readonly string structID = WebConfigurationManager.AppSettings["StructID"];
        private readonly string categoryPrimaryTitle = WebConfigurationManager.AppSettings["Category_Primary_Title"];
        private readonly string attributeRegistration = WebConfigurationManager.AppSettings["Attribute_Registration"];
        private readonly string messageDeleteDocument = WebConfigurationManager.AppSettings["MessageDeleteDocument"];
        private readonly SEClient seClient = SEConnection.GetConnection();
        private readonly SEAdministration seAdministration = SEConnection.GetConnectionAdm();
        private static readonly string searchAttributePermissionCategory = WebConfigurationManager.AppSettings["SoftExpert.SearchAttributePermissionCategory"];

        #endregion

        #region .: Public :.

        public documentDataReturn GetDocumentProperties(string documentId)
        {
            return seClient.viewDocumentData(documentId, "", "", "");
        }

        public bool VerifyDocumentPermission(string iddocumento, string usuario)
        {
            string r = seClient.checkAccessPermission(iddocumento, usuario, 6);

            string[] inx = r.Split(':');
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

        public bool InsertBinaryDocument(DocumentoAtributo documentoAtributo, out string document)
        {
            try
            {
                documentReturn documentReturnOwner = CheckRegisteredDocument(documentoAtributo.Registration, documentoAtributo.CategoryOwner);
                if (documentReturnOwner == null)
                {
                    throw new Exception("Sistema não localizou o aluno!");
                }

                // Checks the current document version
                documentoAtributo.CurrentVersion = GetPrimaryDocuments(documentoAtributo.Registration, documentoAtributo.CategoryPrimary);

                document = documentoAtributo.DocumentIdPrimary;

                documentDataReturn documentDataReturn = GetDocumentProperties(documentoAtributo.DocumentIdOwner);
                if (documentDataReturn.ATTRIBUTTES.Count() > 0)
                {
                    string response = seClient.newDocument(documentoAtributo.CategoryPrimary, documentoAtributo.DocumentIdPrimary, categoryPrimaryTitle, "", "", "", documentoAtributo.User, null, 0, null);

                    string[] responseArray = response.Split(':');
                    if (responseArray.Count() > 0)
                    {
                        if (responseArray.Count() >= 3 && responseArray[2].ToUpper().Contains("SUCESSO"))
                        {
                            InsertDataDocument(documentoAtributo, documentDataReturn);
                        }
                        else
                        {
                            throw new Exception(response);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    DeleteDocument(documentoAtributo.DocumentIdPrimary);
                }
                catch { }

                throw ex;
            }
        }

        public bool DeleteDocument(string documentId)
        {
            try
            {
                //Checks whether the document exists
                documentDataReturn documentDataReturn = GetDocumentProperties(documentId);

                //If the document already exists in the specified category, it deletes the document
                if (documentDataReturn.IDDOCUMENT == documentId)
                {
                    string deleteDocument = seClient.deleteDocument(documentDataReturn.IDCATEGORY, documentDataReturn.IDDOCUMENT, "", messageDeleteDocument);
                }
                else if (!string.IsNullOrEmpty(documentDataReturn.ERROR))
                {
                    throw new Exception(documentDataReturn.ERROR);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region .: Private :.

        private documentReturn CheckRegisteredDocument(string registration, string category)
        {
            try
            {
                attributeData[] attributes = new attributeData[1];
                attributes[0] = new attributeData { IDATTRIBUTE = attributeRegistration, VLATTRIBUTE = registration };

                searchDocumentFilter searchDocumentFilter = new searchDocumentFilter { IDCATEGORY = category };

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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private int GetPrimaryDocuments(string registration, string category)
        {
            try
            {
                attributeData[] attributes = new attributeData[1];
                attributes[0] = new attributeData { IDATTRIBUTE = attributeRegistration, VLATTRIBUTE = registration };

                searchDocumentFilter searchDocumentFilter = new searchDocumentFilter { IDCATEGORY = category };

                searchDocumentReturn searchDocumentReturn = seClient.searchDocument(searchDocumentFilter, "", attributes);

                if (searchDocumentReturn.RESULTS.Count() > 0)
                {
                    string[] s = searchDocumentReturn.RESULTS.OrderByDescending(x => x.IDDOCUMENT).FirstOrDefault().IDDOCUMENT.Split('-');
                    if (s.Count() == 5)
                    {
                        int.TryParse(s[4], out int i);

                        return i;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void InsertDataDocument(DocumentoAtributo documentoAtributo, documentDataReturn documentDataReturn)
        {
            try
            {
                foreach (attributtes item in documentDataReturn.ATTRIBUTTES)
                {
                    if (item.ATTRIBUTTENAME.Contains(prefix))
                    {
                        string valor = "";
                        if (item.ATTRIBUTTEVALUE.Count() > 0)
                        {
                            if (item.ATTRIBUTTENAME != EAttribute.SER_cad_cod_unidade.ToString() && item.ATTRIBUTTENAME != EAttribute.SER_cad_Unidade.ToString() && item.ATTRIBUTTENAME != EAttribute.SER_cad_unidades.ToString())
                            {
                                valor = item.ATTRIBUTTEVALUE[0];
                            }                            
                        }

                        try
                        {
                            seClient.setAttributeValue(documentoAtributo.DocumentIdPrimary, "", item.ATTRIBUTTENAME, valor);
                        }
                        catch (Exception)
                        {
                            throw new Exception("Campo " + item.ATTRIBUTTENAME + " com erro");
                        }
                    }
                }

                try
                {
                    seClient.setAttributeValue(documentoAtributo.DocumentIdPrimary, "", EAttribute.SER_cad_cod_unidade.ToString(), documentoAtributo.UnityCode);
                }
                catch (Exception)
                {
                    throw new Exception("Campo " + EAttribute.SER_cad_cod_unidade.ToString() + " com erro");
                }

                try
                {
                    seClient.setAttributeValue(documentoAtributo.DocumentIdPrimary, "", EAttribute.SER_cad_Unidade.ToString(), documentoAtributo.UnityName);
                }
                catch (Exception)
                {
                    throw new Exception("Campo " + EAttribute.SER_cad_Unidade.ToString() + " com erro");
                }

                try
                {
                    seClient.setAttributeValue(documentoAtributo.DocumentIdPrimary, "", EAttribute.SER_cad_unidades.ToString(), documentoAtributo.UnityCode);
                }
                catch (Exception)
                {
                    throw new Exception("Campo " + EAttribute.SER_cad_unidades.ToString() + " com erro");
                }

                try
                {
                    seClient.setAttributeValue(documentoAtributo.DocumentIdPrimary, "", attributePages, documentoAtributo.Paginas.ToString());
                }
                catch (Exception)
                {
                    throw new Exception("Campo " + attributePages + " com erro");
                }

                try
                {
                    seClient.setAttributeValue(documentoAtributo.DocumentIdPrimary, "", attributeUsuario, documentoAtributo.User.ToString());
                }
                catch (Exception)
                {
                    throw new Exception("Campo " + attributeUsuario + " com erro");
                }

                try
                {
                    seAdministration.newPosition(documentoAtributo.UnityCode, documentoAtributo.UnityCode, out string status, out string detail, out int code, out string recordid, out string recordKey);
                    seAdministration.newDepartment(documentoAtributo.UnityCode, documentoAtributo.UnityCode, documentoAtributo.UnityCode, "", "", "1");

                    seClient.newAccessPermission(documentoAtributo.DocumentIdPrimary, documentoAtributo.UnityCode + ";" + documentoAtributo.UnityCode, newAccessPermissionUserType, newAccessPermissionPermission, newAccessPermissionPermissionType, newAccessPermissionFgaddLowerLevel);
                }
                catch (Exception)
                {
                    throw new Exception("Atualização das permissão com erro");
                }

                try
                {
                    seClient.newDocumentContainerAssociation(documentoAtributo.CategoryOwner, documentoAtributo.DocumentIdOwner, "", structID, documentoAtributo.CategoryPrimary, documentoAtributo.DocumentIdPrimary, out long codeAssociation, out string detailAssociation);
                }
                catch (Exception)
                {
                    throw new Exception("Criação da associação dos documentos com erro");
                }

                if (physicalFile)
                {
                    InsertPhysicalFile(documentoAtributo);
                }
                else
                {
                    InsertEletronicFile(documentoAtributo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool InsertEletronicFile(DocumentoAtributo Indice)
        {
            try
            {
                eletronicFile[] eletronicFiles = new eletronicFile[2];

                eletronicFiles[0] = new eletronicFile
                {
                    BINFILE = Indice.FileBinary,
                    ERROR = "",
                    NMFILE = Indice.FileName
                };

                string var = seClient.uploadEletronicFile(Indice.DocumentIdPrimary, "", Indice.User, eletronicFiles);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool InsertPhysicalFile(DocumentoAtributo Indice)
        {
            try
            {
                #region .: Save File :.

                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(physicalPath);
                }

                if (File.Exists(Path.Combine(physicalPath, Indice.FileName)))
                {
                    File.Delete(Path.Combine(physicalPath, Indice.FileName));
                }

                File.WriteAllBytes(Path.Combine(physicalPath, Indice.FileName), Indice.FileBinary);

                #endregion

                #region .: Query :.

                string queryStringInsert = @"INSERT INTO ADINTERFACE (CDINTERFACE, FGIMPORT, CDISOSYSTEM, FGOPTION, NMFIELD01, NMFIELD02, NMFIELD03, NMFIELD04, NMFIELD05, NMFIELD07) VALUES((SELECT COALESCE(MAX(CDINTERFACE),0)+1 FROM ADINTERFACE), 1, 73, 97, '{0}','00','{1}','{2}','{3}','{4}')";
                string connectionString = WebConfigurationManager.ConnectionStrings["DefaultSesuite"].ConnectionString;

                #endregion

                #region .: Insert Sesuite :.

                using (SqlConnection connectionInsert = new SqlConnection(connectionString))
                {
                    string queryInsert = string.Format(queryStringInsert,
                        Indice.DocumentIdPrimary /*Identificador do Documento*/,
                        Indice.FileName /*Nome do Arquivo*/,
                        Indice.User /*Matrícula do Usuário*/,
                        physicalPathSE + Indice.FileName,
                        Indice.CategoryPrimary.Trim() /*Identificador da categoria*/);

                    using (SqlCommand commandInsert = new SqlCommand(queryInsert, connectionInsert))
                    {
                        connectionInsert.Open();
                        commandInsert.ExecuteNonQuery();
                    }
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

        public Unity GetUnity(string user)
        {
            Unity unity = new Unity();

            attributeData[] attributeDatas = new attributeData[1];
            attributeDatas[0] = new attributeData
            {
                //search enrollment
                IDATTRIBUTE = EAttribute.tfyacess_userid.ToString(),
                VLATTRIBUTE = user
            };

            searchDocumentFilter searchDocumentFilter = new searchDocumentFilter
            {
                IDCATEGORY = searchAttributePermissionCategory
            };

            searchDocumentReturn searchDocumentReturn = seClient.searchDocument(searchDocumentFilter, "", attributeDatas);
            documentReturn retorno = new documentReturn();
            if (searchDocumentReturn.RESULTS.Count() > 0)
            {
                string idDocument = searchDocumentReturn.RESULTS.FirstOrDefault().IDDOCUMENT;
                documentDataReturn documentDataReturn = seClient.viewDocumentData(idDocument, "", "", "");

                string attributeCode = EAttribute.SER_cad_cod_unidade.ToString();
                string attributeName = EAttribute.SER_cad_Unidade.ToString();

                unity.Code = documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == attributeCode) ? documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == attributeCode).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null;
                unity.Name = documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == attributeName) ? documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == attributeName).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null;
            }

            return unity;
        }
    }
}
