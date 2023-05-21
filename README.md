# Desafio AUVO

## Projeto de cálculo de folha de pagamento para o departamento do RH

O projeto consiste em um webapp em padrão MVC para cálculo de espelhos de ponto de funcionários de uma empresa.

![screen](~/images/home.png)

Na Home indique o caminho de um diretório. O programa processa todos os arquivos CSV contidos dentro dele e devolve um arquivo JSON com os cálculos de dias e horas trabalhadas, dias de ausência ou extras e valores a receber. Assim como os valores totais por departamento.

O formato do nome do arquivo deve ser o seguinte:

    Departamento de Financeiro-Abril-2022.csv

O formato do arquivo CSV é padrão:

    Código;Nome;Valor hora;Data;Entrada;Saída;Almoço
    1;João da Silva;R$ 110, 97;01/04/2022;08:00:00;18:00:00;12:00 - 13:00

O formato do arquivo JSON obtido é o seguinte:

```json
[
  {
    "NomeDepartamento": "Departamento de Financeiro",
    "MesVigencia": "Abril",
    "AnoVigencia": "2022",
    "TotalAPagar": 26632.80,
    "TotalDescontos": 665.82,
    "TotalExtras": 1997.46,
    "Funcionarios": [
      {
        "Codigo": 2,
        "Nome": "Maria de Oliveira",
        "TotalReceber": 26632.80,
        "HorasExtras": 18.0,
        "HorasDebito": 6.0,
        "DiasFalta": 0,
        "DiasExtras": 0,
        "DiasTrabalhados": 24
      }
    ]
  },
  {
    "NomeDepartamento": "Departamento de Operações",
    "MesVigencia": "Fevereiro",
    "AnoVigencia": "2022",
    "TotalAPagar": 4882.68,
    "TotalDescontos": 0.00,
    "TotalExtras": 443.88,
    "Funcionarios": [
      {
        "Codigo": 1,
        "Nome": "João da Silva",
        "TotalReceber": 1220.67,
        "HorasExtras": 1.0,
        "HorasDebito": 0.0,
        "DiasFalta": 0,
        "DiasExtras": 0,
        "DiasTrabalhados": 1
      }
    ]
  },
  {
    "NomeDepartamento": "Departamento de Operações Especiais",
    "MesVigencia": "Fevereiro",
    "AnoVigencia": "2022",
    "TotalAPagar": 4882.68,
    "TotalDescontos": 0.00,
    "TotalExtras": 443.88,
    "Funcionarios": [
      {
        "Codigo": 1,
        "Nome": "Felipe Rocha",
        "TotalReceber": 8544.69,
        "HorasExtras": 7.0,
        "HorasDebito": 0.0,
        "DiasFalta": 0,
        "DiasExtras": 0,
        "DiasTrabalhados": 7
      }
    ]
  }
]
```