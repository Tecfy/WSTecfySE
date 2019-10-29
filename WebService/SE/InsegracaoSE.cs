using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
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

        private readonly string physicalPath = WebConfigurationManager.AppSettings["Sesuite.Physical.Path"];
        private readonly string physicalPathSE = WebConfigurationManager.AppSettings["Sesuite.Physical.Path.SE"];
        private readonly string categoryPrimaryTitle = WebConfigurationManager.AppSettings["Category_Primary_Title"];
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

        public bool InsertBinaryDocument(DocumentoAtributo documentoAtributo)
        {
            try
            {
                #region .: Query :.

                string connectionString = WebConfigurationManager.ConnectionStrings["DefaultSesuite"].ConnectionString;

                #endregion

                #region .: Synchronization :.

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand("p_Dossier_Document", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@Registration", SqlDbType.VarChar).Value = documentoAtributo.Registration;
                        command.Parameters.Add("@User", SqlDbType.VarChar).Value = documentoAtributo.User;
                        command.Parameters.Add("@Pages", SqlDbType.Decimal).Value = documentoAtributo.Pages;
                        command.Parameters.Add("@UnityCode", SqlDbType.VarChar).Value = documentoAtributo.UnityCode;
                        command.Parameters.Add("@UnityName", SqlDbType.VarChar).Value = documentoAtributo.UnityName;
                        command.Parameters.Add("@Title", SqlDbType.VarChar).Value = categoryPrimaryTitle;

                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        reader.Read();

                        documentoAtributo.DocumentIdPrimary = reader["DocumentId"].ToString().Trim();

                        InsertPhysicalFile(documentoAtributo, 1);
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

        private bool InsertPhysicalFile(DocumentoAtributo Indice, int exec)
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

                string queryStringInsert = @"INSERT INTO ADINTERFACE (CDINTERFACE, FGIMPORT, CDISOSYSTEM, FGOPTION, NMFIELD01, NMFIELD02, NMFIELD03, NMFIELD04, NMFIELD05, NMFIELD07) VALUES((SELECT COALESCE(MAX(CDINTERFACE),0)+1 FROM ADINTERFACE), 1, 73, 97, '{0}','0','{1}','{2}','{3}','{4}')";
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
            }
            catch (Exception ex)
            {
                if (exec < 5)
                {
                    exec++;
                    int sleep = 3000;
                    int.TryParse(ConfigurationManager.AppSettings["SLEEP"], out sleep);

                    Thread.Sleep(sleep);
                    InsertPhysicalFile(Indice, exec);
                }
                else
                {
                    throw ex;
                }
            }

            return true;
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
