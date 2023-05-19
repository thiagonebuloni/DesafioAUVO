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
                ModelState.AddModelError("folderPath", "Por favor digite um caminho válido.");
                return View("Index");
            }

            // Caminho raiz do app
            string rootPath = _hostingEnvironment.ContentRootPath;

            // Combina o caminho raiz com o diretório de input
            string fullPath = Path.Combine(rootPath, folderPath);

            if (!Directory.Exists(fullPath))
            {
                ModelState.AddModelError("folderPath", "The specified folder path does not exist.");
                return View("Index");
            }

            // Todos os arquivos CSV do diretório
            string[] csvFiles = Directory.GetFiles(fullPath, "*.csv");

            // Extrai os dados de cada arquivo CSV
            foreach (string csvFile in csvFiles)
            {
                int indexSlash = csvFile.LastIndexOf("/");
                string[] nomeArquivo = csvFile.Substring(indexSlash+1).Split("-");
                string nomeDepartamento = nomeArquivo[0];
                string mesVigencia = nomeArquivo[1];
                string anoVigencia = nomeArquivo[2].Substring(0,4);
                
                // Verifica se departamento existe e inclui na lista
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
                            continue;
                        }
                        
                        // Desestrutura valores e faz Parse
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
                    }
                }
            }

            
            // Serializa a lista departamentos em JSON
            string json = JsonConvert.SerializeObject(departamentos, Formatting.Indented);

            // Salva o JSON em um arquivo
            string jsonFilePath = Path.Combine(fullPath, "saida.json");
            System.IO.File.WriteAllText(jsonFilePath, json);

            return View("Index");
        }
    }

}
