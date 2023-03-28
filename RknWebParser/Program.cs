
// Проект обогащает список организаций контактными данными с сайта РКН

using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        HttpClient client = new HttpClient();

        var values = new Dictionary<string, string>{};
        FormUrlEncodedContent content = new FormUrlEncodedContent(values);
        string file = @"C:\Users\SERGI\Desktop\OpenData Shtat\!BigOpenData-Awesome+150.csv";
        string fileCounter = @"C:\Users\SERGI\Desktop\OpenData Shtat\!Обработано_строкRknWebParser.txt";
        int count_later = Convert.ToInt32(File.ReadAllText(fileCounter));
        int count_now = 1;
        using (var stream = new StreamReader(file, Encoding.GetEncoding(1251)))
        {
            while (stream.Peek() >= 0)
            {
                string emailorPhone = string.Empty;
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
                                    string RKN_id = line_splited[1];
                                    string ФИО = line_splited[9];
                                    var response = await client.PostAsync("https://pd.rkn.gov.ru/operators-registry/operators-list/?id=" + RKN_id, content);
                                    string responseString = await response.Content.ReadAsStringAsync();
                                    //File.WriteAllText("export.txt", responseString);
                                    //Console.WriteLine(content);
                                    HtmlDocument doc = new HtmlDocument();
                                    doc.LoadHtml(responseString);


                                    HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                                    // Iterate all rows in the first table
                                    List<string> strings = new List<string>();
                                    HtmlNodeCollection rows = tables[4].SelectNodes("tr");
                                    for (int i = 0; i <= rows.Count - 1; i++)
                                    {
                                        // Iterate all columns in this row
                                        HtmlNodeCollection cols = rows[i].SelectNodes("td");
                                        if (cols != null)
                                        {
                                            for (int j = 0; j <= cols.Count - 1; j++)
                                            {
                                                string fff = cols[j].InnerHtml.ToString();
                                                strings.Add(fff);
                                            }
                                        }
                                    }

                                    for (int k = 0; k < strings.Count; k++)
                                    {
                                        if (strings[k].Equals("ФИО физического лица или наименование юридического лица, ответственных за обработку персональных данных"))
                                        {
                                            if (strings[k + 1].Equals(ФИО))
                                            {
                                                for (int j = 0; j <= strings.Count; j++)
                                                {
                                                    if (strings[j].Equals("номера их контактных телефонов, почтовые адреса и адреса электронной почты"))
                                                    {
                                                        string toExport = strings[j + 1];
                                                        if (!string.IsNullOrEmpty(toExport))
                                                        {
                                                            string resultEmails = "";
                                                            string strRegex = @"[A-Za-z0-9_\-\+]+@[A-Za-z0-9\-]+\.([A-Za-z]{2,3})(?:\.[a-z]{2})?";
                                                            Regex myRegex = new Regex(strRegex, RegexOptions.None);
                                                            MatchCollection regexResaut = myRegex.Matches(toExport);
                                                            if (regexResaut.Count > 0)
                                                            {
                                                                for(int i = 0; i < regexResaut.Count; i++ )
                                                                {
                                                                    if (i != regexResaut.Count - 1)
                                                                    {
                                                                        resultEmails += regexResaut[i].Value + ";";
                                                                    }
                                                                    else
                                                                    {
                                                                        resultEmails += regexResaut[i].Value;
                                                                    }
                                                                }
                                                                emailorPhone = resultEmails;
                                                            }
                                                            else
                                                            {
                                                                emailorPhone = "Пусто";
                                                            }
                                                            awesomeORG = true;
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            emailorPhone = "Строка " + count_now + "|Пусто|Пусто";
                                                            awesomeORG = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                break;
                                            }
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
                        File.AppendAllText(@"C:\Users\SERGI\Desktop\OpenData Shtat\!BigOpenData-Awesome+Contacts 150.csv", emailorPhone + "|" + line + Environment.NewLine, Encoding.GetEncoding("windows-1251")); // сохраняем в хороший файл
                    }
                    else
                    {
                        File.AppendAllText(@"C:\Users\SERGI\Desktop\OpenData Shtat\!BigOpenData-Awesome-NO-Contacts 150.csv", line + Environment.NewLine, Encoding.GetEncoding("windows-1251")); // сохраняем в плохой файл
                    }
                    count_later++;
                    File.WriteAllText(fileCounter, count_later.ToString());
                    Random random= new Random();
                    //Thread.Sleep(random.Next(50, 100)); // Защиты от перебора и так нет :)
                }
                count_now++;
            }
        }
    }
}