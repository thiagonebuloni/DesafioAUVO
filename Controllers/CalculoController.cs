using DesafioAUVO.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Net;
using static Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
using System.Diagnostics;
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
        public async Task<ActionResult> CalculoAsync(string folderPath)
        {
            List<Departamento> departamentos = new List<Departamento>();
            List<Funcionario> funcionarios = new List<Funcionario>();

            // Verifica se o caminho foi inserido no input
            if (string.IsNullOrEmpty(folderPath))
            {
                ModelState.AddModelError("folderPath", "Por favor digite um caminho válido.");
                return View("Calculo");
            }

            // Caminho raiz do app
            string rootPath = _hostingEnvironment.ContentRootPath;

            // Combina o caminho raiz com o diretório de input
            string fullPath = Path.Combine(rootPath, folderPath);

            if (!Directory.Exists(fullPath))
            {
                ModelState.AddModelError("folderPath", "O caminho inserido não existe.");
                return View("Calculo");
            }

            // Todos os arquivos CSV do diretório
            string[] csvFiles = Directory.GetFiles(fullPath, "*.csv");

            // Extrai os dados de cada arquivo CSV
            string mesVigencia = "";
            string anoVigencia = "";

            
            Parallel.ForEach(csvFiles, csvFile => 
            {
                
                int indexSlash = csvFile.LastIndexOf("/");
                string[] nomeArquivo = csvFile.Substring(indexSlash+1).Split("-");
                string nomeDepartamento = nomeArquivo[0];
                mesVigencia = nomeArquivo[1];
                anoVigencia = nomeArquivo[2].Substring(0,4);

                // Verifica se departamento existe e inclui na lista
                int indexDepartamento = departamentos.FindIndex(d => d.NomeDepartamento == nomeDepartamento);
                if (indexDepartamento == -1)
                {
                    Departamento novoDepartamento = new Departamento(nomeDepartamento, mesVigencia, anoVigencia);
                    departamentos.Add(novoDepartamento);
                    indexDepartamento = departamentos.FindIndex(d => d.NomeDepartamento == nomeDepartamento);
                }
                
                bool ehPrimeiraLinha = true;

                using (var reader = new StreamReader(csvFile, System.Text.Encoding.GetEncoding("iso-8859-1")))
                {
                    while (!reader.EndOfStream)
                    {
                        var linha = reader.ReadLine();
                        var valores = linha.Split(';');

                        if (ehPrimeiraLinha)
                        {
                            ehPrimeiraLinha = false;
                            continue;
                        }
                        
                        // Desestrutura valores e faz Parse
                        int codigo = int.Parse(valores[0]);
                        string nome = valores[1];
                        decimal valorHora = decimal.Parse(String.Concat(valores[2].Where(c => !Char.IsWhiteSpace(c))).Substring(2).Replace(",","."));
                        DateOnly data = DateOnly.ParseExact(valores[3], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime entrada = DateTime.ParseExact(valores[4], "HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime saida = DateTime.ParseExact(valores[5], "HH:mm:ss", CultureInfo.InvariantCulture);
                        string almoco = valores[6];

                        // Constantes
                        decimal valorMinuto = valorHora / 60;
                        int minimoMinutosDia = 9 * 60;

                        // Calculo horas trabalhadas
                        TimeSpan difference = saida - entrada;
                        decimal valorAReceber = valorMinuto * Convert.ToDecimal(difference.TotalMinutes);
                        double horasDebito = 0;
                        double horasExtras = 0;

                        // Calculo almoço
                        DateTime almocoInicio = DateTime.ParseExact(almoco.Substring(0,5), "HH:mm", CultureInfo.InvariantCulture);
                        DateTime almocoFinal = DateTime.ParseExact(almoco.Substring(8), "HH:mm", CultureInfo.InvariantCulture);
                        TimeSpan tempoAlmoco = almocoFinal - almocoInicio;

                        // Almoco maior que 1 hora
                        if (tempoAlmoco.TotalMinutes > 60)
                        {
                            valorAReceber -= Convert.ToDecimal(tempoAlmoco.TotalMinutes - 60) * valorMinuto;
                        }

                        // Calculo horas em débito e horas extras
                        if (difference.TotalMinutes < minimoMinutosDia)
                        {
                            horasDebito = (minimoMinutosDia - difference.TotalMinutes) / 60;
                        }
                        else if (difference.TotalMinutes > minimoMinutosDia)
                        {
                            horasExtras = (difference.TotalMinutes - minimoMinutosDia) / 60;
                        }

                        valorAReceber = valorAReceber + (Convert.ToDecimal(horasExtras) * valorHora) - (Convert.ToDecimal(horasDebito) * valorHora);

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

                        int indexFunciEmDepartamento = departamentos[indexDepartamento]
                                                        .Funcionarios.FindIndex(f => f.Codigo == codigo);
                        if (indexFunciEmDepartamento == -1)
                        {
                            departamentos[indexDepartamento]
                            .AcrescentaFuncionarioNoDepartamento(funcionarios[indexFuncionario]);
                        }
                        departamentos[indexDepartamento].SomaPagamentos(valorAReceber);
                        departamentos[indexDepartamento].SomaDescontos(Convert.ToDecimal((horasDebito * 60)) * valorMinuto);
                        departamentos[indexDepartamento].SomaExtras(Convert.ToDecimal((horasExtras * 60)) * valorMinuto);
                    }
                }
            });

            // Calcula dias falta e dias extras
            int diasUteis = DiasUteis.DiasUteisNoMes(int.Parse(anoVigencia), mesVigencia);
            Parallel.ForEach(funcionarios, funcionario => 
            {
                if (funcionario.DiasTrabalhados < diasUteis)
                {
                    int diasFalta = diasUteis - funcionario.DiasTrabalhados;
                    funcionario.SomaDiasFalta(diasFalta);
                }
                else if (funcionario.DiasTrabalhados > diasUteis)
                {
                    int diasExtras = funcionario.DiasTrabalhados - diasUteis;
                    funcionario.SomaDiasExtras(diasExtras);
                }
            });

            
            // Serializa a lista departamentos em JSON
            string json = JsonConvert.SerializeObject(departamentos, Formatting.Indented);

            // Salva o JSON em um arquivo
            string jsonFilePath = Path.Combine(fullPath, "saida.json");
            System.IO.File.WriteAllText(jsonFilePath, json);

            return View("Calculo");
        }
    }

}
