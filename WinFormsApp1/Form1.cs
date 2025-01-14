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

        // Обработчик кнопки выбора папки для сохранения нового файла
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtDestinationFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        // Обработчик кнопки "Выполнить" для преобразования XML
        private void btnExecute_Click(object sender, EventArgs e)
        {
            string sourceFile = txtSourceFile.Text;
            string destinationFolder = txtDestinationFolder.Text;

            if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(destinationFolder))
            {
                MessageBox.Show("Пожалуйста, выберите исходный файл и папку для сохранения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Загружаем старый XML
                XElement oldXml = XElement.Load(sourceFile);
                XElement newXml = new XElement("StationMap",
                    new XAttribute("Step", "13"),
                    new XAttribute("Width", "142"),
                    new XAttribute("Height", "36"),
                    new XElement("points"),
                    new XElement("lines")
                );

                int increment = 1;

                // Найдем максимальные значения координат
                double maxX = oldXml.Descendants("SchemaPoint")
                                    .Select(p => double.TryParse(p.Attribute("X")?.Value, out var x) ? x : 0)
                                    .Max();

                double maxY = oldXml.Descendants("SchemaPoint")
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
                //else if (maxCoord >= 100)
                //{
                //    scale = 10;
                //}

                // Перебираем все <SchemaPoint> в старом XML и переносим их в новый формат
                foreach (var schemaPoint in oldXml.Descendants("SchemaPoint"))
                {
                    string pointId = schemaPoint.Attribute("Id")?.Value;
                    string x = schemaPoint.Attribute("X")?.Value;
                    string y = schemaPoint.Attribute("Y")?.Value;

                    // Преобразуем координаты с использованием выбранного масштаба и округляем до целых чисел
                    string shortX = !string.IsNullOrEmpty(x) && double.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out var xCoord)
                        ? (Math.Round(xCoord / scale)).ToString("0")  // Округляем и сохраняем как целое число
                        : x;

                    string shortY = !string.IsNullOrEmpty(y) && double.TryParse(y, NumberStyles.Any, CultureInfo.InvariantCulture, out var yCoord)
                        ? (Math.Round(yCoord / scale)).ToString("0")  // Округляем и сохраняем как целое число
                        : y;

                    // Генерация номера (например, первые 2 символа Id)
                    string number = pointId?.Substring(0, 2);

                    // Создаем точку в новом XML
                    newXml.Element("points").Add(
                        new XElement("point",
                            new XAttribute("id", increment.ToString()), // Используем инкремент для ID
                            new XAttribute("X", Convert.ToInt32(shortX)), // Преобразуем в целое число
                            new XAttribute("Y", Convert.ToInt32(shortY)), // Преобразуем в целое число
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

                // Ищем родительский элемент <Sections> и продолжаем обработку линий
                var sectionsElement = oldXml.Element("Sections");

                if (sectionsElement != null)
                {

                    // обнуляем инкремент
                    increment = 1;

                    // Перебираем все <Section> внутри <Sections>
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

                                // Преобразуем координаты с учетом масштаба и округляем до целых чисел
                                string shortStartX = !string.IsNullOrEmpty(startX) && double.TryParse(startX, NumberStyles.Any, CultureInfo.InvariantCulture, out var sX)
                                    ? (Math.Round(sX / scale)).ToString("0")  // Округляем и сохраняем как целое число
                                    : startX;

                                string shortStartY = !string.IsNullOrEmpty(startY) && double.TryParse(startY, NumberStyles.Any, CultureInfo.InvariantCulture, out var sY)
                                    ? (Math.Round(sY / scale)).ToString("0")  // Округляем и сохраняем как целое число
                                    : startY;

                                string shortEndX = !string.IsNullOrEmpty(endX) && double.TryParse(endX, NumberStyles.Any, CultureInfo.InvariantCulture, out var eX)
                                    ? (Math.Round(eX / scale)).ToString("0")  // Округляем и сохраняем как целое число
                                    : endX;

                                string shortEndY = !string.IsNullOrEmpty(endY) && double.TryParse(endY, NumberStyles.Any, CultureInfo.InvariantCulture, out var eY)
                                    ? (Math.Round(eY / scale)).ToString("0")  // Округляем и сохраняем как целое число
                                    : endY;

                                string length = section.Attribute("Length")?.Value ?? "0"; // Если длина не задана, берем 0


                                // Получение значения IsMain
                                bool isMain = section.Attribute("IsMain")?.Value == "true";

                                // Определение значения name и specialization
                                string lineName = section.Attribute("Name")?.Value ?? string.Empty;
                                string specialization = isMain ? "15" : string.IsNullOrEmpty(lineName) ? "17" : "2";

                                // Определяем значение kind
                                int kind = shortStartY == shortEndY ? 0 :  // Горизонтальная
                                           shortStartX == shortEndX ? 1 :  // Вертикальная
                                           double.Parse(shortStartY) > double.Parse(shortEndY) ? 2 : 3; // Диагональ

                                newXml.Element("lines").Add(
                                    new XElement("line",
                                        new XAttribute("id", increment.ToString()), // Используем инкремент для линий
                                        new XAttribute("sX", Convert.ToInt32(shortStartX)), // Преобразуем в целое число
                                        new XAttribute("sY", Convert.ToInt32(shortStartY)), // Преобразуем в целое число
                                        new XAttribute("eX", Convert.ToInt32(shortEndX)), // Преобразуем в целое число
                                        new XAttribute("eY", Convert.ToInt32(shortEndY)), // Преобразуем в целое число
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

                // Сохраняем новый XML
                string destinationFile = Path.Combine(destinationFolder, "новый.xml");
                newXml.Save(destinationFile);

                MessageBox.Show($"Преобразование завершено. Файл сохранен в: {destinationFile}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке файлов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
