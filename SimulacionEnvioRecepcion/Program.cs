using System;
using System.Text;
using System.Security.Cryptography;
using ClaveSimetricaClass;
using ClaveAsimetricaClass;

namespace SimuladorEnvioRecepcion
{
    class Program
    {   
        static string UserName = "";
        static string SecurePass = "";
        static ClaveAsimetrica Emisor = new ClaveAsimetrica();
        static ClaveAsimetrica Receptor = new ClaveAsimetrica();
        static ClaveSimetrica ClaveSimetricaEmisor = new ClaveSimetrica();
        static ClaveSimetrica ClaveSimetricaReceptor = new ClaveSimetrica();

        static string TextoAEnviar = "Me he dado cuenta que incluso las personas que dicen que todo está predestinado y que no podemos hacer nada para cambiar nuestro destino igual miran antes de cruzar la calle. Stephen Hawking.";

        static byte[] Firma = Array.Empty<byte>();
        static byte[] ClaveSimetricaKeyCifrada = Array.Empty<byte>();
        static byte[] ClaveSimetricaIVCifrada = Array.Empty<byte>();
        static byte[] TextoCifrado = Array.Empty<byte>();

        static void Main(string[] args)
        {
            /****PARTE 1****/
            //Login / Registro
            Console.WriteLine ("¿Deseas registrarte? (S/N)");
            string registro = Console.ReadLine ();

            if (registro == "S")
            {
                //Realizar registro del cliente
                Registro();                
            }

            //Realizar login
            bool login = Login();
            /***FIN PARTE 1***/

            if (login)
            {              
                // Convertir el mensaje original a bytes    
                byte[] TextoAEnviar_Bytes = Encoding.UTF8.GetBytes(TextoAEnviar); 
                Console.WriteLine("Texto a enviar bytes: {0}", BytesToStringHex(TextoAEnviar_Bytes));    
                
                //LADO EMISOR

                // 1. Firmar mensaje
                Firma = Emisor.FirmarMensaje(Encoding.UTF8.GetBytes(TextoAEnviar));

                // 2. Cifrar mensaje con clave simétrica
                TextoCifrado = ClaveSimetricaEmisor.CifrarMensaje(TextoAEnviar);

                // 3. Cifrar clave simétrica (Key + IV) con clave pública del receptor
                ClaveSimetricaKeyCifrada = Receptor.CifrarMensaje(ClaveSimetricaEmisor.Key);
                ClaveSimetricaIVCifrada = Receptor.CifrarMensaje(ClaveSimetricaEmisor.IV);

                // Mostrar datos que serían "enviados"
                Console.WriteLine("\n===== DATOS ENVIADOS =====");
                Console.WriteLine("Firma: {0}", BytesToStringHex(Firma));
                Console.WriteLine("Texto cifrado: {0}", BytesToStringHex(TextoCifrado));
                Console.WriteLine("Clave simétrica cifrada (Key): {0}", BytesToStringHex(ClaveSimetricaKeyCifrada));
                Console.WriteLine("Clave simétrica cifrada (IV): {0}", BytesToStringHex(ClaveSimetricaIVCifrada));

                //LADO RECEPTOR

                // 4. Descifrar clave simétrica
                ClaveSimetricaReceptor.Key = Receptor.DescifrarMensaje(ClaveSimetricaKeyCifrada);
                ClaveSimetricaReceptor.IV = Receptor.DescifrarMensaje(ClaveSimetricaIVCifrada);

                // 5. Descifrar mensaje
                string MensajeDescifrado = ClaveSimetricaReceptor.DescifrarMensaje(TextoCifrado);

                // 6. Validar firma
                bool firmaCorrecta = Emisor.ComprobarFirma(Firma, Encoding.UTF8.GetBytes(MensajeDescifrado));

                Console.WriteLine("\n===== RECEPCIÓN =====");
                if (firmaCorrecta)
                {
                    Console.WriteLine("Firma válida ✅");
                    Console.WriteLine("Mensaje descifrado correctamente:");
                    Console.WriteLine(MensajeDescifrado);
                }
                else
                {
                    Console.WriteLine("❌ Firma no válida. El mensaje ha sido modificado.");
                }
            }
        }

        public static void Registro()
        {
            Console.WriteLine ("Indica tu nombre de usuario:");
            UserName = Console.ReadLine();

            Console.WriteLine ("Indica tu password:");
            string passwordRegister = Console.ReadLine();

            /***PARTE 1***/
            // Guardar password de manera segura
            SecurePass = GetSha256(passwordRegister);
            Console.WriteLine("Registro completado con seguridad.\n");
        }

        public static bool Login()
        {
            bool auxlogin = false;
            do
            {
                Console.WriteLine ("Acceso a la aplicación");
                Console.WriteLine ("Usuario: ");
                string userName = Console.ReadLine();

                Console.WriteLine ("Password: ");
                string Password = Console.ReadLine();

                /***PARTE 1***/
                // Comparar password hasheado
                string hashedLogin = GetSha256(Password);

                if (userName == UserName && hashedLogin == SecurePass)
                {
                    Console.WriteLine("Login correcto.\n");
                    auxlogin = true;
                }
                else
                {
                    Console.WriteLine("Login incorrecto. Inténtalo de nuevo.\n");
                }
            } while (!auxlogin);

            return auxlogin;
        }

        static string BytesToStringHex (byte[] result)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in result)
                stringBuilder.AppendFormat("{0:x2}", b);
            return stringBuilder.ToString();
        }

        public static string GetSha256(string? input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input ?? ""));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
