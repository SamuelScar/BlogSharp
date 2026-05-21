namespace BlogSharp.Api.Services.IA;

public static class PromptBuilder
{
    public static string CriarPromptResumoPostagem(string conteudo)
    {
        return $"""
            Analise o texto de uma postagem de blog pessoal e retorne um JSON em portugues do Brasil.

            Regras:
            - resumo: uma frase curta com no maximo 240 caracteres.
            - categoria: uma categoria curta, como Tecnologia, Backend, Carreira, Estudos, Cultura ou Geral.
            - tags: de 3 a 6 palavras-chave separadas por virgula.
            - Nao inclua Markdown.
            - Nao inclua campos alem de resumo, categoria e tags.

            Texto da postagem:
            {conteudo}
            """;
    }
}
