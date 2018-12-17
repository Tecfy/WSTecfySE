using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Script.Services;
using System.Web.Services;
using WebService.Connection;
using WebService.SE;

namespace WebService
{
    [ScriptService]
    [WebService(Namespace = "http://www.tecnoset.com.br/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class RequestService : System.Web.Services.WebService
    {
        #region .: Variables :.

        private readonly string strConexaoBaseStudent = ConfigurationManager.AppSettings["appConexaoBaseStudent"];
        private readonly string strConexaoBaseDocControl = ConfigurationManager.AppSettings["appConexaoBaseDocControl"];
        private static bool PROD;

        #endregion

        #region .: Constructor :.

        static RequestService()
        {
            PROD = Convert.ToBoolean(ConfigurationManager.AppSettings["appEnvironProd"]);
        }

        public RequestService()
        {
        }

        #endregion

        #region .: Methods :.

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public DadosAlunos findStudentByName(string name)
        {
            DadosAlunos dadosAluno = new DadosAlunos();
            var integrador = new InsegracaoSE();

            var mt = integrador.VerificaDocumentoNome(name);

            dadosAluno.RetornoStudent = new List<Student>();
            if (mt != null)
            {
                foreach (var item in mt)
                {
                    var it = integrador.VerificarPropriedadesDocumento(item.IDDOCUMENT);
                    Student estudante = new Student
                    {
                        NOMEALUNO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Name"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Name"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        RA = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Registration"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Registration"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        CPFALUNO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_CPF"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_CPF"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        CODCENTRO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        NOMECURSO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Course"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Course"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                        SIGLACENTRO = "0125",
                        NOMECENTRO = "0125",
                        CODCURSO = "0125"
                    };
                    dadosAluno.RetornoStudent.Add(estudante);
                }

            }

            return dadosAluno;
        }

        private static int contador;
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public DadosAlunos findStudentByRa(string ra, string usuario)
        {
            DadosAlunos dadosAluno = new DadosAlunos();
            var integrador = new InsegracaoSE();

            dadosAluno.RetornoStudent = new List<Student>();

            var it = integrador.VerificarPropriedadesDocumento(ra);
            if (string.IsNullOrEmpty(it.ERROR))
            {
                Student estudante = new Student
                {
                    NOMEALUNO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Name"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Name"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                    RA = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Registration"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Registration"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                    CPFALUNO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_CPF"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_CPF"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                    CODCENTRO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Unity"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                    NOMECURSO = it.ATTRIBUTTES.Any(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Course"].ToString()) ? it.ATTRIBUTTES.Where(x => x.ATTRIBUTTENAME == WebConfigurationManager.AppSettings["Attribute_Course"].ToString()).FirstOrDefault().ATTRIBUTTEVALUE.FirstOrDefault() : null,
                    SIGLACENTRO = "0125",
                    NOMECENTRO = "0125",
                    CODCURSO = "0125"
                };
                dadosAluno.RetornoStudent.Add(estudante);
            }

            return dadosAluno;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public DateSystem getDateSystem()
        {
            DateSystem dateSystem;
            SqlConnection sqlConnection = null;
            SqlDataReader sqlDataReader = null;
            DateSystem dateLimite = new DateSystem();
            string str = "select top 1  * from datesystem ";
            try
            {
                try
                {
                    sqlConnection = new SqlConnection(this.strConexaoBaseDocControl);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(str, sqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
                    sqlDataReader = sqlCommand.ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        DateSystem dateSystem1 = new DateSystem()
                        {
                            SemestreVigente = Convert.ToString(sqlDataReader["semestreVigente"]),
                            InicioAtividade = Convert.ToString(sqlDataReader["inicioAtividade"]),
                            DivulgacaoSite = Convert.ToString(sqlDataReader["divulgacaoSite"]),
                            DataEntregaDoc = Convert.ToString(sqlDataReader["dataEntregaDoc"]),
                            AnoVigencia = Convert.ToString(sqlDataReader["anoVigencia"])
                        };
                        dateLimite = dateSystem1;
                    }
                    dateLimite.DataLimite = this.GetDateLimite();
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    CreateLogFiles createLogFile = new CreateLogFiles(base.Server.MapPath("Logs/ErrorLog"));
                    createLogFile.ErrorLog(string.Concat("Metodo GetDateLimit", exception.Message));
                    dateSystem = null;
                    return dateSystem;
                }
                return dateLimite;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
                if (sqlDataReader != null)
                {
                    sqlDataReader.Close();
                }
            }
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public DocumentsStudent getCategory()
        {
            DocumentsStudent documentsStudent = new DocumentsStudent();

            var integrador = new InsegracaoSE();
            var categorias = integrador.CarregarCategorias();
            documentsStudent.RetornoDocument = new List<Document>();
            if (categorias != null)
            {
                foreach (var item in categorias.RESULTARRAY)
                {
                    if (item.IDCATEGORY != WebConfigurationManager.AppSettings["Category_Owner"])
                    {
                        Document doc = new Document
                        {
                            Cod = item.IDCATEGORY.ToString(),
                            Name = item.NMCATEGORY,
                            TextName = item.NMCATEGORY
                        };
                        documentsStudent.RetornoDocument.Add(doc);
                    }
                }
            }

            documentsStudent.RetornoDocument = documentsStudent.RetornoDocument.OrderBy(_s => _s.Name).ToList();
            return documentsStudent;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public DadosProgramAtivity getProgramRegistration(string CenterCod)
        {
            DadosProgramAtivity dadosProgramAtivity;
            SqlConnection sqlConnection = null;
            SqlDataReader sqlDataReader = null;
            DadosProgramAtivity dadosProgramAtivity1 = new DadosProgramAtivity();
            string str = string.Concat("select * from ProgramAtivity where centerCod = '", CenterCod, " '");
            try
            {
                try
                {
                    sqlConnection = new SqlConnection(this.strConexaoBaseDocControl);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(str, sqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
                    sqlDataReader = sqlCommand.ExecuteReader();
                    dadosProgramAtivity1 = new DadosProgramAtivity()
                    {
                        RetornoAtivity = new List<Ativity>()
                    };
                    while (sqlDataReader.Read())
                    {
                        List<Ativity> retornoAtivity = dadosProgramAtivity1.RetornoAtivity;
                        Ativity ativity = new Ativity()
                        {
                            StartDate = Convert.ToString(sqlDataReader["StartDate"]),
                            CenterDescription = Convert.ToString(sqlDataReader["CenterDescription"]),
                            Locale = Convert.ToString(sqlDataReader["Locale"]),
                            CenterCod = Convert.ToString(sqlDataReader["CenterCod"]),
                            Hour = Convert.ToString(sqlDataReader["Hour"])
                        };
                        retornoAtivity.Add(ativity);
                    }
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    CreateLogFiles createLogFile = new CreateLogFiles(base.Server.MapPath("Logs/ErrorLog"));
                    createLogFile.ErrorLog(string.Concat("Metodo Programaderegistros", exception.Message));
                    dadosProgramAtivity = null;
                    return dadosProgramAtivity;
                }
                return dadosProgramAtivity1;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
                if (sqlDataReader != null)
                {
                    sqlDataReader.Close();
                }
            }
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public bool insertDocumentByteArray(byte[] arquivo, String Matricula, string categoria, string ip, string usuario, string extensao)
        {
            try
            {
                var integrador = new InsegracaoSE();

                var da = new DocumentoAtributo();
                da.ArquivoBinario = arquivo;
                da.Categoria = categoria;
                da.Matricula = Matricula;
                da.Arquivo = new FileInfo(Guid.NewGuid() + "." + extensao);

                AdicionarArquivoSaida("insertDocumentBase64 convert", Guid.NewGuid().ToString());

                integrador.InserirDocumentoBinario(da);

                AdicionarArquivoSaida("insertDocumentBase64", Guid.NewGuid().ToString());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public bool insertDocumentBase64(string arquivo, String Matricula, string categoria, string ip, string usuario, string extensao)
        {
            try
            {
                var integrador = new InsegracaoSE();

                var da = new DocumentoAtributo
                {
                    ArquivoBinario = Convert.FromBase64String(arquivo),
                    Categoria = WebConfigurationManager.AppSettings["Category_Primary"],
                    Matricula = Matricula,
                    Usuario = usuario,
                    Arquivo = new FileInfo(Guid.NewGuid() + extensao)
                };

                integrador.InserirDocumentoBinario(da);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public bool insertDocumentEmbarcada(string data, string Identificacao, string id, String ra, string categoria)
        {
            AdicionarArquivof("passou ra: " + ra + " categoria " + categoria);

            var integrador = new InsegracaoSE();

            var da = new DocumentoAtributo
            {
                ArquivoBinario = Convert.FromBase64String(id),
                Categoria = categoria,
                Matricula = ra,
                Arquivo = new FileInfo(Guid.NewGuid() + ".tif"),
                Data = data,
                Identificacao = Identificacao
            };

            integrador.InserirDocumentoBinario(da);

            return false;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public bool insertDocument(string id, String ra, string categoria)
        {
            AdicionarArquivof("passou ra: " + ra + " categoria " + categoria);

            var integrador = new InsegracaoSE();

            var da = new DocumentoAtributo
            {
                ArquivoBinario = Convert.FromBase64String(id),
                Categoria = categoria,
                Matricula = ra,
                Arquivo = new FileInfo(Guid.NewGuid() + ".tif")
            };

            integrador.InserirDocumentoBinario(da);

            return false;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public DocumentsStudent listDocument(string ra = "", string codcurso = "", bool requered = true)
        {
            DocumentsStudent documentsStudent = new DocumentsStudent();

            var integrador = new InsegracaoSE();

            var categorias = integrador.CarregarCategorias();

            documentsStudent.RetornoDocument = new List<Document>();

            if (categorias != null)
            {
                foreach (var item in categorias.RESULTARRAY)
                {
                    if (item.IDCATEGORY != WebConfigurationManager.AppSettings["Category_Owner"])
                    {
                        Document doc = new Document
                        {
                            Cod = item.IDCATEGORY.ToString(),
                            Name = item.NMCATEGORY,
                            TextName = item.NMCATEGORY
                        };

                        documentsStudent.RetornoDocument.Add(doc);
                    }
                }
            }

            documentsStudent.RetornoDocument = documentsStudent.RetornoDocument.OrderBy(_s => _s.Name).ToList();

            return documentsStudent;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public RetornoCategoria getCategorias(string matricula)
        {
            File.AppendAllText(@"C:\\LOG\Categoria" + Guid.NewGuid(), matricula);

            List<Categoria> al = new List<Categoria>();
            var alunos = OpenCSV();

            var results = alunos.Where(p => p.Matricula == matricula).ToList();

            InsegracaoSE ins = new InsegracaoSE();
            var cat = ins.CarregarCategorias().RESULTARRAY;

            foreach (var item in results)
            {
                if (al.Where(_s => _s.CategoriaCod == item.Categoria).Count() == 0)
                    al.Add(new Categoria() { CategoriaCod = item.Categoria, NomeCategoria = cat.Where(_d => _d.IDCATEGORY == item.Categoria).FirstOrDefault().NMCATEGORY });
            }
            var retorno = new RetornoCategoria() { Categorias = al };

            File.AppendAllText(@"C:\\LOG\Categoria" + Guid.NewGuid(), retorno.Categorias.Count.ToString());

            return retorno;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public AlunosRetorno getMatriculas()
        {
            AlunosRetorno ar = new AlunosRetorno();

            List<Aluno> al = new List<Aluno>();
            var inse = new InsegracaoSE();
            var alunos = OpenCSV();
            var results = alunos.GroupBy(p => p.Matricula, (key, g) => new { Matricula = key }).ToList();

            foreach (var item in results)
            {
                var nome = this.findStudentByRa(item.Matricula, "").RetornoStudent[0].NOMEALUNO;

                al.Add(new Aluno() { Matricula = item.Matricula, Nome = nome });
            }

            ar.RetornoAluno = al;

            return ar;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public string setEndProcess(string RA, string CodCurso)
        {
            string str;

            if (!PROD)
            {
                return "true";
            }

            SqlConnection sqlConnection = null;
            SqlDataReader sqlDataReader = null;
            DocumentsStudent documentsStudent = new DocumentsStudent();
            string[] rA = new string[] { "select * from Document where Document.Cod not in\r\n                                    (\r\n\t                                    select distinct DocId \r\n\t                                    from TransactionDoc \r\n\t                                    where ra = '", RA, "'\r\n                                    )\r\n                                    and docForcodCenter  =  ''\r\n                                    and requeredDoc = 'true'\r\n                                    union select * from document where docForcodCenter = '", CodCurso, "' \r\n                                            and Cod not in\r\n\t\t                                    ( \r\n\t\t\t                                    select distinct DocId \r\n\t\t\t                                    from TransactionDoc \r\n\t\t\t                                    where ra = '", RA, "'\r\n\t\t                                    )\r\n                                    order by requeredDoc desc\r\n                                    " };
            string str1 = string.Concat(rA);
            try
            {
                try
                {
                    sqlConnection = new SqlConnection(this.strConexaoBaseDocControl);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(str1, sqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
                    sqlDataReader = sqlCommand.ExecuteReader();
                    documentsStudent = new DocumentsStudent()
                    {
                        RetornoDocument = new List<Document>()
                    };
                    while (sqlDataReader.Read())
                    {
                        List<Document> retornoDocument = documentsStudent.RetornoDocument;
                        Document document = new Document()
                        {
                            Name = Convert.ToString(sqlDataReader["name"]),
                            Cod = Convert.ToString(sqlDataReader["cod"]),
                            TextName = Convert.ToString(sqlDataReader["textName"])
                        };
                        retornoDocument.Add(document);
                    }
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    documentsStudent = null;
                    CreateLogFiles createLogFile = new CreateLogFiles(base.Server.MapPath("Logs/ErrorLog"));
                    createLogFile.ErrorLog(string.Concat("Metodo End Process: GetDocpendentes - ", exception.Message));
                }
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
                if (sqlDataReader != null)
                {
                    sqlDataReader.Close();
                }
            }
            string str2 = "";
            if (documentsStudent != null && documentsStudent.RetornoDocument.Count > 0)
            {
                for (int i = 0; i < documentsStudent.RetornoDocument.Count; i++)
                {
                    str2 = (documentsStudent.RetornoDocument.Count - 1 != i ? string.Concat(str2, documentsStudent.RetornoDocument[i].Cod.ToString().Trim(), ",") : string.Concat(str2, documentsStudent.RetornoDocument[i].Cod.ToString().Trim()));
                }
            }
            try
            {
                try
                {
                    sqlConnection = new SqlConnection(this.strConexaoBaseStudent);
                    sqlConnection.Open();
                    SqlCommand sqlCommand1 = new SqlCommand("dbo.STPECM_Consultas;7", sqlConnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    sqlCommand1.Parameters.Add(new SqlParameter("@RA", RA));
                    sqlCommand1.Parameters.Add(new SqlParameter("@Documentos", str2));
                    sqlCommand1.ExecuteNonQuery();
                    str = "true";
                }
                catch (SqlException sqlException1)
                {
                    SqlException sqlException = sqlException1;
                    CreateLogFiles createLogFile1 = new CreateLogFiles(base.Server.MapPath("Logs/ErrorLog"));
                    createLogFile1.ErrorLog(string.Concat("Metodo Endprocess - SQL:", sqlException.Message));
                    str = "false";
                }
                catch (Exception exception3)
                {
                    Exception exception2 = exception3;
                    CreateLogFiles createLogFile2 = new CreateLogFiles(base.Server.MapPath("Logs/ErrorLog"));
                    createLogFile2.ErrorLog(string.Concat("Metodo Endprocess - EX:", exception2.Message));
                    str = "false";
                }
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
                if (sqlDataReader != null)
                {
                    sqlDataReader.Close();
                }
            }
            return str;
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public bool updateDocument(string Ra, string CodDocument)
        {
            bool flag;

            if (!RequestService.PROD)
            {
                return true;
            }

            SqlConnection sqlConnection = null;
            SqlDataReader sqlDataReader = null;
            string str = this.MonutInsert(Ra, CodDocument);
            try
            {
                try
                {
                    sqlConnection = new SqlConnection(this.strConexaoBaseDocControl);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(str, sqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
                    sqlDataReader = sqlCommand.ExecuteReader();
                    if (sqlDataReader.HasRows)
                    {
                        flag = false;
                        return flag;
                    }
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    CreateLogFiles createLogFile = new CreateLogFiles(base.Server.MapPath("Logs/ErrorLog"));
                    createLogFile.ErrorLog(string.Concat("Metodo updateDocument", exception.Message));
                    flag = false;
                    return flag;
                }
                return true;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
                if (sqlDataReader != null)
                {
                    sqlDataReader.Close();
                }
            }
        }

        #region .: Devplace .:

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

        private List<Aluno> OpenCSV()
        {
            List<Aluno> alunos = new List<Aluno>();
            foreach (var item in Directory.GetFiles(@"C:\ArquivoSE"))
            {
                if (Path.GetExtension(item).ToUpper() == ".CSV")
                {
                    using (var reader = new StreamReader(item))
                    {
                        List<string> listA = new List<string>();
                        List<string> listB = new List<string>();
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var values = line.Split(',');

                            listA.Add(values[0]);
                            listB.Add(values[1]);

                            Aluno al = new Aluno();
                            al.Matricula = values[0];
                            al.Categoria = values[1];
                            alunos.Add(al);

                        }
                    }
                }
            }
            return alunos;
        }

        private string ConstructConsultDoc(string ra, string codcurso, bool requeredDoc)
        {
            string str = (requeredDoc ? "  and requeredDoc = 'true'" : "");
            string[] strArrays = new string[] { "select * from Document where Document.Cod not in\r\n                                        (\r\n\t                                        select distinct DocId \r\n\t                                        from TransactionDoc \r\n\t                                        where ra = '", ra, "'\r\n                                        )\r\n                                        and docForcodCenter  =  ''", str, "union select * from document where docForcodCenter = '", codcurso, "' \r\n                                                and Cod not in\r\n\t\t                                        ( \r\n\t\t\t                                        select distinct DocId \r\n\t\t\t                                        from TransactionDoc \r\n\t\t\t                                        where ra = '", ra, "'\r\n\t\t                                        )\r\n                                        order by requeredDoc desc\r\n                                        " };
            return string.Concat(strArrays);
        }

        private string GetDateLimite()
        {
            string str;
            string str1 = "";
            SqlConnection sqlConnection = null;
            SqlDataReader sqlDataReader = null;
            try
            {
                try
                {
                    sqlConnection = new SqlConnection(this.strConexaoBaseStudent);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand("dbo.STPECM_Consultas;6", sqlConnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    sqlDataReader = sqlCommand.ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        str1 = Convert.ToString(sqlDataReader["DATALimite"]);
                    }
                    str = str1;
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    CreateLogFiles createLogFile = new CreateLogFiles(base.Server.MapPath("Logs/ErrorLog"));
                    createLogFile.ErrorLog(string.Concat("Metodo GetDateLimite", exception.Message));
                    string str2 = "01/01/2011";
                    str1 = str2;
                    str = str2;
                }
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
                if (sqlDataReader != null)
                {
                    sqlDataReader.Close();
                }
            }
            return str;
        }

        private string MonutInsert(string RA, string CodDocument)
        {
            string str = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
            string[] codDocument = new string[] { "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('", CodDocument, "', '", RA, "', '", str, "',  '', '' )" };
            string str1 = string.Concat(codDocument);
            if (CodDocument.Equals("10"))
            {
                string[] rA = new string[] { "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('10', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA);
                string str2 = str1;
                string[] strArrays = new string[] { str2, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('18', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(strArrays);
                string str3 = str1;
                string[] rA1 = new string[] { str3, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('1', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA1);
            }
            if (CodDocument.Equals("1"))
            {
                string[] strArrays1 = new string[] { "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('1', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(strArrays1);
                string str4 = str1;
                string[] rA2 = new string[] { str4, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('10', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA2);
            }
            if (CodDocument.Equals("18"))
            {
                string[] strArrays2 = new string[] { "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('10', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(strArrays2);
                string str5 = str1;
                string[] rA3 = new string[] { str5, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('18', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA3);
            }
            if (CodDocument.Equals("11"))
            {
                string[] strArrays3 = new string[] { "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('11', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(strArrays3);
                string str6 = str1;
                string[] rA4 = new string[] { str6, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('12', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA4);
                string str7 = str1;
                string[] strArrays4 = new string[] { str7, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('6', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(strArrays4);
            }
            if (CodDocument.Equals("12"))
            {
                string[] rA5 = new string[] { "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('12', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA5);
                string str8 = str1;
                string[] strArrays5 = new string[] { str8, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('11', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(strArrays5);
            }
            if (CodDocument.Equals("6"))
            {
                string[] rA6 = new string[] { "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('11', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA6);
                string str9 = str1;
                string[] strArrays6 = new string[] { str9, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('6', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(strArrays6);
            }
            if (CodDocument.Equals("9"))
            {
                string[] rA7 = new string[] { "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('19', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA7);
                string str10 = str1;
                string[] strArrays7 = new string[] { str10, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('9', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(strArrays7);
                string str11 = str1;
                string[] rA8 = new string[] { str11, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('2', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA8);
            }
            if (CodDocument.Equals("19"))
            {
                string[] strArrays8 = new string[] { "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('19', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(strArrays8);
                string str12 = str1;
                string[] rA9 = new string[] { str12, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('9', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA9);
            }
            if (CodDocument.Equals("2"))
            {
                string[] strArrays9 = new string[] { "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('2', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(strArrays9);
                string str13 = str1;
                string[] rA10 = new string[] { str13, "insert into TransactionDoc(DocId, Ra, Date, CodCurso, UserTransaction) values('9', '", RA, "', '", str, "',  '', '' )" };
                str1 = string.Concat(rA10);
            }
            return str1;
        }

        private static void AdicionarArquivof(string metodo)
        {
            contador++;
            File.AppendAllText(@"C:\\LOG\" + Guid.NewGuid(), metodo);
        }

        private static void AdicionarArquivo(string metodo, string id)
        {
            contador++;
            File.AppendAllText(@"C:\\LOG\" + id, metodo + " " + contador);
        }

        private static void AdicionarArquivoSaida(string metodo, string id)
        {
            contador++;
            File.AppendAllText(@"C:\\LOG\" + id, metodo + " " + contador);
        }

        #endregion

        #region .: CLass/Enum :.

        public class RetornoCategoria
        {
            public List<Categoria> Categorias { get; set; }
        }

        public class Categoria
        {
            public string NomeCategoria { get; set; }
            public string CategoriaCod { get; set; }
        }

        public enum TYPOBUSCA
        {
            RA,
            NOME
        }

        #endregion


    }
}
