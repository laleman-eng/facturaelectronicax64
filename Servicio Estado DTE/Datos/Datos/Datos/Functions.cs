using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Functions
{
    public class TFunctions
    {
        public String Encriptar(String _cadenaAencriptar)
        {
            System.String sresult;
            System.Byte[] encryted;

            sresult = System.String.Empty;
            encryted = System.Text.Encoding.Unicode.GetBytes(_cadenaAencriptar);
            sresult = Convert.ToBase64String(encryted);
            return sresult;
        }//fin Encriptar


        public String DesEncriptar(String _cadenaAdesencriptar)
        {
            String sresult;
            System.Byte[] decryted;

            sresult = System.String.Empty;
            decryted = Convert.FromBase64String(_cadenaAdesencriptar);
            //result = System       .Text.Encoding.Unicode.GetString(decryted, 0, decryted.ToArray().Length);
            sresult = System.Text.Encoding.Unicode.GetString(decryted);
            return sresult;
        }
    }
}
