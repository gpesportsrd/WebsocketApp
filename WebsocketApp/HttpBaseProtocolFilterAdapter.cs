using System.Net.Http;

public class HttpClientHandlerAdapter : HttpClientHandler
{
    public HttpClientHandlerAdapter()
    {
        // Ignorar errores de certificado SSL
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
    }
}