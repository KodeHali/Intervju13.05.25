using System.Net;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace PowerofficeIntervju
{

    //Lager en poco som kan inneholde variabler fra begge responstyper. 
    public class EnhetKlasse
    {
        // de følgende variablene fins i begge responstyper og trenger ikke være nullable
        [JsonPropertyName("respons_klasse")]
        public string ResponsKlasse { get; set; }

        [JsonPropertyName("organisasjonsnummer")]
        public string Organisasjonsnummer { get; set; }

        [JsonPropertyName("navn")]
        public string Navn { get; set; }

        [JsonPropertyName("organisasjonsform")]
        public Organisasjonsform Organisasjonsform { get; set; }

        // følgende variabler fins bare i enhet responstypen:
        [JsonPropertyName("underAvvikling")]
        public bool? UnderAvvikling { get; set; }

        [JsonPropertyName("underTvangsavviklingEllerTvangsopplosning")]
        public bool? UnderTvangsavviklingEllerTvangsopplosning { get; set; }

        [JsonPropertyName("konkurs")]
        public bool? Konkurs { get; set; }

        [JsonPropertyName("naeringskode1")]
        public Naeringskode Naeringskode1 { get; set; }

        [JsonPropertyName("antallAnsatte")]
        public int? AntallAnsatte { get; set; }



        // Denne propertien fins bare i slettetenhet responstypen
        [JsonPropertyName("slettedato")]
        public DateTime? Slettedato { get; set; }
    }

    public class Organisasjonsform
    {
        [JsonPropertyName("kode")]
        public string Kode { get; set; }
    }

    public class Naeringskode
    {
        [JsonPropertyName("kode")]
        public string Kode { get; set; }
    }

    class Program
    {
        static async Task Main()
        {

            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            string filePath = Path.Combine(projectRoot, "firmaer.csv"); //Bruker bare disse linjene for debugging i vsc.

            List<string[]> rows = File
                .ReadAllLines(filePath)
                .Skip(1) // vil ikke ha kolonnelinjen
                .Select(line => line.Split(';'))
                .ToList();



            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://data.brreg.no/enhetsregisteret/api/enheter/{enhetOrgNr}")
            };

            var outputFilepath = Path.Combine(projectRoot, "firmaer_output.csv");
            var outputLines = new List<string>
            {
                "OrgNo;FirmaNavn;Status;AntallAnsatte;OrgFormKode;Næringskode"
            };

            //itererer gjennom inputfil-variabelen
            foreach (var row in rows)
            {
                var enhetOrgNr = row[1];
                var firmanavn = row[0]; // instansierer for output filen 

                var resp = await client.GetAsync($"{enhetOrgNr}");
                string status;
                int? antallAnsatte = null;
                string orgFormKode = "";
                string naeringKode = "";

                if (resp.IsSuccessStatusCode)
                {
                    var enhet = await resp.Content.ReadFromJsonAsync<EnhetKlasse>();

                    switch (enhet.ResponsKlasse)
                    {
                        case "Enhet":
                            if (enhet.UnderAvvikling == true
                            || enhet.UnderTvangsavviklingEllerTvangsopplosning == true)
                                status = "UnderAvvikling";
                            else if (enhet.Konkurs == true)
                                status = "Konkurs";
                            else
                                status = "Aktiv";
                            break;

                        case "SlettetEnhet":
                            status = "Slettet";
                            break;

                        default:
                            status = "LOGIKKFEIL";
                            break;
                    }

                    antallAnsatte = enhet.AntallAnsatte;
                    orgFormKode = enhet.Organisasjonsform?.Kode ?? "";
                    naeringKode = enhet.Naeringskode1?.Kode ?? "";
                }
                else if (resp.StatusCode == HttpStatusCode.Gone)
                {
                    status = $"Fjernet{(int)resp.StatusCode}";
                }
                else
                {
                    status = $"Feil";
                }



                outputLines.Add(
                string.Join(";", enhetOrgNr, firmanavn, status,
                            antallAnsatte?.ToString() ?? "",
                            orgFormKode, naeringKode)
                );
            }

            File.WriteAllLines(outputFilepath, outputLines);
            Console.WriteLine($"\n\nSkrev {outputLines.Count - 1} rader til {outputFilepath}");
        }
    }
}
