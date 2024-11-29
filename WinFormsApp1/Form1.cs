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
                    new XElement("points")
                );

                // Переменная для инкремента
                int increment = 1;

                // Перебираем все <SchemaPoint> в старом XML и переносим их в новый формат
                foreach (var schemaPoint in oldXml.Descendants("SchemaPoint"))
                {
                    string pointId = schemaPoint.Attribute("Id").Value;
                    string x = schemaPoint.Attribute("X").Value;
                    string y = schemaPoint.Attribute("Y").Value;

                    // Берем только первые две цифры из координат X и Y
                    string xTruncated = x.Length > 2 ? x.Substring(0, 2) : x;
                    string yTruncated = y.Length > 2 ? y.Substring(0, 2) : y;

                    // Генерация номера (например, первые 2 символа Id)
                    string number = pointId.Substring(0, 2);

                    // Создаем точку в новом XML, используя инкремент для атрибута id
                    newXml.Element("points").Add(
                        new XElement("point",
                            new XAttribute("id", increment.ToString()), // Используем инкремент
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

                    // Увеличиваем инкремент
                    increment++;
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
