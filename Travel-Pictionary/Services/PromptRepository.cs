using System.Text.Json;

namespace Travel_Pictionary.Services;

public sealed class PromptRepository
{
    private const string PromptFileName = "prompt-list.txt";

    public async Task<IReadOnlyList<string>> LoadPromptsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(PromptFileName);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync(cancellationToken);
            var prompts = JsonSerializer.Deserialize<List<string>>(json);

            if (prompts is null)
            {
                throw new InvalidDataException("The prompt list did not contain a JSON array.");
            }

            var usablePrompts = prompts
                .Where(prompt => !string.IsNullOrWhiteSpace(prompt))
                .Select(prompt => prompt.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (usablePrompts.Length == 0)
            {
                throw new InvalidDataException("The prompt list did not contain any usable prompts.");
            }

            return usablePrompts;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The prompt list is not valid JSON.", exception);
        }
        catch (FileNotFoundException exception)
        {
            throw new InvalidDataException($"The prompt asset '{PromptFileName}' could not be found.", exception);
        }
    }
}
