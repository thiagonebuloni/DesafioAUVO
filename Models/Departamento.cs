namespace FirstTryDesafio.Models
{

    public class Departamento
    {
        

        public string? NomeDepartamento { get; set; }
        public string? MesVigencia { get; set; }
        public string? AnoVigencia { get; set; }
        public decimal TotalAPagar { get; set; } = 0;
        public decimal TotalDescontos { get; set; } = 0;
        public decimal TotalExtras { get; set; } = 0;
        public List<Funcionario> Funcionarios { get; set; }
        
        public Departamento
        (
            string? nomeDepartamento,
            string? mesVigencia,
            string? anoVigencia
        )
        {
            NomeDepartamento = nomeDepartamento;
            MesVigencia = mesVigencia;
            AnoVigencia = anoVigencia;
            Funcionarios = new List<Funcionario>();
        }
        
        public void SomaPagamentos(decimal valorAReceber)
        {
            TotalAPagar += Math.Round(valorAReceber, 2);
        }

        public void SomaDescontos(decimal valorDesconto)
        {
            TotalDescontos += Math.Round(valorDesconto, 2);
        }

        public void SomaExtras(decimal valorExtra)
        {
            TotalExtras += Math.Round(valorExtra, 2);
        }

        public void AcrescentaFuncionarioNoDepartamento(Funcionario funcionario)
        {
            Funcionarios.Add(funcionario);
        }
    }
}