CREATE TABLE IF NOT EXISTS Videos (
    Id SERIAL PRIMARY KEY,
    NomeArquivo VARCHAR(255) NOT NULL,
    Conteudo BYTEA NULL,
    Status INTEGER NOT NULL,
    DataCriacao TIMESTAMP NOT NULL,
    MensagemErro TEXT NULL,
    Usuario VARCHAR(100) NOT NULL,
    CaminhoVideo VARCHAR(255) NULL,
    CaminhoZip VARCHAR(255) NULL
);