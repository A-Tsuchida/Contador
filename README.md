# Contador

Aplicativo desktop desenvolvido em F# com Avalonia para controlar um contador de tempo/relógio em tela.

## Pré-requisitos

Antes de compilar e executar o projeto, certifique-se de ter instalado:

- [.NET SDK 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) ou superior
- Um ambiente compatível com aplicações desktop Avalonia (Windows, Linux ou macOS)

> Este projeto usa o SDK .NET 10 e a solução está configurada em `Counter.slnx`.

## Clonando o repositório

```bash
git clone https://github.com/A-Tsuchida/Contador.git
cd Contador
```

## Como compilar

No diretório raiz do repositório, execute:

```bash
dotnet restore
```

Em seguida, compile a solução:

```bash
dotnet build "Counter.slnx" --configuration Release
```

Ou, se preferir compilar diretamente o projeto:

```bash
dotnet build "Counter/Counter.fsproj" --configuration Release
```

## Como executar

Após a compilação, rode o projeto com:

```bash
dotnet run --project "Counter/Counter.fsproj"
```

## Estrutura principal

- `Counter/` — código-fonte do projeto
- `Counter.slnx` — solução do projeto
- `LICENSE.txt` — licença do repositório

## Observações

- Os dados da aplicação são salvos na pasta de dados do aplicativo do sistema operacional (`ApplicationData`), então não há necessidade de configuração em nível do sistema.
- Em desenvolvimento, também é possível usar `dotnet build` sem `--configuration Release` para compilar em modo Debug.
