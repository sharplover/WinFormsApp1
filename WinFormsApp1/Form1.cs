using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // ���������� ������ ������ ��������� XML
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtSourceFile.Text = openFileDialog.FileName;
            }
        }

        // ���������� ������ ������ ����� ��� ���������� ������ �����
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtDestinationFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        // ���������� ������ "���������" ��� �������������� XML
        private void btnExecute_Click(object sender, EventArgs e)
        {
            string sourceFile = txtSourceFile.Text;
            string destinationFolder = txtDestinationFolder.Text;

            if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(destinationFolder))
            {
                MessageBox.Show("����������, �������� �������� ���� � ����� ��� ����������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // ��������� ������ XML
                XElement oldXml = XElement.Load(sourceFile);
                XElement newXml = new XElement("StationMap",
                    new XAttribute("Step", "13"),
                    new XAttribute("Width", "142"),
                    new XAttribute("Height", "36"),
                    new XElement("points")
                );

                // ���������� ��� ����������
                int increment = 1;

                // ���������� ��� <SchemaPoint> � ������ XML � ��������� �� � ����� ������
                foreach (var schemaPoint in oldXml.Descendants("SchemaPoint"))
                {
                    string pointId = schemaPoint.Attribute("Id").Value;
                    string x = schemaPoint.Attribute("X").Value;
                    string y = schemaPoint.Attribute("Y").Value;

                    // ����� ������ ������ ��� ����� �� ��������� X � Y
                    string xTruncated = x.Length > 2 ? x.Substring(0, 2) : x;
                    string yTruncated = y.Length > 2 ? y.Substring(0, 2) : y;

                    // ��������� ������ (��������, ������ 2 ������� Id)
                    string number = pointId.Substring(0, 2);

                    // ������� ����� � ����� XML, ��������� ��������� ��� �������� id
                    newXml.Element("points").Add(
                        new XElement("point",
                            new XAttribute("id", increment.ToString()), // ���������� ���������
                            new XAttribute("X", xTruncated),
                            new XAttribute("Y", yTruncated),
                            new XElement("pointInfo",
                                new XAttribute("number", number),
                                new XAttribute("type", "2"),
                                new XAttribute("textPosition", "3"),
                                new XAttribute("gorlovina", "")
                            )
                        )
                    );

                    // ����������� ���������
                    increment++;
                }

                // ��������� ����� XML
                string destinationFile = Path.Combine(destinationFolder, "�����.xml");
                newXml.Save(destinationFile);

                MessageBox.Show($"�������������� ���������. ���� �������� �: {destinationFile}", "�����", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��� ��������� ������: {ex.Message}", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
