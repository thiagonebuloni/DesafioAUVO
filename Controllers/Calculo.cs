using DesafioAUVO.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Net;
using static Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
using System.Globalization;

namespace DesafioAUVO.Controllers
{
    public class CalculoController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public CalculoController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Calculo(string folderPath)
        {
            List<Departamento> departamentos = new List<Departamento>();
            List<Funcionario> funcionarios = new List<Funcionario>();

            // Check if the folder path is empty or null
            if (string.IsNullOrEmpty(folderPath))
            {
                ModelState.AddModelError("folderPath", "Please provide a valid folder path.");
                return View("Index");
            }

            // Get the root path of the application
            string rootPath = _hostingEnvironment.ContentRootPath;

            // Combine the root path with the specified folder path
            string fullPath = Path.Combine(rootPath, folderPath);

            // Check if the folder path exists
            if (!Directory.Exists(fullPath))
            {
                ModelState.AddModelError("folderPath", "The specified folder path does not exist.");
                return View("Index");
            }
            
            // Create a list to store the data from all CSV files
            // List<Person> people = new List<Person>();

            // Get all CSV files in the specified folder
            string[] csvFiles = Directory.GetFiles(fullPath, "*.csv");

            // Read each CSV file and extract the data
            foreach (string csvFile in csvFiles)
            {
                int indexSlash = csvFile.LastIndexOf("/");
                string[] nomeArquivo = csvFile.Substring(indexSlash+1).Split("-");
                string nomeDepartamento = nomeArquivo[0];
                string mesVigencia = nomeArquivo[1];
                string anoVigencia = nomeArquivo[2].Substring(0,4);
                
                // verifica se departamento existe e inclui na lista
                int indexDepartamento = departamentos.FindIndex(d => d.NomeDepartamento == nomeDepartamento);
                if (indexDepartamento == -1)
                {
                    Departamento novoDepartamento = new Departamento(nomeDepartamento, mesVigencia, anoVigencia);
                    departamentos.Add(novoDepartamento);
                    indexDepartamento = departamentos.FindIndex(d => d.NomeDepartamento == nomeDepartamento);
                }
                
                bool isFirstLine = true;

                using (var reader = new StreamReader(csvFile))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(';');

                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue; // Skip the header line
                        }

                        // if (values.Length != 2)
                        // {
                        //     // Skip the line if it doesn't match the expected format
                        //     continue;
                        // }
                        
                        // Create a new Person object and populate its properties
                        int codigo = int.Parse(values[0]);
                        string nome = values[1];
                        decimal valorHora = decimal.Parse(String.Concat(values[2].Where(c => !Char.IsWhiteSpace(c))).Substring(2).Replace(",","."));
                        DateTime data = DateTime.Parse(values[3], CultureInfo.InvariantCulture);
                        DateTime entrada = DateTime.ParseExact(values[4], "HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime saida = DateTime.ParseExact(values[5], "HH:mm:ss", CultureInfo.InvariantCulture);
                        string almoco = values[6];

                        // Constantes
                        decimal valorMinuto = valorHora / 60;
                        int MinimoMinutosDia = 9 * 60;

                        // Calculo horas trabalhadas
                        TimeSpan difference = saida - entrada;
                        decimal valorAReceber = valorMinuto * (decimal)difference.TotalMinutes;
                        double horasDebito = 0;
                        double horasExtras = 0;

                        // Calculo almoço
                        DateTime almocoInicio = DateTime.ParseExact(almoco.Substring(0,5), "HH:mm", CultureInfo.InvariantCulture);
                        DateTime almocoFinal = DateTime.ParseExact(almoco.Substring(8), "HH:mm", CultureInfo.InvariantCulture);
                        TimeSpan tempoAlmoco = almocoFinal - almocoInicio;

                        // Almoco maior que 1 hora
                        if (tempoAlmoco.TotalMinutes > 60)
                        {
                            valorAReceber -= (decimal)(tempoAlmoco.TotalMinutes - 60) * valorMinuto;
                        }

                        // Calculo horas em débito e horas extras
                        if (difference.TotalMinutes < MinimoMinutosDia)
                        {
                            horasDebito = (MinimoMinutosDia - difference.TotalMinutes) / 60;
                        }
                        else if (difference.TotalMinutes > MinimoMinutosDia)
                        {
                            horasExtras = (difference.TotalMinutes - MinimoMinutosDia) / 60;
                        }

                        // if (horasExtras != 0) double horasExtrasDebito = horasExtras / 60;

                        int indexFuncionario = funcionarios.FindIndex(f => f.Codigo == codigo);
                        if (indexFuncionario == -1)
                        {
                            Funcionario novoFuncionario = new Funcionario
                            (
                                nome,
                                codigo,
                                valorAReceber,
                                horasExtras,
                                horasDebito
                            );
                            novoFuncionario.AcrescentaDiasTrabalhados();

                            funcionarios.Add(novoFuncionario);
                            departamentos[indexDepartamento].AcrescentaFuncionarioNoDepartamento(novoFuncionario);
                            indexFuncionario = funcionarios.FindIndex(f => f.Codigo == codigo);
                        }
                        else
                        {
                            funcionarios[indexFuncionario].SomaTotalReceber(valorAReceber);
                            funcionarios[indexFuncionario].SomaHorasExtras(horasExtras);
                            funcionarios[indexFuncionario].SomaHorasDebito(horasDebito);
                            funcionarios[indexFuncionario].AcrescentaDiasTrabalhados();
                        }

                        departamentos[indexDepartamento].SomaPagamentos(valorAReceber);
                        departamentos[indexDepartamento].SomaDescontos((decimal)(horasDebito * 60) * valorMinuto);
                        departamentos[indexDepartamento].SomaExtras((decimal)(horasExtras * 60) * valorMinuto);
                        // Console.WriteLine($"{departamentos[indexDepartamento].NomeDepartamento}");
                        

                        // Person person = new Person
                        // {
                        //     Codigo = int.Parse(values[0]),
                        //     Nome = values[1],
                        //     ValorHora = decimal.Parse(String.Concat(values[2].Where(c => !Char.IsWhiteSpace(c))).Substring(2).Replace(",",".")),
                        //     Data = DateTime.ParseExact(values[3], "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        //     Entrada = DateTime.ParseExact(values[4], "HH:mm:ss", CultureInfo.InvariantCulture),
                        //     Saida = DateTime.ParseExact(values[5], "HH:mm:ss", CultureInfo.InvariantCulture),
                        //     Almoco = values[6]
                        // };
                        
                        // Add the person to the list
                        // people.Add(person);

                    }
                }
            }

            
            // Serialize the list of people to JSON
            string json = JsonConvert.SerializeObject(departamentos, Formatting.Indented);

            // Save the JSON to a file
            string jsonFilePath = Path.Combine(fullPath, "saida.json");
            System.IO.File.WriteAllText(jsonFilePath, json);

            // Get the relative file path within the web application
            string relativeFilePath = Path.Combine("~", folderPath, "saida.json");

             // Replace backslashes with forward slashes in the file path (for Linux compatibility)
            // relativeFilePath = relativeFilePath.Replace(Path.DirectorySeparatorChar, '/');

            // Return the JSON file for download
            // return File(relativeFilePath, "application/json", "output.json");
            return View("Index");
        }
    }

}
