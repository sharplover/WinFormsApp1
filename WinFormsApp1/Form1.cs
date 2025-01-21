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


        // Метод для превращения прямых углов в диагонали
        private void ConvertRightAnglesToDiagonals(XElement newXml, int increment)
        {
            var lines = newXml.Element("lines")?.Elements("line")?.ToList(); // Копия списка линий
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

                var kind1 = line1.Attribute("kind")?.Value ?? "1";
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

                    // Проверяем, есть ли прямой угол (конец одной линии совпадает с началом другой)
                    if ((eX1 == sX2 && eY1 == sY2) && // Совпадение точки
                        ((line1IsVertical && line2IsHorizontal) || (line1IsHorizontal && line2IsVertical))) // Перпендикулярность
                    {
                        // Создаем диагональную линию
                        newLines.Add(new XElement("line",
                            new XAttribute("id", increment.ToString()),
                            new XAttribute("sX", sX1),
                            new XAttribute("sY", sY1),
                            new XAttribute("eX", eX2),
                            new XAttribute("eY", eY2),
                            new XAttribute("kind", kind1),
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

                        // Помечаем старые линии для удаления
                        linesToRemove.Add(line1);
                        linesToRemove.Add(line2);

                        increment++;

                        break; // Переходим к следующей линии
                    }
                }
            }

            // Удаляем старые линии
            foreach (var line in linesToRemove)
            {
                Debug.WriteLine(line);
                line.Remove();

            }

            // Добавляем новые линии
            foreach (var newLine in newLines)
            {
                newXml.Element("lines")?.Add(newLine);
            }
        }

        // Обработчик кнопки выбора исходного XML
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtSourceFile.Text = openFileDialog.FileName;
            }
        }

        // Обработчик кнопки "Выполнить" для преобразования XML
        private void btnExecute_Click(object sender, EventArgs e)
        {
            string sourceFile = txtSourceFile.Text;

            if (string.IsNullOrEmpty(sourceFile))
            {
                MessageBox.Show("Пожалуйста, выберите исходный файл.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Загружаем старый XML
                XDocument oldXml = XDocument.Load(sourceFile);
                XElement newXml = new XElement("StationMap",
                    new XAttribute("Step", "13"),
                    new XAttribute("Width", "1420"),
                    new XAttribute("Height", "360"),
                    new XElement("points"),
                    new XElement("lines"),
                    new XElement("textCollection"),
                    new XElement("settings")
                );


                string title = "";

                XNamespace ns = oldXml.Root.GetDefaultNamespace(); // Получаем пространство имен корня
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

                newXml.Element("settings").Add(
                    new XAttribute("StationMap_backgroundColor", "-1"),
                    new XAttribute("StationMap_selectionColor", "-16776961"),
                    new XAttribute("MapGrid_visible", "True"),
                    new XAttribute("MapGrid_color", "-2302756"),
                    new XAttribute("MapCursorPoint_color", "-5658199"),
                    new XAttribute("MapCursorPoint_coordinatesVisible", "True"),
                    new XAttribute("MapLineDraw_lineColor", "-8388608"),
                    new XAttribute("MapLineDraw_pointColor", "-8388608"),
                    new XAttribute("MapLineDraw_incorrectLineColor", "-5658199"),
                    new XAttribute("MapLineDraw_incorrectPointColor", "-5658199"),
                    new XAttribute("MapLineDraw_coordinatesVisible", "True"),
                    new XAttribute("MapSelectionBox_borderColor", "-16777077"),
                    new XAttribute("MapSelectionBox_innerColor", "-16776961"),
                    new XAttribute("MapSelectionBox_coordinatesVisible", "True"),
                    new XAttribute("MapLines_defaultColor", "-9868951"),
                    new XAttribute("MapLines_defaultColorWithLength", "-5103070"),
                    new XAttribute("MapLines_wayColor", "-8388608"),
                    new XAttribute("MapLines_peregonColor", "-16777088"),
                    new XAttribute("MapLines_signalColor", "-8388608"),
                    new XAttribute("MapLines_signalVisible", "True"),
                    new XAttribute("MapLines_signalNames", "True"),
                    new XAttribute("MapPoints_errorArrowColor", "-65536"),
                    new XAttribute("MapPoints_simpleArrowColor", "-8388480"),
                    new XAttribute("MapPoints_crossColor", "-5658199"),
                    new XAttribute("MapPoints_tunnelColor", "-16777216"),
                    new XAttribute("MapGroups_groupColor", "-8531"),
                    new XAttribute("MapGroups_groupVisible", "True"),
                    new XAttribute("MapRoutes_routeColor", "-65536"),
                    new XAttribute("MapRoutesBuildSettings.maxRoutesCount", "250"),
                    new XAttribute("MapRoutesBuildSettings.maxRoutesCountForWay", "250"),
                    new XAttribute("MapText_textVisible", "True"),
                    new XAttribute("MapText_defaultColor", "-16777216"),
                    new XAttribute("MapText_defaultDraw", "False")
                );



                /*
                 * 
                 * Создание точек
                 * 
                 */


                int increment = 1;

                // Найдем максимальные значения координат
                double maxX = oldXml.Root.Descendants("SchemaPoint")
                                    .Select(p => double.TryParse(p.Attribute("X")?.Value, out var x) ? x : 0)
                                    .Max();

                double maxY = oldXml.Root.Descendants("SchemaPoint")
                                    .Select(p => double.TryParse(p.Attribute("Y")?.Value, out var y) ? y : 0)
                                    .Max();

                // Выбираем максимальную координату
                double maxCoord = Math.Max(maxX, maxY);

                // Определяем масштаб
                double scale = 1;
                if (maxCoord >= 10000)
                {
                    scale = 100;
                }
                else if (maxCoord >= 1000)
                {
                    scale = 10;
                }

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

                        // Преобразуем координаты с использованием масштаба
                        string shortX = !string.IsNullOrEmpty(x) && double.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out var xCoord)
                            ? (Math.Round(xCoord / scale)).ToString("0")
                            : x;

                        string shortY = !string.IsNullOrEmpty(y) && double.TryParse(y, NumberStyles.Any, CultureInfo.InvariantCulture, out var yCoord)
                            ? (Math.Round(yCoord / scale)).ToString("0")
                            : y;

                        // Ищем, связан ли SchemaPoint с Switch
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

                        // Добавляем точку в новый XML
                        newXml.Element("points").Add(
                            new XElement("point",
                                new XAttribute("id", increment.ToString()),
                                new XAttribute("X", Convert.ToInt32(shortX)),
                                new XAttribute("Y", Convert.ToInt32(shortY)),
                                new XElement("pointInfo",
                                    new XAttribute("number", switchName), // Присваиваем имя или оставляем пустым
                                    new XAttribute("type", "2"),
                                    new XAttribute("textPosition", "3"),
                                    new XAttribute("gorlovina", "")
                                )
                            )
                        );

                        // Увеличиваем инкремент
                        increment++;
                    }
                }


                /*
                 * 
                 * Создание линий
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

                        // Проверяем тип и присваиваем Number только если тип "Station" или "Main"
                        if (editorTrackType == "Station" || editorTrackType == "Main")
                        {
                            editorTrackNumber = editorTrack.Attribute("Number")?.Value ?? "";
                        }

                        var trackSections = editorTrack.Element("Sections");
                        if (trackSections == null) continue;

                        // Собираем длины секций, относящихся к текущей группе EditorTrack
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

                        // Находим самую длинную секцию в текущей группе
                        var longestSection = sectionsInTrack
                            .OrderByDescending(s => s.Length)
                            .FirstOrDefault();

                        if (longestSection != null)
                        {
                            Console.WriteLine($"Самая длинная секция: {longestSection.SectionElement} с длиной {longestSection.Length}");
                        }

                        // Остальная обработка для секций
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

                                    string shortStartX = Math.Round(sX / scale).ToString("0");
                                    string shortStartY = Math.Round(sY / scale).ToString("0");
                                    string shortEndX = Math.Round(eX / scale).ToString("0");
                                    string shortEndY = Math.Round(eY / scale).ToString("0");


                                    string length = section.Attribute("Length")?.Value ?? "0"; // Если длина не задана, берем 0


                                    // Присваиваем имя только для самой длинной секции
                                    string lineName = section.Attribute("Guid")?.Value == longestSection?.SectionElement.Attribute("Guid")?.Value ? editorTrackNumber : "";


                                    // Получение значения IsMain
                                    bool isMain = section.Attribute("IsMain")?.Value == "true";

                                    // Определение значения name и specialization
                                    string specialization = isMain ? "15" : string.IsNullOrEmpty(lineName) ? "17" : "2";

                                    int lineType = !string.IsNullOrEmpty(lineName) ? 2 : 1;

                                    // Определяем значение kind
                                    int kind = shortStartY == shortEndY ? 1 :  // Горизонтальная
                                               shortStartX == shortEndX ? 0 :  // Вертикальная
                                               double.Parse(shortStartY) > double.Parse(shortEndY) ? 3 : 2; // Диагональ

                                    newXml.Element("lines").Add(
                                        new XElement("line",
                                            new XAttribute("id", increment.ToString()), // Используем инкремент для линий
                                            new XAttribute("sX", Convert.ToInt32(shortStartX)), // Преобразуем в целое число
                                            new XAttribute("sY", Convert.ToInt32(shortStartY)), // Преобразуем в целое число
                                            new XAttribute("eX", Convert.ToInt32(shortEndX)), // Преобразуем в целое число
                                            new XAttribute("eY", Convert.ToInt32(shortEndY)), // Преобразуем в целое число
                                            new XAttribute("kind", kind),
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

                    // Применение метода для превращения прямых углов в диагонали
                    ConvertRightAnglesToDiagonals(newXml, increment);

                    // Определяем путь для нового файла
                    string destinationFile = Path.Combine(Path.GetDirectoryName(sourceFile), "новый.xml");
                    newXml.Save(destinationFile);

                    MessageBox.Show($"Преобразование завершено. Файл сохранен в: {destinationFile}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке файлов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
