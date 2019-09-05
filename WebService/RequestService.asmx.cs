using iTextSharp.text.pdf;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
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
        private readonly string pathIn = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.In"]);
        private readonly string pathOut = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.Out"]);
        private readonly string pathToProcessIn = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.ToProcessIn"]);
        private readonly string pathToProcessOut = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.ToProcessOut"]);
        private readonly string pathToProcessError = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.ToProcessError"]);
        private readonly string pathLog = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.Log"]);
        private readonly string pathDocument = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.Document"]);
        private readonly string pathDocumentDelete = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.Document.Delete"]);
        private readonly string extension = WebConfigurationManager.AppSettings["Extension"];

        #region .: Methods :.       

        #region .: CAPservice .:

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public DadosAlunos findStudentByRa(string ra, string usuario)
        {
            File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: findStudentByRa. RA: {0} . Data: {1} ****", ra, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

            DadosAlunos dadosAluno = new DadosAlunos();
            InsegracaoSE integrador = new InsegracaoSE();

            dadosAluno.RetornoStudent = new List<Student>();

            try
            {
                if (string.IsNullOrEmpty(integrador.GetUnity(usuario).Code))
                {
                    File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: findStudentByRa. Erro: {0}. RA: {1}. Data: {2} ****", "O usuário não tem permissão para visualizar os dados desse aluno.", ra, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                }
                else
                {
                    if (integrador.VerifyDocumentPermission(ra, usuario))
                    {
                        com.softexpert.tecfy.documentDataReturn documentDataReturn = integrador.GetDocumentProperties(ra);
                        if (string.IsNullOrEmpty(documentDataReturn.ERROR))
                        {
                            try
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
                            catch (Exception ex)
                            {
                                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: findStudentByRa. Erro: {0}. RA: {1}. Data: {2} ****", ex.Message, ra, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                            }
                        }
                        else
                        {
                            File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: findStudentByRa. Erro: {0}. RA: {1}. Data: {2} ****", documentDataReturn.ERROR, ra, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                        }
                    }
                    else
                    {
                        File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: findStudentByRa. Erro: {0}. RA: {1}. Data: {2} ****", "O usuário não tem permissão para visualizar os dados desse aluno.", ra, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: findStudentByRa. Erro: {0}. Message: {1}, Source: {2}. Data: {3} ****", ex.Message, ex.StackTrace, ex.Source, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
            }

            return dadosAluno;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(EnableSession = true)]
        public bool sendFile(string fileName, byte[] buffer, long offset)
        {
            bool retVal = false;
            try
            {
                //Create Directories
                CreateFolder();

                // Setting the file location to be saved in the server.
                // Reading from the web.config file
                string FilePath = Path.Combine(pathIn, fileName);

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
                    File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: sendFile. Erro: {0}. Arquivo: {1}. Data: {2} ****", ex.Message, fileName, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                    Tentativa++;
                    System.Threading.Thread.Sleep(1000);
                    goto INICIO;
                }
                retVal = true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: sendFile. Erro: {0}. Arquivo: {1}. Data: {2} ****", ex.Message, fileName, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                throw ex;
            }

            return retVal;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(EnableSession = true)]
        public bool checkFile(string fileName, long fileSize)
        {
            try
            {
                //Cria os Diretorios
                CreateFolder();

                string filePath = Path.Combine(pathIn, fileName);

                FileInfo fileInfo = new FileInfo(filePath);

                if (fileInfo.Exists)
                {
                    if (fileInfo.Length == fileSize)
                    {
                        File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: checkFile. Arquivo pronto para ser enviado: {0}, Data: {1} ****", fileName, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                        return true;
                    }
                    else
                    {
                        File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: checkFile. Arquivo corrompido: {0}. Data: {1} ****", fileName, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                        return false;
                    }
                }
                else
                {
                    File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: checkFile. Arquivo não existe: {0}. Data: {1} ****", fileName, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                    return false;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: checkFile. Erro: {0}. Arquivo: {1}. Data: {1} ****", ex.Message, fileName, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                return false;
            }
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(EnableSession = true)]
        public bool submitFile(string fileName, string registration, string user)
        {
            try
            {
                //Cria os Diretorios
                CreateFolder();

                string filePathIn = Path.Combine(pathIn, fileName);
                string filePathOut = Path.Combine(pathOut, Path.GetFileNameWithoutExtension(fileName) + extension);

                if (File.Exists(filePathOut))
                {
                    File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Erro: {0}. Arquivo: {1}, RA: {2}. Data: {3} ****", "Arquivo já existente no Out", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                    return false;
                }

                if (File.Exists(filePathIn))
                {
                    if (Path.GetExtension(filePathIn) == ".pdf" || Path.GetExtension(filePathIn) == ".cry")
                    {
                        File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Arquivo sendo enviado para o SE: {0}, RA: {1}. Inicio: {2} ****", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                        if (Path.GetExtension(filePathIn) == ".cry")
                        {
                            Encrypt.DecryptFile(filePathIn, filePathOut, WebConfigurationManager.AppSettings["Key"]);
                        }
                        else if (Path.GetExtension(filePathIn) == ".pdf")
                        {
                            if (File.Exists(filePathOut))
                            {
                                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Erro: {0}. Arquivo: {1}, RA: {2}. Data: {3} ****", "Arquivo já existente no Out", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                                return false;
                            }
                            else
                            {
                                File.Copy(filePathIn, filePathOut);
                            }
                        }

                        try
                        {
                            byte[] fileBinary = File.ReadAllBytes(filePathOut);

                            int pageCount = 0;
                            try
                            {
                                using (PdfReader reader = new PdfReader(fileBinary))
                                {
                                    pageCount = reader.NumberOfPages;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }

                            if (pageCount >= 0)
                            {
                                bool result = saveJsonFile(fileName, registration, user);

                                Task objTask = Task.Factory.StartNew(() => { processFile(fileName, registration, user); });

                                return result;
                            }
                            else
                            {
                                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Arquivo corrompido número de páginas igual a 0, SE: {0}, RA: {1}. Fim: {2} ****", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Erro: {0}. Arquivo: {1}, RA: {2}. Data: {3} ****", ex.Message, fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                            return false;
                        }
                    }
                    else
                    {
                        File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. O arquivo não possui as extensões permitidas: {0}, RA: {1}. Data: {2} ****", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                        return false;
                    }
                }
                else
                {
                    File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Arquivo não existe: {0}, RA: {1}.  Data: {2} ****", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                    return false;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: submitFile. Erro: {0}. Arquivo: {1}, RA: {2}. Data: {3} ****", ex.Message, fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                return false;
            }

        }

        #endregion

        #region .: Devplace .:

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public JobOut getJobById(int jobId)
        {
            File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: getJobById. JobId: {0}. Data: {1} ****", jobId, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

            JobOut jobOut = new JobOut();

            try
            {
                RestClient client = new RestClient(WebConfigurationManager.AppSettings["API.URL"].ToString() + string.Format(WebConfigurationManager.AppSettings["API.GetJobById"].ToString(), jobId));

                RestRequest request = RestRequestHelper.Get(Method.GET);

                client.Timeout = (1000 * 60 * 60);
                client.ReadWriteTimeout = (1000 * 60 * 60);

                IRestResponse response = client.Execute(request);

                jobOut = SimpleJson.DeserializeObject<JobOut>(response.Content);

                if (!jobOut.success)
                {
                    throw new Exception(jobOut.messages.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: getJobById. Erro: {0}. JobId: {1}. Data: {2} ****", ex.Message, jobId, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                jobOut.successMessage = null;
                jobOut.messages.Add(ex.Message);
            }

            return jobOut;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public JobsRegistrationOut getJobsByRegistration(string registration)
        {
            File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: getJobsByRegistration. Usuário: {0}. Data: {1} ****", registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

            JobsRegistrationOut jobsRegistrationOut = new JobsRegistrationOut();

            try
            {
                string uri = WebConfigurationManager.AppSettings["API.URL"].ToString() + string.Format(WebConfigurationManager.AppSettings["API.GetJobsByRegistration"].ToString(), registration);

                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: getJobsByRegistration. URI: {0}. Data: {1} ****", uri, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                RestClient client = new RestClient(uri);

                RestRequest request = RestRequestHelper.Get(Method.GET);

                client.Timeout = (1000 * 60 * 60);
                client.ReadWriteTimeout = (1000 * 60 * 60);

                IRestResponse response = client.Execute(request);

                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: getJobsByRegistration. Response: {0}. Data: {1} ****", response.ContentLength, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                jobsRegistrationOut = SimpleJson.DeserializeObject<JobsRegistrationOut>(response.Content);

                if (!jobsRegistrationOut.success)
                {
                    throw new Exception(jobsRegistrationOut.messages.FirstOrDefault());
                }

                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), "**** Método: getJobsByRegistration. Success ****" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: getJobsByRegistration. Erro: {0}. Usuário: {1}. Data: {2}  ****", ex.Message, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                jobsRegistrationOut.successMessage = null;
                jobsRegistrationOut.messages.Add(ex.Message);
            }

            return jobsRegistrationOut;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public JobCategorySaveOut setJobCategorySave(int jobCategoryId, string archive)
        {
            File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: setJobCategorySave. JobCategoryId: {0}, Archive: {1}. Data: {2} ****", jobCategoryId, archive.Length, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

            JobCategorySaveOut jobCategorySaveOut = new JobCategorySaveOut();

            try
            {
                JobCategorySaveIn jobCategorySaveIn = new JobCategorySaveIn { jobCategoryId = jobCategoryId, archive = archive };

                RestClient client = new RestClient(WebConfigurationManager.AppSettings["API.URL"].ToString() + string.Format(WebConfigurationManager.AppSettings["API.SetJobCategorySave"].ToString()));

                RestRequest request = RestRequestHelper.Get(Method.POST, SimpleJson.SerializeObject(jobCategorySaveIn));

                client.Timeout = (1000 * 60 * 60);
                client.ReadWriteTimeout = (1000 * 60 * 60);

                IRestResponse response = client.Execute(request);

                jobCategorySaveOut = SimpleJson.DeserializeObject<JobCategorySaveOut>(response.Content);

                if (!jobCategorySaveOut.success)
                {
                    throw new Exception(jobCategorySaveOut.messages.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: setJobCategorySave. Erro: {0}. JobCategoryId: {1}, Archive: {2}. Data: {3}  ****", ex.Message, jobCategoryId, archive.Length, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                jobCategorySaveOut.successMessage = null;
                jobCategorySaveOut.messages.Add(ex.Message);
            }

            return jobCategorySaveOut;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public bool processPendingFiles()
        {
            HttpContext currentContext = HttpContext.Current;

            Task objTask = Task.Factory.StartNew(() =>
            {
                HttpContext.Current = currentContext;
                string Key = Guid.NewGuid().ToString();

                try
                {
                    CreateFolder();

                    string[] filesToProcess = Directory.GetFiles(pathToProcessIn);

                    foreach (string filePathToProcessIn in filesToProcess)
                    {
                        if (File.Exists(filePathToProcessIn))
                        {
                            string fileNameJson = Path.GetFileName(filePathToProcessIn);
                            string filePathToProcessOut = Path.Combine(pathToProcessOut, fileNameJson);

                            File.Move(filePathToProcessIn, filePathToProcessOut);

                            string[] jsonDeserialized;

                            try
                            {
                                using (StreamReader r = new StreamReader(filePathToProcessOut))
                                {
                                    string content = r.ReadToEnd();
                                    jsonDeserialized = JsonConvert.DeserializeObject<string[]>(content);
                                }
                                try
                                {
                                    //"jsonDeserialized" é um array de string com 3 posições, onde:
                                    // 0 = filename
                                    // 1 = registration
                                    // 2 = user
                                    string fileName = jsonDeserialized[0];
                                    string registration = jsonDeserialized[1];
                                    string user = jsonDeserialized[2];

                                    string filePathIn = Path.Combine(pathIn, fileName);
                                    string filePathOut = Path.Combine(pathOut, Path.GetFileNameWithoutExtension(fileName) + extension);

                                    byte[] fileBinary = File.ReadAllBytes(filePathOut);

                                    int pageCount = 0;
                                    try
                                    {
                                        using (PdfReader reader = new PdfReader(fileBinary))
                                        {
                                            pageCount = reader.NumberOfPages;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }

                                    if (pageCount >= 0)
                                    {
                                        Task objTask2 = Task.Factory.StartNew(() => { processFile(fileName, registration, user, false); });
                                    }
                                    else
                                    {
                                        File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: processPendingFiles. Arquivo corrompido número de páginas igual a 0, SE: {0}, RA: {1}. Fim: {2} ****", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: processPendingFiles. Erro: {0}. Arquivo: {1}, RA: {2}. Data: {3} ****", ex.Message, jsonDeserialized[0], jsonDeserialized[1], DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                                }
                            }
                            catch (Exception ex)
                            {
                                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: processPendingFiles, Leitura do JSON. Erro: {0}. Json {1}. Data: {2} ****", ex.Message, filePathToProcessOut, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: processPendingFiles. Erro: {0}. Data: {1} ****", ex.Message, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                }
            });

            return true;
        }

        public bool saveJsonFile(string fileName, string registration, string user)
        {
            try
            {
                CreateFolder();
                string filePathToProcessIn = Path.Combine(pathToProcessIn, fileName + ".json");

                string[] jsonContent = new string[3];

                jsonContent[0] = fileName;
                jsonContent[1] = registration;
                jsonContent[2] = user;

                string jsonToProcess = new JavaScriptSerializer().Serialize(jsonContent);

                File.WriteAllText(filePathToProcessIn, jsonToProcess);

                return true;
            }
            catch
            {
                try
                {                    
                    File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: saveJsonFile. Arquivo sendo enviado para o SE (Erro na Finalização): {0}, RA: {1}. Fim: {2} ****", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                    return false;
                }
                catch { return false; }
            }
        }

        private void processFile(string fileName, string registration, string user, bool moveOut = true)
        {
            string filePathIn = Path.Combine(pathIn, fileName);
            string filePathOut = Path.Combine(pathOut, Path.GetFileNameWithoutExtension(fileName) + extension);
            string filePathToProcessIn = Path.Combine(pathToProcessIn, fileName + ".json");
            string filePathToProcessOut = Path.Combine(pathToProcessOut, fileName + ".json");
            string filePathToProcessError = Path.Combine(pathToProcessError, fileName + ".json");

            try
            {
                InsegracaoSE integrador = new InsegracaoSE();

                if (moveOut)
                {
                    File.Move(filePathToProcessIn, filePathToProcessOut);
                }

                byte[] fileBinary = File.ReadAllBytes(filePathOut);

                int pageCount = 0;
                try
                {
                    using (PdfReader reader = new PdfReader(fileBinary))
                    {
                        pageCount = reader.NumberOfPages;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                Models.Unit.Unity Unity = integrador.GetUnity(user);

                if (string.IsNullOrEmpty(Unity.Code))
                {
                    File.Move(filePathToProcessOut, filePathToProcessError);
                    File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: processFile. Arquivo sendo enviado para o SE (Unidade não encontrada): {0}, RA: {1}. Fim: {2} ****", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                    return;
                }

                DocumentoAtributo documentoAtributo = new DocumentoAtributo
                {
                    FileBinary = fileBinary,
                    CategoryPrimary = WebConfigurationManager.AppSettings["Category_Primary"],
                    CategoryOwner = WebConfigurationManager.AppSettings["Category_Owner"],
                    Registration = registration,
                    User = user,
                    Extension = extension,
                    Now = DateTime.Now,
                    Paginas = pageCount,
                    UnityCode = Unity.Code,
                    UnityName = Unity.Name
                };

                integrador.InsertBinaryDocument(documentoAtributo, out string fileDocument);

                try
                {
                    File.Delete(filePathIn);
                    File.Delete(filePathOut);
                    File.Delete(filePathToProcessOut);

                    File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: processFile. Arquivo sendo enviado para o SE: {0}, RA: {1}. Fim: {2} ****", fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Thread.Sleep(5000);
                    try
                    {
                        File.Move(filePathToProcessOut, filePathToProcessIn);
                        File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: processFile. Arquivo sendo enviado para o SE (Erro {0}): {1}, RA: {2}. Fim: {3} ****", ex.Message, fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(5000);
                try
                {
                    File.Move(filePathToProcessOut, filePathToProcessIn);
                    File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: processFile. Arquivo sendo enviado para o SE (Erro {0}): {1}, RA: {2}. Fim: {3} ****", ex.Message, fileName, registration, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                }
                catch { }
            }
        }

        #endregion

        #region .: :.

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public bool deleteDocuments(string code)
        {
            if (code == WebConfigurationManager.AppSettings["Delete.Code"])
            {
                HttpContext currentContext = HttpContext.Current;

                System.Threading.Tasks.Task objTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    HttpContext.Current = currentContext;
                    File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: deleteDocuments. Inicio do processo. Data: {0} ****", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                    InsegracaoSE integrador = new InsegracaoSE();

                    if (Directory.GetFiles(pathDocumentDelete).Length > 0)
                    {
                        foreach (string file in Directory.GetFiles(pathDocumentDelete))
                        {
                            try
                            {
                                FileInfo fileInfo = new FileInfo(file);

                                using (StreamReader sr = new StreamReader(file))
                                {
                                    List<string> listDocuments = sr.ReadToEnd().Split(';').ToList();

                                    foreach (string item in listDocuments)
                                    {
                                        try
                                        {
                                            File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: deleteDocuments. Inicio exclusão SE, documento: {0}. Data: {1} ****", item, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                                            integrador.DeleteDocument(item);

                                            File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: deleteDocuments. Fim exclusão SE, documento: {0}. Data: {1} ****", item, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                                        }
                                        catch (Exception ex)
                                        {
                                            File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: deleteDocuments. Erro exclusão SE, documento: {1}. Messagem: {0} Data: {2}  ****", ex.Message, item, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                                        }
                                    }
                                }

                                string pathDelete = Path.Combine(pathDocumentDelete, "Processed");

                                if (!Directory.Exists(pathDelete))
                                {
                                    Directory.CreateDirectory(pathDelete);
                                }

                                File.Move(file, Path.Combine(pathDelete, fileInfo.Name));
                            }
                            catch (Exception ex)
                            {
                                File.AppendAllText(string.Format("{0}\\Error_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: deleteDocuments. Erro no processo de exclusão do arquivo: {0}. Mensagem: {1}. Data: {2}  ****", file, ex.Message, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                            }
                        }
                    }

                    File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: deleteDocuments. Final do processo. Data: {0} ****", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);
                });

                return true;
            }
            else
            {
                File.AppendAllText(string.Format("{0}\\Validation_{1}.txt", pathLog, DateTime.Now.ToString("yyyyMMdd")), string.Format("**** Método: deleteDocuments. Código de validação não corresponde (code: {0}). Data: {1} ****", code, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) + Environment.NewLine);

                return false;
            }
        }

        #endregion

        #endregion

        #region .: Helper :.

        private void CreateFolder()
        {
            string[] folders = new string[] { "Path.In", "Path.Out", "Path.ToProcessIn", "Path.ToProcessOut", "Path.ToProcessError", "Path.Log", "Path.Document" };

            foreach (string item in folders)
            {
                string pathInput = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings[item]).ToString();
                if (!Directory.Exists(pathInput))
                {
                    Directory.CreateDirectory(pathInput);
                }
            }
        }

        #endregion
    }
}
