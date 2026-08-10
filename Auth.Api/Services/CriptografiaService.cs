using System.Security.Cryptography;
using System.Text;

namespace Auth.Api.Services
{
    public class CriptografiaService
    {
        private const string Prefijo = "AESGCM";
        private const int TamanoNonce = 12;
        private const int TamanoTag = 16;

        private readonly byte[] _clave;

        public CriptografiaService(IConfiguration configuration)
        {
            string clave =
                configuration["Encryption:Key"]
                ?? throw new InvalidOperationException(
                    "No se encontró Encryption:Key.");

            if (clave.Length != 32)
            {
                throw new InvalidOperationException(
                    "Encryption:Key debe tener exactamente 32 caracteres.");
            }

            _clave = Encoding.UTF8.GetBytes(clave);
        }

        public bool Verificar(
            string passwordPlano,
            string passwordCifrado)
        {
            if (string.IsNullOrWhiteSpace(passwordPlano) ||
                string.IsNullOrWhiteSpace(passwordCifrado))
            {
                return false;
            }

            try
            {
                string[] partes = passwordCifrado.Split(':');

                if (partes.Length != 4 ||
                    partes[0] != Prefijo)
                {
                    return false;
                }

                byte[] nonce =
                    Convert.FromBase64String(partes[1]);

                byte[] tag =
                    Convert.FromBase64String(partes[2]);

                byte[] cifrado =
                    Convert.FromBase64String(partes[3]);

                if (nonce.Length != TamanoNonce ||
                    tag.Length != TamanoTag)
                {
                    return false;
                }

                byte[] textoPlano =
                    new byte[cifrado.Length];

                using var aes = new AesGcm(
                    _clave,
                    TamanoTag
                );

                aes.Decrypt(
                    nonce,
                    cifrado,
                    tag,
                    textoPlano
                );

                byte[] passwordIngresado =
                    Encoding.UTF8.GetBytes(passwordPlano);

                return CryptographicOperations.FixedTimeEquals(
                    textoPlano,
                    passwordIngresado
                );
            }
            catch
            {
                return false;
            }
        }
    }
}