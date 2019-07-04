using System.ComponentModel;

namespace WebService.Models.Common.Enum
{
    public enum EAttribute
    {
        [Description("SER_cad_Cpf")]
        SER_cad_Cpf = 1,
        [Description("SER_cad_RG")]
        SER_cad_RG = 2,
        [Description("SER_cad_Curso")]
        SER_cad_Curso = 3,
        [Description("SER_cad_Matricula")]
        SER_cad_Matricula = 4,
        [Description("SER_cad_NomedoAluno")]
        SER_cad_NomedoAluno = 5,
        [Description("SER_cad_SituacaoAluno")]
        SER_cad_SituacaoAluno = 6,
        [Description("SER_cad_Unidade")]
        SER_cad_Unidade = 7,
        [Description("SER_cad_unidades")]
        SER_cad_unidades = 8,
        [Description("SER_EstagioDoc")]
        SER_EstagioDoc = 9,
        [Description("SER_Input_Compl")]
        SER_Input_Compl = 10,
        [Description("SER_Input_DataRef")]
        SER_Input_DataRef = 11,
        [Description("SER_Input_Data_Vencto")]
        SER_Input_Data_Vencto = 12,
        [Description("SER_Input_NumDoc")]
        SER_Input_NumDoc = 13,
        [Description("SER_Input_Obs")]
        SER_Input_Obs = 14,
        [Description("SER_Operador")]
        SER_Operador = 15,
        [Description("SER_Paginas")]
        SER_Paginas = 16,
        [Description("SER_SituacaoDoc")]
        SER_SituacaoDoc = 17,
        [Description("SER_cad_cod_unidade")]
        SER_cad_cod_unidade = 18,

        [Description("MFP_Data_Job")]
        MFP_Data_Job = 19,
        [Description("MFP_Status")]
        MFP_Status = 20,
        [Description("MFP_Categoria")]
        MFP_Categoria = 21,
        [Description("MFP_Usuario")]
        MFP_Usuario = 22,

        [Description("SER_id_classificador")]
        SER_id_classificador = 23,
        [Description("SER_id_recortador")]
        SER_id_recortador = 24,
        [Description("SER_nome_classificador")]
        SER_nome_classificador = 25,
        [Description("SER_nome_recortador")]
        SER_nome_recortador = 26,

        [Description("SER_classificacao_data")]
        SER_classificacao_data = 27,
        [Description("SER_classificacao_hora")]
        SER_classificacao_hora = 28,
        [Description("SER_recorte_data")]
        SER_recorte_data = 29,
        [Description("SER_recorte_hora")]
        SER_recorte_hora = 30,

        [Description("tfyacess_cappservice")]
        tfyacess_cappservice = 31,
        [Description("tfyacess_classifica")]
        tfyacess_classifica = 32,
        [Description("tfyacess_grupocappservice")]
        tfyacess_grupocappservice = 33,
        [Description("tfyacess_nomeusuario")]
        tfyacess_nomeusuario = 34,
        [Description("tfyacess_recorte")]
        tfyacess_recorte = 35,
        [Description("tfyacess_status")]
        tfyacess_status = 36,
        [Description("tfyacess_userid")]
        tfyacess_userid = 37,
        [Description("tfyacess_dtsync")]
        tfyacess_dtsync = 38,
        [Description("tfyacess_horasync")]
        tfyacess_horasync = 39,
        [Description("tfyacess_digitalizaMPF")]
        tfyacess_digitalizaMPF = 40,
        
    }
}