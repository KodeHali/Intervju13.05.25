

using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;


class Part2
{
    static void Main()
    {
        const string path = "firmaer_output.csv";

        var statuses = File
            .ReadAllLines(path)
            .Skip(1) //Skipper headers
            .Select(line => line.Split(';')[2]); //velger riktig column

        var statusTally = statuses
            .GroupBy(status => status) // lambda uttrykk som grupperer etter verdier uten å endre dem. Finner unike verdier (hver status)
            .ToDictionary(status => status.Key, statusamount => statusamount.Count()); // her gjør jeg dem til en dictionary. Hver unike nøkkel har sitt antall som verdi

        Console.WriteLine("\n\nStatus\t\t\tAntall");  //simulerer bare to kolonner i konsollen med en tabulator escape character
        foreach (var kv in statusTally.OrderBy(kv => kv.Key)) // kv = keyvalue. Bruker LINQ her til å iterere gjennom nøkkelverdi parene
            Console.WriteLine($"{kv.Key}\t\t\t{kv.Value}");







        var organizationalForms = File  // ble litt rar blanding her med norske variabler. bruker samme metode som over
            .ReadAllLines(path)
            .Skip(1)
            .Select(line => line.Split(';')[4]);

        var percentageReference = organizationalForms.Count();

        var organizationalFormsTally = organizationalForms
            .GroupBy(companyType => companyType)
            .ToDictionary(companyType => companyType.Key, companyTypeAmount => companyTypeAmount.Count());

        Console.WriteLine("\n\nOrganisasjonsform\tAntall\tProsent");
        foreach (var kv in organizationalFormsTally.OrderBy(kv => kv.Key))
        {
            // vil ha et flyttall
            double percent = (double)kv.Value / percentageReference * 100;

            // Med 1 desimaltall
            Console.WriteLine(
            $"{kv.Key}\t\t\t{kv.Value}\t{percent:F1}%"
            );
        }




        var categories = File
            .ReadAllLines(path)
            .Skip(1)  // skip header
            .Select(line =>
            {
                var columns = line.Split(';');
                // bruker en ternary operator for å avgjøre om raden er tom eller om det ligger et tall der. Enten eller
                int n = columns[3] == ""
                    ? 0
                    : int.Parse(columns[3]);

                // Lager noen enkle kategorier her
                if (n == 0) return "0";
                else if (n <= 9) return "1-9";
                else if (n <= 49) return "10-49";
                else return "50+";
            });

        // teller hvor mange som er i hver kategori med samme metode som over
        var tally = categories
            .GroupBy(b => b)
            .ToDictionary(g => g.Key, g => g.Count());

        // Print
        Console.WriteLine("\n\nAnsatte\t\tAntall");
        foreach (var kv in tally.OrderBy(kv => kv.Key))
        {
            Console.WriteLine($"{kv.Key}\t\t{kv.Value}");
        }



    }

}
