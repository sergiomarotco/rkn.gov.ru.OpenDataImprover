
// Проект принимает на вход отфильтрованный список операторов персональных данных и формирует 2 списка:
//  - список организаций более 1000      человек в штате
//  - список организаций более Min_Shtat человек в штате

using System.Text;
using System.Xml;
internal class Program
{
    private static void Main()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        string ShatatPatch = @"C:\Users\SERGI\Desktop\OpenData Shtat";

        string file = @"C:\Users\SERGI\Desktop\OpenData\BigOpenData-Awesome.csv";
        // Список организаций, отфильтрованный по плохим словам в имени
        string[] lines = File.ReadAllLines(file, Encoding.GetEncoding(1251));

        int Min_Shtat = 150;
        string[] XML_files = Directory.GetFiles(ShatatPatch, "VO_OTKRDAN3_*.xml", SearchOption.AllDirectories);
        for (int file_number = 1; file_number < XML_files.Length; file_number++)
        {
            XmlDocument docXML = new XmlDocument();

            try
            {
                docXML.Load(XML_files[file_number]); // загрузить XML
            }
            catch { }

            foreach (XmlNode node in docXML.DocumentElement.ChildNodes)
            {
                string НаимОрг = string.Empty;
                string ИННЮЛ = string.Empty;
                int КолРаб = 0;
                if (node.Name.Equals("Документ"))
                {
                    foreach (XmlElement СведССЧРsubnode in node.SelectNodes("СведССЧР"))
                    {
                        КолРаб = Convert.ToInt32(СведССЧРsubnode.GetAttribute("КолРаб"));
                    }
                    if (КолРаб > Min_Shtat)
                    {
                        if (КолРаб <= 1000) // Отсекаем большие, им уже рассылали
                        {
                            foreach (XmlElement СведНПsubnode in node.SelectNodes("СведНП"))
                            {
                                НаимОрг = СведНПsubnode.GetAttribute("НаимОрг");
                                ИННЮЛ = СведНПsubnode.GetAttribute("ИННЮЛ");
                            }
                            //if (КолРаб > 1000)
                                                            //* Еще выше есть if по 1000 работников, его удалить!
                            {
                                int awesomeOrg = -1;
                                for (int l = 0; l < lines.Length; l++)
                                {
                                    string[] OperatorParameters = lines[l].Split('|');
                                    if (OperatorParameters.Length == 10)
                                    {
                                        if (OperatorParameters[2].Equals(ИННЮЛ))
                                        {
                                            awesomeOrg = l; break;
                                        }
                                    }
                                }
                                if (awesomeOrg >= 0) // Если вычислили среди операторов ПДн
                                {
                                    File.AppendAllText(@"C:\Users\SERGI\Desktop\OpenData Shtat\!BigOpenData-Awesome+" + Min_Shtat + ".csv", КолРаб + "|" + lines[awesomeOrg] + Environment.NewLine, Encoding.GetEncoding("windows-1251")); // сохраняем обогощаем данные об операторе ПДн
                                }
                            }
                            File.AppendAllText(@"C:\Users\SERGI\Desktop\OpenData Shtat\!BigOpenData от " + Min_Shtat + ".csv", КолРаб + "|" + ИННЮЛ + "|" + НаимОрг + Environment.NewLine, Encoding.GetEncoding("windows-1251")); // сохраняем чисто большую организацию
                        }
                    }
                }
            }
        }
    }
}