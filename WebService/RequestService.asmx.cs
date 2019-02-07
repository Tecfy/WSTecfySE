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
        #region .: Methods :.       

        #region .: CAPservice .:

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public DadosAlunos findStudentByRa(string ra, string usuario)
        {
            DadosAlunos dadosAluno = new DadosAlunos();
            var integrador = new InsegracaoSE();

            dadosAluno.RetornoStudent = new List<Student>();

            if (integrador.VerificarPermissaoDocumento(ra, usuario))
            {
                var it = integrador.VerificarPropriedadesDocumento(ra);
                if (string.IsNullOrEmpty(it.ERROR))
                {
                    Student estudante = new Student
                    {
                        RA = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Registration"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Registration"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        CPFALUNO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_CPF"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_CPF"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        NOMECURSO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Course"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Course"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        CODCENTRO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        NOMEALUNO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Name"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Name"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                    };
                    dadosAluno.RetornoStudent.Add(estudante);
                }
            }

            return dadosAluno;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public bool insertDocumentBase64(string arquivo, String Matricula, string categoria, string ip, string usuario, string extensao)
        {
            try
            {
                var integrador = new InsegracaoSE();

                var documentoAtributo = new DocumentoAtributo
                {
                    ArquivoBinario = Convert.FromBase64String(arquivo),
                    Categoria = WebConfigurationManager.AppSettings["Category_Primary"],
                    Matricula = Matricula,
                    Usuario = usuario,
                    Arquivo = new FileInfo(Guid.NewGuid() + extensao)
                };

                integrador.InserirDocumentoBinario(documentoAtributo);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(EnableSession = true)]
        public bool sendFile(string fileName, byte[] buffer, long offset)
        {
            //FileName
            bool retVal = false;
            try
            {
                //Cria os Diretorios
                CreateFolder();

                string path = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.In"]);

                // Setting the file location to be saved in the server.
                // Reading from the web.config file
                if (Directory.Exists(path))
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
                catch
                {
                    Tentativa++;
                    System.Threading.Thread.Sleep(1000);
                    goto INICIO;
                }
                retVal = true;
            }
            catch (Exception ex)
            {
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
            //Cria os Diretorios
            CreateFolder();

            string path = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.In"]);

            string filePath = Path.Combine(path, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists)
            {
                if (fileInfo.Length == fileSize)
                {
                    return true;
                }
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(EnableSession = true)]
        public bool submitFile(string fileName, string registration, string user)
        {
            //Cria os Diretorios
            CreateFolder();

            string pathIn = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.In"]);
            string pathOut = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.Out"]);
            string pathEnd = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.End"]);

            string filePathIn = Path.Combine(pathIn, fileName);
            string filePathOut = Path.Combine(pathOut, fileName);
            string filePathEnd = Path.Combine(pathEnd, fileName);

            if (File.Exists(filePathIn))
            {
                Helper.Encrypt.DecryptFile(filePathIn, filePathOut, WebConfigurationManager.AppSettings["Key"]);

                FileInfo fileInfo = new FileInfo(filePathOut);

                try
                {
                    var integrador = new InsegracaoSE();

                    var documentoAtributo = new DocumentoAtributo
                    {
                        ArquivoBinario = System.IO.File.ReadAllBytes(filePathIn),
                        Categoria = WebConfigurationManager.AppSettings["Category_Primary"],
                        Matricula = registration,
                        Usuario = user,
                        Arquivo = new FileInfo(Guid.NewGuid() + fileInfo.Extension)
                    };

                    integrador.InserirDocumentoBinario(documentoAtributo);

                    File.Move(filePathIn, filePathEnd);
                    File.Delete(filePathOut);

                    DeleteFiles();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
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

        #region .: Helper :.

        private void CreateFolder()
        {
            var folders = new string[] { "Path.In", "Path.Out", "Path.End" };

            foreach (var item in folders)
            {
                var pathInput = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings[item]).ToString();
                if (!Directory.Exists(pathInput))
                {
                    Directory.CreateDirectory(pathInput);
                }
            }
        }

        private void DeleteFiles()
        {
            string pathEnd = ServerMapHelper.GetServerMap(WebConfigurationManager.AppSettings["Path.End"]);
            int days = Convert.ToInt32(WebConfigurationManager.AppSettings["Days"]);

            foreach (var item in Directory.GetFiles(pathEnd))
            {
                FileInfo fileInfo = new FileInfo(item);
                if (fileInfo.CreationTime.AddDays(days) <= DateTime.Now)
                {
                    File.Delete(item);
                }
            }
        }

        #endregion 
    }
}
