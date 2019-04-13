using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Script.Services;
using System.Web.Services;
using WebService.Connection;
using WebService.Helper;
using WebService.SE;

namespace WebService
{
    [ScriptService]
    [WebService(Namespace = "http://www.tecnoset.com.br/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class RequestService : System.Web.Services.WebService
    {
        readonly string pathLog = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.Log"]);

        #region .: Methods :.       

        #region .: CAPservice .:

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public DadosAlunos findStudentByRa(string ra, string usuario)
        {
            DadosAlunos dadosAluno = new DadosAlunos();
            var integrador = new InsegracaoSE();

            dadosAluno.RetornoStudent = new List<Student>();

            if (integrador.VerifyDocumentPermission(ra, usuario))
            {
                var documentDataReturn = integrador.GetDocumentProperties(ra);
                if (string.IsNullOrEmpty(documentDataReturn.ERROR))
                {
                    Student estudante = new Student
                    {
                        RA = documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Registration"].ToString()) ? documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Registration"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        CPFALUNO = documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_CPF"].ToString()) ? documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_CPF"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        NOMECURSO = documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Course"].ToString()) ? documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Course"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        CODCENTRO = documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity"].ToString()) ? documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        NOMEALUNO = documentDataReturn.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Name"].ToString()) ? documentDataReturn.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Name"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                    };
                    dadosAluno.RetornoStudent.Add(estudante);
                }
            }

            return dadosAluno;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(EnableSession = true)]
        public bool sendFile(string fileName, byte[] buffer, long offset)
        {
            //FileName
            bool retVal = false;
            try
            {
                string path = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path"]);

                // Setting the file location to be saved in the server.
                // Reading from the web.config file
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string FilePath = Path.Combine(path, fileName);

                // New file, create an empty file
                if (offset == 0)
                {
                    File.Create(FilePath).Close();
                }

                // Open a file stream and write the buffer.
                // Don't open with FileMode.Append because the transfer may wish to
                // Start a different point
                int Tentativa = 0;
                INICIO:
                try
                {
                    if (Tentativa < 6)
                    {
                        using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            fs.Seek(offset, SeekOrigin.Begin);
                            fs.Write(buffer, 0, buffer.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: sendFile. Erro: {0}. Arquivo: {1}  ****", ex.Message, fileName) + Environment.NewLine);

                    Tentativa++;
                    System.Threading.Thread.Sleep(1000);
                    goto INICIO;
                }
                retVal = true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: sendFile. Erro: {0}. Arquivo: {1}  ****", ex.Message, fileName) + Environment.NewLine);
                throw ex;
                //sending error to an email id
                //common.SendError(ex);
            }

            return retVal;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(EnableSession = true)]
        public bool checkFile(string fileName, long fileSize)
        {
            try
            {
                string path = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path"]);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = Path.Combine(path, fileName);

                FileInfo fileInfo = new FileInfo(filePath);

                if (fileInfo.Exists)
                {
                    if (fileInfo.Length == fileSize)
                    {
                        File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: checkFile. Arquivo pronto para ser enviado: {0} ****", fileName) + Environment.NewLine);
                        return true;
                    }
                    else
                    {
                        File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: checkFile. Arquivo corrompido: {0} ****", fileName) + Environment.NewLine);
                        return false;
                    }
                }
                else
                {
                    File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: checkFile. Arquivo não existe: {0} ****", fileName) + Environment.NewLine);
                    return false;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: checkFile. Erro: {0}. Arquivo: {1}  ****", ex.Message, fileName) + Environment.NewLine);

                return false;
            }
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(EnableSession = true)]
        public bool submitFile(string fileName, string registration, string user)
        {
            try
            {
                string path = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path"]);
                string pathDossier = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.Document"]);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (!Directory.Exists(pathDossier))
                {
                    Directory.CreateDirectory(pathDossier);
                }

                string filePath = Path.Combine(path, fileName);

                if (File.Exists(filePath))
                {
                    if (Path.GetExtension(filePath) == ".pdf")
                    {
                        File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Arquivo sendo enviado para o SE: {0}. Inicio: {1} ****", fileName, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                        try
                        {
                            var integrador = new InsegracaoSE();

                            var documentoAtributo = new DocumentoAtributo
                            {
                                FileBinary = System.IO.File.ReadAllBytes(filePath),
                                CategoryPrimary = WebConfigurationManager.AppSettings["Category_Primary"],
                                CategoryOwner = WebConfigurationManager.AppSettings["Category_Owner"],
                                Registration = registration,
                                User = user,
                                Extension = Path.GetExtension(filePath),
                                Now = DateTime.Now
                            };

                            integrador.InsertBinaryDocument(documentoAtributo, out string document);

                            File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Arquivo sendo enviado para o SE: {0}. Fim: {1} ****", fileName, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                            document = Path.Combine(pathDossier, document + ".pdf");

                            File.Move(filePath, document);

                            return true;
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Erro: {0}. Arquivo: {1}  ****", ex.Message, fileName) + Environment.NewLine);
                            return false;
                        }
                    }
                    else
                    {
                        File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Arquivo sem extensão: {0} ****", fileName) + Environment.NewLine);
                        return false;
                    }
                }
                else
                {
                    File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Arquivo não existe: {0} ****", fileName) + Environment.NewLine);
                    return false;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Erro: {0}. Arquivo: {1}  ****", ex.Message, fileName) + Environment.NewLine);

                return false;
            }
        }

        #endregion

        #region .: Devplace .:

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public JobOut getJobById(int jobId)
        {
            JobOut jobOut = new JobOut();

            try
            {
                var client = new RestClient(WebConfigurationManager.AppSettings["API.URL"].ToString() + string.Format(WebConfigurationManager.AppSettings["API.GetJobById"].ToString(), jobId));

                var request = RestRequestHelper.Get(Method.GET);

                IRestResponse response = client.Execute(request);

                jobOut = SimpleJson.DeserializeObject<JobOut>(response.Content);

                if (!jobOut.success)
                {
                    throw new Exception(jobOut.messages.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                jobOut.successMessage = null;
                jobOut.messages.Add(ex.Message);
            }

            return jobOut;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public JobsRegistrationOut getJobsByRegistration(string registration)
        {
            JobsRegistrationOut jobsRegistrationOut = new JobsRegistrationOut();

            try
            {
                var client = new RestClient(WebConfigurationManager.AppSettings["API.URL"].ToString() + string.Format(WebConfigurationManager.AppSettings["API.GetJobsByRegistration"].ToString(), registration));

                var request = RestRequestHelper.Get(Method.GET);

                IRestResponse response = client.Execute(request);

                jobsRegistrationOut = SimpleJson.DeserializeObject<JobsRegistrationOut>(response.Content);

                if (!jobsRegistrationOut.success)
                {
                    throw new Exception(jobsRegistrationOut.messages.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                jobsRegistrationOut.successMessage = null;
                jobsRegistrationOut.messages.Add(ex.Message);
            }

            return jobsRegistrationOut;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public JobCategorySaveOut setJobCategorySave(int jobCategoryId, string archive)
        {
            JobCategorySaveOut jobCategorySaveOut = new JobCategorySaveOut();

            try
            {
                JobCategorySaveIn jobCategorySaveIn = new JobCategorySaveIn { jobCategoryId = jobCategoryId, archive = archive };

                var client = new RestClient(WebConfigurationManager.AppSettings["API.URL"].ToString() + string.Format(WebConfigurationManager.AppSettings["API.SetJobCategorySave"].ToString()));

                var request = RestRequestHelper.Get(Method.POST, SimpleJson.SerializeObject(jobCategorySaveIn));

                IRestResponse response = client.Execute(request);

                jobCategorySaveOut = SimpleJson.DeserializeObject<JobCategorySaveOut>(response.Content);

                if (!jobCategorySaveOut.success)
                {
                    throw new Exception(jobCategorySaveOut.messages.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                jobCategorySaveOut.successMessage = null;
                jobCategorySaveOut.messages.Add(ex.Message);
            }

            return jobCategorySaveOut;
        }

        #endregion

        #endregion
    }
}
