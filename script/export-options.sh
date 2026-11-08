#!/usr/bin/env bash
set -euo pipefail

# Run from any directory. Requires Bash and the .NET 10 SDK.
root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
output="${1:-$root/sample.env}"
temporary="$(mktemp -d)"
trap 'rm -rf -- "$temporary"' EXIT

# dotnet-install installs outside PATH by default. IDE terminals may also have
# inherited PATH before a system SDK was installed.
if ! command -v dotnet >/dev/null 2>&1; then
    for directory in "${DOTNET_ROOT:-}" "${HOME:-}/.dotnet" \
        /usr/share/dotnet /usr/local/share/dotnet '/c/Program Files/dotnet'; do
        [[ -n "$directory" ]] || continue
        if [[ -x "$directory/dotnet" || -x "$directory/dotnet.exe" ]]; then
            export PATH="$directory:$PATH"
            break
        fi
    done
fi
if ! command -v dotnet >/dev/null 2>&1; then
    cat >&2 <<EOF
Cannot find dotnet in this Bash environment. Install the .NET 10 SDK or add its directory to PATH.
For Linux/WSL/macOS, run:
  bash "$root/script/dotnet-install.sh" --channel 10.0
Then rerun this script; it also checks DOTNET_ROOT and ~/.dotnet.
On Windows, use Git Bash to access the Windows SDK. WSL needs a Linux SDK installed inside WSL.
EOF
    exit 1
fi
if ! dotnet --list-sdks | grep -q '^10\.'; then
    echo 'The .NET 10 SDK is required; dotnet was found but no .NET 10 SDK is installed.' >&2
    exit 1
fi

# Build every project, including projects not yet added to the solution.
while IFS= read -r -d '' project; do
    dotnet build "$project" --configuration Release --nologo --verbosity quiet >&2
    dotnet msbuild "$project" -nologo -verbosity:quiet \
        -property:Configuration=Release -getProperty:TargetPath >> "$temporary/assemblies.txt"
done < <(find "$root" -type d \( -name .git -o -name bin -o -name obj \) -prune \
    -o -type f -name '*.csproj' -print0)

if [[ ! -s "$temporary/assemblies.txt" ]]; then
    echo 'No project assemblies found.' >&2
    exit 1
fi

cat > "$temporary/Export.csproj" <<'PROJECT'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
PROJECT

cat > "$temporary/Program.cs" <<'CSHARP'
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

var paths = File.ReadAllLines(args[0]).Select(p => p.Trim()).Where(p => p.Length > 0)
    .Select(Path.GetFullPath).Distinct().Order().ToArray();
var directories = paths.Select(Path.GetDirectoryName).Distinct().ToArray();
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    foreach (var directory in directories)
    {
        var candidate = Path.Combine(directory!, name.Name + ".dll");
        if (File.Exists(candidate))
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(candidate);
    }
    return null;
};

var entries = new SortedDictionary<string, (string Type, string Value)>(StringComparer.Ordinal);
foreach (var path in paths)
{
    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
    foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract &&
                 !t.ContainsGenericParameters && t.Name.EndsWith("Options", StringComparison.Ordinal)))
    {
        var section = type.GetField("ConfigurationSectionName", BindingFlags.Public | BindingFlags.Static);
        if (section is not { IsLiteral: true, IsInitOnly: false } || section.FieldType != typeof(string))
            continue;
        var name = (string?)section.GetRawConstantValue();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException($"Empty configuration section on {type.FullName}.");
        Walk(type, Create(type), name.Replace(":", "__"), new HashSet<Type>());
    }
}

if (entries.Count == 0)
    throw new InvalidOperationException("No configuration options were found in the built assemblies.");
using var writer = new StreamWriter(args[1], false, new UTF8Encoding(false));
writer.NewLine = "\n";
foreach (var (key, entry) in entries)
{
    writer.WriteLine($"{key}={entry.Value} # {entry.Type}");
}

object? Create(Type type) => type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic |
    BindingFlags.Instance, null, Type.EmptyTypes, null) is { } constructor
    ? constructor.Invoke(null) : null;

void Walk(Type type, object? value, string key, HashSet<Type> parents)
{
    var actual = Nullable.GetUnderlyingType(type) ?? type;
    if (actual == typeof(string) || actual.IsPrimitive || actual.IsEnum || actual == typeof(decimal) ||
        actual == typeof(TimeSpan) || actual == typeof(DateTime) || actual == typeof(DateTimeOffset) ||
        actual == typeof(Guid) || actual == typeof(Uri))
    {
        var text = value is null ? "" : Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        if (value is bool boolean) text = boolean ? "true" : "false";
        if (text.Contains('\r') || text.Contains('\n'))
            throw new InvalidOperationException($"Multiline default cannot be represented in an env file: {key}");
        if (!entries.TryAdd(key, (type.ToString(), text)))
            throw new InvalidOperationException($"Duplicate configuration key: {key}");
        return;
    }
    if (!parents.Add(actual))
        throw new InvalidOperationException($"Recursive options type at {key}: {actual}");
    try
    {
        var enumerable = actual.GetInterfaces().Append(actual)
            .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (enumerable is not null)
        {
            var element = enumerable.GetGenericArguments()[0];
            var items = (value as IEnumerable)?.Cast<object?>().ToArray() ?? [];
            // Include an example index even when the default collection is empty.
            if (items.Length == 0) items = [element.IsValueType ? Activator.CreateInstance(element) : Create(element)];
            for (var i = 0; i < items.Length; i++)
                Walk(element, items[i], $"{key}__{i}", parents);
            return;
        }
        value ??= Create(actual);
        foreach (var property in actual.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.GetIndexParameters().Length == 0 && p.GetMethod?.IsPublic == true &&
                         p.SetMethod?.IsPublic == true).OrderBy(p => p.Name, StringComparer.Ordinal))
            Walk(property.PropertyType, value is null ? null : property.GetValue(value),
                $"{key}__{property.Name}", parents);
    }
    finally { parents.Remove(actual); }
}
CSHARP

dotnet build "$temporary/Export.csproj" --configuration Release --nologo --verbosity quiet >&2
dotnet "$temporary/bin/Release/net10.0/Export.dll" "$temporary/assemblies.txt" "$temporary/sample.env"
cp -- "$temporary/sample.env" "$output"
printf 'Generated %s\n' "$output"
