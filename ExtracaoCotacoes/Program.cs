using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ExtracaoCotacoes
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json");
            var configuration = builder.Build();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-BR");

            Console.WriteLine("Imagem do site de testes no Docker Hub: " +
                "renatogroffe/site-indicadores-economia-nginx");
            Console.WriteLine("Iniciando a extração das cotações...");

            DateTime dataHoraExtracao = DateTime.Now;

            var seleniumConfigurations = new SeleniumConfigurations();
            new ConfigureFromConfigurationOptions<SeleniumConfigurations>(
                configuration.GetSection("SeleniumConfigurations"))
                    .Configure(seleniumConfigurations);

            var pagina = new PaginaCotacoes(seleniumConfigurations);
            pagina.CarregarPagina();
            var cotacoes = pagina.ObterCotacoes();
            pagina.Fechar();


            Console.WriteLine("Gerando o arquivo .xlsx (Excel) com as cotações...");
            var excelConfigurations = new ExcelConfigurations();
            new ConfigureFromConfigurationOptions<ExcelConfigurations>(
                configuration.GetSection("ExcelConfigurations"))
                    .Configure(excelConfigurations);

            string arquivoXlsx = ArquivoExcelCotacoes.GerarArquivo(
                excelConfigurations, dataHoraExtracao, cotacoes);
            Console.WriteLine($"O arquivo {arquivoXlsx} foi gerado com sucesso!");
        }
    }
}