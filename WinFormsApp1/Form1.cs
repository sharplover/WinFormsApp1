using System;
using System.Diagnostics;
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

        // ���������� ������ "���������" ��� �������������� XML
        private void btnExecute_Click(object sender, EventArgs e)
        {
            string sourceFile = txtSourceFile.Text;

            if (string.IsNullOrEmpty(sourceFile))
            {
                MessageBox.Show("����������, �������� �������� ����.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // ��������� ������ XML
                XDocument oldXml = XDocument.Load(sourceFile);
                XElement newXml = new XElement("StationMap",
                    new XAttribute("Step", "13"),
                    new XAttribute("Width", "142"),
                    new XAttribute("Height", "36"),
                    new XElement("points"),
                    new XElement("lines"),
                    new XElement("textCollection")
                );


                // string title = oldXml.Element("Schema")?.Attribute("Title")?.Value ?? "";
                string title = "";

                XNamespace ns = oldXml.Root.GetDefaultNamespace(); // �������� ������������ ���� �����
                XElement schemaElement = oldXml.Descendants(ns + "Schema").FirstOrDefault();
                if (schemaElement != null)
                {
                    title = schemaElement.Attribute("Title")?.Value ?? "";
                    
                }
                else
                {
                    Debug.WriteLine("Element 'Schema' not found.");
                }

                Debug.WriteLine($"Title: {title}");
                Debug.WriteLine(2 + 2);

                newXml.Element("textCollection").Add(
                   new XElement("text",
                   new XAttribute("location_X", "5"),
                   new XAttribute("location_Y", "2"),
                   new XAttribute("size_W", "8"),
                   new XAttribute("size_H", "2"),
                   new XAttribute("text", title),
                   new XAttribute("alignment", "2"),
                   new XAttribute("fontFamilyName", "Microsoft Sans Serif"),
                   new XAttribute("fontStyle", "100"),
                   new XAttribute("fontSize", "15"),
                   new XAttribute("color", "-16777216"),
                   new XAttribute("angle", "0")
                   )
              );




                int increment = 1;

                // ������ ������������ �������� ���������
                double maxX = oldXml.Root.Descendants("SchemaPoint")
                                    .Select(p => double.TryParse(p.Attribute("X")?.Value, out var x) ? x : 0)
                                    .Max();

                double maxY = oldXml.Root.Descendants("SchemaPoint")
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
                foreach (var schemaPoint in oldXml.Root.Descendants("SchemaPoint"))
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

                    // ������� ����� � ����� XML
                    newXml.Element("points").Add(
                        new XElement("point",
                            new XAttribute("id", increment.ToString()), // ���������� ��������� ��� ID
                            new XAttribute("X", Convert.ToInt32(shortX)), // ����������� � ����� �����
                            new XAttribute("Y", Convert.ToInt32(shortY)), // ����������� � ����� �����
                            new XElement("pointInfo",
                                new XAttribute("number", ""),
                                new XAttribute("type", "2"),
                                new XAttribute("textPosition", "3"),
                                new XAttribute("gorlovina", "")
                            )
                        )
                    );

                    increment++;
                }

                // ���� ������������ ������� <Sections> � ���������� ��������� �����
                var editorTracksElement = oldXml.Root.Element("EditorTracks");
                var sectionsElement = oldXml.Root.Element("Sections");

                if (editorTracksElement != null && sectionsElement != null)
                {
                    increment = 1;

                    foreach (var editorTrack in editorTracksElement.Elements("EditorTrack"))
                    {

                        string editorTrackNumber = editorTrack.Attribute("Number")?.Value ?? "";

                        var trackSections = editorTrack.Element("Sections");
                        if (trackSections == null) continue;

                        // �������� ����� ������, ����������� � ������� ������ EditorTrack
                        var sectionsInTrack = trackSections.Elements("Section")
                            .Select(s => s.Attribute("Guid")?.Value)
                            .Where(guid => !string.IsNullOrEmpty(guid))
                            .Select(guid => sectionsElement.Elements("Section")
                                .FirstOrDefault(sec => sec.Attribute("Guid")?.Value == guid))
                            .Where(sec => sec != null)
                            .Select(sec => new
                            {
                                SectionElement = sec,
                                Length = double.TryParse(sec.Attribute("Length")?.Value, out var len) ? len : 0
                            })
                            .ToList();

                        // ������� ����� ������� ������ � ������� ������
                        var longestSection = sectionsInTrack
                            .OrderByDescending(s => s.Length)
                            .FirstOrDefault();

                        if (longestSection != null)
                        {
                            Console.WriteLine($"����� ������� ������: {longestSection.SectionElement} � ������ {longestSection.Length}");
                        }

                        // ��������� ��������� ��� ������
                        foreach (var sectionData in sectionsInTrack)
                        {
                            var section = sectionData.SectionElement;
                            var startElement = section.Element("Start");
                            var endElement = section.Element("End");

                            if (startElement != null && endElement != null)
                            {
                                var startPoint = oldXml.Root.Descendants("SchemaPoint")
                                    .FirstOrDefault(p => p.Attribute("Id")?.Value == startElement.Attribute("Id")?.Value);
                                var endPoint = oldXml.Root.Descendants("SchemaPoint")
                                    .FirstOrDefault(p => p.Attribute("Id")?.Value == endElement.Attribute("Id")?.Value);

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


                                    // ����������� ��� ������ ��� ����� ������� ������
                                    string lineName = section.Attribute("Guid")?.Value == longestSection?.SectionElement.Attribute("Guid")?.Value ? editorTrackNumber : "";


                                    // ��������� �������� IsMain
                                    bool isMain = section.Attribute("IsMain")?.Value == "true";

                                    // ����������� �������� name � specialization
                                    string sectionName = section.Attribute("Name")?.Value ?? string.Empty;
                                    string specialization = isMain ? "15" : string.IsNullOrEmpty(sectionName) ? "17" : "2";

                                    int type = !string.IsNullOrEmpty(lineName) ? 2 : 1;

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
                                                new XAttribute("type", type),
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

                    // ���������� ���� ��� ������ �����
                    string destinationFile = Path.Combine(Path.GetDirectoryName(sourceFile), "�����.xml");
                    newXml.Save(destinationFile);

                    MessageBox.Show($"�������������� ���������. ���� �������� �: {destinationFile}", "�����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��� ��������� ������: {ex.Message}", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
