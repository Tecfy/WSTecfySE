﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using TecnoSet.Ecm.Wpf.Services.SE;
using WebService.com.softexpert.tecfy;
using WebService.Connection;
using WebService.Helper;
using WebService.Models.Common.Enum;
using WebService.Models.Unit;

namespace WebService.SE
{
    public class InsegracaoSE
    {
        #region .: Attributes :.

        private readonly string pathLog = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.Log"]);
        private readonly string pathToProcessOut = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.ToProcessOut"]);
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
                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertBinaryDocument. Arquivo sendo enviado para o SE: {0}. Inicio: {1} ****", documentoAtributo.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                #region .: Synchronization :.

                if (string.IsNullOrEmpty(documentoAtributo.DocumentId))
                {
                    string documentIdPrimary = string.Empty;
                    InsertDocumentSE(documentoAtributo, out documentIdPrimary);
                    SaveJsonFile(documentoAtributo.FileNameJson, documentoAtributo.Registration, documentoAtributo.User, documentIdPrimary);
                    documentoAtributo.DocumentIdPrimary = documentIdPrimary;
                    InsertPhysicalFile(documentoAtributo);
                }
                else
                {
                    documentoAtributo.DocumentIdPrimary = documentoAtributo.DocumentId;

                    int status = 0;
                    ValidDocumentSE(documentoAtributo.DocumentId, out status);

                    switch (status)
                    {
                        case 1:
                            //Document does not exist
                            string documentIdPrimary = string.Empty;
                            InsertDocumentSE(documentoAtributo, out documentIdPrimary);
                            SaveJsonFile(documentoAtributo.FileNameJson, documentoAtributo.Registration, documentoAtributo.User, documentIdPrimary);
                            documentoAtributo.DocumentIdPrimary = documentIdPrimary;
                            InsertPhysicalFile(documentoAtributo);
                            break;
                        case 2:
                            //Document exists without file
                            InsertPhysicalFile(documentoAtributo);
                            break;
                        default:
                            //Document exists with file     
                            throw new Exception("ValidDocumentSE");
                    }
                }

                #endregion

                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertBinaryDocument. Arquivo sendo enviado para o SE: {0}. Fim: {1} ****", documentoAtributo.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertBinaryDocument. Arquivo sendo enviado para o SE (Erro {0}): {1}. Fim: {2} ****", ex.Message, documentoAtributo.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
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

        private bool InsertDocumentSE(DocumentoAtributo documentoAtributo, out string documentIdPrimary)
        {
            try
            {
                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertDocumentSE. Arquivo sendo enviado para o SE: {0}. Inicio: {1} ****", documentoAtributo.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                string connectionString = WebConfigurationManager.ConnectionStrings["DefaultSesuite"].ConnectionString;

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

                        documentIdPrimary = reader["DocumentId"].ToString().Trim();
                    }
                }

                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertDocumentSE. Arquivo sendo enviado para o SE: {0}. Fim: {1} ****", documentoAtributo.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertDocumentSE. Arquivo sendo enviado para o SE (Erro {0}): {1}. Fim: {2} ****", ex.Message, documentoAtributo.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                throw new Exception("InsertDocumentSE");
            }
        }

        private bool InsertPhysicalFile(DocumentoAtributo Indice)
        {
            try
            {
                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertPhysicalFile. Arquivo sendo enviado para o SE: {0}. Inicio: {1} ****", Indice.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                #region .: Save File :.

                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(physicalPath);
                }

                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertPhysicalFile. Arquivo sendo enviado para o SE: {0}. Passo 1. Inicio: {1} ****", Indice.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                if (File.Exists(Path.Combine(physicalPath, Indice.FileName)))
                {
                    File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertPhysicalFile. Arquivo sendo enviado para o SE: {0}. Passo 1.1 Path: {1} ****", Indice.Registration, Path.Combine(physicalPath, Indice.FileName)) + Environment.NewLine);
                    File.Delete(Path.Combine(physicalPath, Indice.FileName));
                }

                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertPhysicalFile. Arquivo sendo enviado para o SE: {0}. Passo 2. Inicio: {1} ****", Indice.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                File.WriteAllBytes(Path.Combine(physicalPath, Indice.FileName), Indice.FileBinary);

                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertPhysicalFile. Arquivo sendo enviado para o SE: {0}. Passo 3. Inicio: {1} ****", Indice.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                #endregion

                #region .: Query :.

                string queryStringInsert = @"INSERT INTO ADINTERFACE (CDINTERFACE, FGIMPORT, CDISOSYSTEM, FGOPTION, NMFIELD01, NMFIELD02, NMFIELD03, NMFIELD04, NMFIELD05, NMFIELD07) VALUES((SELECT COALESCE(MAX(CDINTERFACE),0)+1 FROM ADINTERFACE), 1, 73, 97, '{0}','0','{1}','{2}','{3}','{4}')";
                string connectionString = WebConfigurationManager.ConnectionStrings["DefaultSesuite"].ConnectionString;

                #endregion

                #region .: Insert Sesuite :.

                using (SqlConnection connectionInsert = new SqlConnection(connectionString))
                {
                    File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertPhysicalFile. Arquivo sendo enviado para o SE: {0}. Passo 4. Inicio: {1} ****", Indice.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

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

                    File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertPhysicalFile. Arquivo sendo enviado para o SE: {0}. Passo 5. Inicio: {1} ****", Indice.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                }

                #endregion                
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertPhysicalFile. Arquivo sendo enviado para o SE (Erro {0}): {1}. Fim: {2} ****", ex.Message, Indice.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                throw new Exception("InsertPhysicalFile");
            }

            File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: InsertPhysicalFile. Arquivo sendo enviado para o SE: {0}. Fim: {1} ****", Indice.Registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

            return true;
        }

        private bool SaveJsonFile(string fileName, string registration, string user, string documentId)
        {
            try
            {
                string filePathToProcessIn = Path.Combine(pathToProcessOut, fileName + ".json");

                string[] jsonContent = new string[4];

                jsonContent[0] = fileName;
                jsonContent[1] = registration;
                jsonContent[2] = user;
                jsonContent[3] = documentId;

                string jsonToProcess = new JavaScriptSerializer().Serialize(jsonContent);

                File.WriteAllText(filePathToProcessIn, jsonToProcess);

                return true;
            }
            catch
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: saveJsonFile. Arquivo sendo enviado para o SE: {0}, RA: {1}. Fim: {2} ****", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                throw new Exception("SaveJsonFile");
            }
        }

        private void ValidDocumentSE(string documentIdPrimary, out int status)
        {
            try
            {
                documentDataReturn documentDataReturn = seClient.viewDocumentData(documentIdPrimary, "", "", "");

                if (documentDataReturn.ERROR == null)
                {
                    eletronicFile[] eletronicFiles = seClient.downloadEletronicFile(documentDataReturn.IDDOCUMENT, "", "", "", "", "", "", "1");

                    if (!eletronicFiles.Any(x => x.ERROR != null))
                    {
                        status = 3;//Document exists with file                        
                    }
                    else
                    {
                        status = 2;//Document exists without file
                    }
                }
                else
                {
                    status = 1;//Document does not exist
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: ValidDocumentSE. Arquivo sendo enviado para o SE (Erro {0}): {1}. Fim: {2} ****", ex.Message, documentIdPrimary, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                throw new Exception("ValidDocumentSE");
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
