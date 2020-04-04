using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Datos
{
    public partial class Inicio : Form
    {
        public Boolean Paso = false;
        public static Form1 MenuP = new Form1();

        public Inicio()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(this.Inicio_FormClosing);
        }

        private void Inicio_Load(object sender, EventArgs e)
        {

        }

        private void Inicio_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Paso == false)
            {
                Application.Exit();
            }
        }

        private void Bt_Salir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Bt_Ingresar_Click(object sender, EventArgs e)
        {
            string str;
            DataTable dt;
            if (Tx_User.Text == "")
            {
                str = "Debe ingresar usuario";
                MessageBox.Show(str);
            }
            else if (Tx_Password.Text == "")
                MessageBox.Show("Debe ingresar password");
            else
            {
                if ("admin" == Tx_User.Text.ToLower())
                {
                    if ("NLn/O75YwCWsDBYz3mgV0A==" == Encriptar(Tx_Password.Text))
                    {
                        Paso = true;
                        MenuP.Show();
                        this.Hide();
                    }
                    else
                        MessageBox.Show("Password incorrecta");
                }
                else
                    MessageBox.Show("Usuario incorrecto");
            }
        }

        private string Encriptar(string texto)
        {
            String key = "Factura Electronica VK";
            try
            {
                //arreglo de bytes donde guardaremos la llave
                byte[] keyArray;
                //arreglo de bytes donde guardaremos el texto
                //que vamos a encriptar
                byte[] Arreglo_a_Cifrar =
                UTF8Encoding.UTF8.GetBytes(texto);

                //se utilizan las clases de encriptación
                //provistas por el Framework
                //Algoritmo MD5
                MD5CryptoServiceProvider hashmd5 =
                new MD5CryptoServiceProvider();
                //se guarda la llave para que se le realice
                //hashing
                keyArray = hashmd5.ComputeHash(
                UTF8Encoding.UTF8.GetBytes(key));

                hashmd5.Clear();

                //Algoritmo 3DAS
                TripleDESCryptoServiceProvider tdes =
                new TripleDESCryptoServiceProvider();

                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                //se empieza con la transformación de la cadena
                ICryptoTransform cTransform =
                tdes.CreateEncryptor();

                //arreglo de bytes donde se guarda la
                //cadena cifrada
                byte[] ArrayResultado =
                cTransform.TransformFinalBlock(Arreglo_a_Cifrar,
                0, Arreglo_a_Cifrar.Length);

                tdes.Clear();

                //se regresa el resultado en forma de una cadena
                return Convert.ToBase64String(ArrayResultado, 0, ArrayResultado.Length);
            }
            catch (Exception w)
            {
                MessageBox.Show("Error Encriptar:" + w.Message + ", TRACE " + w.StackTrace);
                return "";
            }
        }
    }
}
