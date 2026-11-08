# posh-crouton/furbot

![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white) 
![Discord](https://img.shields.io/badge/Discord-%235865F2.svg?style=for-the-badge&logo=discord&logoColor=white)

Inspired by [erraineon/Beef](https://github.com/erraineon/Beef). Discord bot to periodically poll various sources for content and forward them to Discord channels. 

## 📋 Prerequisites

.NET 10.0 SDK. Get started with [script/dotnet-install.sh](./script/dotnet-install.sh) (Linux/MacOS) or [script/dotnet-install.ps1](./script/dotnet-install.ps1) (Windows) if you don't have it already. Docker deployments only require Docker with Linux container support.

## 🚀 Get started

* Get hooks using `git config core.hooksPath '$GIT_DIR/../hooks'`
* Restore tools using `dotnet tool restore`

## ▶️ Run

To run locally, use the below command. For production, see the Docker instructions under Deploy.

```bash
cd src/FurBot && dotnet run 
```

## 🧪 Test

No tests yet! 

### 📊 Coverage

To generate a coverage report, use `dotnet test --collect:"XPlat Code Coverage"`.

To render the coverage report, use `dotnet reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html` (prerequisite: `dotnet tool restore`). This will generate a coverage report at [/coveragereport/index.html](./coveragereport/index.html).

### 📈 Benchmarks

No benchmarks yet!

## 🛳️ Deploy

Create a `.env` file in the repository root with one `KEY=value` per line. Use double underscores (`__`) for nested configuration keys.

Run `bash script/export-options.sh` to build all projects and generate `sample.env` from the compiled options types. It includes type comments and property defaults; empty collections get an example entry at index `0`. Copy it to `.env`, fill in credentials, and replace or remove example collection entries before running. An optional first argument selects a different output file.

Build and start the container from the repository root:

```bash
docker build -t furbot .
docker run -d --name furbot --restart unless-stopped --env-file .env furbot
```

## ⚖️ Licensing

Copyright &copy; [posh-crouton](https://github.com/posh-crouton/) &amp; contributors, 2026. 

This project is released to you under the MIT license. 

No third party notices are known to apply. 

For more information, see [LICENSE.md](./LICENSE.md).
