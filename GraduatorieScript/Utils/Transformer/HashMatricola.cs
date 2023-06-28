namespace GraduatorieScript.Utils.Transformer;

public static class HashMatricola
{
    public static string? HashMatricolaMethod(string? input)
    {
        /*
            riga_matricola = row.select(".Dati1")[1]
            sha256_hash = hashlib.sha256((riga_matricola.string + saltGlobal).encode()).hexdigest()
            sha256_hash = sha256_hash[:maxCharHash]
            riga_matricola.string = sha256_hash
         */
        return input;
    }
}