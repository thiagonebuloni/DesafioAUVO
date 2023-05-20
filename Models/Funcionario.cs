namespace DesafioAUVO.Models
{

    public class Funcionario
    {
        public int Codigo { get; set; }
        public string? Nome { get; set; }
        public decimal TotalReceber { get; set; } = 0;
        public double HorasExtras { get; set; } = 0;
        public double HorasDebito { get; set; } = 0;
        public int DiasFalta { get; set; } = 0;
        public int DiasExtras { get; set; } = 0;
        public int DiasTrabalhados { get; set; } = 0;

        public Funcionario
        (
            string? nome,
            int codigo,
            decimal valorAReceber,
            double horasExtras,
            double horasDebito
        )
        {
            Nome = nome;
            Codigo = codigo;
            TotalReceber = Math.Round(valorAReceber, 2);
            HorasExtras = horasExtras;
            HorasDebito = horasDebito;
        }

        public void SomaTotalReceber(decimal valor)
        {
            TotalReceber += Math.Round(valor, 2);
        }

        public void SomaHorasExtras(double horasExtras)
        {
            HorasExtras += horasExtras;
        }

        public void SomaHorasDebito(double horasDebito)
        {
            HorasDebito += horasDebito;
        }

        public void SomaDiasFalta(int diasFalta)
        {
            DiasFalta += diasFalta;
        }

        public void SomaDiasExtras(int diasExtras)
        {
            DiasExtras += diasExtras;
        }

        public void AcrescentaDiasTrabalhados()
        {
            DiasTrabalhados++;
        }
    }
}