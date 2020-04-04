using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Datos.Functions;

namespace Datos
{
    public partial class Form1 : Form
    {
        public XmlDocument xDoc;
        public XmlNodeList Configuracion;
        public XmlNodeList lista;
        public String sPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public TFunctions Func = new TFunctions();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void LeerConfigTexto(String Arch)
        {
            try
            {
                string contenido = String.Empty;
                contenido = File.ReadAllText(Arch);
                //string[] lineas = contenido.Split(Environment.NewLine);
                //foreach (string linea in lineas)
                //{
                //    Console.WriteLine(linea);
                //}
                contenido = Func.DesEncriptar(contenido);
                xDoc = new XmlDocument();
                xDoc.LoadXml(contenido);
                Configuracion = xDoc.GetElementsByTagName("Configuracion");
                lista = ((XmlElement)Configuracion[0]).GetElementsByTagName("ServidorSAP");
                foreach (XmlElement nodo in lista)
                {
                    var nArchivos = nodo.GetElementsByTagName("Servidor");
                    txServidor.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("ServLicencia");
                    txServidorLic.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("UsuarioSAP");
                    txUsuarioSAP.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("PasswordSAP");
                    txPassSAP.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("SQL");
                    txVersionSQL.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("UsuarioSQL");
                    txUsuarioBase.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("PasswordSQL");
                    txPassBase.Text = (String)(nArchivos[0].InnerText);
                }

                lista = ((XmlElement)Configuracion[0]).GetElementsByTagName("SistemaSAP");
                foreach (XmlElement nodo in lista)
                {
                    var nArchivos = nodo.GetElementsByTagName("SAP");
                    txSistemaSAP.Text = (String)(nArchivos[0].InnerText);
                }

                lista = ((XmlElement)Configuracion[0]).GetElementsByTagName("EasyDoc");
                foreach (XmlElement nodo in lista)
                {
                    var nArchivos = nodo.GetElementsByTagName("Procesa21");
                    cbxOP21.Text = (String)(nArchivos[0].InnerText);
                    nArchivos = nodo.GetElementsByTagName("OP21");
                    txOP21.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("Procesa27");
                    cbxOP27.Text = (String)(nArchivos[0].InnerText);
                    nArchivos = nodo.GetElementsByTagName("OP27");
                    txOP27.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("Procesa28");
                    cbxOP28.Text = (String)(nArchivos[0].InnerText);
                    nArchivos = nodo.GetElementsByTagName("OP28");
                    txOP28.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("Procesa29");
                    cbxOP29.Text = (String)(nArchivos[0].InnerText);
                    nArchivos = nodo.GetElementsByTagName("OP29");
                    txOP29.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("Procesa30");
                    cbxOP30.Text = (String)(nArchivos[0].InnerText);
                    nArchivos = nodo.GetElementsByTagName("OP30");
                    txOP30.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("Procesa31");
                    cbxOP31.Text = (String)(nArchivos[0].InnerText);
                    nArchivos = nodo.GetElementsByTagName("OP31");
                    txOP31.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("Procesa36");
                    cbxOP36.Text = (String)(nArchivos[0].InnerText);
                    nArchivos = nodo.GetElementsByTagName("OP36");
                    txOP36.Text = (String)(nArchivos[0].InnerText);
                }

                lista = ((XmlElement)Configuracion[0]).GetElementsByTagName("Mail");
                foreach (XmlElement nodo in lista)
                {
                    var nArchivos = nodo.GetElementsByTagName("EnviarMail");
                    cbxMailEnvio.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("HoraEnvio1");
                    txHoraEnvio1.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("HoraEnvio2");
                    txHoraEnvio2.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("MailFrom");
                    txMailEnvia.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("MailSmtpHost");
                    txMailSmtp.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("MailUser");
                    txMailUsuario.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("MailPass");
                    txMailPassword.Text = (String)(nArchivos[0].InnerText);

                    nArchivos = nodo.GetElementsByTagName("Puerto");
                    txMailPuerto.Text = (String)(nArchivos[0].InnerText);
                }

                lista = ((XmlElement)Configuracion[0]).GetElementsByTagName("BaseSAP");
                foreach (XmlElement nodo in lista)
                {
                    //var nArchivos = nodo.GetElementsByTagName("BaseSAP");
                    //lbxBases.Items.Add((String)(nArchivos[0].InnerText));
                    lbxBases.Items.Add((String)(nodo.InnerText));
                }
                btnGuardar.Enabled = true;
            }
            catch (Exception w)
            {
                MessageBox.Show("Error : " + w.Message + " ** Trace: " + w.StackTrace);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Limpiar();
            OpenFileDialog open = new OpenFileDialog();
            //le agregamos un filtro para los tipos de archivos a leer en este caso XML.
            open.Filter = "xml files (*.xml)|*.xml";
            //cuando presionamos sobre el botón validamos que el resultado esperado sea la selección de un archivo.
            if (open.ShowDialog() == DialogResult.OK && open.ToString() != " ")
            {
                var Arch = open.FileName;
                var contenido = String.Empty;
                contenido = File.ReadAllText(Arch);
                contenido = Func.Encriptar(contenido);
                StreamWriter escrito = File.CreateText(sPath + "\\Config.txt");
                escrito.Write(contenido.ToString());
                escrito.Flush();
                escrito.Close();
                LeerConfigTexto(sPath + "\\Config.txt");
            }
            else
                MessageBox.Show("Debe seleccionar un archivo");

        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            XDocument miXML = null;
            XmlDocument oXml;
            miXML = null;
            XElement xNodo = null;

            try
            {
                miXML = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                                        new XElement("Configuracion",
                                            new XElement("ServidorSAP",
                                                new XElement("Servidor", txServidor.Text),
                                                new XElement("ServLicencia", txServidorLic.Text),
                                                new XElement("UsuarioSAP", txUsuarioSAP.Text),
                                                new XElement("PasswordSAP", txPassSAP.Text),
                                                new XElement("SQL", txVersionSQL.Text),
                                                new XElement("UsuarioSQL", txUsuarioBase.Text),
                                                new XElement("PasswordSQL", txPassBase.Text)
                                                ),
                                            new XElement("SistemaSAP",
                                                new XElement("SAP", txSistemaSAP.Text)
                                                ),
                                            new XElement("EasyDoc",
                                                new XElement("Procesa21", cbxOP21.Text),
                                                new XElement("OP21", txOP21.Text),
                                                new XElement("Procesa27", cbxOP27.Text),
                                                new XElement("OP27", txOP27.Text),
                                                new XElement("Procesa28", cbxOP28.Text),
                                                new XElement("OP28", txOP28.Text),
                                                new XElement("Procesa29", cbxOP29.Text),
                                                new XElement("OP29", txOP29.Text),
                                                new XElement("Procesa30", cbxOP30.Text),
                                                new XElement("OP30", txOP30.Text),
                                                new XElement("Procesa31", cbxOP31.Text),
                                                new XElement("OP31", txOP31.Text),
                                                new XElement("Procesa36", cbxOP36.Text),
                                                new XElement("OP36", txOP36.Text)
                                                ),
                                            new XElement("Mail",
                                                new XElement("EnviarMail", cbxMailEnvio.Text),
                                                new XElement("HoraEnvio1", txHoraEnvio1.Text),
                                                new XElement("HoraEnvio2", txHoraEnvio2.Text),
                                                new XElement("MailFrom", txMailEnvia.Text),
                                                new XElement("MailSmtpHost", txMailSmtp.Text),
                                                new XElement("MailUser", txMailUsuario.Text),
                                                new XElement("MailPass", txMailPassword.Text),
                                                new XElement("Puerto", txMailPuerto.Text)
                                                )
                                            ));

                string text = "";
                Int32 i = 0;
                foreach (var item in lbxBases.Items)
                {
                    if (i == 0)
                    {
                        if (item.ToString().Trim() != "")
                        {
                            xNodo = new XElement("BasesSAP",
                                                    new XElement("BaseSAP", item.ToString().Trim())
                                                    );
                            miXML.Descendants("Configuracion").LastOrDefault().Add(xNodo);
                        }
                    }
                    else
                    {
                        if (item.ToString().Trim() != "")
                        {
                            xNodo = new XElement("BaseSAP", item.ToString().Trim());
                            miXML.Descendants("BasesSAP").LastOrDefault().Add(xNodo);
                        }
                    }
                    i++;
                }

                oXml = new XmlDocument();
                using (var xmlReader = miXML.CreateReader())
                {
                    oXml.Load(xmlReader);
                }
                if (btnGuardar.Text == "Crear")
                {
                    var s = Func.Encriptar(oXml.InnerXml);
                    File.WriteAllText(sPath + "\\Config.txt", s);
                    btnGuardar.Text = "Guardar";
                }
                else
                {
                    var s = Func.Encriptar(oXml.InnerXml);
                    File.WriteAllText(sPath + "\\Config.txt", s);
                }

                MessageBox.Show("Datos registrados satisfactoriamente");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (txAgregarBase.Text == "")
                MessageBox.Show("Debe ingresar nombre Base de Datos que desea agregar");
            else
            {
                lbxBases.Items.Add(txAgregarBase.Text);
                txAgregarBase.Text = "";
            }
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            Object item = lbxBases.SelectedItem;
            if (item == null)
                MessageBox.Show("Debe seleccionar una base");
            else
            {
                var UsuarioSeleccionado = item.ToString();
                //MessageBox.Show(UsuarioSeleccionado);
                lbxBases.Items.Remove(item);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Limpiar();
            OpenFileDialog open = new OpenFileDialog();
            //le agregamos un filtro para los tipos de archivos a leer en este caso XML.
            open.Filter = "txt files (*.txt)|*.txt";
            //cuando presionamos sobre el botón validamos que el resultado esperado sea la selección de un archivo.
            if (open.ShowDialog() == DialogResult.OK && open.ToString() != " ")
            {
                var Arch = open.FileName;
                LeerConfigTexto(Arch);
            }
            else
                MessageBox.Show("Debe seleccionar un archivo");
        }

        private void Limpiar()
        {
            txServidor.Text = "";
            txServidorLic.Text = "";
            txUsuarioSAP.Text = "";
            txPassSAP.Text = "";
            txVersionSQL.Text = "";
            txUsuarioBase.Text = "";
            txPassBase.Text = "";
            txSistemaSAP.Text = "";
            cbxOP21.Text = "No";
            txOP21.Text = "";
            cbxOP27.Text = "No";
            txOP27.Text = "";
            cbxOP28.Text = "No";
            txOP28.Text = "";
            cbxOP29.Text = "No";
            txOP29.Text = "";
            cbxOP30.Text = "No";
            txOP30.Text = "";
            cbxOP31.Text = "No";
            txOP31.Text = "";
            cbxOP36.Text = "No";
            txOP36.Text = "";
            txMailEnvia.Text = "";
            txMailSmtp.Text = "";
            txMailUsuario.Text = "";
            txMailPassword.Text = "";
            txMailPuerto.Text = "";
            cbxMailEnvio.Text = "No";
            txHoraEnvio1.Text = "";
            txHoraEnvio2.Text = "";
            txAgregarBase.Text = "";
            lbxBases.Items.Clear();
        }
    }
}
