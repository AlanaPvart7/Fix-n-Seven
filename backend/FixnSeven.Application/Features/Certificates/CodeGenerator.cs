using System.Security.Cryptography;
namespace FixnSeven.Application.Features.Certificates;

// Codigos aleatorios y faciles de dictar (sin 0/O ni 1/I)
public static class CodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Create(int length) =>
        new(Enumerable.Range(0, length).Select(_ => Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]).ToArray());

    // XXXX-XXXX-XXXX
    public static string CreateGrouped(int groups, int groupLength) =>
        string.Join("-", Enumerable.Range(0, groups).Select(_ => Create(groupLength)));
}
