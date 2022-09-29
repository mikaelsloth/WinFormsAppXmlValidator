namespace WinFormsAppXmlValidator
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string xmlFile;
        private string xsdFile;
        int nooftries = 0;

        private void Button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = "xml";
            openFileDialog1.Filter = "Xml Files|*.xml";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
                textBox2.Text = openFileDialog1.FileName;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = "xsd";
            openFileDialog1.Filter = "Xsd Schemas|*.xsd";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
                textBox3.Text = openFileDialog1.FileName;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            IValidateXml validator = new XDocumentValidation();
            textBox1.Text = validator.Validate(xmlFile, xsdFile);
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            xmlFile = textBox2.Text;
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            xsdFile = textBox3.Text;
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            if (!File.Exists(((TextBox)sender).Text))
            {
                _ = MessageBox.Show("Windows can not find the file specified. \r\n" + "Please check the path and filename entered");
                nooftries++;
                if (nooftries == 3)
                {
                    ((TextBox)sender).Text = "";
                    nooftries = 0;
                }
            }
            else
            {
                nooftries = 0;
            }
        }
    }

    internal interface IValidateXml
    {
        string Validate(string xmlfile, string xsdfile);
    }

    internal abstract class XmlValidation : IValidateXml
    {
        public abstract string Validate(string xmlFile, string xsdfile);

        protected void XsdValidationCallback(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Result += "\r\n" + "WARNING: " + args.Message;
            else if (args.Severity == XmlSeverityType.Error)
                Result += "\r\n" + "ERROR: " + args.Message;
        }

        protected void XmlValidationEventHandler(object sender, ValidationEventArgs e)
        {
            Errors = true;
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Result += "\r\n" + "ERROR: " + e.Message;
                    break;
                case XmlSeverityType.Warning:
                    Result += "\r\n" + "WARNING: " + e.Message;
                    break;
            }

            Result += e.Exception != null ? "Error mesage : " + e.Exception.Message + "\r\n" : "";
        }

        protected string Result { get; set; } = string.Empty;
        protected bool Errors { get; set; } = false;
    }

    internal class XmlDocumentValidation : XmlValidation
    {
        public override string Validate(string xmlFile, string xsdfile)
        {
            try
            {
                if (!string.IsNullOrEmpty(xmlFile))
                {
                    if (!string.IsNullOrEmpty(xsdfile))
                    {
                        // read Xml file
                        XmlDocument xmldoc = new();
                        xmldoc.Load(xmlFile);

                        Result += "Xml file loaded correctly.\r\n";

                        // validate against xsd
                        if (!string.IsNullOrEmpty(xsdfile))
                        {
                            // read schema
                            XmlSchema myschema = XmlSchema.Read(new XmlTextReader(xsdfile), XsdValidationCallback);

                            XmlSchemaSet schemas = new()
                            {
                                XmlResolver = new XmlUrlResolver()
                            };
                            _ = schemas.Add(myschema);
                            schemas.Compile();

                            Result += "Xsd file loaded correctly.\r\n";

                            // validate against it
                            xmldoc.Schemas = schemas;
                            ValidationEventHandler eventHandler = new(XmlValidationEventHandler);
                            xmldoc.Validate(XmlValidationEventHandler);
                            Result += "\r\n" + "The file " + xmldoc + (Errors ? " did not validate" : " validated OK");
                            Errors = false;
                        }
                    }
                    else
                    {
                        _ = MessageBox.Show("Please select a xsd file to validate against");
                    }
                }
                else
                {
                    _ = MessageBox.Show("Please select a XML file");
                }
            }
            catch (Exception ex)
            {
                Result += "\r\n" + "An error of type " + ex.GetType().ToString() + " occured." + "\r\n" + "The error message is: " + ex.Message + "\r\nSource : " + ex.Source;
            }

            return Result;
        }
    }

    internal class XDocumentValidation : XmlValidation
    {
        public override string Validate(string xmlFile, string xsdfile)
        {
            try
            {
                if (!string.IsNullOrEmpty(xmlFile))
                {
                    if (!string.IsNullOrEmpty(xsdfile))
                    {
                        // read Xml file
                        XDocument xmldoc = XDocument.Load(xmlFile);

                        Result += "Xml file loaded correctly.\r\n";

                        // validate against xsd
                        if (!string.IsNullOrEmpty(xsdfile))
                        {
                            // read schema
                            XmlSchema myschema = XmlSchema.Read(new XmlTextReader(xsdfile), XsdValidationCallback);

                            XmlSchemaSet schemas = new()
                            {
                                XmlResolver = new XmlUrlResolver()
                            };
                            _ = schemas.Add(myschema);
                            schemas.Compile();

                            Result += "Xsd file loaded correctly.\r\n";

                            // validate against it
                            ValidationEventHandler eventHandler = new(XmlValidationEventHandler);
                            xmldoc.Validate(schemas, eventHandler);
                            Result += "\r\n" + "The file " + xmldoc.ToString() + (Errors ? " did not validate" : " validated OK");
                            Errors = false;
                        }
                    }
                    else
                    {
                        _ = MessageBox.Show("Please select a xsd file to validate against");
                    }
                }
                else
                {
                    _ = MessageBox.Show("Please select a XML file");
                }
            }
            catch (Exception ex)
            {
                Result += "\r\n" + "An error of type " + ex.GetType().ToString() + " occured." + "\r\n" + "The error message is: " + ex.Message + "\r\nSource : " + ex.Source;
            }

            return Result;
        }
    }
}
