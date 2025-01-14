using System;
using System.Globalization;
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
                    new XElement("points"),
                    new XElement("lines")
                );

                int increment = 1;

                // ������ ������������ �������� ���������
                double maxX = oldXml.Descendants("SchemaPoint")
                                    .Select(p => double.TryParse(p.Attribute("X")?.Value, out var x) ? x : 0)
                                    .Max();

                double maxY = oldXml.Descendants("SchemaPoint")
                                    .Select(p => double.TryParse(p.Attribute("Y")?.Value, out var y) ? y : 0)
                                    .Max();

                // �������� ������������ ����������
                double maxCoord = Math.Max(maxX, maxY);

                // ���������� �������
                double scale = 1;
                if (maxCoord >= 10000)
                {
                    scale = 100;
                }
                else if (maxCoord >= 1000)
                {
                    scale = 10;
                }
                //else if (maxCoord >= 100)
                //{
                //    scale = 10;
                //}

                // ���������� ��� <SchemaPoint> � ������ XML � ��������� �� � ����� ������
                foreach (var schemaPoint in oldXml.Descendants("SchemaPoint"))
                {
                    string pointId = schemaPoint.Attribute("Id")?.Value;
                    string x = schemaPoint.Attribute("X")?.Value;
                    string y = schemaPoint.Attribute("Y")?.Value;

                    // ����������� ���������� � �������������� ���������� �������� � ��������� �� ����� �����
                    string shortX = !string.IsNullOrEmpty(x) && double.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out var xCoord)
                        ? (Math.Round(xCoord / scale)).ToString("0")  // ��������� � ��������� ��� ����� �����
                        : x;

                    string shortY = !string.IsNullOrEmpty(y) && double.TryParse(y, NumberStyles.Any, CultureInfo.InvariantCulture, out var yCoord)
                        ? (Math.Round(yCoord / scale)).ToString("0")  // ��������� � ��������� ��� ����� �����
                        : y;

                    // ��������� ������ (��������, ������ 2 ������� Id)
                    string number = pointId?.Substring(0, 2);

                    // ������� ����� � ����� XML
                    newXml.Element("points").Add(
                        new XElement("point",
                            new XAttribute("id", increment.ToString()), // ���������� ��������� ��� ID
                            new XAttribute("X", Convert.ToInt32(shortX)), // ����������� � ����� �����
                            new XAttribute("Y", Convert.ToInt32(shortY)), // ����������� � ����� �����
                            new XElement("pointInfo",
                                new XAttribute("number", number),
                                new XAttribute("type", "2"),
                                new XAttribute("textPosition", "3"),
                                new XAttribute("gorlovina", "")
                            )
                        )
                    );

                    increment++;
                }

                // ���� ������������ ������� <Sections> � ���������� ��������� �����
                var sectionsElement = oldXml.Element("Sections");

                if (sectionsElement != null)
                {

                    // �������� ���������
                    increment = 1;

                    // ���������� ��� <Section> ������ <Sections>
                    foreach (var section in sectionsElement.Descendants("Section"))
                    {
                        var startElement = section.Element("Start");
                        var endElement = section.Element("End");

                        if (startElement != null && endElement != null)
                        {
                            string startId = startElement.Attribute("Id")?.Value;
                            string endId = endElement.Attribute("Id")?.Value;

                            if (startId == null || endId == null)
                            {
                                continue;
                            }

                            var startPoint = oldXml.Descendants("SchemaPoint").FirstOrDefault(p => p.Attribute("Id")?.Value == startId);
                            var endPoint = oldXml.Descendants("SchemaPoint").FirstOrDefault(p => p.Attribute("Id")?.Value == endId);

                            if (startPoint != null && endPoint != null)
                            {
                                string startX = startPoint.Attribute("X")?.Value;
                                string startY = startPoint.Attribute("Y")?.Value;
                                string endX = endPoint.Attribute("X")?.Value;
                                string endY = endPoint.Attribute("Y")?.Value;

                                // ����������� ���������� � ������ �������� � ��������� �� ����� �����
                                string shortStartX = !string.IsNullOrEmpty(startX) && double.TryParse(startX, NumberStyles.Any, CultureInfo.InvariantCulture, out var sX)
                                    ? (Math.Round(sX / scale)).ToString("0")  // ��������� � ��������� ��� ����� �����
                                    : startX;

                                string shortStartY = !string.IsNullOrEmpty(startY) && double.TryParse(startY, NumberStyles.Any, CultureInfo.InvariantCulture, out var sY)
                                    ? (Math.Round(sY / scale)).ToString("0")  // ��������� � ��������� ��� ����� �����
                                    : startY;

                                string shortEndX = !string.IsNullOrEmpty(endX) && double.TryParse(endX, NumberStyles.Any, CultureInfo.InvariantCulture, out var eX)
                                    ? (Math.Round(eX / scale)).ToString("0")  // ��������� � ��������� ��� ����� �����
                                    : endX;

                                string shortEndY = !string.IsNullOrEmpty(endY) && double.TryParse(endY, NumberStyles.Any, CultureInfo.InvariantCulture, out var eY)
                                    ? (Math.Round(eY / scale)).ToString("0")  // ��������� � ��������� ��� ����� �����
                                    : endY;

                                string length = section.Attribute("Length")?.Value ?? "0"; // ���� ����� �� ������, ����� 0


                                // ��������� �������� IsMain
                                bool isMain = section.Attribute("IsMain")?.Value == "true";

                                // ����������� �������� name � specialization
                                string lineName = section.Attribute("Name")?.Value ?? string.Empty;
                                string specialization = isMain ? "15" : string.IsNullOrEmpty(lineName) ? "17" : "2";

                                // ���������� �������� kind
                                int kind = shortStartY == shortEndY ? 0 :  // ��������������
                                           shortStartX == shortEndX ? 1 :  // ������������
                                           double.Parse(shortStartY) > double.Parse(shortEndY) ? 2 : 3; // ���������

                                newXml.Element("lines").Add(
                                    new XElement("line",
                                        new XAttribute("id", increment.ToString()), // ���������� ��������� ��� �����
                                        new XAttribute("sX", Convert.ToInt32(shortStartX)), // ����������� � ����� �����
                                        new XAttribute("sY", Convert.ToInt32(shortStartY)), // ����������� � ����� �����
                                        new XAttribute("eX", Convert.ToInt32(shortEndX)), // ����������� � ����� �����
                                        new XAttribute("eY", Convert.ToInt32(shortEndY)), // ����������� � ����� �����
                                        new XAttribute("kind", kind),
                                        new XElement("lineInfo",
                                            new XAttribute("type", "1"),
                                            new XAttribute("name", lineName),
                                            new XAttribute("specialization", specialization),
                                            new XAttribute("lengthInVagons", "0"),
                                            new XAttribute("length", length),
                                            new XAttribute("park", ""),
                                            new XAttribute("lengthLeft", "0"),
                                            new XAttribute("nameLeft", ""),
                                            new XAttribute("signalLeft", "3"),
                                            new XAttribute("lengthRight", "0"),
                                            new XAttribute("nameRight", ""),
                                            new XAttribute("signalRight", "3")
                                        )
                                    )
                                );

                                increment++;
                            }
                        }
                    }


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
