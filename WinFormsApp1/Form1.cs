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

        //��� �����
        enum LineKind
        {
            Vertical = 0,
            Horizontal = 1,
            DiagonalUp = 2,
            DiagonalDown = 3
        }

        // ������ ����������
        private LineKind DetermineLineKind(int sX, int sY, int eX, int eY)
        {
            if (sY == eY) return LineKind.Horizontal;
            if (sX == eX) return LineKind.Vertical;
            return sY > eY ? LineKind.DiagonalDown : LineKind.DiagonalUp;
        }

        // ����� ��� ����������� ������ ����� � ���������
        private void ConvertRightAnglesToDiagonals(XElement newXml, int increment)
        {
            var lines = newXml.Element("lines")?.Elements("line")?.ToList(); // ����� ������ �����
            if (lines == null || lines.Count < 2) return;

            var linesToRemove = new HashSet<XElement>();
            var newLines = new List<XElement>();

            foreach (var line1 in lines)
            {
                if (linesToRemove.Contains(line1)) continue;

                var sX1 = int.Parse(line1.Attribute("sX")?.Value ?? "0");
                var sY1 = int.Parse(line1.Attribute("sY")?.Value ?? "0");
                var eX1 = int.Parse(line1.Attribute("eX")?.Value ?? "0");
                var eY1 = int.Parse(line1.Attribute("eY")?.Value ?? "0");

                var lineInfo1 = line1.Element("lineInfo");
                var lineType1 = lineInfo1?.Attribute("type")?.Value ?? "";
                var lineName1 = lineInfo1?.Attribute("name")?.Value ?? "";
                var specialization1 = lineInfo1?.Attribute("specialization")?.Value ?? "";
                var length1 = lineInfo1?.Attribute("length")?.Value ?? "";

                bool line1IsVertical = sX1 == eX1;
                bool line1IsHorizontal = sY1 == eY1;

                foreach (var line2 in lines)
                {
                    if (line1 == line2 || linesToRemove.Contains(line2)) continue;

                    var sX2 = int.Parse(line2.Attribute("sX")?.Value ?? "0");
                    var sY2 = int.Parse(line2.Attribute("sY")?.Value ?? "0");
                    var eX2 = int.Parse(line2.Attribute("eX")?.Value ?? "0");
                    var eY2 = int.Parse(line2.Attribute("eY")?.Value ?? "0");

                    bool line2IsVertical = sX2 == eX2;
                    bool line2IsHorizontal = sY2 == eY2;

                    // ���������, ���� �� ������ ���� (����� ����� ����� ��������� � ������� ������)
                    if ((eX1 == sX2 && eY1 == sY2) && // ���������� �����
                        ((line1IsVertical && line2IsHorizontal) || (line1IsHorizontal && line2IsVertical))) // ������������������
                    {

                        var kind = DetermineLineKind(sX1, sY1, eX2, eY2);

                        // ������� ������������ �����
                        newLines.Add(new XElement("line",
                            new XAttribute("id", increment.ToString()),
                            new XAttribute("sX", sX1),
                            new XAttribute("sY", sY1),
                            new XAttribute("eX", eX2),
                            new XAttribute("eY", eY2),
                           new XAttribute("kind", (int)kind),
                            new XElement("lineInfo",
                        new XAttribute("type", lineType1),
                        new XAttribute("name", lineName1),
                        new XAttribute("specialization", specialization1),
                        new XAttribute("lengthInVagons", "0"),
                        new XAttribute("length", length1),
                        new XAttribute("park", ""),
                        new XAttribute("lengthLeft", "0"),
                        new XAttribute("nameLeft", ""),
                        new XAttribute("signalLeft", "3"),
                        new XAttribute("lengthRight", "0"),
                        new XAttribute("nameRight", ""),
                        new XAttribute("signalRight", "3")
                    )
                        ));

                        // �������� ������ ����� ��� ��������
                        linesToRemove.Add(line1);
                        linesToRemove.Add(line2);

                        increment++;

                        break; // ��������� � ��������� �����
                    }
                }
            }

            // ������� ������ �����
            foreach (var line in linesToRemove)
            {
                Debug.WriteLine(line);
                line.Remove();

            }

            // ��������� ����� �����
            foreach (var newLine in newLines)
            {
                newXml.Element("lines")?.Add(newLine);
            }
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
                XDocument oldXml = XDocument.Load(sourceFile);

                // ���������� ����� ��� �������� ���� ��������� SchemaPoint
                var schemaPoints = oldXml.Root.Descendants("SchemaPoint").ToList();
                Debug.WriteLine($"������� {schemaPoints.Count} ��������� SchemaPoint");

                foreach (var point in schemaPoints)
                {
                    Debug.WriteLine($"X: {point.Attribute("X")?.Value}, Y: {point.Attribute("Y")?.Value}");
                }

                // ������ ������������ �������� X � Y
                CultureInfo culture = CultureInfo.InvariantCulture;

                double maxX = schemaPoints
                                 .Select(p => double.TryParse(p.Attribute("X")?.Value, NumberStyles.Float, culture, out var x) ? x : 0)
                                 .Max();

                double maxY = schemaPoints
                                 .Select(p => double.TryParse(p.Attribute("Y")?.Value, NumberStyles.Float, culture, out var y) ? y : 0)
                                 .Max();

                Debug.WriteLine($"������������ �������� X: {maxX}");
                Debug.WriteLine($"������������ �������� Y: {maxY}");


                // ��������� �������� �� ���������� �������� ������ ��� ��������
                int calculatedWidth = (int)Math.Ceiling(maxX);
                int calculatedHeight = (int)Math.Ceiling(maxY);


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


                XElement newXml = new XElement("StationMap",
                    new XAttribute("Step", "13"),
                    new XAttribute("Width", ((calculatedWidth / scale) + 10).ToString("0")),  // ������ �� ���������
                    new XAttribute("Height", ((calculatedHeight / scale) + 10).ToString("0")), // ������ �� ���������
                    new XElement("points"),
                    new XElement("lines"),
                    new XElement("textCollection"),
                    new XElement("settings")
                );


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


                /*
                 * 
                 * �������� �����
                 * 
                 */


                int increment = 1;

                var switchsElement = oldXml.Root.Element("Switchs");
                var schemaPointsElement = oldXml.Root.Element("Points");

                if (schemaPointsElement != null)
                {
                    foreach (var schemaPoint in schemaPointsElement.Elements("SchemaPoint"))
                    {
                        string pointId = schemaPoint.Attribute("Id")?.Value;
                        if (string.IsNullOrEmpty(pointId)) continue;

                        string x = schemaPoint.Attribute("X")?.Value;
                        string y = schemaPoint.Attribute("Y")?.Value;

                        // ����������� ���������� � �������������� ��������
                        string shortX = !string.IsNullOrEmpty(x) && double.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out var xCoord)
                            ? (Math.Round(xCoord / scale)).ToString("0")
                            : x;

                        string shortY = !string.IsNullOrEmpty(y) && double.TryParse(y, NumberStyles.Any, CultureInfo.InvariantCulture, out var yCoord)
                            ? (Math.Round(yCoord / scale)).ToString("0")
                            : y;

                        // ����, ������ �� SchemaPoint � Switch
                        string switchName = "";
                        if (switchsElement != null)
                        {
                            foreach (var oneSwitch in switchsElement.Elements("Switch"))
                            {
                                var switchPoint = oneSwitch.Element("Point");
                                if (switchPoint == null) continue;

                                string switchPointId = switchPoint.Attribute("Id")?.Value;
                                if (pointId == switchPointId)
                                {
                                    switchName = oneSwitch.Attribute("Name")?.Value ?? "";
                                    break;
                                }
                            }
                        }

                        // ��������� ����� � ����� XML
                        newXml.Element("points").Add(
                            new XElement("point",
                                new XAttribute("id", increment.ToString()),
                                new XAttribute("X", Convert.ToInt32(shortX)),
                                new XAttribute("Y", Convert.ToInt32(shortY)),
                                new XElement("pointInfo",
                                    new XAttribute("number", switchName), // ����������� ��� ��� ��������� ������
                                    new XAttribute("type", "2"),
                                    new XAttribute("textPosition", "3"),
                                    new XAttribute("gorlovina", "")
                                )
                            )
                        );

                        // ����������� ���������
                        increment++;
                    }
                }


                /*
                 * 
                 * �������� �����
                 * 
                 */

                var editorTracksElement = oldXml.Root.Element("EditorTracks");
                var sectionsElement = oldXml.Root.Element("Sections");

                if (editorTracksElement != null && sectionsElement != null)
                {
                    increment = 1;

                    foreach (var editorTrack in editorTracksElement.Elements("EditorTrack"))
                    {

                        string editorTrackNumber = "";
                        string editorTrackType = editorTrack.Attribute("Type")?.Value;

                        // ��������� ��� � ����������� Number ������ ���� ��� "Station" ��� "Main"
                        if (editorTrackType == "Station" || editorTrackType == "Main")
                        {
                            editorTrackNumber = editorTrack.Attribute("Number")?.Value ?? "";
                        }

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

                                    double sX = !string.IsNullOrEmpty(startX) && double.TryParse(startX, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedSX) ? parsedSX : 0;
                                    double sY = !string.IsNullOrEmpty(startY) && double.TryParse(startY, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedSY) ? parsedSY : 0;
                                    double eX = !string.IsNullOrEmpty(endX) && double.TryParse(endX, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedEX) ? parsedEX : 0;
                                    double eY = !string.IsNullOrEmpty(endY) && double.TryParse(endY, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedEY) ? parsedEY : 0;

                                    if (sX > eX)
                                    {
                                        (sX, eX) = (eX, sX);
                                        (sY, eY) = (eY, sY);
                                    }


                                    int shortStartX = (int)Math.Round(sX / scale);
                                    int shortStartY = (int)Math.Round(sY / scale);
                                    int shortEndX = (int)Math.Round(eX / scale);
                                    int shortEndY = (int)Math.Round(eY / scale);


                                    string length = section.Attribute("Length")?.Value ?? "0"; // ���� ����� �� ������, ����� 0


                                    // ����������� ��� ������ ��� ����� ������� ������
                                    string lineName = section.Attribute("Guid")?.Value == longestSection?.SectionElement.Attribute("Guid")?.Value ? editorTrackNumber : "";


                                    // ��������� �������� IsMain
                                    bool isMain = section.Attribute("IsMain")?.Value == "true";

                                    // ����������� �������� name � specialization
                                    string specialization = isMain ? "15" : string.IsNullOrEmpty(lineName) ? "17" : "2";

                                    int lineType = !string.IsNullOrEmpty(lineName) ? 2 : 1;

                                    // ���������� �������� kind
                                    var kind = DetermineLineKind(shortStartX, shortStartY, shortEndX, shortEndY);

                                    newXml.Element("lines").Add(
                                        new XElement("line",
                                            new XAttribute("id", increment.ToString()), // ���������� ��������� ��� �����
                                            new XAttribute("sX", shortStartX), // ����������� � ����� �����
                                            new XAttribute("sY", shortStartY), // ����������� � ����� �����
                                            new XAttribute("eX", shortEndX), // ����������� � ����� �����
                                            new XAttribute("eY", shortEndY), // ����������� � ����� �����
                                            new XAttribute("kind", (int)kind),
                                            new XElement("lineInfo",
                                                new XAttribute("type", lineType),
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

                    // ���������� ������ ��� ����������� ������ ����� � ���������
                    ConvertRightAnglesToDiagonals(newXml, increment);

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
