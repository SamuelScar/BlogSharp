namespace BlogSharp.Api.Services.IA;

public static class PromptBuilder
{
    /// <summary>
    /// Monta o prompt que obriga a IA a responder no contrato JSON esperado pelo projeto.
    /// </summary>
    public static string CriarPromptResumoPostagem(string conteudo)
    {
        return $$"""
            Analise o texto de uma postagem de blog pessoal conforme o desafio de IA do projeto.

            A IA deve gerar somente os dados definidos na documentacao:
            - Um resumo curto da postagem.
            - Palavras-chave relacionadas ao texto.
            - Uma sugestao de categoria.

            Contrato obrigatorio da resposta:
            - Retorne somente um objeto JSON valido.
            - Use exatamente as chaves "resumo", "tags" e "categoria".
            - Todos os valores devem ser strings nao vazias em portugues do Brasil.
            - "resumo" deve ser uma frase curta sobre a postagem.
            - "tags" deve ser uma unica string com 3 a 6 palavras-chave separadas por virgula.
            - "categoria" deve ser uma categoria curta, como Tecnologia, Backend, Carreira, Estudos, Cultura ou Geral.
            - Nao retorne Markdown, lista, explicacao, texto antes ou depois do JSON.
            - Nao retorne arrays, objetos aninhados, null ou campos adicionais.
            - Baseie a resposta apenas no texto da postagem.

            Formato esperado:
            {"resumo":"Resumo curto da postagem.","tags":"API, REST, ASP.NET Core","categoria":"Tecnologia"}

            Texto da postagem:
            {{conteudo}}
            """;
    }
}
