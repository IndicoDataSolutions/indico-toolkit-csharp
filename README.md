# Indico Toolkit

**This repository contains software that is not officially supported by Indico. It may
  be outdated or contain bugs. The operations it performs are potentially destructive.
  Use at your own risk.**

Classes, functions, and abstractions for building workflows using the Indico IPA
(Intelligent Process Automation) platform.

- [Result File](https://github.com/IndicoDataSolutions/indico-toolkit-csharp/blob/main/IndicoToolkit/Results)
  and [Etl Output](https://github.com/IndicoDataSolutions/indico-toolkit-csharp/blob/main/IndicoToolkit/EtlOutput)
  Data Classes that parse standard IPA JSON output into idiomatic, type-safe C# objects.


## Usage

Clone the repo and copy relevant folders from the `IndicoToolkit` project into your C#
project.

```bash
git clone git@github.com:IndicoDataSolutions/indico-toolkit-csharp.git
```


### Tests

Run the unit tests defined in the `IndicoToolkit.Tests` project.

```bash
cd IndicoToolkit.Tests
dotnet test
```
