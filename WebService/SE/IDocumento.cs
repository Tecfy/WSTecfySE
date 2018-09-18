namespace WebService.SE
{
    interface IDocumento<t>
    {
        bool InserirDocumento(t Indice);

        bool UploadDocumento(t Indice, string id);
    }
}
