
// Проект принимает список операторов ПДн и на выходе выводит список организаций, отфильтрованный по плохим словам в имени организации

using System.Text;
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
string[] badNames = File.ReadAllLines(@"C:\Users\SERGI\Desktop\OpenData\badNames.txt");
string file = @"C:\Users\SERGI\Desktop\OpenData\BigOpenData.csv";

int count_later = Convert.ToInt32(File.ReadAllText(@"C:\Users\SERGI\Desktop\OpenData\Обработано_строк.txt"));
int count_now = 0;
using (var stream = new StreamReader(file, Encoding.GetEncoding(1251)))
{
    while (stream.Peek() >= 0)
    {
        if (count_later < count_now) // Читай текст -> Пропускаем ранее прочитанные строки. Изучить как начать читать ОПТИМАЛЬНЕЕ СРАЗУ С НУЖНОЙ СТРОКИ
        {
            bool awesomeORG = true;
            var line = stream.ReadLine();
            if (line != null)
            {
                if (line.Contains('|'))
                {

                    if (!string.IsNullOrEmpty(line)) // Пропускаем пустые строки
                    {
                        string[] line_splited = line.Split('|');
                        if (line_splited[1] != "")
                        {
                            for (int j = 0; j < badNames.Length; j++)
                            {
                                if (line_splited[1].Contains(badNames[j], StringComparison.OrdinalIgnoreCase)) // Проверка имени организации
                                {
                                    awesomeORG = false;
                                }
                            }
                        }
                        else
                        {
                            awesomeORG = false; // Пустое имя организации - Индивидуальные предприниматели не интересуют
                        }
                    }
                    else
                    {
                        awesomeORG = false;
                    }
                }
            }
            if (awesomeORG)
            {
                File.AppendAllText(@"C:\Users\SERGI\Desktop\OpenData\BigOpenData-Awesome.csv", line + Environment.NewLine, Encoding.GetEncoding("windows-1251")); // сохраняем в хороший файл
            }
            else
            {
                File.AppendAllText(@"C:\Users\SERGI\Desktop\OpenData\BigOpenData-Filtered.csv", line + Environment.NewLine, Encoding.GetEncoding("windows-1251")); // сохраняем в плохой файл
            }
            count_later++;
            File.WriteAllText(@"C:\Users\SERGI\Desktop\OpenData\Обработано_строк.txt", count_later.ToString());
        }
        count_now++;
    }
}