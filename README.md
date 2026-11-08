# posh-crouton/furbot

![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white) 
![Discord](https://img.shields.io/badge/Discord-%235865F2.svg?style=for-the-badge&logo=discord&logoColor=white)

Inspired by [erraineon/Beef](https://github.com/erraineon/Beef). Discord bot to periodically poll various sources for content and forward them to Discord channels. 

## 📋 Prerequisites

.NET 6.0 SDK or newer. Get started with [script/dotnet-install.sh](./script/dotnet-install.sh) (Linux/MacOS) or [script/dotnet-isntall.ps1](./script/dotnet-install.ps1) (Windows) if you don't have it already. 

## 🚀 Get started

* Get hooks using `git config core.hooksPath '$GIT_DIR/../hooks'`
* Restore tools using `dotnet tool restore`

## ▶️ Run

To run locally, use the below command. In production, it's recommended to use Docker (coming soon) instead.

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

No deploy strategy yet! 

## ⚖️ Licensing

Copyright &copy; [posh-crouton](https://github.com/posh-crouton/) &amp; contributors, 2026. 

This project is released to you under the MIT license. 

No third party notices are known to apply. 

For more information, see [LICENSE.md](./LICENSE.md).
